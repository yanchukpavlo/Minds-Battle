using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager inctance;

    [SerializeField] TMPro.TMP_Text infoText;
    [SerializeField] float timeForFlipCard = 2f;
    [SerializeField] float timeForRemembered = 10f;

    int cardsAmount;
    int playerInGame;
    int previusCardId = -1;
    Card previusCard;
    PlayerController[] players;
    PlayerController localPlayer;
    InteractState currentState;

    public PlayerController LocalPlayer { get { return localPlayer; } set { localPlayer = value; } }

    private void Awake()
    {
        inctance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    #region PunRPC

    [PunRPC]
    public void Set—ardsAmount(int cardsAmount)
    {
        CardContainer.inctance.CardAmount = cardsAmount;
    }

    [PunRPC]
    public void SetInteractable(int id, bool isCan)
    {
        if (id == localPlayer.Id)
        {
            localPlayer.Interactable(isCan);
        }

        infoText.text = PhotonNetwork.PlayerList[id].NickName + " turn";
    }

    [PunRPC]
    public void SetInteractable(int id, int time)
    {
        if (id == localPlayer.Id)
        {
            localPlayer.Interactable(time);
        }

        infoText.text = PhotonNetwork.PlayerList[id].NickName + " turn";
    }

    [PunRPC]
    void ImInGame()
    {
        playerInGame++;

        if (playerInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    [PunRPC]
    public void AddPoint(int id)
    {
        players[id].Score++;
    }

    #endregion

    #region Public methods

    public void SetPlayer(int pos, PlayerController playerController)
    {
        players[pos] = playerController;
    }

    public void InteractWithCard(Card card)
    {
        switch (currentState)
        {
            case InteractState.First:
                currentState = InteractState.Second;
                CardContainer.inctance.photonView.RPC("FlipCard", RpcTarget.All, card.Nubmer);

                previusCard = card;
                previusCardId = card.Id;

                localPlayer.Interactable(1);
                break;

            case InteractState.Second:
                currentState = InteractState.First;
                CardContainer.inctance.photonView.RPC("FlipCard", RpcTarget.All, card.Nubmer);

                if (previusCardId == card.Id)
                {
                    previusCardId = -1;

                    StartCoroutine(WaitAndDo(1, delegate
                    {
                        CardContainer.inctance.photonView.RPC("DroppedCard", RpcTarget.All, previusCard.Nubmer);
                        CardContainer.inctance.photonView.RPC("DroppedCard", RpcTarget.All, card.Nubmer);
                    }));
                    photonView.RPC("AddPoint", RpcTarget.All, localPlayer.Id);
                    localPlayer.Interactable(1);
                }
                else
                {
                    StartCoroutine(WaitAndDo(1, delegate 
                    {
                        CardContainer.inctance.photonView.RPC("FlipCard", RpcTarget.All, previusCard.Nubmer);
                        CardContainer.inctance.photonView.RPC("FlipCard", RpcTarget.All, card.Nubmer);
                        NextPlayer();
                    }));
                }
                break;
        }
    }

    public void EndGame()
    {
        infoText.text = "Winner: " + GetWiner();
        StartCoroutine(WaitAndDo(3, delegate
        {
            PhotonNetwork.LeaveRoom();
            NetworkManager.inctance.ChangeScene("Menu");
        }));
    }

    #endregion

    #region Private methods
    string GetWiner()
    {
        string s = "";
        int best = 0;

        foreach (var item in players)
        {
            if (best < item.Score)
            {
                best = item.Score;
                s = item.NickName;
            }
            else if (best == item.Score) s += " " + item.NickName;
        }
        return s;
    }

    void SpawnPlayer()
    {
        GameObject inst = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        PlayerController playerController = inst.GetComponent<PlayerController>();
        playerController.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
        playerController.transform.parent = FindObjectOfType<Canvas>().transform;
        RectTransform rt = inst.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(PrepareToGame());
        }
    }

    void NextPlayer()
    {
        int nextId = localPlayer.Id + 1;
        if (nextId >= players.Length)
        {
            nextId = 0;
            Debug.Log(nextId);
        }

        photonView.RPC("SetInteractable", RpcTarget.All, nextId, 1);
    }

    #endregion

    #region Enumerators

    IEnumerator PrepareToGame()
    {
        cardsAmount = NetworkManager.inctance.CardAmount;

        float timeBetweenFlipCard = timeForFlipCard / cardsAmount;

        CardContainer.inctance.CreateDeck(cardsAmount);
        photonView.RPC("Set—ardsAmount", RpcTarget.All, cardsAmount);

        yield return new WaitForSeconds(2);
        CardContainer.inctance.photonView.RPC("SwapDeck", RpcTarget.All, timeBetweenFlipCard);

        yield return new WaitForSeconds(timeForRemembered + timeForFlipCard);
        CardContainer.inctance.photonView.RPC("SwapDeck", RpcTarget.All, timeBetweenFlipCard);

        yield return new WaitForSeconds(timeForFlipCard + 1f);
        photonView.RPC("SetInteractable", RpcTarget.All, localPlayer.Id, true);
    }

    IEnumerator WaitAndDo(float time, System.Action action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }

    #endregion

}

enum InteractState
{
    First,
    Second
}