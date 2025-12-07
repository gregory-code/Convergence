using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameMaster : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] private UserPlayer player;

    [SerializeField] TextMeshProUGUI WhoGoesFirstText;


    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardAndCaptainCardLibrary = new List<BaseCard>();

    [SerializeField] private GameObject WaitingForOpponent;

    public List<BaseCard> GetCaptainLibrary() {  return CaptainLibrary; }
    public List<BaseCard> GetEveryCardLibrary() {  return CardAndCaptainCardLibrary; }

    [Header("Player Data")]
    public FirebasePlayerInfo FirebasePlayer;

    [SerializeField] private bool bIsPlayer1;

    [SerializeField] private string player1_ID; // SerializeField just for testing of course
    [SerializeField] private string player2_ID;

    [SerializeField] private bool player1IsReady; // SerializeField just for testing of course
    [SerializeField] private bool player2IsReady;

    [SerializeField] bool bDevTest;

    void Start()
    {
        Crossroads();
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3);

        if(bDevTest == false)
            SetPlayerIDs();

        if(bIsPlayer1) // Only executes on player 1, so as we don't get two random numbers
        {
            bool whoGoesFirst = Random.Range(0, 2) == 0;
            this.photonView.RPC("WhoGoesFirst", RpcTarget.AllBuffered, whoGoesFirst);
        }

        yield return new WaitForSeconds(1);

        player.StartMulligan();

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
        this.photonView.RPC("PlayerLocksIn", RpcTarget.AllBuffered, bIsPlayer1);
    }

    private IEnumerator PlayersAddingNewCaptains()
    {
        while(player1IsReady == false || player2IsReady == false)
            yield return new WaitForEndOfFrame();


    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

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
        if(Player1GoesFirst && bIsPlayer1 || Player1GoesFirst == false && bIsPlayer1 == false)
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
