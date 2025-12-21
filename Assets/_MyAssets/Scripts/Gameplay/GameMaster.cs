using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] private UserPlayer player;
    [SerializeField] private EnemyPlayer enemy;

    [SerializeField] TextMeshProUGUI WhoGoesFirstText;

    [SerializeField] PlayingCard PlayingCardPrefab;

    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardAndCaptainCardLibrary = new List<BaseCard>();

    [SerializeField] private GameObject WaitingForOpponent;

    private List<PlayingCard> Player1Allies = new List<PlayingCard>();
    private List<PlayingCard> Player1Discard = new List<PlayingCard>();
    private List<PlayingCard> Player2Allies = new List<PlayingCard>();
    private List<PlayingCard> Player2Discard = new List<PlayingCard>();

    [SerializeField] private GameObject opponentSpark;
    private int opponentSparkValue;

    [SerializeField] private GameObject allySpark;
    public int allySparkValue { get; private set; }

    [SerializeField] private TextMeshProUGUI opponentSparkText;
    [SerializeField] private TextMeshProUGUI allySparkText;

    [SerializeField] private Transform allySide;
    [SerializeField] private Transform allyDiscard;
    [SerializeField] private Transform enemySide;
    [SerializeField] private Transform enemyDiscard;

    [SerializeField] private Sprite[] numberSpriteOutline;
    [SerializeField] private Sprite[] numberSpriteWhole;
    private float vfxYSpawn = -3.0f;
    [SerializeField] private GameObject allySparkGainParticleSystem;
    [SerializeField] private GameObject enemySparkGainParticleSystem;

    public List<BaseCard> GetCaptainLibrary() { return CaptainLibrary; }
    public List<BaseCard> GetEveryCardLibrary() { return CardAndCaptainCardLibrary; }

    [Header("Player Data")]
    public FirebasePlayerInfo FirebasePlayer;

    private bool bIsPlayer1;

    public bool bPlayer1sTurn { get; private set; }

    private string player1_ID; // SerializeField just for testing of course
    private string player2_ID;

    private bool bHit3SparkThreshold;
    private bool bHit10SparkThreshold;

    private bool player1IsReady; // SerializeField just for testing of course
    private bool player2IsReady;

    private BaseCard captainHolder;

    [SerializeField] bool bDevTest;
    [SerializeField] bool bSkipMulligan;

    void Start()
    {
        opponentSpark.SetActive(false);
        allySpark.SetActive(false);

        Crossroads();
        StartCoroutine(StartGame());
    }

    public Transform GetDiscardPilieTransform(bool isPlayer1)
    {
        return (isPlayer1 == bIsPlayer1) ? allyDiscard : enemyDiscard;
    }

    public void AddCardToDiscard(PlayingCard cardtoAdd)
    {
        if (bIsPlayer1)
        {
            Player1Discard.Add(cardtoAdd);
        }
        else
        {
            Player2Discard.Add(cardtoAdd);
        }
    }

    private IEnumerator StartGame()
    {
        opponentSparkValue = 0;
        allySparkValue = 0;
        opponentSparkText.text = "0/20";
        allySparkText.text = "0/20";

        yield return new WaitForSeconds(3);

        if (bDevTest == false)
            SetPlayerIDs();

        if (bIsPlayer1) // Only executes on player 1, so as we don't get two random numbers
        {
            bool whoGoesFirst = UnityEngine.Random.Range(0, 2) == 0;
            this.photonView.RPC("WhoGoesFirst", RpcTarget.AllBuffered, whoGoesFirst);
        }

        yield return new WaitForSeconds(1);

        if(bIsPlayer1)
            player.SetIsPlayer1();

        if (bSkipMulligan == false)
            player.StartMulligan();
        else
            player.SkipMulligan(8);

        StartCoroutine(PlayersAddingNewCaptains(false));
    }


    public void SetPlayerIDs()
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        player1_ID = props["P1"] as string;
        player2_ID = props["P2"] as string;

        bIsPlayer1 = (player1_ID == FirebasePlayer.GetUserID());

        enemy.LoadOpponentID((bIsPlayer1) ? player2_ID : player1_ID, bIsPlayer1);
    }

    public void Crossroads()
    {
        player1IsReady = false;
        player2IsReady = false;
    }

    public void PlayerConfirmedNewCaptain(BaseCard captain)
    {
        WaitingForOpponent.SetActive(true);
        captainHolder = captain;
        this.photonView.RPC("PlayerLocksIn", RpcTarget.AllBuffered, bIsPlayer1);
    }

    private IEnumerator PlayersAddingNewCaptains(bool bRequestSwitchTurns)
    {
        while (player1IsReady == false || player2IsReady == false)
            yield return new WaitForEndOfFrame();

        WhoGoesFirstText.text = "";

        AddAllyToBoard(captainHolder);

        yield return new WaitForSeconds(0.5f);

        if (bRequestSwitchTurns && bIsPlayer1)
            RequestSwitchTurns();

        opponentSpark.SetActive(true);
        allySpark.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if(bIsPlayer1 == bPlayer1sTurn)
        {
            player.StartTurn(false);
        }
    }

    private int GetCardIndexForLibrary(BaseCard card, List<BaseCard> library)
    {
        for (int i = 0; i < library.Count; i++)
        {
            if (library[i].CardName == card.CardName)
                return i;
        }
        return 0;
    }

    public List<PlayingCard> GetAllAllies(bool bGetMyTeam) 
    { 
        if(bGetMyTeam)
        {
            return (bIsPlayer1) ? Player1Allies : Player2Allies;
        }
        else
        {
            return (bIsPlayer1) ? Player2Allies : Player1Allies;
        }
    }

    public void AddAllyToBoard(BaseCard cardToAdd)
    {
        bool bIsCaptain = (cardToAdd.Type.type == CardType.Captain);
        int cardIndex = (bIsCaptain) ? GetCardIndexForLibrary(cardToAdd, CaptainLibrary) : GetCardIndexForLibrary(cardToAdd, CardAndCaptainCardLibrary);
        this.photonView.RPC("AddAllyToBoard", RpcTarget.AllBuffered, bIsPlayer1, bIsCaptain, cardIndex);
    }

    [PunRPC]
    void AddAllyToBoard(bool isPlayer1, bool IsCaptain, int cardLibraryIndex)
    {
        List<PlayingCard> playerAllies = (isPlayer1) ? Player1Allies : Player2Allies;
        List<BaseCard> cardLibrary = (IsCaptain) ? CaptainLibrary : CardAndCaptainCardLibrary ;
        Transform side = (isPlayer1 == bIsPlayer1) ? allySide : enemySide;

        BaseCard newCardCopy = ScriptableObject.Instantiate(cardLibrary[cardLibraryIndex]);

        PlayingCard card = Instantiate(PlayingCardPrefab, side.transform);
        float startingYPos = (isPlayer1 == bIsPlayer1) ? -500.0f : 500.0f ;
        card.Init(player, newCardCopy, isPlayer1);
        card.transform.localPosition = new Vector3(0, startingYPos, 0);
        playerAllies.Add(card);

        int count = playerAllies.Count;

        const float minCount = 2f;
        const float maxCount = 9f;

        const float maxValue = 400f;
        const float minValue = 240f;

        float c = Mathf.Clamp(count, minCount, maxCount);
        float t = (c - minCount) / (maxCount - minCount);

        float spacing = Mathf.Lerp(maxValue, minValue, t);

        for (int i = 0; i < count; i++)
        {
            if (playerAllies[i] == null)
                continue;

            float offset = (i - (count - 1) / 2f) * spacing;

            playerAllies[i].transform.SetAsFirstSibling();
            playerAllies[i].StartMoveCard(offset, true, 0.5f);
        }
    }

    public void RequestPlayCard(PlayingCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting, bool bAttackPrediction)
    {
        int cardIndex = 0;
        bool bIsCaptain = false;
        if(cardToPlay.myCard is CaptainCard captain)
        {
            bIsCaptain = true;
            cardIndex = GetCardIndexForLibrary(cardToPlay.myCard, CaptainLibrary);
        }
        else
        {
            cardIndex = GetCardIndexForLibrary(cardToPlay.myCard, CardAndCaptainCardLibrary);
        }

        int captainUsingIndex = 0;
        List<int> captainTargetingIndex = new List<int>();
        if (bIsPlayer1)
        {
            captainUsingIndex = Player1Allies.IndexOf(captainUsing);

            foreach (PlayingCard targets in captainTargeting)
                captainTargetingIndex.Add((bTargetingEnemy) ? Player2Allies.IndexOf(targets) : Player1Allies.IndexOf(targets));
        }
        else
        {
            captainUsingIndex = Player2Allies.IndexOf(captainUsing);

            foreach (PlayingCard targets in captainTargeting)
                captainTargetingIndex.Add((bTargetingEnemy) ? Player1Allies.IndexOf(targets) : Player2Allies.IndexOf(targets));
        }

        if(bIsPlayer1 && captainUsingIndex == -1)
        {
            captainUsingIndex = Player2Allies.IndexOf(captainUsing);

            captainTargetingIndex.Clear();

            foreach (PlayingCard targets in captainTargeting)
                captainTargetingIndex.Add((bTargetingEnemy) ? Player1Allies.IndexOf(targets) : Player2Allies.IndexOf(targets));
        }
        else if(bIsPlayer1 == false && captainUsingIndex == -1)
        {
            captainUsingIndex = Player1Allies.IndexOf(captainUsing);

            captainTargetingIndex.Clear();

            foreach (PlayingCard targets in captainTargeting)
                captainTargetingIndex.Add((bTargetingEnemy) ? Player2Allies.IndexOf(targets) : Player1Allies.IndexOf(targets));
        }

        if (bAttackPrediction)
        {
            this.photonView.RPC("AttackPrediction", RpcTarget.AllBuffered, bIsPlayer1, cardIndex, captainUsingIndex, bTargetingEnemy, captainTargetingIndex.ToArray(), bIsCaptain);
        }
        else
        {
            this.photonView.RPC("PlayCard", RpcTarget.AllBuffered, bIsPlayer1, cardIndex, captainUsingIndex, bTargetingEnemy, captainTargetingIndex.ToArray(), bIsCaptain);
        }
    }

    [PunRPC]
    void PlayCard(bool isPlayer1, int cardToPlayIndex, int captainUsingIndex, bool bTargetingEnemy, int[] captainTargetingIndex, bool isCaptain)
    {
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            player.PlayClientCard(bTargetingEnemy);
        }
        else // other player
        {
            PlayingCard captainUsing = null;
            List<PlayingCard> captainTarget = new List<PlayingCard>();
            if (isPlayer1)
            {
                captainUsing = Player1Allies[captainUsingIndex];

                for (int i = 0; i < captainTargetingIndex.Length; i++)
                    captainTarget.Add((bTargetingEnemy) ? Player2Allies[captainTargetingIndex[i]] : Player1Allies[captainTargetingIndex[i]]);
            }
            else
            {
                captainUsing = Player2Allies[captainUsingIndex];

                for (int i = 0; i < captainTargetingIndex.Length; i++)
                    captainTarget.Add((bTargetingEnemy) ? Player1Allies[captainTargetingIndex[i]] : Player2Allies[captainTargetingIndex[i]]);
            }

            BaseCard cardToPlay = (isCaptain) ? CaptainLibrary[cardToPlayIndex] : CardAndCaptainCardLibrary[cardToPlayIndex];
            enemy.PlayEnemyCard(cardToPlay, captainUsing, bTargetingEnemy, captainTarget);
        }
    }

    [PunRPC]
    void AttackPrediction(bool isPlayer1, int cardToPlayIndex, int captainUsingIndex, bool bTargetingEnemy, int[] captainTargetingIndex, bool isCaptain)
    {
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            PlayingCard captainUsing = null;
            if (isPlayer1)
            {
                captainUsing = Player1Allies[captainUsingIndex];
            }
            else
            {
                captainUsing = Player2Allies[captainUsingIndex];
            }

            WaitingForOpponent.SetActive(true);
            player.WaitingForReaction(true, captainUsing);
        }
        else // other player
        {
            PlayingCard captainUsing = null;
            List<PlayingCard> captainTarget = new List<PlayingCard>();
            if (isPlayer1)
            {
                captainUsing = Player1Allies[captainUsingIndex];

                for (int i = 0; i < captainTargetingIndex.Length; i++)
                    captainTarget.Add((bTargetingEnemy) ? Player2Allies[captainTargetingIndex[i]] : Player1Allies[captainTargetingIndex[i]]);
            }
            else
            {
                captainUsing = Player2Allies[captainUsingIndex];

                for (int i = 0; i < captainTargetingIndex.Length; i++)
                    captainTarget.Add((bTargetingEnemy) ? Player1Allies[captainTargetingIndex[i]] : Player2Allies[captainTargetingIndex[i]]);
            }

            BaseCard cardToPlay = (isCaptain) ? CaptainLibrary[cardToPlayIndex] : CardAndCaptainCardLibrary[cardToPlayIndex];
            cardToPlay.bWaitForReaction = true;
            player.AllowReaction(true, cardToPlay, captainUsing, bTargetingEnemy, captainTarget);
            enemy.EnemyIsAttackingPredicition(cardToPlay, captainUsing, bTargetingEnemy, captainTarget);
        }
    }

    public void RequestFinishReaction()
    {
        this.photonView.RPC("FinishReaction", RpcTarget.AllBuffered, bIsPlayer1);
    }

    [PunRPC]
    void FinishReaction(bool isPlayer1)
    {
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            enemy.EnemyFinishReaction();
        }
        else // other player
        {
            WaitingForOpponent.SetActive(false);
            enemy.ClearLineAttacks();
            player.EnemyFinishReaction();
        }
    }

    public void RequestDrawCards(int cardsToDraw)
    {
        this.photonView.RPC("DrawCards", RpcTarget.AllBuffered, bIsPlayer1, cardsToDraw);
    }

    [PunRPC]
    void DrawCards(bool isPlayer1, int cardsToDraw)
    {
        if(isPlayer1 == bIsPlayer1) // Owner, this client
        {
            player.RequestDrawCards(cardsToDraw);
        }
        else // other player
        {
            enemy.RequestDrawCards(cardsToDraw);
        }
    }

    public void RequestIncreaseSpark(PlayingCard captainGainingSpark, int SparkToAdd)
    {
        int captainUsingIndex = (bIsPlayer1) ? Player1Allies.IndexOf(captainGainingSpark) : Player2Allies.IndexOf(captainGainingSpark);

        if(captainUsingIndex == -1)
            captainUsingIndex = (bIsPlayer1) ? Player2Allies.IndexOf(captainGainingSpark) : Player1Allies.IndexOf(captainGainingSpark);

        this.photonView.RPC("IncreaseSpark", RpcTarget.AllBuffered, bIsPlayer1, captainUsingIndex, SparkToAdd);
    }

    [PunRPC]
    void IncreaseSpark(bool isPlayer1, int captainGainingSparkIndex, int SparkToAdd)
    {
        Vector3 vfxSpawnPos = new Vector3();
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            if(bIsPlayer1)
                vfxSpawnPos = Player1Allies[captainGainingSparkIndex].transform.position;
            else
                vfxSpawnPos = Player2Allies[captainGainingSparkIndex].transform.position;
            
            allySparkValue += SparkToAdd;
            allySparkText.text = allySparkValue + "/20";
        }
        else // other player
        {
            if(bIsPlayer1)
                vfxSpawnPos = Player2Allies[captainGainingSparkIndex].transform.position;
            else
                vfxSpawnPos = Player1Allies[captainGainingSparkIndex].transform.position;

            opponentSparkValue += SparkToAdd;
            opponentSparkText.text = opponentSparkValue + "/20";
        }

        #region Extremely Sad Spark Spawn Logic

        vfxSpawnPos.y = vfxYSpawn;

        GameObject sparkPrefab = (bIsPlayer1 == isPlayer1) ? allySparkGainParticleSystem : enemySparkGainParticleSystem;
        GameObject sparkGain = Instantiate(sparkPrefab, vfxSpawnPos, sparkPrefab.transform.rotation);

        var wholeRenderer = sparkGain.transform.Find("Whole").GetComponent<ParticleSystemRenderer>();

        Material wholeMat = new Material(wholeRenderer.sharedMaterial);
        wholeRenderer.material = wholeMat;
        wholeMat.SetTexture("_BaseMap", numberSpriteWhole[SparkToAdd - 1].texture);

        var outlineRenderer = sparkGain.transform.Find("Outline").GetComponent<ParticleSystemRenderer>();
        

        Material outlineMat = new Material(outlineRenderer.sharedMaterial);
        outlineRenderer.material = outlineMat;
        outlineMat.SetTexture("_BaseMap", numberSpriteOutline[SparkToAdd - 1].texture);

        ParticleSystem sparkParticle = sparkGain.GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule emission = sparkParticle.emission;
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        ParticleSystem.Burst burst = bursts[0];
        burst.count = new ParticleSystem.MinMaxCurve(SparkToAdd);
        bursts[0] = burst;

        emission.SetBursts(bursts);

        Destroy(sparkGain, sparkParticle.main.duration + sparkParticle.main.startLifetime.constantMax);

        #endregion


        if ((allySparkValue >= 3 || opponentSparkValue >= 3) && bHit3SparkThreshold == false)
        {
            Crossroads();
            bHit3SparkThreshold = true;
            player.StartPickNewCaptain();
            StartCoroutine(PlayersAddingNewCaptains(true));
            return;
        }

        if ((allySparkValue >= 10 || opponentSparkValue >= 10) && bHit10SparkThreshold == false)
        {
            Crossroads();
            bHit10SparkThreshold = true;
            player.StartPickNewCaptain();
            StartCoroutine(PlayersAddingNewCaptains(true));
            return;
        }
    }

    public void RequestFinishMulligan(int cardsToMulligan)
    {
        this.photonView.RPC("RequestFinishMulligan", RpcTarget.AllBuffered, bIsPlayer1, cardsToMulligan);
    }

    [PunRPC]
    void RequestFinishMulligan(bool isPlayer1, int cardsToMulligan)
    {
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            player.RequestDrawCards(cardsToMulligan);
            StartCoroutine(player.MullgianWrapUp());
        }
        else // other player
        {
            enemy.RequestDrawCards(cardsToMulligan);
            StartCoroutine(enemy.MullgianWrapUp(cardsToMulligan));
        }
    }

    public void RequestSwitchTurns()
    {
        this.photonView.RPC("SwitchTurns", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SwitchTurns()
    {
        StaticGameplayDelegates.TurnEnded(bPlayer1sTurn);

        bPlayer1sTurn = !bPlayer1sTurn;

        if(bPlayer1sTurn)
        {
            foreach(PlayingCard teammate in Player1Allies)
            {
                if (teammate.myCard.bDead == true)
                    continue;

                teammate.myCard.bOncePerTurn = true;
                StartCoroutine(teammate.EnergizeAndExhaust(true));
            }
        }
        else
        {
            foreach (PlayingCard teammate in Player2Allies)
            {
                if (teammate.myCard.bDead == true)
                    continue;

                teammate.myCard.bOncePerTurn = true;
                StartCoroutine(teammate.EnergizeAndExhaust(true));
            }
        }

        if (bIsPlayer1 == bPlayer1sTurn)
        {
            player.StartTurn(true);
        }

        StaticGameplayDelegates.TurnStarted(bPlayer1sTurn);
    }

    [PunRPC]
    void PlayerLocksIn(bool isPlayer1)
    {
        if (isPlayer1)
        {
            player1IsReady = true;
        }
        else if (isPlayer1 == false)
        {
            player2IsReady = true;
        }

        if(player1IsReady && player2IsReady)
        {
            WaitingForOpponent.SetActive(false);
        }
    }

    [PunRPC]
    void WhoGoesFirst(bool Player1GoesFirst)
    {
        bPlayer1sTurn = Player1GoesFirst;

        if (Player1GoesFirst == bIsPlayer1)
        {
            WhoGoesFirstText.text = "Going First";
            WhoGoesFirstText.color = Color.lightGreen;
        }
        else
        {
            WhoGoesFirstText.text = "Going Second";
            WhoGoesFirstText.color = Color.orange;
        }
    }

}
