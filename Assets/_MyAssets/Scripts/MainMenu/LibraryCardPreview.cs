using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LibraryCardPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject RemoveCard;
    [SerializeField] Image CardArt;
    [SerializeField] Image Outline;
    [SerializeField] Image RarityIcon;

    private CardsMenu OwnerMenu;
    public BaseCard Card { get; private set; }

    public void Init(CardsMenu ownerMenu, BaseCard card)
    {
        this.OwnerMenu = ownerMenu;
        this.Card = card;

        CardArt.sprite = card.CardArt;
        Outline.sprite = card.Type.squareOutline;
        RarityIcon.sprite = card.Rarity.icon;

    }

    public void AddCard()
    {
        OwnerMenu.AddCard(Card, true);
    }

    public bool MatchingName(string cardNameToCheck)
    {
        return cardNameToCheck == Card.CardName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OwnerMenu.ShowCardEffect(Card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
