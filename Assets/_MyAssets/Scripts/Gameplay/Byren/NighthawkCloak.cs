using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Byren/NighthawkCloak")]
public class NighthawkCloak : EquipmentCard
{
    private PlayingCard ownerCaptain;

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        StaticGameplayDelegates.onDealtDamage += DealtDamage;
        StaticGameplayDelegates.onTurnStarted += TurnStarted;

        ownerCaptain = captainTargeting[0];

        yield return new WaitForEndOfFrame();
    }

    private void DealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage)
    {
        if (ownerCaptain.uniqueID == allyDealingDamage.uniqueID)
        {
            if (damageDealt >= 3 && bOncePerTurn)
            {
                allyDealingDamage.BeginEnergize();

                bOncePerTurn = false;

                if (allyDealingDamage.DoIOwnThis())
                    FindFirstObjectByType<GameMaster>().RequestDrawCards(1);
            }
        }
    }

    private void TurnStarted(UserPlayer player, bool bPlayers1Turn)
    {
        bOncePerTurn = true;
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onTurnStarted -= TurnStarted;
        StaticGameplayDelegates.onDealtDamage -= DealtDamage;
    }
}
