using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] private UserPlayer player;

    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardAndCaptainCardLibrary = new List<BaseCard>();

    public List<BaseCard> GetCaptainLibrary() {  return CaptainLibrary; }
    public List<BaseCard> GetEveryCardLibrary() {  return CardAndCaptainCardLibrary; }

    [Header("Player Data")]
    public FirebasePlayerInfo FirebasePlayer;
    [SerializeField] private string player1_ID; // just for testing of course
    [SerializeField] private string player2_ID;
    [SerializeField] private bool bIsPlayer1;

    [SerializeField] bool bDevTest;

    void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3);

        if(bDevTest == false)
            SetPlayerIDs();

        player.StartMulligan();
    }

    public void SetPlayerIDs()
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        player1_ID = props["P1"] as string;
        player2_ID = props["P2"] as string;

        bIsPlayer1 = (player1_ID == FirebasePlayer.GetUserID());
    }
}
