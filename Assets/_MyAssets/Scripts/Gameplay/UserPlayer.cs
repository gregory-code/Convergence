using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserPlayer : MonoBehaviour, IDataPersistence
{
    [SerializeField] private GameMaster gameMaster;
    [SerializeField] private VisualDeck visualDeck;
    [SerializeField] private Hand playerHand;
    [SerializeField] private List<BaseCard> UserCaptains = new List<BaseCard>();
    [SerializeField] private List<BaseCard> UserDeck = new List<BaseCard>();

    [SerializeField] private GameObject SelectCaptain;
    [SerializeField] private VisibleCard[] SelectCaptainsVisible;

    [SerializeField] private GameObject MullgianPanel;

    private int DeckIndex;

    private void Start()
    {
        DeckIndex = PlayerPrefs.GetInt("SelectedDeck");
    }

    public void StartMulligan()
    {
        playerHand.SetMulligan(true);
        MullgianPanel.SetActive(true);
        StartCoroutine(DrawCards(5));
    }

    public void ConfirmMulligan()
    {
        MullgianPanel.SetActive(false);
        StartCoroutine(DrawCards(playerHand.ConfirmMulligan()));
    }

    private IEnumerator DrawCards(int cardsToDraw)
    {
        for(int i = 0; i < cardsToDraw; i++)
        {
            visualDeck.DrawTopCard();
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < cardsToDraw; i++)
        {
            int index = UnityEngine.Random.Range(0, UserDeck.Count);
            BaseCard randomCard = UserDeck[index];
            UserDeck.RemoveAt(index);
            playerHand.AddCard(randomCard);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void AddCard(BaseCard cardsToAdd)
    {
        BaseCard newCard = ScriptableObject.Instantiate(cardsToAdd);
        UserDeck.Add(newCard);
        visualDeck.AddCardToVisualDeck();
    }

    public void AddCaptainToFight()
    {
        SelectCaptain.SetActive(true);
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        List<string> decklist = new List<string>();
        if (data.Child("Deck" + DeckIndex).Exists)
        {
            for (int j = 0; j < data.Child("Deck" + DeckIndex).ChildrenCount; ++j)
            {
                decklist.Add(data.Child("Deck" + DeckIndex).Child("" + j).Value.ToString());
            }
        }

        int captainsIndex = 0;
        foreach(BaseCard card in gameMaster.GetCaptainLibrary())
        {
            if(decklist.Contains(card.CardName))
            {
                BaseCard newCaptain = ScriptableObject.Instantiate(card);
                SelectCaptainsVisible[captainsIndex].SetCard(newCaptain);
                captainsIndex++;
                UserCaptains.Add(newCaptain);
            }
        }

        List<BaseCard> cardLibrary = gameMaster.GetEveryCardLibrary();
        for (int i = 0; i < cardLibrary.Count; i++)
        {
            for (int j = 0; j < decklist.Count; j++)
            {
                if (decklist[j] == cardLibrary[i].CardName)
                {
                    AddCard(cardLibrary[i]);
                }
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }
}
