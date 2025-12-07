using UnityEngine;

public class BaseCard : ScriptableObject
{
    public string CardName;

    public TypeCard Type;
    public RarityCard Rarity;
    public CardCaptain Captain;
    public Sprite SeriesOverlay;

    public Sprite CardArt;
    public Vector2 CardArtAdjustment;
    public Vector2 CardArtSize;
    public Sprite CardPreviewArt;

    public bool bSwift;

    [TextArea(3, 10)]
    public string DescriptionText;
}
