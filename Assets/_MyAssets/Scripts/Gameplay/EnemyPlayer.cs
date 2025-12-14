using Firebase.Database;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyPlayer : MonoBehaviour
{
    [SerializeField] private VisualDeck enemyDeck;
    [SerializeField] private GameMaster gameMaster;
    [SerializeField] protected FirebasePlayerInfo firebasePlayerInfo;

    [SerializeField] private PlayingCard playingCardPrefab;
    private List<PlayingCard> PlayingCardsInHand = new List<PlayingCard>(); // these are for testing you can remove them

    private bool bIsPlayer1;

    private int DeckIndex;
    private bool bObtainedDeck = false;
    private string enemyID;
    private List<BaseCard> UserCaptains = new List<BaseCard>(); // these are for testing you can remove them
    private List<BaseCard> UserDeck = new List<BaseCard>(); // these are for testing you can remove them

    void Start()
    {
        StartCoroutine(LoadingOpponentDeck());
    }

    public void LoadOpponentID(string ID, bool bIsPlayer1)
    {
        enemyID = ID;
        bObtainedDeck = true;
    }

    public void RequestDrawCards(int cardsToDraw)
    {
        StartCoroutine(DrawCardsToHand(cardsToDraw));
    }

    private IEnumerator DrawCardsToHand(int cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            enemyDeck.DrawTopCard();
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < cardsToDraw; i++)
        {
            AddCardToHand(null);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void AddCardToDeck(BaseCard cardsToAdd)
    {
        if(cardsToAdd != null)
        {
            BaseCard newCard = ScriptableObject.Instantiate(cardsToAdd);
            UserDeck.Add(newCard);
        }
        enemyDeck.AddCardToVisualDeck();
    }

    public void AddCardToHand(BaseCard newCardToadd)
    {
        PlayingCard newPlayingCard = Instantiate(playingCardPrefab, this.transform);
        newPlayingCard.Init(null, newCardToadd, bIsPlayer1);
        newPlayingCard.transform.localPosition = new Vector3(0, 150, 0);
        PlayingCardsInHand.Add(newPlayingCard);
        newPlayingCard.StartMoveCard(-5, false, 0.5f);

        ReOrganizeHand();
    }

    public IEnumerator MullgianWrapUp(int cardsToMulligan)
    {
        yield return new WaitForSeconds(1.4f);

        int desiredAmountInHand = PlayingCardsInHand.Count - cardsToMulligan;

        while(PlayingCardsInHand.Count != desiredAmountInHand)
        {
            PlayingCardsInHand[0].StartMoveCard(350, false, 0.2f);
            yield return new WaitForSeconds(0.1f);
            AddCardToDeck(PlayingCardsInHand[0].myCard);
            yield return new WaitForSeconds(0.1f);
            PlayingCardsInHand[0].CleanupDestroy();
            PlayingCardsInHand.RemoveAt(0);
        }

        yield return new WaitForSeconds(0.4f);

        ReOrganizeHand();

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(enemyDeck.ShuffleAnimation());
    }


    private void ReOrganizeHand()
    {

        int count = PlayingCardsInHand.Count;

        const float minCount = 2f;
        const float maxCount = 10f;

        const float maxValue = 250f;
        const float minValue = 150f;

        float c = Mathf.Clamp(count, minCount, maxCount);
        float t = (c - minCount) / (maxCount - minCount);

        float spacing = Mathf.Lerp(maxValue, minValue, t);

        for (int i = 0; i < count; i++)
        {
            if (PlayingCardsInHand[i] == null)
                continue;

            float offset = (i - (count - 1) / 2f) * spacing;

            PlayingCardsInHand[i].transform.SetAsFirstSibling();
            PlayingCardsInHand[i].StartMoveCard(offset, true, 0.5f);
        }
    }

    public void PlayEnemyCard(BaseCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        PlayingCardsInHand[0].SetCard(cardToPlay);
        cardToPlay.PlayCard(captainUsing, bTargetingEnemy, captainTargeting);
        StartCoroutine(captainUsing.EnergizeAndExhaust(false));
        StartCoroutine(RemoveCardFromHand());
    }

    private IEnumerator RemoveCardFromHand()
    {
        PlayingCard card = PlayingCardsInHand[0];
        PlayingCardsInHand[0].StartMoveCard(-300, false, 0.5f);
        PlayingCardsInHand.RemoveAt(0);
        yield return new WaitForSeconds(0.1f);
        ReOrganizeHand();

        yield return new WaitForSeconds(1.3f);
    }

    private IEnumerator LoadingOpponentDeck()
    {

        while (bObtainedDeck == false)
        {
            yield return new WaitForEndOfFrame();
        }

        var dataBaseTask = firebasePlayerInfo.DataBaseReference.Child("users").Child(enemyID).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else if (dataBaseTask.Result.Value == null)
        {
            // No content
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            DeckIndex = (snapShot.Child("DeckIndex").Exists) ? DeckIndex = int.Parse(snapShot.Child("DeckIndex").Value.ToString()) : 0;

            yield return new WaitForEndOfFrame();

            List<string> decklist = new List<string>();
            if (snapShot.Child("Deck" + DeckIndex).Exists)
            {
                for (int j = 0; j < snapShot.Child("Deck" + DeckIndex).ChildrenCount; ++j)
                {
                    decklist.Add(snapShot.Child("Deck" + DeckIndex).Child("" + j).Value.ToString());
                }
            }

            int captainsIndex = 0;
            foreach (BaseCard card in gameMaster.GetCaptainLibrary())
            {
                if (decklist.Contains(card.CardName))
                {
                    BaseCard newCaptain = ScriptableObject.Instantiate(card);
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
                        AddCardToDeck(cardLibrary[i]);
                    }
                }
            }
        }

    }
}
