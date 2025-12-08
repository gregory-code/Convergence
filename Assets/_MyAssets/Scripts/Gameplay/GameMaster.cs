using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameMaster : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] private UserPlayer player;

    [SerializeField] TextMeshProUGUI WhoGoesFirstText;

    [SerializeField] PlayingCard PlayingCardPrefab;

    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardAndCaptainCardLibrary = new List<BaseCard>();

    [SerializeField] private GameObject WaitingForOpponent;

    private List<PlayingCard> Player1Allies = new List<PlayingCard>();
    private List<PlayingCard> Player2Allies = new List<PlayingCard>();

    [SerializeField] private GameObject opponentSpark;
    [SerializeField] private GameObject allySpark;

    [SerializeField] private TextMeshProUGUI opponentSparkText;
    [SerializeField] private TextMeshProUGUI allySparkText;

    [SerializeField] private Transform allySide;
    [SerializeField] private Transform enemySide;

    public List<BaseCard> GetCaptainLibrary() { return CaptainLibrary; }
    public List<BaseCard> GetEveryCardLibrary() { return CardAndCaptainCardLibrary; }

    [Header("Player Data")]
    public FirebasePlayerInfo FirebasePlayer;

    private bool bIsPlayer1;

    private bool bPlayer1sTurn;

    private string player1_ID; // SerializeField just for testing of course
    private string player2_ID;

    private bool player1IsReady; // SerializeField just for testing of course
    private bool player2IsReady;

    private BaseCard captainHolder;

    [SerializeField] bool bDevTest;

    void Start()
    {
        opponentSpark.SetActive(false);
        allySpark.SetActive(false);

        Crossroads();
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        opponentSparkText.text = "0/20";
        allySparkText.text = "0/20";

        yield return new WaitForSeconds(3);

        if (bDevTest == false)
            SetPlayerIDs();

        if (bIsPlayer1) // Only executes on player 1, so as we don't get two random numbers
        {
            bool whoGoesFirst = Random.Range(0, 2) == 0;
            this.photonView.RPC("WhoGoesFirst", RpcTarget.AllBuffered, whoGoesFirst);
        }

        yield return new WaitForSeconds(1);

        player.StartMulligan(bIsPlayer1);

        StartCoroutine(PlayersAddingNewCaptains());
    }


    public void SetPlayerIDs()
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        player1_ID = props["P1"] as string;
        player2_ID = props["P2"] as string;

        bIsPlayer1 = (player1_ID == FirebasePlayer.GetUserID());
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

    private IEnumerator PlayersAddingNewCaptains()
    {
        while (player1IsReady == false || player2IsReady == false)
            yield return new WaitForEndOfFrame();

        WhoGoesFirstText.text = "";

        AddAllyToBoard(captainHolder);

        yield return new WaitForSeconds(0.5f);

        opponentSpark.SetActive(true);
        allySpark.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if(bIsPlayer1 && bPlayer1sTurn || (bIsPlayer1 == false && bPlayer1sTurn == false))
        {
            player.StartTurn(false);
        }
    }


    public IEnumerator LoadData(DataSnapshot data)
    {
        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

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

    public void AddAllyToBoard(BaseCard cardToAdd)
    {
        bool bIsCaptain = (cardToAdd.Captain != CardCaptain.None);
        int cardIndex = (bIsCaptain) ? GetCardIndexForLibrary(cardToAdd, CaptainLibrary) : GetCardIndexForLibrary(cardToAdd, CardAndCaptainCardLibrary);
        this.photonView.RPC("AddAllyToBoard", RpcTarget.AllBuffered, bIsPlayer1, bIsCaptain, cardIndex);
    }

    [PunRPC]
    void AddAllyToBoard(bool isPlayer1, bool IsCaptain, int cardLibraryIndex)
    {
        List<PlayingCard> playerAllies = (isPlayer1) ? Player1Allies : Player2Allies;
        List<BaseCard> cardLibrary = (IsCaptain) ? CaptainLibrary : CardAndCaptainCardLibrary ;
        Transform side = (isPlayer1 && bIsPlayer1 || (isPlayer1 == false && bIsPlayer1 == false)) ? allySide : enemySide;

        BaseCard newCardCopy = ScriptableObject.Instantiate(cardLibrary[cardLibraryIndex]);

        PlayingCard card = Instantiate(PlayingCardPrefab, side.transform);
        float startingYPos = (isPlayer1 && bIsPlayer1 || (isPlayer1 == false && bIsPlayer1 == false)) ? -500.0f : 500.0f ;
        card.Init(player, newCardCopy, bIsPlayer1);
        card.transform.localPosition = new Vector3(0, startingYPos, 0);
        playerAllies.Add(card);

        int count = playerAllies.Count;

        const float minCount = 2f;
        const float maxCount = 9f;

        const float maxValue = 45f;
        const float minValue = 140f;

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

    public void RequestSwitchTurns()
    {
        this.photonView.RPC("SwitchTurns", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SwitchTurns()
    {
        bPlayer1sTurn = !bPlayer1sTurn;
        if (bIsPlayer1 && bPlayer1sTurn || (bIsPlayer1 == false && bPlayer1sTurn == false))
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

        if (Player1GoesFirst && bIsPlayer1 || (Player1GoesFirst == false && bIsPlayer1 == false))
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
