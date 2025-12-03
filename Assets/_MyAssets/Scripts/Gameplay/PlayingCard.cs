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

    private Vector3 desiredPos;

    public void Init(UserPlayer ownerplayer, BaseCard card)
    {
        desiredPos = Vector3.zero;

        this.ownerPlayer = ownerplayer;
        myCard = card;
        GetComponent<VisibleCard>().SetCard(card);

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
        if (ownerPlayer.bBlockHand)
            return;

        originalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        StopAllCoroutines();
        StartCoroutine(MoveCard(50, false, 0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ownerPlayer.bBlockHand)
           return;

        transform.SetSiblingIndex(originalIndex);

        StopAllCoroutines();
        StartCoroutine(MoveCard(-5, false, 0.5f));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ownerPlayer.bBlockHand)
          return;

        ownerPlayer.BlockHand(true);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ownerPlayer.BlockHand(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(ownerPlayer.bInMulligan)
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

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}