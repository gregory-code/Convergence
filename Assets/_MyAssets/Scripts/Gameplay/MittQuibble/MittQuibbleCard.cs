using UnityEngine;

[CreateAssetMenu(menuName = "TCG/MittQuibble/MittQuibble")]
public class MittQuibbleCard : CaptainCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(1);
            FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(1);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }

}
