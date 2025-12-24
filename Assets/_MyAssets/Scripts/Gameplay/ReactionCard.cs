using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReactionCard : BaseCard
{
    public ReactionType reactionType;

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        thisCard = thisPlayingCard;

        yield return new WaitForEndOfFrame();
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

    public override void Cleanup()
    {

    }
}
