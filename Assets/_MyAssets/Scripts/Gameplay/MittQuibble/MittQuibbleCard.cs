using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/MittQuibble/MittQuibble")]
public class MittQuibbleCard : CaptainCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(1);
            FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(thisPlayingCard, 1);
        }

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }

}
