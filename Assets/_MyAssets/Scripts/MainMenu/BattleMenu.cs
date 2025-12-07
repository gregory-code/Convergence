using Firebase.Database;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] FirebasePlayerInfo FirebasePlayer;

    bool isLobbyRoom = true;

    [SerializeField] private Image[] DeckChecks;
    [SerializeField] private Image[] DeckCaptainIcons;
    [SerializeField] private TextMeshProUGUI[] DeckNames;
    [SerializeField] private Sprite emptyCheck;
    [SerializeField] private Sprite check;

    private int selectedDeckIndex = 0;

    string gameRoomID = "";

    private bool bSearchingForMatch;

    [SerializeField] private List<string> standardRoomIDs = new List<string>();


    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void ChosenDeckIndex(int index)
    {
        selectedDeckIndex = index;
        StartCoroutine(FirebasePlayer.UpdateObject("DeckIndex", index));

        foreach (Image image in DeckChecks)
        {
            image.sprite = emptyCheck;
            image.color = Color.white;
        }
        DeckChecks[index].sprite = check;
        DeckChecks[index].color = Color.green;
    }

    public void SetDeckNames(int index, string nameString)
    {
        DeckNames[index].text = nameString;
    }


    public void SetDeckCaptains(int index, Sprite[] cap)
    {
        if (cap[0] == null)
        {
            Debug.Log("Null?");
            DeckCaptainIcons[index * 3].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        else
        {
            DeckCaptainIcons[index * 3].sprite = cap[0]; // index * 3
            DeckCaptainIcons[index * 3].color = new Color(1, 1, 1, 1);
        }

        if (cap[1] == null)
        {
            DeckCaptainIcons[index * 3 + 1].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        else
        {
            DeckCaptainIcons[index * 3 + 1].sprite = cap[1]; // index * 3
            DeckCaptainIcons[index * 3 + 1].color = new Color(1, 1, 1, 1);
        }

        if (cap[2] == null)
        {
            DeckCaptainIcons[index * 3 + 2].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        else
        {
            DeckCaptainIcons[index * 3 + 2].sprite = cap[2]; // index * 3
            DeckCaptainIcons[index * 3 + 2].color = new Color(1, 1, 1, 1);
        }
    }

    public void FindRandomMatch()
    {
        bSearchingForMatch = !bSearchingForMatch;

        if (PhotonNetwork.InRoom == false || PhotonNetwork.CurrentRoom.Name != "LobbyRoom")
        {
            //NotificationScript.createNotif($"Not connected", Color.red);
            bSearchingForMatch = false;
        }

        List<string> RoomIDs = new List<string>();

        RoomIDs = standardRoomIDs;

        string myID = FirebasePlayer.GetUserID();

        foreach (string roomID in RoomIDs)
        {
            if (roomID == myID && bSearchingForMatch == false)
            {
                this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, myID, false);
                return;
            }
            else if (roomID != myID) // do some MMR matchmaking here
            {
                this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, roomID, false);
                this.photonView.RPC("MatchFoundRPC", RpcTarget.All, myID, roomID);
                return;
            }
        }

        this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, myID, true);
    }

    public override void OnJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        if (isLobbyRoom == true)
        {
            roomOptions.MaxPlayers = 20;
            PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
        }
        else
        {
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.JoinOrCreateRoom(gameRoomID, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        //onUserJoinedRoom?.Invoke(isLobbyRoom);

        if (isLobbyRoom == false)
        {
            isLobbyRoom = true;

            List<string> playerIDs = gameRoomID.ToString().Split('õ').ToList();

            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash["P1"] = playerIDs[0];
            hash["P2"] = playerIDs[1];
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            PhotonNetwork.LoadLevel(2);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        //NotificationScript.createNotif($"{message}", Color.red);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        //onUserLeftRoom?.Invoke();

        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        //onOtherUserJoinedRoom?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        //onOtherUserLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        /*
        NotificationScript.createNotif($"{cause}", Color.red);*/
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        if (data.Child("DeckIndex").Exists)
        {
            selectedDeckIndex = int.Parse(data.Child("DeckIndex").Value.ToString());
            ChosenDeckIndex(selectedDeckIndex); // index for DeckIndex
        }
        else
        {
            selectedDeckIndex = 0;
            ChosenDeckIndex(0); // index for DeckIndex
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }

    [PunRPC]
    void MatchFoundRPC(string player1ID, string player2ID)
    {
        if (FirebasePlayer.GetUserID() != player1ID && FirebasePlayer.GetUserID() != player2ID) 
            return;

        isLobbyRoom = false;
        gameRoomID = player1ID + "õ" + player2ID;

        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void UpdateRoomIDsRPC(string roomID, bool bAddToList)
    {
        if (bAddToList)
            standardRoomIDs.Add(roomID);
        else
            standardRoomIDs.Remove(roomID);
    }
}