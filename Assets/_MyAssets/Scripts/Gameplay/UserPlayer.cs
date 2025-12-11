using Firebase.Database;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UserPlayer : MonoBehaviour, IDataPersistence, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameMaster gameMaster;
    [SerializeField] private VisualDeck visualDeck;
    [SerializeField] private LineScript lineRenderer;

    [SerializeField] private List<BaseCard> UserCaptains = new List<BaseCard>();
    [SerializeField] private List<BaseCard> UserDeck = new List<BaseCard>();

    [SerializeField] private GameObject SelectCaptain;
    [SerializeField] private VisibleCard[] SelectCaptainsVisible;

    [SerializeField] private GameObject MullgianPanel;
    [SerializeField] private GameObject CaptainPanel;

    [SerializeField] private CanvasGroup playerOptionsCanvasGroup;

    private int DeckIndex;

    private bool bIsPlayer1;

    [SerializeField] private Vector3 hiddenHand;
    [SerializeField] private Vector3 hoverHand;

    public bool bBlockHand { get; private set; }
    public bool bInMulligan { get; private set; }
    public bool bChoosingCard { get; private set; }

    [SerializeField] private PlayingCard playingCardPrefab;
    private List<PlayingCard> PlayingCardsInHand = new List<PlayingCard>();
    private List<PlayingCard> CardsToMulligan = new List<PlayingCard>();

    private void Start()
    {

    }

    private void Update()
    {
        if (bChoosingCard == false)
            return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 2.9f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        lineRenderer.UpdateReticleLocation(worldMousePos);
    }

    public void SetIsPlayer1() { bIsPlayer1 = true; }

    public void StartMulligan()
    {
        MullgianPanel.SetActive(true);
        bInMulligan = true;

        StartCoroutine(DrawCardsToHand(5));
    }

    public void ConfirmMulligan()
    {
        MullgianPanel.SetActive(false);
        bInMulligan = false;

        bBlockHand = true;
        StartCoroutine(DrawCardsToHand(CardsToMulligan.Count));
        StartCoroutine(MullgianWrapUp());
        StartPickNewCaptain();
    }

    public void StartPickNewCaptain()
    {
        StartCoroutine(PickNewCaptain());
    }

    private IEnumerator PickNewCaptain()
    {
        yield return new WaitForSeconds(3);

        CaptainPanel.SetActive(true);
    }

    public void ConfirmNewCaptain(int captainIndex)
    {
        CaptainPanel.SetActive(false);
        gameMaster.PlayerConfirmedNewCaptain(SelectCaptainsVisible[captainIndex].myCard);
    }

    public void StartTurn(bool shouldDraw)
    {
        StartCoroutine(GetPlayerOptions(true));
        bBlockHand = false;

        if(shouldDraw)
        {
            StartCoroutine(DrawCardsToHand(1));
        }
    }

    private IEnumerator GetPlayerOptions(bool bShow)
    {
        float time = 0.2f;
        float alpha = (bShow) ? 1 : 0 ;
        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;

            playerOptionsCanvasGroup.alpha = Mathf.Lerp(playerOptionsCanvasGroup.alpha, alpha, 20 * Time.deltaTime);
        }

        playerOptionsCanvasGroup.interactable = bShow;
        playerOptionsCanvasGroup.blocksRaycasts = bShow;
        playerOptionsCanvasGroup.alpha = alpha;

    }

    public void EndTurn()
    {
        StartCoroutine(GetPlayerOptions(false));
        gameMaster.RequestSwitchTurns();
    }

    private IEnumerator DrawCardsToHand(int cardsToDraw)
    {
        for(int i = 0; i < cardsToDraw; i++)
        {
            visualDeck.DrawTopCard();
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < cardsToDraw; i++)
        {
            int index = UnityEngine.Random.Range(0, UserDeck.Count);
            BaseCard randomCard = UserDeck[index];
            UserDeck.RemoveAt(index);
            AddCardToHand(randomCard);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void AddCardToDeck(BaseCard cardsToAdd)
    {
        BaseCard newCard = ScriptableObject.Instantiate(cardsToAdd);
        UserDeck.Add(newCard);
        visualDeck.AddCardToVisualDeck();
    }
    public void AddCardToHand(BaseCard newCardToadd)
    {
        PlayingCard newPlayingCard = Instantiate(playingCardPrefab, this.transform);
        newPlayingCard.Init(this, newCardToadd, bIsPlayer1);
        newPlayingCard.transform.localPosition = new Vector3(0, -150, 0);
        PlayingCardsInHand.Add(newPlayingCard);
        newPlayingCard.StartMoveCard(-5, false, 0.5f);

        ReOrganizeHand();
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

    public void AddCaptainToFight()
    {
        SelectCaptain.SetActive(true);
    }


    private IEnumerator ReenableHand()
    {
        yield return new WaitForSeconds(1.0f);
        bBlockHand = false;
    }

    private IEnumerator MoveHand(Vector3 handPos)
    {
        float duration = 0.8f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, handPos, Time.deltaTime * 15.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = handPos;
    }

    public void AddOrRemoveCardToMulligan(PlayingCard card, bool bMullgianThis)
    {
        if (bMullgianThis)
        {
            CardsToMulligan.Add(card);
        }
        else if (bMullgianThis == false && CardsToMulligan.Contains(card))
        {
            CardsToMulligan.Remove(card);
        }
    }

    private IEnumerator MullgianWrapUp()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < CardsToMulligan.Count; i++)
        {
            if (CardsToMulligan[i] == null)
                continue;

            CardsToMulligan[i].StartMoveCard(-350, false, 0.2f);
            yield return new WaitForSeconds(0.1f);
            AddCardToDeck(CardsToMulligan[i].myCard);
            PlayingCardsInHand.Remove(CardsToMulligan[i]);
            yield return new WaitForSeconds(0.1f);
            CardsToMulligan[i].CleanupDestroy();
        }

        ReOrganizeHand();
        StartCoroutine(ReenableHand());
        CardsToMulligan.Clear();

        yield return new WaitForSeconds(0.4f);
        StartCoroutine(visualDeck.ShuffleAnimation());
    }

    public void BlockHand(bool bState)
    {
        bBlockHand = bState;
        if (bState)
        {
            StopAllCoroutines();
            StartCoroutine(MoveHand(hiddenHand));
        }
    }

    public void StartStopLineRenderer(bool bStart, PlayingCard cardToStart)
    {
        bChoosingCard = bStart;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 2.9f;
        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 startingPos = new Vector3(world.x, -3.2f, -10); // hard coded lul

        lineRenderer.enable(bStart, startingPos);
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        DeckIndex = (data.Child("DeckIndex").Exists) ? DeckIndex = int.Parse(data.Child("DeckIndex").Value.ToString()) : 0;

        yield return new WaitForEndOfFrame();

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
                    AddCardToDeck(cardLibrary[i]);
                }
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        if (key == "username")
        {
            //foundUsername = data.ToString();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bBlockHand)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHand(hoverHand));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bBlockHand)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHand(hiddenHand));
    }
}
