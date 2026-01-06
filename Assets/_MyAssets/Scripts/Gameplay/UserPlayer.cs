using Firebase.Database;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserPlayer : MonoBehaviour, IDataPersistence, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected FirebasePlayerInfo firebasePlayerInfo;
    [SerializeField] private GameMaster gameMaster;
    [SerializeField] private VisualDeck allyVisualDeck;
    [SerializeField] private VisualDeck enemyVisualDeck;
    [SerializeField] private LineScript lineRenderer;
    [SerializeField] private Transform enemySideHandTransform;

    private List<BaseCard> UserCaptains = new List<BaseCard>();
    private List<BaseCard> UserDeck = new List<BaseCard>();

    private List<BaseCard> EnemyCaptains = new List<BaseCard>();
    private List<BaseCard> EnemyDeck = new List<BaseCard>();

    [SerializeField] private GameObject SelectCaptain;
    [SerializeField] private VisibleCard[] SelectCaptainsVisible;

    [SerializeField] private CardAmountHover allyDeckCardAmount;
    [SerializeField] private CardAmountHover enemyHandCardAmount;
    [SerializeField] private CardAmountHover enemyDeckCardAmount;

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

    [SerializeField] private TextMeshProUGUI ChoicePanelButtonText;
    [SerializeField] private Image ChoicePanelButtonImage;
    [SerializeField] private GameObject ChoicePanelButton;
    [SerializeField] private Sprite ChoicePanelButtonShowTable;
    [SerializeField] private Sprite ChoicePanelButtonHideTable;
    [SerializeField] private Sprite ChoicePanelButtonExit;

    [SerializeField] private GameObject playerReactionHoldUpGroup;
    [SerializeField] private Image reactionTimerImage;
    private float reactionTime;
    private Coroutine reactionTimerCorotine;

    private int DeckIndex;
    private int EnemyDeckIndex;

    public bool bIsPlayer1 { get; private set; }

    [SerializeField] private Vector3 hiddenHand;
    [SerializeField] private Vector3 hoverHand;

    public bool bBlockHand { get; private set; }
    public bool bInMulligan { get; private set; }
    public bool bChoosingCaptain { get; private set; }
    public bool bChoosingTarget { get; private set; }
    public bool bSkipCaptainChoice { get; private set; }

    [SerializeField] private LineAttackPredictionScript lineAttackPredictionPrefab;
    private List<LineAttackPredictionScript> linePredictions = new List<LineAttackPredictionScript>();

    [HideInInspector]
    public PlayingCard currentCaptain;
    [HideInInspector]
    public List<PlayingCard> currentTargets = new List<PlayingCard>();

    public PlayingCard currentCard { get; private set; }

    private bool bObtainedDeck = false;
    private string enemyID;

    [SerializeField] private PlayingCard playingCardPrefab;
    private List<PlayingCard> PlayingCardsInHand = new List<PlayingCard>();
    private List<PlayingCard> EnemyCardsInHand = new List<PlayingCard>();
    public int GetCardCountInHand(bool bMyHand) { return (bMyHand) ? PlayingCardsInHand.Count : EnemyCardsInHand.Count; }
    public List<PlayingCard> GetPlayingCardsInHand() { return PlayingCardsInHand; }

    private List<PlayingCard> CardsToMulligan = new List<PlayingCard>();

    private List<PlayingCard> DaybreakCards = new List<PlayingCard>();
    private List<ChoiceCard> ChoiceCards = new List<ChoiceCard>();
    [SerializeField] private ChoiceCard ChoiceCardPrefab;
    [SerializeField] private Transform ChoiceCardPanelTransform;
    [SerializeField] private CanvasGroup ChoicePanel;
    [SerializeField] private TextMeshProUGUI ChoicePanelInstructions;

    public void AddToDaybreak(PlayingCard thisCard) { DaybreakCards.Add(thisCard); }

    private bool bIsInspecting;

    private void Start()
    {
        allyDeckCardAmount.onChangeCardText += HoveredAllyDeck;
        enemyHandCardAmount.onChangeCardText += HoveredEnemyHand;
        enemyDeckCardAmount.onChangeCardText += HoveredEnemyDeck;
        StartCoroutine(LoadingOpponentDeck());
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
                if(bInUniqueMenu == false && bInDiscardPile == false && bInChoiceMenu == false)
                {
                    StaticGameplayDelegates.Inspect();
                }
            }
        }

        if ((bChoosingCaptain == false && bChoosingTarget == false) || lineRenderer.IsHoveringOverCard())
            return;

        lineRenderer.UpdateReticleLocation(GetUIToWorldPoint(Input.mousePosition));
    }

    public void LoadOpponentID(string ID, bool bIsPlayer1)
    {
        enemyID = ID;
        bObtainedDeck = true;
    }

    public void StopInspecting()
    {
        bIsInspecting = false;
        InspectionPanel.SetActive(false);
    }

    public void WaitingForReaction(bool bWaiting, PlayingCard captainUsing)
    {
        if (DaybreakCards.Count > 0)
        {
            BlockHand(true);
            StartCoroutine(GetPlayerOptions(false));
        }
        else
        {
            BlockHand(bWaiting);
            StartCoroutine(GetPlayerOptions(!bWaiting));
        }

        if(gameMaster.reactionPlayingCard != null && bWaiting)
        {
            if(gameMaster.reactionPlayingCard.myCard.Type.type == CardType.Ally)
            {

            }
            else
            {
                StartCoroutine(gameMaster.reactionPlayingCard.PlayReaction(captainUsing, captainUsing.transform.parent, gameMaster.reactionPlayingCard.myCard));
            }
        }
    }

    // This is when it sees an enemy throwing out a reaction //
    public bool bAllowingReactions { get; private set; }

    public void AllowReaction(bool bAllow)
    {
        bAllowingReactions = bAllow;
        BlockHand(!bAllow);
        StartCoroutine(GetReactionOptions(bAllow));

        playerReactionHoldUpGroup.SetActive(bAllow);

        if (bAllow)
        {
            reactionTime = 5.0f;
            reactionTimerCorotine = StartCoroutine(ReactionTime());
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

        AllowReaction(false);

        gameMaster.RequestFinishReaction();
    }

    public void EnemyFinishReaction(bool bClient)
    {
        if (gameMaster.reactionPlayingCard != null)
        {
            gameMaster.reactionPlayingCard.myCard.bWaitForReaction = false;
        }

        if(bClient)
        {
            if (gameMaster.bPlayer1sTurn == bIsPlayer1)
            {
                BlockHand(DaybreakCards.Count > 0);
                StartCoroutine(GetPlayerOptions(DaybreakCards.Count <= 0));
            }
        }
        else
        {
            gameMaster.ClearLineAttacks();
        }


        gameMaster.ResetAllDisplayAttackStats();
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

    public void RequestPlayCard(PlayingCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting, bool bForceSwift)
    {
        currentCard = cardToPlay;
        currentCaptain = captainUsing;
        currentTargets = captainTargeting;
        gameMaster.RequestPlayCard(cardToPlay, captainUsing, bTargetingEnemy, captainTargeting, false, false, bForceSwift);
    }

    public void PlayClientCard(bool bTargetingEnemy)
    {
        StartCoroutine(currentCard.myCard.PlayCard(currentCard, currentCaptain, bTargetingEnemy, currentTargets));

        if(currentCard.myCard.bSwift == false)
            StartCoroutine(currentCaptain.EnergizeAndExhaust(false));

        StartCoroutine(RemoveCardFromHand(currentCard, true));
    }

    public void ClickedChoicePanelButton()
    {
        if(bInChoiceMenu)
        {
            bShowingTable = !bShowingTable;
            SetPanelButtonTableState(bShowingTable);

            SetChoicePanel(bShowingTable, false);
        }

        if(bInDiscardPile)
        {
            SetChoicePanel(false, true);
            ChoicePanelButton.SetActive(false);
            bInDiscardPile = false;
        }
    }

    public void ShowDiscardPile(bool bMyDiscardPile)
    {
        if (bInChoiceMenu)
            return;

        ChoicePanelInstructions.text = "Discard Pile";

        ChoicePanelButton.SetActive(true);

        SetChoicePanel(true, false);
        ChoicePanelButtonImage.sprite = ChoicePanelButtonExit;
        ChoicePanelButtonText.text = "Close Menu";
        bInDiscardPile = true;

        List<PlayingCard> discardPile = gameMaster.GetDiscardPile(bMyDiscardPile);

        ClearChoiceMenu();

        foreach (PlayingCard card in discardPile)
        {
            AddItemToChoice(card, null);
        }
    }

    private bool bInChoiceMenu;
    private bool bInDiscardPile;

    [HideInInspector]
    public bool bInUniqueMenu;

    private bool bShowingTable;

    private void SetPanelButtonTableState(bool ShowTable)
    {
        ChoicePanelButtonImage.sprite = (ShowTable) ? ChoicePanelButtonShowTable : ChoicePanelButtonHideTable ;
        ChoicePanelButtonText.text = (ShowTable) ? "Show Table" : "Hide Table" ;
    }

    private void SetChoicePanel(bool bShow, bool bFullyHide)
    {
        ChoicePanel.alpha = bShow ? 0.7f : 0.2f;
        ChoicePanel.interactable = bShow ? true : false ;
        ChoicePanel.blocksRaycasts = bShow ? true : false ;
        if (bFullyHide)
        {
            ChoicePanel.alpha = 0.0f;
            ChoicePanelButton.gameObject.SetActive(false);
        }
    }

    public void DoUniqueChoice(PlayingCard playingCardEffect, PlayingCard usingCaptain)
    {
        BlockHand(true);
        SetChoicePanel(true, false);
        StartCoroutine(GetPlayerOptions(false));

        ClearChoiceMenu();

        bInUniqueMenu = true;

        if(playingCardEffect.myCard is CrownOfNature)
        {
            ChoicePanelInstructions.text = "Pick an Ally";

            foreach (BaseCard card in UserDeck)
            {
                if(card.Type.type == CardType.Ally)
                {
                    AddItemToChoice(null, card);
                }
            }
        }

        if (playingCardEffect.myCard is INeedMore)
        {
            ChoicePanelInstructions.text = "Play an Equipment";

            foreach (PlayingCard card in PlayingCardsInHand)
            {
                if(card.myCard is EquipmentCard equipment)
                {
                    if (card.EligableEquipment(equipment, usingCaptain))
                    {
                        ChoiceCard newChoice = AddItemToChoice(card, card.myCard);
                        newChoice.usingCaptain = usingCaptain;
                        newChoice.bINeedMore = true;
                    }
                }
            }
        }

        if (playingCardEffect.myCard is DestinyStopwatch)
        {
            ChoicePanelInstructions.text = "Parry with an Attack";

            foreach (PlayingCard card in PlayingCardsInHand)
            {
                if (card.myCard is ActionCard attackingAction)
                {
                    if (attackingAction.bAttackingCard)
                    {
                        ChoiceCard newChoice = AddItemToChoice(card, card.myCard);
                        newChoice.usingCaptain = usingCaptain;
                        newChoice.bDestinyStopwatch = true;
                    }
                }
            }
        }

        if (ChoiceCards.Count <= 0)
        {
            FinishUniqueChoice();
        }
    }

    public IEnumerator DrawCardFromDeck(BaseCard cardToDraw)
    {
        for (int i = 0; i < UserDeck.Count; i++)
        {
            if (UserDeck[i].CardName == cardToDraw.CardName)
            {
                gameMaster.RequestShowOpponentIveDrawn(cardToDraw);
                allyVisualDeck.DrawTopCard();
                yield return new WaitForSeconds(0.34f);
                UserDeck.RemoveAt(i);
                AddCardToHand(cardToDraw, true);
                yield break;
            }
        }
    }

    public void FinishUniqueChoice()
    {
        BlockHand(false);
        SetChoicePanel(false, true);
        StartCoroutine(GetPlayerOptions(true));
        bInUniqueMenu = false;
    }

    private bool bPickingCaptain;
    public void StartPickNewCaptain()
    {
        bPickingCaptain = true;

        BlockHand(true);
        SetChoicePanel(false, true);
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
        bPickingCaptain = false;

        BlockHand(false);
        CaptainPanel.SetActive(false);
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().interactable = false;
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().blocksRaycasts = false;
        SelectCaptainsVisible[captainIndex].GetComponent<CanvasGroup>().alpha = 0.3f;
        gameMaster.PlayerConfirmedNewCaptain(SelectCaptainsVisible[captainIndex].myCard);
    }


    public void StartTurn(bool shouldDraw)
    {
        bBlockHand = true;

        if(shouldDraw)
        {
            gameMaster.RequestDrawCards(1);
        }

        DaybreakCards.Clear();
        ClearChoiceMenu();

        StartCoroutine(DaybreakCheck());

        StaticGameplayDelegates.TurnStarted(this, bIsPlayer1);
    }

    private void ClearChoiceMenu()
    {
        foreach (ChoiceCard choice in ChoiceCards)
        {
            Destroy(choice.gameObject);
        }
        ChoiceCards.Clear();
    }

    private ChoiceCard AddItemToChoice(PlayingCard card, BaseCard cardEffect)
    {
        ChoiceCard newChoice = Instantiate(ChoiceCardPrefab, ChoiceCardPanelTransform.transform);
        newChoice.Init(this, card, cardEffect);
        ChoiceCards.Add(newChoice);
        return newChoice;
    }

    public IEnumerator DaybreakCheck()
    {
        yield return new WaitForSeconds(0.1f);

        if(DaybreakCards.Count > 0)
        {
            ChoicePanelInstructions.text = "Trigger the effects";

            bInChoiceMenu = true;
            bInDiscardPile = false;
            bInUniqueMenu = false;
            bShowingTable = true;
            ChoicePanelButton.SetActive(true);

            SetPanelButtonTableState(true);
            SetChoicePanel(true, false);

            foreach (PlayingCard card in DaybreakCards)
            {
                AddItemToChoice(card, null);
            }

            while(DaybreakCards.Count > 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        gameMaster.RequestRemoveLingers();

        bInChoiceMenu = false;
        ChoicePanelButton.SetActive(false);

        if(bPickingCaptain == false)
        {
            StartCoroutine(GetPlayerOptions(true));
            bBlockHand = false;
        }

        SetChoicePanel(false, true);
    }

    public void StopAndWaitChoosingSomething()
    {
        ChoicePanelButton.SetActive(false);
        SetChoicePanel(false, false);
    }

    public void RemoveChioceCard(PlayingCard daybreakCard)
    {
        DaybreakCards.Remove(daybreakCard);

        if (DaybreakCards.Count > 0)
        {
            SetChoicePanel(true, false);
            ChoicePanelButton.SetActive(true);
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
        DaybreakCards.Clear();
        SetChoicePanel(false, true);
        gameMaster.RequestSwitchTurns();
    }

    public void RequestDrawCards(int cardsToDraw, bool bClientSide)
    {
        StartCoroutine(DrawCardsToHand(cardsToDraw, bClientSide));
    }

    private IEnumerator DrawCardsToHand(int cardsToDraw, bool bClientSide)
    {
        if(bClientSide)
        {
            for (int i = 0; i < cardsToDraw; i++)
            {
                allyVisualDeck.DrawTopCard();
                yield return new WaitForSeconds(0.04f);
            }

            yield return new WaitForSeconds(0.3f);

            for (int i = 0; i < cardsToDraw; i++)
            {
                int index = UnityEngine.Random.Range(0, UserDeck.Count);
                BaseCard randomCard = UserDeck[index];
                UserDeck.RemoveAt(index);
                AddCardToHand(randomCard, bClientSide);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            for (int i = 0; i < cardsToDraw; i++)
            {
                enemyVisualDeck.DrawTopCard();
                yield return new WaitForSeconds(0.04f);
            }

            yield return new WaitForSeconds(0.3f);

            for (int i = 0; i < cardsToDraw; i++)
            {
                AddCardToHand(null, bClientSide);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void AddCardToDeck(BaseCard cardsToAdd, bool bClientSide)
    {
        if(bClientSide)
        {
            BaseCard newCard = ScriptableObject.Instantiate(cardsToAdd);
            UserDeck.Add(newCard);
            allyVisualDeck.AddCardToVisualDeck();
        }
        else
        {
            if (cardsToAdd != null)
            {
                BaseCard newCard = ScriptableObject.Instantiate(cardsToAdd);
                EnemyDeck.Add(newCard);
            }
            enemyVisualDeck.AddCardToVisualDeck();
        }
    }
    public void AddCardToHand(BaseCard newCardToadd, bool bClientSide)
    {
        if(bClientSide)
        {
            PlayingCard newPlayingCard = Instantiate(playingCardPrefab, this.transform);
            newPlayingCard.Init(this, newCardToadd, bIsPlayer1, gameMaster.GetNextID(), null);
            newPlayingCard.transform.localPosition = new Vector3(0, -150, 0);
            PlayingCardsInHand.Add(newPlayingCard);
            newPlayingCard.StartMoveCard(-5.0f, false, 0.5f);

            ReOrganizeHand(bClientSide);
        }
        else
        {
            PlayingCard newPlayingCard = Instantiate(playingCardPrefab, enemySideHandTransform);
            newPlayingCard.Init(null, newCardToadd, bIsPlayer1, gameMaster.GetNextID(), null);
            newPlayingCard.transform.localPosition = new Vector3(0, 150, 0);
            EnemyCardsInHand.Add(newPlayingCard);
            newPlayingCard.StartMoveCard(-5, false, 0.5f);

            ReOrganizeHand(bClientSide);
        }
    }

    public void PlayAllyCard(PlayingCard allyCard, bool bClientSide)
    {
        if (bClientSide)
        {
            StartCoroutine(RemoveCardFromHand(allyCard, bClientSide));
        }
        else
        {
            EnemyCardsInHand[0].StartMoveCard(350.0f, false, 0.5f);
            StartCoroutine(RemoveCardFromHand(EnemyCardsInHand[0], bClientSide));
        }
    }

    private IEnumerator RemoveCardFromHand(PlayingCard cardToRemove, bool bClientSide)
    {
        cardToRemove.PreventRegularMoving();

        if (bClientSide)
            PlayingCardsInHand.Remove(cardToRemove);
        else
            EnemyCardsInHand.Remove(cardToRemove);

        yield return new WaitForSeconds(0.1f);
        ReOrganizeHand(bClientSide);
    }

    public void PlayEnemyCard(BaseCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting, bool bForceSwift)
    {
        if (cardToPlay is ActionCard action)
        {
            if (action.bAttackingCard)
            {
                List<PlayingCard> myTeam = StaticGameplayDelegates.GetEnemies(captainUsing);
                bool bAllyIsEnergized = false;

                foreach (PlayingCard ally in myTeam)
                {
                    if (ally.bEnergized)
                        bAllyIsEnergized = true;
                }

                if (bAllyIsEnergized)
                {
                    StartCoroutine(cardToPlay.PlayCard(EnemyCardsInHand[0], captainUsing, bTargetingEnemy, captainTargeting));
                    return;
                }
            }
        }

        if (cardToPlay is CaptainCard captain)
        {
            if (captain.bIsAllyCard == false)
            {
                // Passive Proc or activatable ability

                StartCoroutine(cardToPlay.PlayCard(captainUsing, captainUsing, bTargetingEnemy, null));

                if (cardToPlay.bSwift == false && bForceSwift == false)
                    StartCoroutine(captainUsing.EnergizeAndExhaust(false));

                return;
            }
            else
            {
                if (cardToPlay.bSwift == false && bForceSwift == false)
                    StartCoroutine(captainUsing.EnergizeAndExhaust(false));

                return;
            }
        }

        EnemyCardsInHand[0].SetCard(cardToPlay, bIsPlayer1);

        if (cardToPlay.bSwift == false && bForceSwift == false)
            StartCoroutine(captainUsing.EnergizeAndExhaust(false));

        StartCoroutine(cardToPlay.PlayCard(EnemyCardsInHand[0], captainUsing, bTargetingEnemy, captainTargeting));
        StartCoroutine(RemoveCardFromHand(EnemyCardsInHand[0], false));
    }

    public IEnumerator EnemyRevealCardAndDraw(BaseCard cardToDraw)
    {
        enemyVisualDeck.DrawTopCard();
        yield return new WaitForSeconds(0.04f);

        PlayingCard newPlayingCard = Instantiate(playingCardPrefab, this.transform);
        newPlayingCard.Init(null, cardToDraw, bIsPlayer1, -1, null); // THIS IS -1 BECAUSE OTHERWISE IT CREATES A UNIQUE CARD THAT THE CLIENT NEVER SEES, THUS MAKING UNIQUE ID DIFFERENT VALUES
        newPlayingCard.transform.localPosition = new Vector3(0, 250, 0);
        newPlayingCard.StartMoveCard(-550, false, 0.5f);

        yield return new WaitForSeconds(0.8f);

        newPlayingCard.StartMoveCard(250, false, 0.3f);

        yield return new WaitForSeconds(0.3f);

        AddCardToHand(null, false);
    }

    public void EnemyIsAttackingPredicition(BaseCard cardToPlay, PlayingCard captainUsing)
    {
        if (captainUsing.myCard.Type.type == CardType.Ally)
        {
            gameMaster.reactionPlayingCard = captainUsing;
            captainUsing.DisplayAttackStats(false, false, captainUsing, captainUsing);
        }
        else
        {
            EnemyCardsInHand[0].SetCard(cardToPlay, bIsPlayer1);

            if (cardToPlay.bSwift == false)
                StartCoroutine(captainUsing.EnergizeAndExhaust(false));

            StartCoroutine(EnemyCardsInHand[0].PlayReaction(captainUsing, captainUsing.transform.parent, cardToPlay));

            //StartCoroutine(cardToPlay.PlayCard(PlayingCardsInHand[0], captainUsing, bTargetingEnemy, captainTargeting));
            gameMaster.reactionPlayingCard = EnemyCardsInHand[0];
            StartCoroutine(RemoveCardFromHand(EnemyCardsInHand[0], false));

            captainUsing.DisplayAttackStats(false, false, EnemyCardsInHand[0], captainUsing);
        }
    }

    private void ReOrganizeHand(bool bClientSide)
    {

        int count = (bClientSide) ? PlayingCardsInHand.Count : EnemyCardsInHand.Count;

        const float minCount = 2f;
        const float maxCount = 22f;

        const float maxValue = 250f;
        const float minValue = 50f;

        float c = Mathf.Clamp(count, minCount, maxCount);
        float t = (c - minCount) / (maxCount - minCount);

        float spacing = Mathf.Lerp(maxValue, minValue, t);

        if(bClientSide)
        {
            for (int i = 0; i < count; i++)
            {
                if (PlayingCardsInHand[i] == null)
                    continue;

                float offset = (i - (count - 1) / 2f) * spacing;

                PlayingCardsInHand[i].transform.SetAsFirstSibling();
                PlayingCardsInHand[i].StartMoveCard(offset, true, 0.5f);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (EnemyCardsInHand[i] == null)
                    continue;

                float offset = (i - (count - 1) / 2f) * spacing;

                EnemyCardsInHand[i].transform.SetAsFirstSibling();
                EnemyCardsInHand[i].StartMoveCard(offset, true, 0.5f);
            }
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


    public IEnumerator MullgianWrapUp(bool bClientSide, int cardsToMulligan)
    {
        if(bClientSide)
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
                AddCardToDeck(CardsToMulligan[i].myCard, bClientSide);
                PlayingCardsInHand.Remove(CardsToMulligan[i]);
                yield return new WaitForSeconds(0.1f);
                CardsToMulligan[i].CleanupDestroy();
            }

            ReOrganizeHand(bClientSide);
            StartCoroutine(ReenableHand());
            CardsToMulligan.Clear();

            yield return new WaitForSeconds(0.4f);
            StartCoroutine(allyVisualDeck.ShuffleAnimation());
        }
        else
        {
            yield return new WaitForSeconds(1.4f);

            int desiredAmountInHand = PlayingCardsInHand.Count - cardsToMulligan;

            while (PlayingCardsInHand.Count != desiredAmountInHand)
            {
                PlayingCardsInHand[0].StartMoveCard(350, false, 0.2f);
                yield return new WaitForSeconds(0.1f);
                AddCardToDeck(PlayingCardsInHand[0].myCard, bClientSide);
                yield return new WaitForSeconds(0.1f);
                PlayingCardsInHand[0].CleanupDestroy();
                PlayingCardsInHand.RemoveAt(0);
            }

            yield return new WaitForSeconds(0.4f);

            ReOrganizeHand(bClientSide);

            yield return new WaitForSeconds(0.2f);
            StartCoroutine(enemyVisualDeck.ShuffleAnimation());
        }
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

    public bool bForceChoosingCaptain { get; private set; }
    public void ForceChooseCaptain(bool bForce)
    {
        bForceChoosingCaptain = bForce;

        bSkipCaptainChoice = !bForce;
        bChoosingCaptain = bForce;
        bChoosingTarget = !bForce;
    }

    public void ChooseCaptainWhileLineIsRendering()
    {
        bChoosingCaptain = false;
        bChoosingTarget = true;

        bForceChoosingCaptain = false;

        lineRenderer.SelectCaptain();
    }

    public void ClearTargetAllRenders()
    {
        for (int i = 0; i < linePredictions.Count; i++)
        {
            Destroy(linePredictions[i].gameObject);
        }
        linePredictions.Clear();
    }

    public void TargetAllWithLineRenders(PlayingCard captainUsing)
    {
        currentTargets.Clear();

        foreach(PlayingCard card in StaticGameplayDelegates.GetTeammates(captainUsing))
            currentTargets.Add(card);

        ClearTargetAllRenders();

        for (int i = 0; i < currentTargets.Count; i++)
        {
            LineAttackPredictionScript lineAttack = Instantiate(lineAttackPredictionPrefab);
            lineAttack.ShowPrediction(captainUsing.transform.position, currentTargets[i].transform.position, Color.orange);
            linePredictions.Add(lineAttack);
        }
    }

    public void TargetEnemyWithLineRenderes(PlayingCard captainUsing, PlayingCard target)
    {
        LineAttackPredictionScript lineAttack = Instantiate(lineAttackPredictionPrefab);
        lineAttack.ShowPrediction(captainUsing.transform.position, target.transform.position, Color.orange);
        linePredictions.Add(lineAttack);
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

    private void HoveredEnemyHand(bool bStartedHover)
    {
        if (bStartedHover)
            enemyHandCardAmount.cardAmountText.text = "" + PlayingCardsInHand.Count;
    }

    private void HoveredEnemyDeck(bool bStartedHover)
    {
        if (bStartedHover)
        {
            int cardsInDeck = enemyVisualDeck.VisualDeckAmount();
            enemyDeckCardAmount.cardAmountText.text = "" + cardsInDeck;
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
                    AddCardToDeck(cardLibrary[i], true);
                }
            }
        }

        yield return new WaitForEndOfFrame();
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

            EnemyDeckIndex = (snapShot.Child("DeckIndex").Exists) ? EnemyDeckIndex = int.Parse(snapShot.Child("DeckIndex").Value.ToString()) : 0;

            yield return new WaitForEndOfFrame();

            List<string> decklist = new List<string>();
            if (snapShot.Child("Deck" + EnemyDeckIndex).Exists)
            {
                for (int j = 0; j < snapShot.Child("Deck" + EnemyDeckIndex).ChildrenCount; ++j)
                {
                    decklist.Add(snapShot.Child("Deck" + EnemyDeckIndex).Child("" + j).Value.ToString());
                }
            }

            int captainsIndex = 0;
            foreach (BaseCard card in gameMaster.GetCaptainLibrary())
            {
                if (decklist.Contains(card.CardName))
                {
                    BaseCard newCaptain = ScriptableObject.Instantiate(card);
                    captainsIndex++;
                    EnemyCaptains.Add(newCaptain);
                }
            }

            List<BaseCard> cardLibrary = gameMaster.GetEveryCardLibrary();
            for (int i = 0; i < cardLibrary.Count; i++)
            {
                for (int j = 0; j < decklist.Count; j++)
                {
                    if (decklist[j] == cardLibrary[i].CardName)
                    {
                        AddCardToDeck(cardLibrary[i], false);
                    }
                }
            }
        }

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
