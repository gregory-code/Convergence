using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarCardPreview : MonoBehaviour
{
    [SerializeField] Image CardType;
    [SerializeField] Image Outline;
    [SerializeField] TextMeshProUGUI CopiesText;
    [SerializeField] TextMeshProUGUI NameText;

    private CardsMenu OwnerMenu;

    public void Init(CardsMenu ownerMenu, BaseCard card)
    {
        this.OwnerMenu = ownerMenu;
        Outline.sprite = ownerMenu.GetOutlineFromType(card.Type);
        CardType.sprite = ownerMenu.GetIconFromType(card.Type);
        NameText.text = card.CardName;

        CopiesText.text = "1/" + ownerMenu.GetMaxCopiesFromRarity(card.Rarity);
    }
}
