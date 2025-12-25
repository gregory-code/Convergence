using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionCard : BaseCard
{
    public bool bAttackingCard;
    public bool bHasPredcition;

    [HideInInspector]
    public List<PlayingCard> CaptainTargeting = new List<PlayingCard>();

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        CaptainTargeting = captainTargeting;
        thisCard = thisPlayingCard;
                
        yield return new WaitForEndOfFrame();

        if(bAttackingCard)
            yield return WaitForReaction(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = new CardPlayContext
        {
            thisPlayingCard = thisPlayingCard,
            captainUsing = captainUsing,
            bTargetingEnemy = bTargetingEnemy,
            captainTargeting = captainTargeting,
            damage = 0,
            bMagicDamage = false
        };

        return context;
    }

    public IEnumerator WaitForReaction(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        if(bTargetingEnemy == false || captainTargeting.Count <= 0)
            yield break;

        List<PlayingCard> enemyTeam = StaticGameplayDelegates.GetAllAllies(false);
        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetAllAllies(true);
        List<PlayingCard> teamConsidering = (enemyTeam.Contains(captainTargeting[0])) ? enemyTeam : allyTeam;

        bool bEnemyIsEnergized = false;

        foreach (PlayingCard enemy in teamConsidering)
        {
            if (enemy.bEnergized)
                bEnemyIsEnergized = true;
        }

        if(bEnemyIsEnergized)
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
        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetAllAllies(true);
        List<PlayingCard> enemyTeam = StaticGameplayDelegates.GetAllAllies(false);

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

        damage = Mathf.Max(damage, 0);

        return damage;
    }

    public int GetTeammatePhysicalBuffs()
    {
        return 0;
    }

    public int GetEnemyDefenseBuffs()
    {
        return 0;
    }

    public bool ConvertToMagic(bool bIsMagic, PlayingCard thisPlayingCard, CaptainCard captainUsing)
    {
        if (bIsMagic)
            return true;

        foreach(PlayingCard equipmentCard in captainUsing.GetEquipments())
        {
            if (equipmentCard.myCard is OddHat)
                return true;
        }

        return false;
    }

    public override void Cleanup()
    {

    }
}
