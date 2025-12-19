using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionCard : BaseCard
{
    public bool bAttackingCard;
    public bool bMagicAttack;

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return new WaitForEndOfFrame();
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
