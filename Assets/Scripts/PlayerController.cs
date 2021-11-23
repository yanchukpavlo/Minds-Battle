using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    Player pthotonPlayer;

    int score;
    int id;
    string nickName;

    public int Score { get { return score; } set { score = value; } }
    public int Id { get { return id; } set { id = value; } }
    public string NickName { get { return nickName; }}

    public void Start()
    {
        Interactable(false);
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        pthotonPlayer = player;
        id = player.ActorNumber - 1;
        nickName = player.NickName;
        GameManager.inctance.SetPlayer(id, this);
        if(photonView.IsMine)
        {
            GameManager.inctance.LocalPlayer = this;
        }
    }

    public void Interactable(bool isCan)
    {
        transform.GetChild(0).gameObject.SetActive(!isCan);
    }

    public void Interactable(float timer)
    {
        StartCoroutine(WaitAndInteract(timer));
    }
    

    IEnumerator WaitAndInteract(float time)
    {
        Interactable(false);
        yield return new WaitForSeconds(time);
        Interactable(true);
    }
}
