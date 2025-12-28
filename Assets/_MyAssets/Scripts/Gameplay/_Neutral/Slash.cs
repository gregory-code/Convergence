using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/Slash")]
public class Slash : ActionCard
{
    private PlayingCard attachedCaptain;

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        StaticGameplayDelegates.onDealtDamage += DamageWasDealt;
        StaticGameplayDelegates.onRemoveLingers += RemoveLingers;

        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (CaptainTargeting[0].myCard is CaptainCard attackeeTarget)
        {
            int damage = CalculateAttackDamage(3, false, false, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
            attackeeTarget.TakeDamage(damage, false, thisPlayingCard, captainUsing, bTargetingEnemy, CaptainTargeting[0]);
        }

        yield return new WaitForEndOfFrame();
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = base.PredictCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        if (captainUsing.myCard is CaptainCard captainUsingTheAttack)
        {
            context.damage = CalculateAttackDamage(3, false, false, thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);
            context.bMagicDamage = ConvertToMagic(false, thisPlayingCard, captainUsingTheAttack);
        }

        return context;
    }

    public void DamageWasDealt(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage)
    {
        if(cardThatWasUsed.uniqueID == thisCard.uniqueID && damageDealt > 0)
        {
            // Apply linger
            if (allyDealingDamage.myCard is CaptainCard captain)
            {
                bActiveInEffectLinger = true;
                attachedCaptain = allyDealingDamage;

                cardThatWasUsed.BeginCardAttachment(allyDealingDamage, captain.GetNextAvailiableSlots());
                captain.AttachLinger(cardThatWasUsed, allyDealingDamage);

                return;
            }
        }
        else
        {
            cardThatWasUsed.BeginPlayAndDiscard(allyDealingDamage);
        }

    }

    private void RemoveLingers(bool bPlayers1Turn)
    {
        if(bPlayers1Turn == attachedCaptain.bIsPlayer1 && bActiveInEffectLinger)
        {
            if(attachedCaptain.myCard is CaptainCard captain)
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

        StaticGameplayDelegates.onDealtDamage -= DamageWasDealt;
        StaticGameplayDelegates.onRemoveLingers -= RemoveLingers;
    }
}
