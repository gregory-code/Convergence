using UnityEngine;

[CreateAssetMenu(menuName = "BaseCard")]
public class BaseCard : ScriptableObject
{
    public string CardName;
    public string InternalName;

    public CardType Type;
    public CardRarity Rarity;

}
