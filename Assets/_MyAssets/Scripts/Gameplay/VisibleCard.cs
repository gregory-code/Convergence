using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisibleCard : MonoBehaviour
{
    [SerializeField] private Image CardArt;
    [SerializeField] private Image CardTemplate;
    [SerializeField] private Image Overlay;
    [SerializeField] private Image RightVial;
    [SerializeField] private Image LeftVial;

    [SerializeField] private TextMeshProUGUI CardName;
    [SerializeField] private TextMeshProUGUI CardEffectText;
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private TextMeshProUGUI EquipmentText;

    public void SetCard(BaseCard card) // <--- OwnerMenu will have to change
    {
        if(card.Type.type != CardType.Captain)
        {
            HealthText.text = "";
            EquipmentText.text = "";

            LeftVial.sprite = card.Rarity.icon;
            Overlay.gameObject.SetActive(false);
        }
        else
        {
            Overlay.sprite = card.SeriesOverlay;
            Overlay.gameObject.SetActive(true);

            LeftVial.sprite = card.Type.outline;
        }

        CardArt.transform.localPosition = card.CardArtAdjustment;

        CardArt.sprite = card.CardArt;
        CardName.text = card.CardName;
        CardTemplate.sprite = card.Type.templateOutline;
        RightVial.sprite = card.Type.icon;

    }
}
