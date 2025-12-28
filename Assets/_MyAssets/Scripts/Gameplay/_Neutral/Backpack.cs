using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/Backpack")]
public class Backpack : EquipmentCard
{
    private PlayingCard ownerCaptain;
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        StaticGameplayDelegates.onEquipmentAttached += EquipmentAttached;

        ownerCaptain = captainTargeting[0];

        if (thisCard.DoIOwnThis())
            FindFirstObjectByType<GameMaster>().RequestDrawCards(1);

        yield return new WaitForEndOfFrame();
    }

    private void EquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment)
    {
        List<PlayingCard> allies = StaticGameplayDelegates.GetTeammates(ownerCaptain);

        foreach(PlayingCard card in allies)
        {
            if(card.uniqueID == allyGettingTheEquipment.uniqueID)
                if (allyGettingTheEquipment.DoIOwnThis())
                    FindFirstObjectByType<GameMaster>().RequestDrawCards(1);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();

        StaticGameplayDelegates.onEquipmentAttached -= EquipmentAttached;
    }
}
