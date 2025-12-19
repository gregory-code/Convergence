using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentCard : BaseCard
{
    public bool bPrestige;
    public EquipmentType equipmentType;

    public int physicalStatChange;
    public int magicStatChange;
    public int defenseStatChange;

    public void AttachEquipment(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment)
    {
        if (allyGettingTheEquipment.myCard is CaptainCard captain)
        {
            equipment.BeginCardAttachment(allyGettingTheEquipment, captain.GetSlotsInEffect());
            captain.AttachEquipment(equipment, allyDoingTheEquipping);
        }

    }

    public void RemoveEquipment(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment)
    {
        if (allyWhoHadTheEquipment.myCard is CaptainCard captain)
        {
            equipment.RemoveCardAttachment(allyWhoHadTheEquipment);
            captain.RemoveEquipment(equipment, allyRemovingTheEquipment);
        }
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
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        AttachEquipment(thisPlayingCard, captainUsing, captainTargeting[0]);
        
        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {

    }
}
