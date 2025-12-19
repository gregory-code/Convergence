using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/EnergyBlast")]
public class EnergyBlast : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        if (captainTargeting[0].myCard is CaptainCard attackeeTarget)
        {
            int damage = CalculateAttackDamage(2, false, false, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting[0]);
            attackeeTarget.PredictOrDealDamage(false, damage, false, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting[0]);
        }

        if (thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(1);
        }

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
