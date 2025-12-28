using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/WindWizard/WindWizard")]
public class WindWizard : CaptainCard
{
    UserPlayer player;
    public int sparkAmount { get; private set; }

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

        StaticGameplayDelegates.onTurnStarted += TurnStarted;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator ActivateEffect(PlayingCard thisPlayingCard)
    {
        yield return base.ActivateEffect(thisPlayingCard);

        int sparkGain = 0;

        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetTeammates(thisCard);
        foreach (PlayingCard card in allyTeam)
        {
            if (card.myCard is CaptainCard captain)
            {
                if(captain.bIsAllyCard && captain.CaptainWhoPlayedMe != null)
                {
                    if (card.DoIOwnThis() && captain.CaptainWhoPlayedMe.myCard == this)
                    {
                        sparkGain++;
                    }
                }
            }
        }

        player.RemoveChioceCard(thisPlayingCard);

        if (sparkGain <= 0)
            yield break;

        FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(thisPlayingCard, sparkGain);
    }

    private void TurnStarted(UserPlayer player, bool bPlayers1Turn)
    {
        int sparkGain = 0;

        if (thisCard == null)
            return;

        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetTeammates(thisCard);

        foreach (PlayingCard card in allyTeam)
        {
            if (card.myCard is CaptainCard captain)
            {
                if (captain.bIsAllyCard && captain.CaptainWhoPlayedMe != null)
                {
                    if (card.DoIOwnThis() && captain.CaptainWhoPlayedMe.myCard == this)
                    {
                        sparkGain++;
                    }
                }
            }
        }

        if (thisCard.bIsPlayer1 != bPlayers1Turn || thisCard.DoIOwnThis() == false || bDead)
            return;

        sparkAmount = sparkGain;

        this.player = player;

        if (sparkAmount > 0)
        {
            player.AddToDaybreak(thisCard);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onTurnStarted -= TurnStarted;
    }
}
