using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Faye/Faye")]
public class FayeCard : CaptainCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

        StaticGameplayDelegates.onDealtDamage += DealtDamage;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(thisPlayingCard, 1);
        }

        yield return new WaitForEndOfFrame();
    }

    private void DealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage)
    {
        if (allyRecivingDamage.myCard is FayeCard faye)
        {
            if (faye == this && allyRecivingDamage.DoIOwnThis())
            {
                if (faye.currentHealth <= 0)
                {
                    FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(allyRecivingDamage, 1);
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
