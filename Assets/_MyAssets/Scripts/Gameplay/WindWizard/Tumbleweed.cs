using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/WindWizard/Tumbleweed")]
public class Tumbleweed : CaptainCard
{
    private UserPlayer player;
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

        StaticGameplayDelegates.onTurnStarted += TurnStarted;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (thisPlayingCard.DoIOwnThis())
        {
            thisPlayingCard.StartMoveCard(-350, false, 0.3f);
            FindAnyObjectByType<GameMaster>().AddAllyToBoard(thisPlayingCard.myCard, captainUsing);
        }

        yield return new WaitForSeconds(0.2f);

        thisPlayingCard.CleanupDestroy();

        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator ActivateEffect(PlayingCard thisPlayingCard)
    {
        yield return base.ActivateEffect(thisPlayingCard);

        if (thisPlayingCard.DoIOwnThis())
        {
            FindAnyObjectByType<GameMaster>().RequestDrawCards(1);
        }

        FindFirstObjectByType<UserPlayer>().RemoveChioceCard(thisPlayingCard);
    }

    private void TurnStarted(UserPlayer player, bool bPlayers1Turn)
    {
        if (uniqueID == -1)
        {
            return;
        }

        if (thisCard.bIsPlayer1 != bPlayers1Turn || thisCard.DoIOwnThis() == false)
            return;

        this.player = player;

        player.AddToDaybreak(thisCard);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onTurnStarted -= TurnStarted;
    }
}
