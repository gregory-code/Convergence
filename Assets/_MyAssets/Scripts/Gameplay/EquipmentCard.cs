using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentCard : BaseCard
{
    public bool bPrestige;
    public EquipmentType equipmentType;

    public int bonusHealthStatChange;
    public int physicalStatChange;
    public int magicStatChange;
    public int defenseStatChange;

    public void AttachEquipment(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment)
    {
        if (allyGettingTheEquipment.myCard is CaptainCard captain)
        {
            equipment.BeginCardAttachment(allyGettingTheEquipment, captain.GetNextAvailiableSlots());
            captain.AttachEquipment(equipment, allyDoingTheEquipping);
        }

        StaticGameplayDelegates.EquipmentAttached(equipment, allyDoingTheEquipping, allyGettingTheEquipment);
    }

    public void RemoveEquipment(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment)
    {
        if (allyWhoHadTheEquipment.myCard is CaptainCard captain)
        {
            equipment.RemoveCardAttachment(allyWhoHadTheEquipment);
            captain.RemoveEquipment(equipment, allyRemovingTheEquipment);
        }

        StaticGameplayDelegates.EquipmentRemoved(equipment, allyRemovingTheEquipment, allyWhoHadTheEquipment);
    }

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        if (bPrestige)
        {
            if (captainTargeting[0].myCard is CaptainCard capatain)
            {
                foreach (PlayingCard equipmentPlayingCard in capatain.GetEquipments())
                {
                    if (equipmentPlayingCard.myCard is EquipmentCard equipmentAttached)
                    {
                        if (equipmentType == equipmentAttached.equipmentType)
                        {
                            equipmentAttached.RemoveEquipment(equipmentPlayingCard, captainUsing, captainUsing);
                            bSwift = true;
                            captainTargeting[0].BeginEnergize();
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.4f);

        AttachEquipment(thisPlayingCard, captainUsing, captainTargeting[0]);
        
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator ActivateEffect(PlayingCard thisPlayingCard)
    {
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator SecondaryPlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return new WaitForEndOfFrame();
    }

    public override CardPlayContext PredictCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        CardPlayContext context = new CardPlayContext
        {
            thisPlayingCard = thisPlayingCard,
            captainUsing = captainUsing,
            bTargetingEnemy = bTargetingEnemy,
            captainTargeting = captainTargeting,
            damage = 0,
            bMagicDamage = false
        };

        return context;
    }

    public override void Cleanup()
    {

    }
}
