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

    [SerializeField] private CardAmountHover allyDeckCardAmount;
    [SerializeField] private GameObject MullgianPanel;
    [SerializeField] private GameObject CaptainPanel;
    [SerializeField] private GameObject InspectionPanel;
    [SerializeField] private VisibleCard InspectionCard;
    [SerializeField] private Transform InspectionTransform;
    [SerializeField] private InspectionItem InspectionItemPrefab;
    private List<InspectionItem> InspectionItemList = new List<InspectionItem>();
    [SerializeField] private GameObject inspectionStats;
    [SerializeField] private TextMeshProUGUI healthInspectionText;
    [SerializeField] private TextMeshProUGUI physicalInspectionText;
    [SerializeField] private TextMeshProUGUI magicInspectionText;
    [SerializeField] private TextMeshProUGUI defenseInspectionText;

    [SerializeField] private CanvasGroup playerOptionsCanvasGroup;
    [SerializeField] private CanvasGroup playerReactionCanvasGroup;

    [SerializeField] private GameObject playerReactionHoldUpGroup;
    [SerializeField] private Image reactionTimerImage;
    private float reactionTime;
    private Coroutine reactionTimerCorotine;

    private int DeckIndex;

    public bool bIsPlayer1 { get; private set; }

    [SerializeField] private Vector3 hiddenHand;
    [SerializeField] private Vector3 hoverHand;

    public bool bBlockHand { get; private set; }
    public bool bInMulligan { get; private set; }
    public bool bChoosingCaptain { get; private set; }
    public bool bChoosingTarget { get; private set; }
    public bool bSkipCaptainChoice { get; private set; }

    [SerializeField] private LineAttackPredictionScript lineAttackPredictionPrefab;
    private List<LineAttackPredictionScript> lineAttacks = new List<LineAttackPredictionScript>();

    [HideInInspector]
    public PlayingCard currentCaptain;
    [HideInInspector]
    public List<PlayingCard> currentTargets = new List<PlayingCard>();

    [HideInInspector]
    public PlayingCard currentReactionCard;
    public PlayingCard currentCard { get; private set; }

    [SerializeField] private PlayingCard playingCardPrefab;
    [SerializeField] private List<PlayingCard> PlayingCardsInHand = new List<PlayingCard>(); // FOR TESTING TO SEE THE VALUES
    private List<PlayingCard> CardsToMulligan = new List<PlayingCard>();

    private bool bIsInspecting;

    private void Start()
    {
        allyDeckCardAmount.onChangeCardText += HoveredAllyDeck;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(bChoosingTarget && lineRenderer.IsHoveringOverCard() == false)
            {
                BlockHand(false);

                if (currentCaptain != null)
                    currentCaptain.CancelUsingCard();

                StartStopLineRenderer(false, null, Color.white);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if(bIsInspecting)
            {
                StopInspecting();
            }
            else
            {
                StaticGameplayDelegates.Inspect();
            }
        }

        if ((bChoosingCaptain == false && bChoosingTarget == false) || lineRenderer.IsHoveringOverCard())
            return;

        lineRenderer.UpdateReticleLocation(GetUIToWorldPoint(Input.mousePosition));
    }

    public void StopInspecting()
    {
        bIsInspecting = false;
        InspectionPanel.SetActive(false);
    }

    public void WaitingForReaction(bool bWaiting, PlayingCard captainToPlay)
    {
        BlockHand(bWaiting);
        StartCoroutine(GetPlayerOptions(!bWaiting));

        if(currentReactionCard != null && bWaiting)
        {
            StartCoroutine(currentReactionCard.PlayReaction(captainToPlay, captainToPlay.transform.parent, currentReactionCard.myCard));
        }
    }

    // This is when it sees an enemy throwing out a reaction //
    public bool bAllowingReactions { get; private set; }
    public BaseCard reactionAnticipating { get; private set; }
    public PlayingCard reactionCaptainUsingAnticipating { get; private set; }
    public List<PlayingCard> reactionCaptainTargetingAnticipating { get; private set; }
    public void AllowReaction(bool bAllow, BaseCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        bAllowingReactions = bAllow;
        BlockHand(!bAllow);
        StartCoroutine(GetReactionOptions(bAllow));
        playerReactionHoldUpGroup.SetActive(bAllow);

        if (bAllow)
        {
            reactionTime = 5.0f;
            reactionTimerCorotine = StartCoroutine(ReactionTime());

            reactionAnticipating = cardToPlay;
            reactionCaptainUsingAnticipating = captainUsing;
            reactionCaptainTargetingAnticipating = captainTargeting;
        }
    }

    private IEnumerator ReactionTime()
    {
        reactionTimerImage.fillAmount = 1.0f;
        while(reactionTime > 0)
        {
            yield return new WaitForEndOfFrame();
            reactionTime -= Time.deltaTime;
            reactionTimerImage.fillAmount = Mathf.InverseLerp(0f, 5f, reactionTime);
        }
        FinishReaction();
    }

    public void FinishReaction()
    {
        if(reactionTimerCorotine != null)
            StopCoroutine(reactionTimerCorotine);

        AllowReaction(false, null, null, false, null);

        gameMaster.RequestFinishReaction();
    }

    public void EnemyFinishReaction()
    {
        currentReactionCard.myCard.bWaitForReaction = false;
        BlockHand(false);
        StartCoroutine(GetPlayerOptions(true));

        List<PlayingCard> enemies = StaticGameplayDelegates.GetAllAllies(false);
        foreach (PlayingCard enemy in enemies)
        {
            enemy.DisplayAttackStats(true, true, null, null);
        }

        List<PlayingCard> allies = StaticGameplayDelegates.GetAllAllies(true);
        foreach (PlayingCard ally in allies)
        {
            ally.DisplayAttackStats(true, true, null, null);
        }
    }

    public void HoldUpReaction()
    {
        if (reactionTimerCorotine != null)
            StopCoroutine(reactionTimerCorotine);

        playerReactionHoldUpGroup.SetActive(false);
    }
    // This is when it sees an enemy throwing out a reaction //

    public void SetIsPlayer1() { bIsPlayer1 = true; }
    public bool IsMyTurn() { return gameMaster.bPlayer1sTurn == bIsPlayer1; }

    public void RequestPlayCard(PlayingCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        currentCard = cardToPlay;
        currentCaptain = captainUsing;
        currentTargets = captainTargeting;
        gameMaster.RequestPlayCard(cardToPlay, captainUsing, bTargetingEnemy, captainTargeting, false);
    }

    public void PlayClientCard(bool bTargetingEnemy)
    {
        StartCoroutine(currentCard.myCard.PlayCard(currentCard, currentCaptain, bTargetingEnemy, currentTargets));

        if(currentCard.myCard.bSwift == false)
            StartCoroutine(currentCaptain.EnergizeAndExhaust(false));

        StartCoroutine(RemoveCardFromHand(currentCard));
    }

    public void StartPickNewCaptain()
    {
        BlockHand(true);
        StartCoroutine(GetPlayerOptions(false));
        StartCoroutine(PickNewCaptain());
    }

    private IEnumerator PickNewCaptain()
    {
        yield return new WaitForSeconds(3);

        CaptainPanel.SetActive(true);
    }

    public void ConfirmNewCaptain(int captainIndex)
    {
        BlockHand(false);
        CaptainPanel.SetActive(false);
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().interactable = false;
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().alpha = 0.3f;
        gameMaster.PlayerConfirmedNewCaptain(SelectCaptainsVisible[captainIndex].myCard);
    }

    public void StartTurn(bool shouldDraw)
    {
        StartCoroutine(GetPlayerOptions(true));
        bBlockHand = false;

        if(shouldDraw)
        {
            gameMaster.RequestDrawCards(1);
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

    private IEnumerator GetReactionOptions(bool bShow)
    {
        float time = 0.2f;
        float alpha = (bShow) ? 1 : 0;
        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;

            playerReactionCanvasGroup.alpha = Mathf.Lerp(playerReactionCanvasGroup.alpha, alpha, 20 * Time.deltaTime);
        }

        playerReactionCanvasGroup.interactable = bShow;
        playerReactionCanvasGroup.blocksRaycasts = bShow;
        playerReactionCanvasGroup.alpha = alpha;
    }

    public void EndTurn()
    {
        StartCoroutine(GetPlayerOptions(false));
        gameMaster.RequestSwitchTurns();
    }

    public void RequestDrawCards(int cardsToDraw)
    {
        StartCoroutine(DrawCardsToHand(cardsToDraw));
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
        newPlayingCard.StartMoveCard(-5.0f, false, 0.5f);

        ReOrganizeHand();
    }

    private IEnumerator RemoveCardFromHand(PlayingCard cardToRemove)
    {
        cardToRemove.PreventRegularMoving();
        PlayingCardsInHand.Remove(cardToRemove);
        yield return new WaitForSeconds(0.1f);
        ReOrganizeHand();
    }

    private void ReOrganizeHand()
    {

        int count = PlayingCardsInHand.Count;

        const float minCount = 2f;
        const float maxCount = 22f;

        const float maxValue = 250f;
        const float minValue = 50f;

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

    private Vector3 desiredPos;
    private IEnumerator MoveHand(Vector3 handPos)
    {
        desiredPos = handPos;

        float duration = 0.8f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, Time.deltaTime * 15.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = desiredPos;
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

    public void StartMulligan()
    {
        MullgianPanel.SetActive(true);
        bInMulligan = true;

        gameMaster.RequestDrawCards(5);
    }

    public void SkipMulligan(int cardsToStartWith)
    {
        gameMaster.RequestDrawCards(cardsToStartWith);
        StartPickNewCaptain();
    }

    public void ConfirmMulligan()
    {
        gameMaster.RequestFinishMulligan(CardsToMulligan.Count);

        StartPickNewCaptain();
    }


    public IEnumerator MullgianWrapUp()
    {
        MullgianPanel.SetActive(false);
        bInMulligan = false;
        bBlockHand = true;

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

    public void InspectCard(PlayingCard cardToInspect, bool IOwnit)
    {
        for (int i = 0; i < InspectionItemList.Count; i++)
        {
            Destroy(InspectionItemList[i].gameObject);
        }
        InspectionItemList.Clear();

        bIsInspecting = true;
        InspectionPanel.SetActive(true);
        InspectionCard.SetCard(cardToInspect.myCard);

        inspectionStats.SetActive(false);

        InspectionItem inspectionSelf = Instantiate(InspectionItemPrefab, InspectionTransform);
        inspectionSelf.Init(this, cardToInspect, IOwnit);
        InspectionItemList.Add(inspectionSelf);

        if (cardToInspect.myCard is CaptainCard captain)
        {
            inspectionStats.SetActive(true);
            healthInspectionText.text = $"{captain.maxHealth + captain.GetBonusHealth()}";
            physicalInspectionText.text = captain.GetPhysical() + "";
            magicInspectionText.text = captain.GetMagic() + "";
            defenseInspectionText.text = captain.GetDefense() + "";

            InspectionCard.SetHealthText(captain.currentHealth, captain.maxHealth);
            foreach(PlayingCard equipment in captain.GetEquipments())
            {
                InspectionItem inspection = Instantiate(InspectionItemPrefab, InspectionTransform);
                inspection.Init(this, equipment, IOwnit);
                InspectionItemList.Add(inspection);
            }

            foreach (PlayingCard linger in captain.GetLingersInEffect())
            {
                InspectionItem inspection = Instantiate(InspectionItemPrefab, InspectionTransform);
                inspection.Init(this, linger, IOwnit);
                InspectionItemList.Add(inspection);
            }
        }
    }

    public void BlockHand(bool bState)
    {
        bBlockHand = bState;
        if (bState)
        {
            StartCoroutine(MoveHand(hiddenHand));
        }
    }

    public void StartStopLineRenderer(bool bStart, PlayingCard cardToStart, Color theme)
    {
        bChoosingCaptain = bStart;
        bChoosingTarget = false;
        bSkipCaptainChoice = false;
        currentCaptain = null;
        currentCard = cardToStart;
        lineRenderer.FirstEnable(bStart, GetUIToWorldPoint(Input.mousePosition), theme);

        if (cardToStart != null && bStart)
        {
            if (cardToStart.myCard.bTargetsSelf)
            {
                bSkipCaptainChoice = true;
                bChoosingCaptain = false;
                bChoosingTarget = true;
            }
        }
    }

    public void ChooseCaptainWhileLineIsRendering()
    {
        bChoosingCaptain = false;
        bChoosingTarget = true;

        lineRenderer.SelectCaptain();
    }

    public void TargetAllWithLineRenders(PlayingCard captainUsing)
    {
        currentTargets.Clear();

        foreach(PlayingCard card in StaticGameplayDelegates.GetAllAllies(true))
            currentTargets.Add(card);

        ClearLineAttacks();

        for (int i = 0; i < currentTargets.Count; i++)
        {
            LineAttackPredictionScript lineAttack = Instantiate(lineAttackPredictionPrefab);
            lineAttack.ShowPrediction(captainUsing.transform.position, currentTargets[i].transform.position, Color.orange);
            lineAttacks.Add(lineAttack);
        }
    }

    public void ClearLineAttacks()
    {
        for (int i = 0; i < lineAttacks.Count; i++)
        {
            Destroy(lineAttacks[i].gameObject);
        }
        lineAttacks.Clear();
    }

    private Vector3 GetUIToWorldPoint(Vector3 referencePoint)
    {
        referencePoint.z = 2.9f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(referencePoint);
        return worldMousePos;
    }

    public void HoveringCard(bool bHovering, PlayingCard cardHovered)
    {
        currentCaptain = (bHovering) ? cardHovered : null;
        lineRenderer.FocusCaptain(bHovering, cardHovered.transform);
    }

    public void HoveringTarget(bool bHovering, PlayingCard cardHovered)
    {
        currentTargets.Clear();
        if(bHovering)
        {
            currentTargets.Add(cardHovered);
        }

        lineRenderer.FocusTarget(bHovering, cardHovered.transform);
    }

    private void HoveredAllyDeck(bool bStartedHover)
    {
        if (bStartedHover)
        {
            int cardsInDeck = UserDeck.Count;
            allyDeckCardAmount.cardAmountText.text = "" + cardsInDeck;
        }
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

        StartCoroutine(MoveHand(hoverHand));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bBlockHand)
            return;

        StartCoroutine(MoveHand(hiddenHand));
    }
}
