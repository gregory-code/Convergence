using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Byren/DestinyStopwatch")]
public class DestinyStopwatch : ReactionCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        FindFirstObjectByType<UserPlayer>().bInUniqueMenu = true;

        List<PlayingCard> newTarget = new List<PlayingCard>();
        newTarget.Add(captainUsing);

        if (thisPlayingCard.DoIOwnThis())
        {
            FindFirstObjectByType<UserPlayer>().DoUniqueChoice(thisPlayingCard, captainUsing);
        }

        thisPlayingCard.BeginWaitForMenuAndDiscard(captainUsing);

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
