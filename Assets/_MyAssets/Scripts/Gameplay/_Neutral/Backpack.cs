using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/Backpack")]
public class Backpack : EquipmentCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        StaticGameplayDelegates.onEquipmentAttached += EquipmentAttached;

        if (thisCard.DoIOwnThis())
            FindFirstObjectByType<GameMaster>().RequestDrawCards(1);

        yield return new WaitForEndOfFrame();
    }

    private void EquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment)
    {
        if (allyDoingTheEquipping.DoIOwnThis())
            FindFirstObjectByType<GameMaster>().RequestDrawCards(1);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onEquipmentAttached -= EquipmentAttached;
    }
}
