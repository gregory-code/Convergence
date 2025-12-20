using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Byren/Byren")]
public class ByrenCard : CaptainCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

        StaticGameplayDelegates.onDealtDamage += DealtDamage;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        yield return new WaitForEndOfFrame();
    }

    private void DealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage)
    {
        if (bDead)
            return;

        if(allyDealingDamage.myCard is ByrenCard byren)
        {
            if(byren == this && allyDealingDamage.DoIOwnThis())
            {
                if (damageDealt >= 3)
                {
                    FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(allyDealingDamage, 1);
                }
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onDealtDamage -= DealtDamage;
    }
}
