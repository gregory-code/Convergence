using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/HealingWind")]
public class HealingWind : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        foreach(PlayingCard ally in captainTargeting)
        {
            if (ally.myCard is CaptainCard healingTarget)
            {
                if(captainUsing.myCard is CaptainCard healingUser)
                {
                    int health = 3;
                    health += healingUser.GetMagic();
                    healingTarget.HealHealth(health, false, thisPlayingCard, captainUsing, bTargetingEnemy, ally);
                }
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = base.PredictCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainUsing.myCard is CaptainCard captainUsingTheHeal)
        {
            context.damage = -3;
            context.damage -= captainUsingTheHeal.GetMagic();
            context.bMagicDamage = true;
        }

        return context;
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
