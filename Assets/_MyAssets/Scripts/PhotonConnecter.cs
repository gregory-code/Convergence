using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PhotonConnecter : MonoBehaviourPunCallbacks
{
    [SerializeField] private Image WifiConnectionIcon;
    bool isLobbyRoom = true;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
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
            //roomOptions.MaxPlayers = 2;
            //PhotonNetwork.JoinOrCreateRoom(gameRoomID, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        WifiConnectionIcon.color = Color.green;

        /*UpdatePlayersOnline();

        onUserJoinedRoom?.Invoke(isLobbyRoom);

        if (isLobbyRoom == false)
        {
            isLobbyRoom = true;

            List<string> playerIDs = gameRoomID.ToString().Split('õ').ToList();
            gameMenu.GetBattlePlayList(playLists.battleSongs);
            gameMenu.SetPlayerIDs(playerIDs[0], playerIDs[1]);
            StartCoroutine(gameMenu.SetUpGame(format));

        }*/
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        WifiConnectionIcon.color = Color.red;

        //NotificationScript.createNotif($"{message}", Color.red);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        /*changeWifiState(false);

        onUserLeftRoom?.Invoke();

        PhotonNetwork.JoinLobby();*/
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        /*UpdatePlayersOnline();

        onOtherUserJoinedRoom?.Invoke(newPlayer);*/
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        /*UpdatePlayersOnline();

        onOtherUserLeftRoom?.Invoke(otherPlayer);*/
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        /*changeWifiState(false);

        NotificationScript.createNotif($"{cause}", Color.red);*/
    }
}
