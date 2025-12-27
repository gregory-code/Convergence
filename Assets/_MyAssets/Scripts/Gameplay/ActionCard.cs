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
                
        yield return new WaitForEndOfFrame();

        if(bAttackingCard)
            yield return WaitForReaction(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
    }

    public override IEnumerator ActivateEffect(PlayingCard thisPlayingCard)
    {
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator SecondaryPlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
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
