using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/Guard")]
public class Guard : ReactionCard
{
    private bool bIsPlayer1;
    private PlayingCard attachedCaptain;

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        StaticGameplayDelegates.onRemoveLingers += RemoveLingers;

        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainTargeting[0].myCard is CaptainCard captain)
        {
            bIsPlayer1 = captainUsing.bIsPlayer1;
            bActiveInEffectLinger = true;
            attachedCaptain = captainTargeting[0];

            thisPlayingCard.BeginCardAttachment(captainTargeting[0], captain.GetNextAvailiableSlots());
            captain.AttachLinger(thisPlayingCard, captainUsing);
        }

        yield return new WaitForEndOfFrame();
    }

    private void RemoveLingers(bool bPlayers1Turn)
    {
        if (bPlayers1Turn == bIsPlayer1 && bActiveInEffectLinger)
        {
            if (attachedCaptain.myCard is CaptainCard captain)
            {
                bActiveInEffectLinger = false;
                thisCard.RemoveCardAttachment(attachedCaptain);
                captain.RemoveLinger(thisCard, attachedCaptain);
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onRemoveLingers -= RemoveLingers;
    }
}
