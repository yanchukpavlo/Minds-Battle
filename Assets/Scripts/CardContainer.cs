using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

public class CardContainer : MonoBehaviourPunCallbacks
{
    public static CardContainer inctance;

    [SerializeField] GameObject cardPrefab;
    Transform perentTransform;
    UnityEngine.UI.GridLayoutGroup _grid;
    int cardAmount;
    public List<Card> deck;

    public int CardAmount { get { return cardAmount; } set { cardAmount = value; } }

    private void Awake()
    {
        inctance = this;
        deck = new List<Card>();
        perentTransform = FindObjectOfType<Canvas>().transform.GetChild(0);
        _grid = perentTransform.GetComponent<UnityEngine.UI.GridLayoutGroup>();
    }
    
    [PunRPC]
    public void SwapDeck(float delay)
    {
        StartCoroutine(SwapAllDeck(delay));
    }

    [PunRPC]
    public void FlipCard(int nubmer)
    {
        deck[nubmer].Flip();
    }

    [PunRPC]
    public void DroppedCard(int nubmer)
    {
        deck[nubmer].Dropped();
    }

    [PunRPC]
    public void InstantiateCard(int id, int sprite)
    {
        GameObject inst = Instantiate(cardPrefab, perentTransform);
        Card card = inst.GetComponent<Card>();
        card.Id = id;
        card.Icon = sprite;
        card.Nubmer = deck.Count;

        deck.Add(card);
    }

    public void CreateDeck(int amount)
    {
        int halfAmount = amount / 2;
        List<int> randomNumberList = new List<int>();
        for (int i = 0; i < halfAmount; i++)
        {
            randomNumberList.Add(Random.Range(0, Card.iconsLength));
        }


        for (int i = 0; i < halfAmount; i++)
        {
            photonView.RPC("InstantiateCard", RpcTarget.All, randomNumberList[i], randomNumberList[i]);
        }

        for (int i = 0; i < halfAmount; i++)
        {
            int r = Random.Range(0, randomNumberList.Count);

            photonView.RPC("InstantiateCard", RpcTarget.All, randomNumberList[r], randomNumberList[r]);
            randomNumberList.Remove(randomNumberList[r]);
        }
    }

    IEnumerator SwapAllDeck(float delayBetween)
    {
        foreach (var item in deck)
        {
            yield return new WaitForSeconds(delayBetween);
            item.Flip();
        }
    }
}
