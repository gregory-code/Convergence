using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Card Type Database")]
public class TypeCard : ScriptableObject
{
    public CardType type;
    public Sprite icon;
    public Sprite templateOutline;
    public Sprite outline;
    public Sprite squareOutline;
}
