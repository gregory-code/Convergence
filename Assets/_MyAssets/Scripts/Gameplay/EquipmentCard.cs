using UnityEngine;

public abstract class EquipmentCard : BaseCard
{
    public bool bPrestige;
    public EquipmentType equipmentType;

    public void AttachEquipment(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment)
    {

    }

    public void RemoveEquipment(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment)
    {

    }

    public override void Init(UserPlayer ownerPlayer)
    {

    }

    public override void PlayCard(PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {

    }

    public override void Cleanup()
    {

    }
}
