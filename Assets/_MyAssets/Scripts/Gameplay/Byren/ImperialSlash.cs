using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Byren/ImperialSlash")]
public class ImperialSlash : ActionCard
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
            int damage = CalculateAttackDamage(3, false, true, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
            attackeeTarget.TakeDamage(damage, false, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
        }

        yield return new WaitForEndOfFrame();
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = base.PredictCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainUsing.myCard is CaptainCard captainUsingTheAttack)
        {
            context.damage = CalculateAttackDamage(3, false, true, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
            context.bMagicDamage = ConvertToMagic(false, thisPlayingCard, captainUsingTheAttack);
        }
        return context;
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
