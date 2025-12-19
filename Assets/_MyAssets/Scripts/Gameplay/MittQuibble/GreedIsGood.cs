using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/MittQuibble/GreedIsGood")]
public class GreedIsGood : ActionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        thisPlayingCard.BeginPlayAndDiscard(captainUsing);

        if(thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(3);
        }

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
