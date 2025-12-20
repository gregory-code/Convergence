using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    private int originalIndex;
    public BaseCard myCard { get; private set; }

    [SerializeField] private UserPlayer ownerPlayer;
    [SerializeField] private GameObject mulliganOverlay;

    private bool bMulliganThis;

    public bool bIsPlayer1 { get; private set; }

    public bool bEnergized { get; private set; }

    private bool bPreventRegularMoving;

    private Vector3 desiredPos;
    private Vector3 desiredRotation;
    private Vector3 desiredSize;

    private Vector3 originalSize;

    private bool bHovering;

    [SerializeField] Vector2[] EquipmentPos;

    [SerializeField] GameObject DeathPanel;
    [SerializeField] public GameObject AttackPredictionPanel;
    [SerializeField] GameObject PhysicalGameObject;
    [SerializeField] GameObject MagicGameObject;
    [SerializeField] GameObject DefenseGameObject;
    [SerializeField] TextMeshProUGUI healthPredictionLeft;
    [SerializeField] TextMeshProUGUI healthPredictionRight;
    [SerializeField] TextMeshProUGUI physicalText;
    [SerializeField] TextMeshProUGUI magicText;
    [SerializeField] TextMeshProUGUI defenseText;

    public void Init(UserPlayer ownerplayer, BaseCard card, bool bIsPlayer1)
    {
        desiredPos = Vector3.zero;
        originalSize = this.transform.localScale;

        bEnergized = true;

        this.bIsPlayer1 = bIsPlayer1;

        this.ownerPlayer = ownerplayer;
        SetCard(card);

        if(ownerplayer != null)
        {
            StaticGameplayDelegates.onInspect += InspectCard;
            StaticGameplayDelegates.onTurnEnded += TurnEnds;
            StaticGameplayDelegates.onKilled += SomeoneKilled;
            myCard.Init(ownerplayer);
        }


        if(myCard == null)
        {
            GetComponent<VisibleCard>().SetAsCardBack();
        }
    }

    public void SetHealthText(int newHealth, int maxHealth)
    { 
        GetComponent<VisibleCard>().SetHealthText(newHealth, maxHealth); 
    }

    public void PreventRegularMoving()
    {
        bPreventRegularMoving = true;
    }

    public void SetCard(BaseCard card)
    {
        if (card == null)
            return;

        myCard = card;
        GetComponent<VisibleCard>().SetCard(card);
    }

    public bool DoIOwnThis()
    {
        if (ownerPlayer == null) // for enemy cards
            return false;

        return (ownerPlayer.bIsPlayer1 == bIsPlayer1);
    }

    private bool EligableTarget(PlayingCard cardTryingToUse)
    {
        if (DoIOwnThis())
        {
            if (ownerPlayer.currentCaptain == this && ownerPlayer.currentCaptain.bEnergized == false)
                return false;

            if (cardTryingToUse.myCard.bTargetsAllies)
            {
                return true;
            }

            if (cardTryingToUse.myCard.bTargetsAlliesExceptSelf && ownerPlayer.currentCaptain != this)
            {
                return true;
            }

            if (cardTryingToUse.myCard.bTargetsSelf && ownerPlayer.currentCaptain == this)
            {
                return true;
            }
        }
        
        if(DoIOwnThis() == false)
        {
            if (cardTryingToUse.myCard.bTargetsEnemies && bEnergized == false && myCard.bDead == false)
            {
                return true;
            }
        }

        return false;
    }

    private bool EligableEquipment(EquipmentCard equipment, PlayingCard captainEquipping)
    {
        bool bEligableBasicEquipment = true;

        if (captainEquipping.myCard is CaptainCard captain)
        {
            foreach (PlayingCard equipmentPlayingCard in captain.GetEquipments())
            {
                if (equipmentPlayingCard.myCard is EquipmentCard equipmentAttached)
                {
                    if (equipment.bPrestige)
                    {
                        if (equipment.equipmentType == equipmentAttached.equipmentType || StaticGameplayDelegates.GetAllySparkCount() >= 10)
                            return true;
                    }
                    else
                    {

                        if (captain.GetEquipments().Count >= captain.maxEquipment || equipment.equipmentType == equipmentAttached.equipmentType)
                            bEligableBasicEquipment = false;
                    }
                }
            }
        }

        if (equipment.bPrestige == false && bEligableBasicEquipment)
            return true;

        return false;
    }

    public void StartMoveCard(float pos, bool X, float duration)
    {
        StartCoroutine(MoveCard(pos, X, duration));
    }

    public IEnumerator MoveCard(float pos, bool X, float duration)
    {
        if (X)
            desiredPos.x = pos;
        else
            desiredPos.y = pos;

        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, Time.deltaTime * 10.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = desiredPos;
    }

    public IEnumerator EnergizeAndExhaust(bool bEnergized)
    {
        this.bEnergized = bEnergized;

        desiredRotation.z = (bEnergized) ? 90.0f : 180.0f;

        if(bEnergized)
        {
            StartCoroutine(ShrinkOrGrow(1.0f));
        }
        else
        {
            StartCoroutine(ShrinkOrGrow(0.9f));
        }

            float duration = 0.5f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, desiredRotation, Time.deltaTime * 10.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localEulerAngles = desiredRotation;
    }

    public IEnumerator ShrinkOrGrow(float shrinkMagnitude)
    {
        desiredSize = originalSize * shrinkMagnitude;

        float duration = 0.5f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, desiredSize, Time.deltaTime * 10.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = desiredSize;
    }

    public void Die()
    {
        myCard.respawnTurns = 1;
        myCard.bOncePerTurn = false;
        myCard.bDead = true;

        DeathPanel.SetActive(true);

        StartCoroutine(EnergizeAndExhaust(false));
    }

    public void Respawn()
    {
        myCard.bOncePerTurn = true;
        myCard.bDead = false;

        DeathPanel.SetActive(false);

        StartCoroutine(EnergizeAndExhaust(true));

        if(myCard is CaptainCard capatin)
        {
            capatin.SetToFullHealth();
            SetHealthText(capatin.currentHealth, capatin.maxHealth);
        }
    }

    public void BeginCardAttachment(PlayingCard parentCharacter, int equipmentSlotIndex)
    {
        StartCoroutine(AttachCard(parentCharacter, parentCharacter.transform.parent.transform, equipmentSlotIndex));
    }

    public void RemoveCardAttachment(PlayingCard parentCharacter)
    {
        StartCoroutine(UnAttachCard(parentCharacter, parentCharacter.transform.parent.transform));
    }


    public IEnumerator AttachCard(PlayingCard parentCharacter, Transform fieldTransform, int equipmentSlotIndex)
    {
        transform.SetParent(fieldTransform, false);
        float y = parentCharacter.DoIOwnThis() ? -300 : 300 ;
        transform.localPosition = new Vector3(0, y, 0);

        StartCoroutine(MoveCard(parentCharacter.transform.localPosition.x, true, 0.2f));
        StartCoroutine(MoveCard(parentCharacter.transform.localPosition.y, false, 0.2f));

        yield return new WaitForSeconds(0.3f);

        Image[] images = this.transform.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }

        transform.SetParent(parentCharacter.transform);
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = new Vector3(0.5f, 0.67f, 0.15f);
        RectTransform transformRect = GetComponent<RectTransform>();
        transformRect.sizeDelta = new Vector2(225, 64);
        transform.localPosition = EquipmentPos[equipmentSlotIndex];
    }

    public IEnumerator UnAttachCard(PlayingCard parentCharacter, Transform fieldTransform)
    {
        transform.SetParent(fieldTransform, false);
        transform.localEulerAngles = new Vector3(0,0,90);
        transform.localScale = originalSize;
        RectTransform transformRect = GetComponent<RectTransform>();
        transformRect.sizeDelta = new Vector2(384, 128);

        StartCoroutine(MoveCard(parentCharacter.transform.localPosition.x, true, 0.2f));
        StartCoroutine(MoveCard(parentCharacter.transform.localPosition.y, false, 0.2f));

        yield return new WaitForSeconds(0.1f);

        Image[] images = this.transform.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.raycastTarget = true;
        }

        Transform discardPile = StaticGameplayDelegates.GetDiscardPileTransform(parentCharacter.bIsPlayer1);
        StaticGameplayDelegates.AddCardToDiscard(this);

        transform.SetParent(discardPile, true);

        StartCoroutine(MoveCard(0, true, 0.4f));
        StartCoroutine(MoveCard(0, false, 0.4f));
        StartCoroutine(ShrinkOrGrow(1.0f));
    }

    public void BeginPlayAndDiscard(PlayingCard usingCaptain)
    {
        StartCoroutine(PlayAndDiscard(usingCaptain, usingCaptain.transform.parent.transform));
    }

    public IEnumerator PlayAndDiscard(PlayingCard usingCaptain, Transform fieldTransform)
    {
        transform.SetParent(fieldTransform, false);
        float y = usingCaptain.DoIOwnThis() ? -300 : 300;
        transform.localPosition = new Vector3(0, y, 0);

        StartCoroutine(MoveCard(usingCaptain.transform.localPosition.x, true, 0.2f));
        StartCoroutine(MoveCard(usingCaptain.transform.localPosition.y, false, 0.2f));

        yield return new WaitForSeconds(0.6f);

        Transform discardPile = StaticGameplayDelegates.GetDiscardPileTransform(usingCaptain.bIsPlayer1);
        StaticGameplayDelegates.AddCardToDiscard(this);

        transform.SetParent(discardPile, true);

        StartCoroutine(MoveCard(0, true, 0.4f));
        StartCoroutine(MoveCard(0, false, 0.4f));
        StartCoroutine(ShrinkOrGrow(1.0f));

        //CleanupDestroy();
    }

    private void InspectCard()
    {
        if(bHovering)
        {
            ownerPlayer.InspectCard(this, DoIOwnThis());
        }
    }

    private void TurnEnds(bool bPlayers1Turn)
    {
        if (myCard.respawnTurns > 0 && myCard.bDead)
        {
            myCard.respawnTurns--;
        }
        else if (myCard.respawnTurns <= 0 && myCard.bDead)
        {
            Respawn();
        }
    }

    private void SomeoneKilled(int killingDamage, PlayingCard cardThatWasUsed, PlayingCard allyDoingTheKilling, PlayingCard allyKilled)
    {
        if (allyDoingTheKilling == this && allyDoingTheKilling.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(allyDoingTheKilling, 2);
        }
    }

    public void DisplayAttackStats(bool bStopDisplaying, bool bRecivingEnd)
    {
        if(bStopDisplaying)
        {
            AttackPredictionPanel.SetActive(false);
            PhysicalGameObject.SetActive(false);
            MagicGameObject.SetActive(false);
            DefenseGameObject.SetActive(false);
            return;
        }

        if (bRecivingEnd)
        {
            DefenseGameObject.SetActive(true);
            if (myCard is CaptainCard captain)
                defenseText.text = captain.GetDefense() + "";
        }
        else
        {
            CardPlayContext context = ownerPlayer.currentCard.myCard.PredictCard(ownerPlayer.currentCard, ownerPlayer.currentCaptain, true, ownerPlayer.currentCaptain);

            if (context.bMagicDamage)
            {
                MagicGameObject.SetActive(true);
                magicText.text = context.damage + "";
            }
            else
            {
                PhysicalGameObject.SetActive(true);
                physicalText.text = context.damage + "";
            }
        }
    }

    public void DisplayHealthChange(int newHealth)
    {
        if(myCard is CaptainCard captain)
        {
            AttackPredictionPanel.SetActive(true);

            int maxHealth = captain.maxHealth;
            int currentHealth = captain.currentHealth;

            healthPredictionLeft.text = currentHealth + "";
            healthPredictionLeft.color = (currentHealth >= maxHealth) ? Color.green : Color.white;
            if (currentHealth <= 2)
                healthPredictionLeft.color = Color.red;

            healthPredictionRight.text = newHealth + "";
            healthPredictionRight.color = (newHealth >= maxHealth) ? Color.green : Color.white;
            if (newHealth <= 2)
                healthPredictionRight.color = Color.red;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bHovering = true;

        if (ownerPlayer == null) // for enemy cards
            return;

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingCaptain && DoIOwnThis() && bEnergized == true)
        {
            ownerPlayer.HoveringCard(true, this);

        }

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingTarget)
        {
            if(EligableTarget(ownerPlayer.currentCard) || (ownerPlayer.bSkipCaptainChoice && ownerPlayer.currentCard.myCard.bTargetsSelf && DoIOwnThis() && bEnergized == true))
            {
                if(ownerPlayer.currentCard.myCard is EquipmentCard equipment)
                {
                    if(EligableEquipment(equipment, this))
                        ownerPlayer.HoveringTarget(true, this);
                }
                else
                {
                    ownerPlayer.HoveringTarget(true, this);

                    if (ownerPlayer.currentCard.myCard is ActionCard action)
                    {
                        if(action.bAttackingCard)
                        {
                            for(int i = 0; i < ownerPlayer.currentTargets.Count; i++)
                            {
                                CardPlayContext context = ownerPlayer.currentCard.myCard.PredictCard(ownerPlayer.currentCard, ownerPlayer.currentCaptain, true, ownerPlayer.currentTargets[i]);
                                
                                if(ownerPlayer.currentTargets[i].myCard is CaptainCard captain)
                                {
                                    DisplayHealthChange(captain.currentHealth - context.damage);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (ownerPlayer.bBlockHand)
            return;

        originalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (bPreventRegularMoving)
            return;

        StartCoroutine(MoveCard(50, false, 0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bHovering = false;

        List<PlayingCard> enemies = StaticGameplayDelegates.GetAllAllies(false);
        foreach (PlayingCard enemy in enemies)
        {
            enemy.AttackPredictionPanel.SetActive(false);
        }

        if (ownerPlayer == null) // for enemy cards
            return;

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingCaptain && DoIOwnThis())
        {
            ownerPlayer.HoveringCard(false, this);
        }

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingTarget)
        {
            ownerPlayer.HoveringTarget(false, this);
        }

        if (ownerPlayer.bBlockHand)
           return;

        transform.SetSiblingIndex(originalIndex);

        if (bPreventRegularMoving)
            return;

        StartCoroutine(MoveCard(-5, false, 0.5f));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ownerPlayer == null || bPreventRegularMoving) // for enemy cards
            return;


        if (ownerPlayer.bBlockHand || ownerPlayer.IsMyTurn() == false)
          return;

        ownerPlayer.currentTargets.Clear();

        ownerPlayer.StartStopLineRenderer(true, this, myCard.Type.color);
        ownerPlayer.BlockHand(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ownerPlayer == null) // for enemy cards
            return;

        if(ownerPlayer.bChoosingTarget && ownerPlayer.bSkipCaptainChoice && ownerPlayer.currentTargets.Count > 0)
        {
            ownerPlayer.RequestPlayCard(ownerPlayer.currentCard, ownerPlayer.currentTargets[0], false, ownerPlayer.currentTargets);
        }

        if (ownerPlayer.bChoosingCaptain && ownerPlayer.currentCaptain != null)
        {
            ownerPlayer.ChooseCaptainWhileLineIsRendering();

            if(ownerPlayer.currentCard.myCard is ActionCard action)
            { 
                if(action.bAttackingCard)
                {
                    ownerPlayer.currentCaptain.DisplayAttackStats(false, false);

                    List<PlayingCard> enemies = StaticGameplayDelegates.GetAllAllies(false);
                    foreach(PlayingCard enemy in enemies)
                    {
                        enemy.DisplayAttackStats(false, true);
                    }
                    return;
                }
            }

            return;
        }

        CancelUsingCard();
    }

    public void CancelUsingCard()
    {
        ownerPlayer.StartStopLineRenderer(false, this, Color.white);
        ownerPlayer.BlockHand(false);

        List<PlayingCard> enemies = StaticGameplayDelegates.GetAllAllies(false);
        foreach (PlayingCard enemy in enemies)
        {
            enemy.DisplayAttackStats(true, true);
        }

        List<PlayingCard> allies = StaticGameplayDelegates.GetAllAllies(true);
        foreach (PlayingCard ally in allies)
        {
            ally.DisplayAttackStats(true, true);
        }

        if (bPreventRegularMoving)
            return;

        StartCoroutine(MoveCard(-5, false, 0.5f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ownerPlayer == null) // for enemy cards
            return;

        if(ownerPlayer.bChoosingTarget && ownerPlayer.bSkipCaptainChoice == false)
        {
            bool bTargetingEnemy = (ownerPlayer.currentCaptain.DoIOwnThis() && ownerPlayer.currentTargets[0].DoIOwnThis()) ? false : true;
            ownerPlayer.RequestPlayCard(ownerPlayer.currentCard, ownerPlayer.currentCaptain, bTargetingEnemy, ownerPlayer.currentTargets);
            CancelUsingCard();
        }

        if (ownerPlayer.bInMulligan)
        {
            bMulliganThis = !bMulliganThis;
            mulliganOverlay.SetActive(bMulliganThis);
            ownerPlayer.AddOrRemoveCardToMulligan(this, bMulliganThis);
            return;
        }
    }

    public void CleanupDestroy()
    {
        if (ownerPlayer != null)
        {
            StaticGameplayDelegates.onInspect -= InspectCard;
        }

        if(myCard != null)
        {
            myCard.Cleanup();
        }


        StopAllCoroutines();
        Destroy(this.gameObject);
    }
}