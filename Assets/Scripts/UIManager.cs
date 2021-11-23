using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    [Header("Menus")]
    [SerializeField] GameObject menuCore;
    [SerializeField] GameObject menuLobby;

    [Header("Menu Core")]
    [SerializeField] Button buttonCreateRoom;
    [SerializeField] Button buttonJoinRoom;

    [Header("Menu Lobby")]
    [SerializeField] TextMeshProUGUI textPlayerList;
    [SerializeField] TMP_Dropdown dropdownLevel;
    [SerializeField] Button buttonStartGame;

    GameObject currentMenu;

    void Start()
    {
        if (NetworkManager.inctance == null) FindObjectOfType<NetworkManager>(true).gameObject.SetActive(true);

        currentMenu = menuCore;
        buttonCreateRoom.interactable = false;
        buttonJoinRoom.interactable = false;
    }

    public void OnButtonCreateRoom(TMP_InputField name)
    {
        NetworkManager.inctance.CreateRoom(name.text);
    }
    
    public void OnButtonJoinRoom(TMP_InputField name)
    {
        NetworkManager.inctance.JoinRoom(name.text);
    }
    
    public void OnButtonLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(Screen.Core);
    }

    public void OnButtonStartGame()
    {
        NetworkManager.inctance.CardAmount = short.Parse(dropdownLevel.options[dropdownLevel.value].text);
        NetworkManager.inctance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }


    public void OnUpdatePlayerName(TMP_InputField name)
    {
        PhotonNetwork.NickName = name.text;
    }


    void SetScreen(Screen screen)
    {
        if (currentMenu != null) currentMenu.SetActive(false);

        switch (screen)
        {
            case Screen.Core:
                currentMenu = menuCore;
                break;

            case Screen.Lobby:
                currentMenu = menuLobby;
                break;
        }

        currentMenu.SetActive(true);
    }


    public override void OnConnectedToMaster()
    {
        buttonCreateRoom.interactable = true;
        buttonJoinRoom.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        SetScreen(Screen.Lobby);
        photonView.RPC("UpdatePlayersListText", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersListText();
    }


    [PunRPC]
    public void UpdatePlayersListText()
    {
        textPlayerList.text = "";

        foreach (var item in PhotonNetwork.PlayerList)
        {
            textPlayerList.text += item.NickName + "\n";
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            buttonStartGame.gameObject.SetActive(false);
            dropdownLevel.gameObject.SetActive(false);
        }
    }
}

enum Screen
{
    Core,
    Lobby
}
