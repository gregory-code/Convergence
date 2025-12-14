using UnityEngine;

public abstract class ReactionCard : BaseCard
{
    public ReactionType reactionType;

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {

    }

    public override void Cleanup()
    {

    }
}
