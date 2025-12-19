using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventionCard : BaseCard
{
    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {

    }
}
