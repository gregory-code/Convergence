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

    }

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        AttachEquipment(thisPlayingCard, captainUsing, captainTargeting);
    }

    public override void Cleanup()
    {

    }
}
