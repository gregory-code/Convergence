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
    private BaseCard Card;

    public void Init(CardsMenu ownerMenu, BaseCard card)
    {
        this.OwnerMenu = ownerMenu;
        this.Card = card;

        CardArt.sprite = card.CardArt;
        Outline.sprite = card.Type.squareOutline;
        RarityIcon.sprite = card.Rarity.icon;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OwnerMenu.ShowCardEffect(Card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
