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

    [SerializeField] private GameObject CardBack;

    [SerializeField] private TextMeshProUGUI CardName;
    [SerializeField] private TextMeshProUGUI CardEffectText;
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private TextMeshProUGUI EquipmentText;

    public BaseCard myCard { get; private set; }

    public void SetAsCardBack()
    {
        CardBack.SetActive(true);
    }

    public void SetHealthText(int newHealth, int maxHealth)
    {
        HealthText.text = "" + newHealth;
        HealthText.color = (newHealth >= maxHealth) ? Color.green : Color.white ;

        if(newHealth <= 2)
            HealthText.color = Color.red;
    }

    public void SetCard(BaseCard card) // <--- OwnerMenu will have to change
    {
        myCard = card;

        if(CardBack != null)
            CardBack.SetActive(false);

        if (card.Type.type != CardType.Captain)
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
                SetHealthText(captain.maxHealth, captain.maxHealth);
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

        RichTextFormatter.ApplyFormatting(CardEffectText, card.DescriptionText);
        CardArt.transform.localPosition = card.CardArtAdjustment;
        CardArt.transform.localScale = card.CardArtSize;

        CardArt.sprite = card.CardArt;
        CardName.text = card.CardName;
        RightVial.sprite = card.Type.icon;

    }
}
