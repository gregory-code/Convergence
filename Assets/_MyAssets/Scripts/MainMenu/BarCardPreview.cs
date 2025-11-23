using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarCardPreview : MonoBehaviour
{
    [SerializeField] Image CardTypeIcon;
    [SerializeField] Image Outline;
    [SerializeField] TextMeshProUGUI CopiesText;
    [SerializeField] TextMeshProUGUI NameText;

    private BaseCard Card;
    private CardsMenu OwnerMenu;
    private int maxCopies;

    private string cardName;

    public void Init(CardsMenu ownerMenu, BaseCard card, int copies)
    {
        OwnerMenu = ownerMenu;

        maxCopies = card.Rarity.maxCopies;

        GetComponent<Image>().sprite = card.CardPreviewArt;

        this.Card = card;

        cardName = card.CardName;

        if (card.Type.type != CardType.Captain)
        {
            Outline.sprite = card.Type.outline;
        }

        CardTypeIcon.sprite = card.Type.icon;
        NameText.text = card.CardName;

        UpdateCopes(copies);
    }

    public void RemoveCard()
    {
        OwnerMenu.RemoveCard(Card);
    }

    public void UpdateCopes(int copies)
    {
        CopiesText.text = copies + "/" + maxCopies;
        CopiesText.color = (copies == maxCopies) ? Color.green : Color.white;
    }

    public bool MatchingName(string cardNameToCheck)
    {
        return cardNameToCheck == cardName;
    }
}
