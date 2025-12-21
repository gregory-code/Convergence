using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCard : ScriptableObject
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

    public bool bTargetsSelf;
    public bool bTargetsAllies;
    public bool bTargetsAlliesExceptSelf;
    public bool bTargetsEnemies;

    public bool bSwift;

    [TextArea(3, 10)]
    public string DescriptionText;

    [HideInInspector]
    public bool bOncePerTurn = true;
    [HideInInspector]
    public bool bWaitForReaction = false;
    [HideInInspector]
    public bool bDead = false;
    [HideInInspector]
    public int respawnTurns = 0;

    public abstract void Init(UserPlayer ownerPlayer);
    public abstract IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting);
    public abstract CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting);
    public abstract void Cleanup();
}
