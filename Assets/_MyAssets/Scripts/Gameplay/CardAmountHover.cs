using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardAmountHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI cardAmountText;

    public delegate void OnChangeCardText(bool bStartedHover);
    public event OnChangeCardText onChangeCardText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onChangeCardText?.Invoke(true);
        cardAmountText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onChangeCardText?.Invoke(false);
        cardAmountText.gameObject.SetActive(false);
    }
}
