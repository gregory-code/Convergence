using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Prince/Prince")]
public class PrinceCard : CaptainCard
{
    private PlayingCard thisPrince;

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

    private void TurnStarted(bool bPlayers1Turn)
    {
        List<PlayingCard> allyTeam = StaticGameplayDelegates.GetAllAllies(true); // this fails sadly
        foreach (PlayingCard card in allyTeam)
        {
            if (card.myCard is PrinceCard captain)
            {
                if(card.myCard == this)
                    thisPrince = card;
            }
        }

        if(thisPrince == null)
        {
            return;
        }

        if (thisPrince.bIsPlayer1 != bPlayers1Turn)
            return;

        int sparkGain = 0;

        foreach(PlayingCard card in allyTeam)
        {
            if(card.myCard is CaptainCard captain)
            {
                if (captain.currentHealth >= captain.maxHealth && card.DoIOwnThis() && captain != this)
                    sparkGain++;
            }
        }

        if (sparkGain <= 0 && thisPrince != null)
            return;

        FindAnyObjectByType<GameMaster>().RequestIncreaseSpark(thisPrince, sparkGain);
    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
