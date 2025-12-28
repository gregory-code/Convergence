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
    public bool bTargetsAll;

    public bool bSwift;

    [TextArea(3, 10)]
    public string DescriptionText;

    public bool bActiveInEffectLinger = false;
    public int physicalStatLinger;
    public int magicStatLinger;
    public int defenseStatLinger;

    [HideInInspector]
    public PlayingCard thisCard;
    [HideInInspector]
    public bool bOncePerTurn = true;
    [HideInInspector]
    public bool bWaitForReaction = false;
    [HideInInspector]
    public bool bDead = false;
    [HideInInspector]
    public int respawnTurns = 0;

    [HideInInspector]
    public List<PlayingCard> CaptainTargeting = new List<PlayingCard>();

    [HideInInspector]
    public int predictionDamage = 0;

    [HideInInspector]
    public List<PlayingCard> captainsAffectedByPredictionDamageIncrease = new List<PlayingCard>();

    public IEnumerator WaitForReaction(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        if (bTargetingEnemy == false || captainTargeting.Count <= 0)
            yield break;

        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetTeammates(captainUsing);
        List<PlayingCard> enemyTeam = StaticGameplayDelegates.GetEnemies(captainUsing);

        bool bEnemyIsEnergized = false;

        foreach (PlayingCard enemy in enemyTeam)
        {
            if (enemy.bEnergized)
                bEnemyIsEnergized = true;
        }

        if (bEnemyIsEnergized)
        {
            bWaitForReaction = true;

            if (thisPlayingCard.DoIOwnThis())
            {
                StaticGameplayDelegates.RequestAttackPrediction(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
            }

            while (bWaitForReaction)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }

    }

    public int CalculateAttackDamage(int baseDamage, bool bIsMagic, bool bIgnoreDefense, PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetTeammates(captainUsing);
        List<PlayingCard> enemyTeam = StaticGameplayDelegates.GetEnemies(captainUsing);

        int damage = baseDamage;

        CaptainCard attackingCaptain = (CaptainCard)captainUsing.myCard;
        CaptainCard attackeeTarget = (CaptainCard)captainTargeting.myCard;

        bIsMagic = ConvertToMagic(bIsMagic, thisPlayingCard, attackingCaptain);

        int teammatePhysicalBuff = GetTeammatePhysicalBuffs();
        int enemyDefenseBuff = GetEnemyDefenseBuffs();

        if (bIsMagic)
        {
            damage += attackingCaptain.GetMagic();
        }
        else //Physical
        {
            damage += attackingCaptain.GetPhysical();
            damage += teammatePhysicalBuff;

            damage -= (bIgnoreDefense) ? 0 : attackeeTarget.GetDefense();
            damage -= (bIgnoreDefense) ? 0 : enemyDefenseBuff;
        }

        foreach (PlayingCard target in captainsAffectedByPredictionDamageIncrease)
        {
            if (target != null)
            {
                if (captainTargeting.uniqueID == target.uniqueID)
                {
                    damage += predictionDamage;
                    break;
                }
            }
        }

        damage = Mathf.Max(damage, 0);

        return damage;
    }

    public int GetTeammatePhysicalBuffs()
    {
        return 0;
    }

    public int GetTeammateDefenseBuffs()
    {
        int defense = 0;

        if (this is CaptainCard thisCapatain)
        {
            List<PlayingCard> allies = StaticGameplayDelegates.GetTeammates(thisCapatain.thisCard);
            if (allies == null)
                return 0;

            foreach (PlayingCard ally in allies)
            {
                if (ally.myCard is CaptainCard allyCaptain)
                {
                    if (allyCaptain.bDead)
                        continue;

                    foreach (PlayingCard equipment in allyCaptain.GetEquipments())
                    {
                        if (equipment.myCard is CrownOfNature crown)
                        {
                            if (crown.crownsOwner != null)
                            {
                                if (thisCapatain.CaptainWhoPlayedMe != null)
                                {
                                    if (thisCapatain.CaptainWhoPlayedMe.uniqueID == crown.crownsOwner.uniqueID)
                                    {
                                        defense += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return defense;
    }

    public int GetEnemyDefenseBuffs()
    {
        return 0;
    }

    public bool ConvertToMagic(bool bIsMagic, PlayingCard thisPlayingCard, CaptainCard captainUsing)
    {
        if (bIsMagic)
            return true;

        if(thisPlayingCard.myCard is ActionCard action)
        {
            foreach (PlayingCard equipmentCard in captainUsing.GetEquipments())
            {
                if (equipmentCard.myCard is OddHat)
                    return true;
            }

            return false;
        }

        return bIsMagic;
    }

    public abstract void Init(UserPlayer ownerPlayer);
    public abstract IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting);
    public abstract IEnumerator ActivateEffect(PlayingCard thisPlayingCard);
    public abstract IEnumerator SecondaryPlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting);
    public abstract CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting);
    public abstract void Cleanup();
}
