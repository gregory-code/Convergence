using System.Collections.Generic;

public class GlobalEnums
{

}

public enum CardCaptain
{
    None,
    Byren,
    Frida,
    DrDaemon,
    Ophelia,
    Olly,
    Prince,
    Layla,
    Alpha,
    Lochana,
    MittQuibble,
    Ivy,
    Faye,
    WindWizard,
    IceWizard
}

public enum Series
{
    None,
    Convergence,
    Jigen,
    NotPokemon,
    NotNatureWars,
    NatureWars
}

public enum CardType
{
    Captain,
    Action,
    Reaction,
    Equipment,
    Ally,
    Invention
}

public enum CardRarity
{
    Captain,
    Common,
    Rare,
    Legendary
}

public enum EquipmentType
{
    Weapon, // physical
    MagicItem, // magic and utility
    Shield, // defense
    Clothing, // all rounder
    Unique // gimmicks
}

public enum ReactionType
{
    EnemyAttacks,
    AllyDies
}

[System.Serializable]
public struct CardPlayContext
{
    public PlayingCard thisPlayingCard;
    public PlayingCard captainUsing;
    public bool bTargetingEnemy;
    public PlayingCard captainTargeting;

    public int damage;
    public bool bMagicDamage;
}