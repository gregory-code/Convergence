using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/SilverLight")]
public class SilverLight : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        if (CaptainTargeting[0].myCard is CaptainCard attackeeTarget)
        {
            int damage = CalculateAttackDamage(3, true, false, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
            attackeeTarget.TakeDamage(damage, true, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
        }

        yield return new WaitForEndOfFrame();
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = base.PredictCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainUsing.myCard is CaptainCard captainUsingTheAttack)
        {
            context.damage = CalculateAttackDamage(3, true, true, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
            context.bMagicDamage = true;
        }

        return context;
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
