using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "TCG/Card Rarity Database")]
public class RarityCard : ScriptableObject
{
    public CardRarity rarity;
    public Sprite icon;
    public int maxCopies;
}
