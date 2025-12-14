using UnityEngine;

[CreateAssetMenu(menuName = "TCG/MittQuibble/GreedIsGood")]
public class GreedIsGood : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        if(thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(3);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
