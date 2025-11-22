using UnityEngine;

[CreateAssetMenu(menuName = "TCG/BaseCard")]
public class BaseCard : ScriptableObject
{
    public string CardName;

    public TypeCard Type;
    public RarityCard Rarity;
    public CaptainCard CaptainCard;
    public Sprite SeriesOverlay;

    public Sprite CardArt;
    public Vector2 CardArtAdjustment;
    public Sprite CardPreviewArt;
}
