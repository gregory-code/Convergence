using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;


public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] private UserPlayer player;

    [SerializeField] TextMeshProUGUI WhoGoesFirstText;
    [SerializeField] TextMeshProUGUI CurrentIDText;

    [SerializeField] PlayingCard PlayingCardPrefab;

    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardAndCaptainCardLibrary = new List<BaseCard>();

    [SerializeField] private GameObject WaitingForOpponent;

    [SerializeField] private CardAmountHover clientSideDiscardAmount;
    [SerializeField] private CardAmountHover enemyDiscardAmount;

    private List<PlayingCard> ActivePlayingCards = new List<PlayingCard>();
    private List<PlayingCard> Player1Discard = new List<PlayingCard>();
    private List<PlayingCard> Player2Discard = new List<PlayingCard>();

    private int uniqueID;

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

    public Sprite[] GetNumberSpriteWholes() {  return numberSpriteWhole; }

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

    //The reaction soup//

    [HideInInspector]
    public PlayingCard reactionPlayingCard;
    [HideInInspector]
    public PlayingCard reactionCaptainUsing;
    [HideInInspector]
    public List<PlayingCard> reactionCaptainTargeting;
    [HideInInspector]
    public bool bReactionTargetingEnemy;

    /// <summary>
    /// There is no summery, we are making soup
    /// </summary>

    void Start()
    {
        clientSideDiscardAmount.onChangeCardText += HoveredClientDiscard;
        enemyDiscardAmount.onChangeCardText += HoveredEnemyDiscard;

        opponentSpark.SetActive(false);
        allySpark.SetActive(false);

        Crossroads();
        StartCoroutine(StartGame());
    }

    public Transform GetDiscardPilieTransform(bool doIOwnThis)
    {
        return (doIOwnThis) ? allyDiscard : enemyDiscard;
    }

    public void AddCardToDiscard(PlayingCard cardtoAdd, bool doIOwnThis)
    {
        if (doIOwnThis)
        {
            if(bIsPlayer1)
            {

                Player1Discard.Add(cardtoAdd);
            }
            else
            {
                Player2Discard.Add(cardtoAdd);

            }
        }
        else
        {
            if (bIsPlayer1)
            {
                Player2Discard.Add(cardtoAdd);

            }
            else
            {
                Player1Discard.Add(cardtoAdd);

            }
        }
    }

    public int GetCardsInHand(bool IsPlayer1)
    {
        if (IsPlayer1 == bIsPlayer1)
        {
            return player.GetCardCountInHand(true);
        }
        else
        {
            return player.GetCardCountInHand(false);
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

        player.LoadOpponentID((bIsPlayer1) ? player2_ID : player1_ID, bIsPlayer1);
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

        if (bIsPlayer1)
            yield return new WaitForSeconds(0.3f);

        AddAllyToBoard(captainHolder, null);

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

    public void ResetAllDisplayAttackStats()
    {
        foreach (PlayingCard character in ActivePlayingCards)
        {
            character.DisplayAttackStats(true, true, null, null);
        }
    }

    public List<PlayingCard> GetTeammates(PlayingCard captainForReference) 
    {
        bool bOwnerIsPlayer1 = captainForReference.bIsPlayer1;
        List<PlayingCard> teammates = new List<PlayingCard>();
        foreach(PlayingCard teammate in ActivePlayingCards)
        {
            if(teammate.bIsPlayer1 == bOwnerIsPlayer1)
                teammates.Add(teammate);
        }
        return teammates;
    }

    public List<PlayingCard> GetEnemies(PlayingCard captainForReference)
    {
        bool bOwnerIsPlayer1 = captainForReference.bIsPlayer1;
        List<PlayingCard> enemies = new List<PlayingCard>();
        foreach (PlayingCard enemy in ActivePlayingCards)
        {
            if (enemy.bIsPlayer1 != bOwnerIsPlayer1)
                enemies.Add(enemy);
        }
        return enemies;
    }

    public List<PlayingCard> GetDiscardPile(bool bGetMyDiscardPile)
    {
        if (bGetMyDiscardPile)
        {
            return (bIsPlayer1) ? Player1Discard : Player2Discard;
        }
        else
        {
            return (bIsPlayer1) ? Player2Discard : Player1Discard;
        }
    }
    private void HoveredClientDiscard(bool bStartedHover)
    {
        if (bStartedHover)
            clientSideDiscardAmount.cardAmountText.text = (bIsPlayer1) ? "" + Player1Discard.Count : "" + Player2Discard.Count;
    }

    private void HoveredEnemyDiscard(bool bStartedHover)
    {
        if (bStartedHover)
            enemyDiscardAmount.cardAmountText.text = (bIsPlayer1) ? "" + Player2Discard.Count : "" + Player1Discard.Count;
    }

    private int GetCaptainUsingIndex(PlayingCard captainUsing)
    {
        if (captainUsing == null)
            return -1;

        for(int i = 0; i < ActivePlayingCards.Count; i++)
        {
            if(captainUsing.uniqueID == ActivePlayingCards[i].uniqueID)
                return i;
        }

        return -1;
    }

    private PlayingCard GetCaptainFromIndex(int captainIndex)
    {
        if (captainIndex == -1 || captainIndex > ActivePlayingCards.Count)
            return null;

        return ActivePlayingCards[captainIndex];
    }

    public int GetNextID()
    {
        uniqueID++;
        CurrentIDText.text = "Recent ID: " + uniqueID;
        CurrentIDText.gameObject.SetActive(bSkipMulligan);
        return uniqueID;
    }

    public void AddAllyToBoard(BaseCard cardToAdd, PlayingCard captainPlayingThisAlly)
    {
        int captainUsingIndex = GetCaptainUsingIndex(captainPlayingThisAlly);

        bool bIsCaptain = (cardToAdd.Type.type == CardType.Captain);
        int cardIndex = (bIsCaptain) ? GetCardIndexForLibrary(cardToAdd, CaptainLibrary) : GetCardIndexForLibrary(cardToAdd, CardAndCaptainCardLibrary);
        this.photonView.RPC("AddAllyToBoard", RpcTarget.AllBuffered, bIsPlayer1, bIsCaptain, cardIndex, captainUsingIndex);
    }

    [PunRPC]
    void AddAllyToBoard(bool isPlayer1, bool IsCaptain, int cardLibraryIndex, int captainPlayingIndex)
    {
        PlayingCard captainPlayingAlly = GetCaptainFromIndex(captainPlayingIndex);

        List<BaseCard> cardLibrary = (IsCaptain) ? CaptainLibrary : CardAndCaptainCardLibrary ;
        Transform side = (isPlayer1 == bIsPlayer1) ? allySide : enemySide;

        BaseCard newCardCopy = ScriptableObject.Instantiate(cardLibrary[cardLibraryIndex]);

        PlayingCard card = Instantiate(PlayingCardPrefab, side.transform);
        float startingYPos = (isPlayer1 == bIsPlayer1) ? -500.0f : 500.0f ;
        card.Init(player, newCardCopy, isPlayer1, GetNextID(), captainPlayingAlly);
        card.transform.localPosition = new Vector3(0, startingYPos, 0);
        ActivePlayingCards.Add(card);

        if(card.myCard.Type.type == CardType.Ally)
        {

            StartCoroutine(card.EnergizeAndExhaust(false));

            if (isPlayer1 == bIsPlayer1) // Client
            {
                // I don't think I need this actually \_('<')_/
                //player.PlayAllyCard(card);
            }
            else
            {
                player.PlayAllyCard(card, false);
            }
        }

        List<PlayingCard> playerAllies = new List<PlayingCard>();
        for (int i = 0; i < ActivePlayingCards.Count; i++)
        {
            if (ActivePlayingCards[i].bIsPlayer1 == isPlayer1)
                playerAllies.Add(ActivePlayingCards[i]);
        }

        ReOrganize(playerAllies);
    }

    private void ReOrganize(List<PlayingCard> playerAllies)
    {
        int count = playerAllies.Count;

        const float minCount = 2f;
        const float maxCount = 7f;

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

    public void RequestRemoveAllyFromBoard(PlayingCard allyToRemove)
    {
        int captainUsingIndex = GetCaptainUsingIndex(allyToRemove);

        if (captainUsingIndex == -1)
            return;

        this.photonView.RPC("RemoveAllyFromBoard", RpcTarget.AllBuffered, bIsPlayer1, captainUsingIndex);
    }

    [PunRPC]
    void RemoveAllyFromBoard(bool isPlayer1, int allyIndex)
    {
        ActivePlayingCards.RemoveAt(allyIndex);

        List<PlayingCard> playerAllies = new List<PlayingCard>();
        for (int i = 0; i < ActivePlayingCards.Count; i++)
        {
            if (ActivePlayingCards[i].bIsPlayer1 == isPlayer1)
                playerAllies.Add(ActivePlayingCards[i]);
        }

        ReOrganize(playerAllies);
    }

    public void RequestPlayCard(PlayingCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting, bool bAttackPrediction, bool bDaybreakAbility, bool bForceSwift)
    {
        bool bIsCaptain = (cardToPlay.myCard.Type.type == CardType.Captain);
        int cardIndex = (bIsCaptain) ? GetCardIndexForLibrary(cardToPlay.myCard, CaptainLibrary) : GetCardIndexForLibrary(cardToPlay.myCard, CardAndCaptainCardLibrary);

        int captainUsingIndex = GetCaptainUsingIndex(captainUsing);
        List<int> captainTargetingIndex = new List<int>();
        foreach (PlayingCard target in captainTargeting)
        {
            captainTargetingIndex.Add(GetCaptainUsingIndex(target));

        }

        if (bAttackPrediction)
        {
            this.photonView.RPC("AttackPrediction", RpcTarget.AllBuffered, bIsPlayer1, cardIndex, captainUsingIndex, bTargetingEnemy, captainTargetingIndex.ToArray(), bIsCaptain);
        }
        else
        {
            this.photonView.RPC("PlayCard", RpcTarget.AllBuffered, bIsPlayer1, cardIndex, captainUsingIndex, bTargetingEnemy, captainTargetingIndex.ToArray(), bIsCaptain, bDaybreakAbility, bForceSwift);
        }
    }

    [PunRPC]
    void PlayCard(bool isPlayer1, int cardToPlayIndex, int captainUsingIndex, bool bTargetingEnemy, int[] captainTargetingIndex, bool isCaptain, bool bDaybreakAbility, bool bForceSwift)
    {
        PlayingCard captainUsing = GetCaptainFromIndex(captainUsingIndex);
        List<PlayingCard> captainTarget = new List<PlayingCard>();
        for (int i = 0; i < captainTargetingIndex.Length; i++)
        {
            captainTarget.Add(GetCaptainFromIndex(captainTargetingIndex[i]));
        }

        if (bDaybreakAbility)
        {
            StartCoroutine(captainUsing.myCard.SecondaryPlayCard(captainUsing, captainUsing, bTargetingEnemy, captainTarget));
            return;
        }

        player.bInUniqueMenu = false;

        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            player.PlayClientCard(bTargetingEnemy);
        }
        else // other player
        {
            BaseCard cardToPlay = (isCaptain) ? CaptainLibrary[cardToPlayIndex] : CardAndCaptainCardLibrary[cardToPlayIndex];

            player.PlayEnemyCard(cardToPlay, captainUsing, bTargetingEnemy, captainTarget, bForceSwift);
        }
    }

    [PunRPC]
    void AttackPrediction(bool isPlayer1, int cardToPlayIndex, int captainUsingIndex, bool bTargetingEnemy, int[] captainTargetingIndex, bool isCaptain)
    {
        PlayingCard captainUsing = GetCaptainFromIndex(captainUsingIndex);
        List<PlayingCard> captainTarget = new List<PlayingCard>();
        for (int i = 0; i < captainTargetingIndex.Length; i++)
        {
            captainTarget.Add(GetCaptainFromIndex(captainTargetingIndex[i]));
        }

        BaseCard cardToPlay = (isCaptain) ? CaptainLibrary[cardToPlayIndex] : CardAndCaptainCardLibrary[cardToPlayIndex];

        reactionCaptainUsing = captainUsing;
        reactionCaptainTargeting = captainTarget;
        bReactionTargetingEnemy = bTargetingEnemy;

        bWaitingForReaction = true;
        StartCoroutine(WaitingForReactionLoop());

        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            WaitingForOpponent.SetActive(true);
            player.WaitingForReaction(true, captainUsing);
        }
        else // other player
        {
            cardToPlay.bWaitForReaction = true;
            player.AllowReaction(true);
            player.EnemyIsAttackingPredicition(cardToPlay, captainUsing);
        }
    }

    private bool bWaitingForReaction;
    [SerializeField] private LineAttackPredictionScript lineAttackPredictionPrefab;
    private List<LineAttackPredictionScript> lineAttacks = new List<LineAttackPredictionScript>();
    private IEnumerator WaitingForReactionLoop()
    {
        while (bWaitingForReaction)
        {
            if(reactionCaptainTargeting.Count <= 0 || reactionCaptainUsing == null || reactionPlayingCard == null)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }    

            ClearLineAttacks();

            for (int i = 0; i < reactionCaptainTargeting.Count; i++)
            {
                if (reactionCaptainTargeting[i].myCard is CaptainCard newCaptain)
                {
                    reactionCaptainTargeting[i].predictingNewHealthChange = newCaptain.currentHealth;
                }
            }

            for (int i = 0; i < reactionCaptainTargeting.Count; i++)
            {
                LineAttackPredictionScript lineAttack = Instantiate(lineAttackPredictionPrefab);
                lineAttack.ShowPrediction(reactionPlayingCard.transform.position, reactionCaptainTargeting[i].transform.position, Color.red);
                lineAttacks.Add(lineAttack);

                CardPlayContext context = reactionPlayingCard.myCard.PredictCard(reactionPlayingCard, reactionCaptainUsing, bReactionTargetingEnemy, reactionCaptainTargeting[i]);

                if (reactionCaptainTargeting[i].myCard is CaptainCard captain)
                {
                    reactionCaptainTargeting[i].DisplayHealthChange(captain, context.damage);
                    reactionCaptainTargeting[i].DisplayAttackStats(false, true, reactionPlayingCard, reactionCaptainUsing);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        bWaitingForReaction = false;
    }

    public void ClearLineAttacks()
    {

        for (int i = 0; i < reactionCaptainTargeting.Count; i++)
        {
            if (reactionCaptainTargeting[i].myCard is CaptainCard captain)
            {
                reactionCaptainTargeting[i].DisplayAttackStats(true, true, null, null);
            }
        }

        for (int i = 0; i < lineAttacks.Count; i++)
        {
            Destroy(lineAttacks[i].gameObject);
        }
        lineAttacks.Clear();
    }

    public void RequestFinishReaction()
    {
        this.photonView.RPC("FinishReaction", RpcTarget.AllBuffered, bIsPlayer1);
    }

    [PunRPC]
    void FinishReaction(bool isPlayer1)
    {
        bWaitingForReaction = false;

        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            player.EnemyFinishReaction(true);
        }
        else // other player
        {
            WaitingForOpponent.SetActive(false);
            ClearLineAttacks();
            player.EnemyFinishReaction(false);
        }
    }

    public void RequestChangeReaction(List<PlayingCard> newTargets, List<PlayingCard> targetsDamageIncreaseAffect, int damageIncrease)
    {
        if(newTargets == null)
        {
            int[] targetList = new int[1];
            targetList[0] = -1;

            List<int> targetsForHeartBreaker = new List<int>();

            for (int i = 0; i < reactionCaptainTargeting.Count; i++)
            {
                targetsForHeartBreaker.Add(GetCaptainUsingIndex(reactionCaptainTargeting[i]));
            }

            this.photonView.RPC("ChangeReaction", RpcTarget.AllBuffered, bIsPlayer1, targetList, targetsForHeartBreaker.ToArray(), damageIncrease);
            return;
        }

        List<int> captainTargetingIndex = new List<int>();
        foreach (PlayingCard target in newTargets)
        {
            captainTargetingIndex.Add(GetCaptainUsingIndex(target));
        }

        List<int> targetsThatIsAffectedIndex = new List<int>();
        foreach (PlayingCard target in targetsDamageIncreaseAffect)
        {
            targetsThatIsAffectedIndex.Add(GetCaptainUsingIndex(target));
        }

        this.photonView.RPC("ChangeReaction", RpcTarget.AllBuffered, bIsPlayer1, captainTargetingIndex.ToArray(), targetsThatIsAffectedIndex.ToArray(), damageIncrease);
    }

    [PunRPC]
    void ChangeReaction(bool isPlayer1, int[] newTargetIndex, int[] targetsDamageIncraseAffectsIndex, int damageIncrease)
    {
        if(reactionPlayingCard != null)
            reactionPlayingCard.myCard.predictionDamage = damageIncrease;

        if (newTargetIndex[0] == -1) // this is heart breaker
        {

        }
        else
        {
            List<PlayingCard> captainTarget = new List<PlayingCard>();
            for (int i = 0; i < newTargetIndex.Length; i++)
            {
                captainTarget.Add(GetCaptainFromIndex(newTargetIndex[i]));
            }

            reactionCaptainTargeting = captainTarget;
            reactionPlayingCard.myCard.CaptainTargeting = captainTarget;
        }

        List<PlayingCard> captainsAffecting = new List<PlayingCard>();
        for (int i = 0; i < targetsDamageIncraseAffectsIndex.Length; i++)
        {
            captainsAffecting.Add(GetCaptainFromIndex(targetsDamageIncraseAffectsIndex[i]));
        }

        if (reactionPlayingCard != null)
            reactionPlayingCard.myCard.captainsAffectedByPredictionDamageIncrease = captainsAffecting;
    }

    public void RequestDevotionHealthSwap(PlayingCard devotionPlayer, PlayingCard beingSavedPlayer)
    {
        int devotionIndex = GetCaptainUsingIndex(devotionPlayer);
        int beingSavedIndex = GetCaptainUsingIndex(beingSavedPlayer);

        this.photonView.RPC("DevotionHealthSwap", RpcTarget.AllBuffered, bIsPlayer1, devotionIndex, beingSavedIndex);
    }

    [PunRPC]
    void DevotionHealthSwap(bool isPlayer1, int devotionIndex, int beingSavedIndex)
    {
        PlayingCard devotionPlayer = GetCaptainFromIndex(devotionIndex);
        PlayingCard beingSavedPlayer = GetCaptainFromIndex(beingSavedIndex);

        if(devotionPlayer.myCard is CaptainCard captainDevotionPlayer)
        {
            if(beingSavedPlayer.myCard is CaptainCard beingDevotionCard)
            {
                beingDevotionCard.currentHealth = captainDevotionPlayer.currentHealth;
                beingSavedPlayer.SetHealthText();
            }
        }
    }


    public void RequestRemoveLingers()
    {
        this.photonView.RPC("RemoveLingers", RpcTarget.AllBuffered, bIsPlayer1);
    }

    [PunRPC]
    void RemoveLingers(bool isPlayer1)
    {
        StaticGameplayDelegates.RemoveLingers(bPlayer1sTurn);

        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
        }
        else // other player
        {
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
            player.RequestDrawCards(cardsToDraw, true);
        }
        else // other player
        {
            player.RequestDrawCards(cardsToDraw, false);
        }
    }

    public void RequestShowOpponentIveDrawn(BaseCard cardToShow)
    {
        for(int i = 0; i < CardAndCaptainCardLibrary.Count; i++)
        {
            if(CardAndCaptainCardLibrary[i].CardName == cardToShow.CardName)
            {
                this.photonView.RPC("ShowOpponentIveDrawn", RpcTarget.OthersBuffered, bIsPlayer1, i);
            }
        }
    }

    [PunRPC]
    void ShowOpponentIveDrawn(bool isPlayer1, int cardIndex)
    {
        BaseCard cardToPlay = CardAndCaptainCardLibrary[cardIndex];
        StartCoroutine(player.EnemyRevealCardAndDraw(cardToPlay));
    }

    public void RequestIncreaseSpark(PlayingCard captainGainingSpark, int SparkToAdd)
    {
        int captainUsingIndex = GetCaptainUsingIndex(captainGainingSpark);

        this.photonView.RPC("IncreaseSpark", RpcTarget.AllBuffered, bIsPlayer1, captainUsingIndex, SparkToAdd);
    }

    [PunRPC]
    void IncreaseSpark(bool isPlayer1, int captainGainingSparkIndex, int SparkToAdd)
    {
        if (isPlayer1 == bIsPlayer1) // Owner, this client
        {
            
            allySparkValue += SparkToAdd;
            allySparkText.text = allySparkValue + "/20";
        }
        else // other player
        {
            opponentSparkValue += SparkToAdd;
            opponentSparkText.text = opponentSparkValue + "/20";
        }

        #region Extremely Sad Spark Spawn Logic

        Vector3 vfxSpawnPos = new Vector3();
        vfxSpawnPos = ActivePlayingCards[captainGainingSparkIndex].transform.position;
        vfxSpawnPos.y = vfxYSpawn;

        GameObject sparkPrefab = (bIsPlayer1 == isPlayer1) ? allySparkGainParticleSystem : enemySparkGainParticleSystem;
        GameObject sparkGain = Instantiate(sparkPrefab, vfxSpawnPos, sparkPrefab.transform.rotation);

        var wholeRenderer = sparkGain.transform.Find("Whole").GetComponent<ParticleSystemRenderer>();

        Material wholeMat = new Material(wholeRenderer.sharedMaterial);
        wholeRenderer.material = wholeMat;
        wholeMat.SetTexture("_BaseMap", numberSpriteWhole[SparkToAdd].texture);

        var outlineRenderer = sparkGain.transform.Find("Outline").GetComponent<ParticleSystemRenderer>();
        

        Material outlineMat = new Material(outlineRenderer.sharedMaterial);
        outlineRenderer.material = outlineMat;
        outlineMat.SetTexture("_BaseMap", numberSpriteOutline[SparkToAdd].texture);

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
            player.RequestDrawCards(cardsToMulligan, true);
            StartCoroutine(player.MullgianWrapUp(true, 0));
        }
        else // other player
        {
            player.RequestDrawCards(cardsToMulligan, false);
            StartCoroutine(player.MullgianWrapUp(false, cardsToMulligan));
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

        foreach (PlayingCard teammate in ActivePlayingCards)
        {
            if (teammate.myCard.bDead == true || teammate.bIsPlayer1 != bPlayer1sTurn)
                continue;

            teammate.myCard.bOncePerTurn = true;

            if (teammate.myCard is CaptainCard captain && captain.bIsAllyCard == false)
                StartCoroutine(teammate.EnergizeAndExhaust(true));
        }

        if (bIsPlayer1 == bPlayer1sTurn)
        {
            player.StartTurn(true);
        }
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
