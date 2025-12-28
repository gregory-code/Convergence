using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/WindWizard/Tornado")]
public class Tornado : CaptainCard
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

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = base.PredictCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainUsing.myCard is CaptainCard captainUsingTheAttack)
        {
            context.damage = CalculateAttackDamage(1, false, false, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
        }

        return context;
    }

    public override IEnumerator ActivateEffect(PlayingCard thisPlayingCard)
    {
        yield return base.ActivateEffect(thisPlayingCard);

        if (thisPlayingCard.DoIOwnThis())
        {
            List<PlayingCard> enemies = StaticGameplayDelegates.GetEnemies(CaptainWhoPlayedMe);

            GameMaster gameMaster = FindFirstObjectByType<GameMaster>();
            gameMaster.reactionPlayingCard = thisPlayingCard;
            gameMaster.reactionCaptainUsing = thisPlayingCard;
            gameMaster.reactionCaptainTargeting = enemies;

            gameMaster.RequestPlayCard(thisCard, thisCard, true, enemies, false, true, false);
        }
    }

    public override IEnumerator SecondaryPlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.SecondaryPlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        yield return WaitForReaction(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        for (int i = 0; i < captainTargeting.Count; i++)
        {
            if (captainTargeting[i].myCard is CaptainCard attackeeTarget)
            {
                int damage = CalculateAttackDamage(1, false, false, thisCard, thisCard, true, captainTargeting[i]);
                attackeeTarget.TakeDamage(damage, false, thisCard, thisCard, true, captainTargeting[i]);
            }
        }

       FindFirstObjectByType<UserPlayer>().RemoveChioceCard(thisPlayingCard);
    }

    private void TurnStarted(UserPlayer player, bool bPlayers1Turn)
    {
        if(thisCard.uniqueID == -1)
            return;

        if (CaptainWhoPlayedMe == null)
            return;

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
