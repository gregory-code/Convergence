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

    public void Init(BaseCard card)
    {
        Outline.sprite = card.Type.outline;
        CardType.sprite = card.Type.icon;
        NameText.text = card.CardName;

        CopiesText.text = "1/" + card.Rarity.maxCopies;
    }
}
