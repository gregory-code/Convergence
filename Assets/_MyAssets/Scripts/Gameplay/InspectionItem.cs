using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionItem : MonoBehaviour
{
    [SerializeField] private Image Icon;
    [SerializeField] private TextMeshProUGUI Description;

    public void Init(BaseCard card)
    {
        Description.text = card.DescriptionText;
        Icon.sprite = card.CardArt;
    }
}
