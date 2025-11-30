using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    private int originalIndex;
    private BaseCard myCard;
    [SerializeField] private Hand ownerHand;

    [SerializeField] private GameObject mulliganOverlay;
    private bool bMulliganThis;

    public void Init(Hand ownerHand, BaseCard card)
    {
        this.ownerHand = ownerHand;
        myCard = card;
        GetComponent<VisibleCard>().SetCard(card);

    }

    public IEnumerator MoveCardUpToPlace()
    {
        Vector3 desiredSpot = new Vector3(0, -5, 0);

        /*float duration = 1.0f;
        while (duration > 0)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredSpot, 5 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }*/

        transform.localPosition = desiredSpot;

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator MoveCard(Vector3 handPos)
    {
        float duration = 0.5f;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, handPos, Time.deltaTime * 10.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = handPos;
    }

    public BaseCard GetCard() { return myCard; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ownerHand.GetBlockHand())
            return;

        originalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        StopAllCoroutines();
        StartCoroutine(MoveCard(new Vector3(transform.localPosition.x, 50, transform.localPosition.z)));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ownerHand.GetBlockHand())
            return;

        transform.SetSiblingIndex(originalIndex);

        StopAllCoroutines();
        StartCoroutine(MoveCard(new Vector3(transform.localPosition.x, -5, transform.localPosition.z)));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ownerHand.GetBlockHand())
            return;

        ownerHand.BlockHand(true);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ownerHand.BlockHand(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(ownerHand.GetMulligan())
        {
            bMulliganThis = !bMulliganThis;
            mulliganOverlay.SetActive(bMulliganThis);
            ownerHand.AddOrRemoveCardToMulligan(this, bMulliganThis);
            return;
        }
    }
}