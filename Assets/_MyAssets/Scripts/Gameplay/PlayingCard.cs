using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    private int originalIndex;
    public BaseCard myCard { get; private set; }
    [SerializeField] private UserPlayer ownerPlayer;

    [SerializeField] private GameObject mulliganOverlay;
    private bool bMulliganThis;

    private bool bIsPlayer1;
    
    private bool bEnergized;

    private bool bPreventRegularMoving;

    private Vector3 desiredPos;
    private Vector3 desiredRotation;

    private bool bHovering;

    public void Init(UserPlayer ownerplayer, BaseCard card, bool bIsPlayer1)
    {
        desiredPos = Vector3.zero;

        bEnergized = true;

        this.bIsPlayer1 = bIsPlayer1;

        this.ownerPlayer = ownerplayer;
        myCard = card;

        if(ownerplayer != null)
        {
            ownerplayer.onInspect += InspectCard;
            myCard.Init(ownerplayer);
        }

        SetCard(myCard);

        if(myCard == null)
        {
            GetComponent<VisibleCard>().SetAsCardBack();
        }
    }

    public void PreventRegularMoving()
    {
        bPreventRegularMoving = true;
    }

    public void SetCard(BaseCard card)
    {
        if (myCard != null)
        {
            GetComponent<VisibleCard>().SetCard(card);
        }
    }

    private bool DoIOwnThis()
    {
        if (ownerPlayer == null) // for enemy cards
            return false;

        return (ownerPlayer.bIsPlayer1 == bIsPlayer1);
    }

    private bool EligableTarget(PlayingCard cardTryingToUse)
    {
        if (DoIOwnThis())
        {
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
            if (cardTryingToUse.myCard.bTargetsEnemies && bEnergized == false)
            {
                return true;
            }
        }

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

        desiredRotation.z = (bEnergized) ? 90.0f : 180.0f ;

        float duration = 0.5f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, desiredRotation, Time.deltaTime * 10.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localEulerAngles = desiredRotation;
    }

    private void InspectCard()
    {
        if(bHovering)
        {
            ownerPlayer.InspectCard(myCard, DoIOwnThis());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bHovering = true;

        if (ownerPlayer == null || bEnergized == false) // for enemy cards
            return;

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingCaptain && DoIOwnThis())
        {
            ownerPlayer.HoveringCard(true, this);

        }

        if (myCard.Type.type == CardType.Captain && ownerPlayer.bChoosingTarget)
        {
            if(EligableTarget(ownerPlayer.currentCard) || (ownerPlayer.bSkipCaptainChoice && ownerPlayer.currentCard.myCard.bTargetsSelf && DoIOwnThis()))
            {
                ownerPlayer.HoveringTarget(true, this); 
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

        if(ownerPlayer.bChoosingTarget && ownerPlayer.bSkipCaptainChoice && ownerPlayer.currentTarget != null)
        {
            ownerPlayer.RequestPlayCard(ownerPlayer.currentCard, ownerPlayer.currentTarget, false, ownerPlayer.currentTarget);
        }

        if (ownerPlayer.bChoosingCaptain && ownerPlayer.currentCaptain != null)
        {
            ownerPlayer.ChooseCaptainWhileLineIsRendering();
            return;
        }

        CancelUsingCard();
    }

    public void CancelUsingCard()
    {
        ownerPlayer.StartStopLineRenderer(false, this, Color.white);
        ownerPlayer.BlockHand(false);

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
            bool bTargetingEnemy = (ownerPlayer.currentCaptain.DoIOwnThis() && ownerPlayer.currentTarget.DoIOwnThis()) ? false : true;
            ownerPlayer.RequestPlayCard(ownerPlayer.currentCard, ownerPlayer.currentCaptain, bTargetingEnemy, ownerPlayer.currentTarget);
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
            ownerPlayer.onInspect -= InspectCard;
        }

        if(myCard != null)
        {
            myCard.Cleanup();
        }


        StopAllCoroutines();
        Destroy(this.gameObject);
    }
}