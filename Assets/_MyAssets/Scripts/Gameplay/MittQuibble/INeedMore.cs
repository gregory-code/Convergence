using UnityEngine;

[CreateAssetMenu(menuName = "TCG/MittQuibble/INeedMore")]
public class INeedMore : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);


    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
