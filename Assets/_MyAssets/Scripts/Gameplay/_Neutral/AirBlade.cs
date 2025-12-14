using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/AirBlade")]
public class AirBlade : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        if (captainUsing.myCard is CaptainCard attackingCaptain)
        {
            int damage = 1;
            damage += attackingCaptain.GetPhysical();

            if (captainTargeting.myCard is CaptainCard attackeeTarget)
            {
                damage -= attackeeTarget.GetDefense();
                damage = Mathf.Max(damage, 0);
                attackeeTarget.PredictOrDealDamage(false, damage, false, captainUsing, captainTargeting);
            }

            if (captainTargeting.myCard is AllyCard allyTarget)
            {
                damage -= allyTarget.GetDefense();
                damage = Mathf.Max(damage, 0);
                allyTarget.PredictOrDealDamage(false, damage, false, captainUsing, captainTargeting);
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
