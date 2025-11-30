using TMPro;
using Unity.VisualScripting;
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

    private BaseCard myCard;

    public void SetCard(BaseCard card) // <--- OwnerMenu will have to change
    {
        myCard = card;

        if(card.Type.type != CardType.Captain)
        {
            HealthText.text = "";
            EquipmentText.text = "";

            LeftVial.sprite = card.Rarity.icon;
        }
        else
        {
            CaptainCard captain = card as CaptainCard;
            if(captain)
            {
                HealthText.text = "" + captain.maxHealth;
                EquipmentText.text = "" + captain.maxEquipment;
            }

            LeftVial.sprite = card.Type.outline;
        }

        if(card.SeriesOverlay == null)
        {
            CardTemplate.sprite = card.Type.templateOutline;
            Overlay.gameObject.SetActive(false);
        }
        else
        {
            CardTemplate.sprite = card.Type.templateCaptain;
            Overlay.sprite = card.SeriesOverlay;
            Overlay.gameObject.SetActive(true);
        }


        CardArt.transform.localPosition = card.CardArtAdjustment;
        CardArt.transform.localScale = card.CardArtSize;

        CardArt.sprite = card.CardArt;
        CardName.text = card.CardName;
        RightVial.sprite = card.Type.icon;

    }
}
