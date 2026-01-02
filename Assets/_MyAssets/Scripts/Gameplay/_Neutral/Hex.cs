using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/Hex")]
public class Hex : ActionCard
{
    private bool bIsPlayer1;
    private PlayingCard attachedCaptain;
    private PlayingCard usingCaptain;

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        StaticGameplayDelegates.onRemoveLingers += RemoveLingers;
        StaticGameplayDelegates.onTurnEnded += TurnEnded;

        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainTargeting[0].myCard is CaptainCard captain)
        {
            bIsPlayer1 = captainUsing.bIsPlayer1;
            bActiveInEffectLinger = true;
            usingCaptain = captainUsing;
            attachedCaptain = captainTargeting[0];

            thisPlayingCard.BeginCardAttachment(captainTargeting[0], captain.GetNextAvailiableSlots());
            captain.AttachLinger(thisPlayingCard, captainUsing);
        }

        yield return new WaitForEndOfFrame();
    }

    private void RemoveLingers(bool bPlayers1Turn)
    {
        if(bActiveInEffectLinger)
        {
            if (bPlayers1Turn == bIsPlayer1)
            {
                if (attachedCaptain.myCard is CaptainCard captain)
                {
                    bActiveInEffectLinger = false;
                    thisCard.RemoveCardAttachment(attachedCaptain);
                    captain.RemoveLinger(thisCard, attachedCaptain);
                }
            }
        }
    }

    private void TurnEnded(bool bPlayers1Turn)
    {
        if(bPlayers1Turn != bIsPlayer1 && bActiveInEffectLinger)
        {
            attachedCaptain.BeginFatigue();
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onRemoveLingers -= RemoveLingers;
        StaticGameplayDelegates.onTurnEnded -= TurnEnded;
    }
}
