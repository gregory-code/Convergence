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

    private Vector3 desiredPos;

    public void Init(UserPlayer ownerplayer, BaseCard card, bool bIsPlayer1)
    {
        desiredPos = Vector3.zero;

        this.bIsPlayer1 = bIsPlayer1;

        this.ownerPlayer = ownerplayer;
        myCard = card;

        if(myCard != null)
        {
            GetComponent<VisibleCard>().SetCard(card);
        }
        else
        {
            GetComponent<VisibleCard>().SetAsCardBack();
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
        if (ownerPlayer == null) // for enemy cards
            return false;


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
            if (cardTryingToUse.myCard.bTargetsEnemies)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ownerPlayer == null) // for enemy cards
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

        StartCoroutine(MoveCard(50, false, 0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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

        StartCoroutine(MoveCard(-5, false, 0.5f));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ownerPlayer == null) // for enemy cards
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


        if (ownerPlayer.bChoosingCaptain && ownerPlayer.currentCaptain != null)
        {
            ownerPlayer.ChooseCaptainWhileLineIsRendering();
            return;
        }

        CancelUsingCard();
    }

    public void CancelUsingCard()
    {
        StartCoroutine(MoveCard(-5, false, 0.5f));
        ownerPlayer.StartStopLineRenderer(false, this, Color.white);
        ownerPlayer.BlockHand(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ownerPlayer == null) // for enemy cards
            return;


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
        StopAllCoroutines();
        Destroy(this.gameObject);
    }
}