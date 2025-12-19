using System.Collections.Generic;
using UnityEngine;

public static class StaticGameplayDelegates
{
    public delegate void OnInspect();
    public static event OnInspect onInspect;
    public static void Inspect() { onInspect?.Invoke(); }

    // ********** Hall of Delegates ********** //
    public delegate void OnTurnStarted();
    public static event OnTurnStarted onTurnStarted;
    public static void TurnStarted() { onTurnStarted?.Invoke(); }

    public delegate void OnTurnEnded();
    public static event OnTurnEnded onTurnEnded;
    public static void TurnEnded() { onTurnEnded?.Invoke(); }

    public delegate void OnKilled(int killingDamage, PlayingCard allyDoingTheKilling, PlayingCard cardThatWasUsed, PlayingCard allyKilled);
    public static event OnKilled onKilled;
    public static void Killed(int killingDamage, PlayingCard allyDoingTheKilling, PlayingCard cardThatWasUsed, PlayingCard allyKilled) { onKilled?.Invoke(killingDamage, allyDoingTheKilling, cardThatWasUsed, allyKilled); }

    public delegate void OnDealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage);
    public static event OnDealtDamage onDealtDamage;
    public static void DealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage) { onDealtDamage?.Invoke(damageDealt, bWasMagic, cardThatWasUsed, allyDealingDamage, allyRecivingDamage); }

    public delegate void OnHealed(int healthHealed, PlayingCard allyDoingTheHealing, PlayingCard cardThatWasUsed, PlayingCard allyBeingHealed);
    public static event OnHealed onHealed;
    public static void Healed(int healthHealed, PlayingCard allyDoingTheHealing, PlayingCard cardThatWasUsed, PlayingCard allyBeingHealed) { onHealed?.Invoke(healthHealed, allyDoingTheHealing, cardThatWasUsed, allyBeingHealed); }

    public delegate void OnEquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment);
    public static event OnEquipmentAttached onEquipmentAttached;
    public static void EquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment) { onEquipmentAttached?.Invoke(equipment,allyDoingTheEquipping, allyGettingTheEquipment); }

    public delegate void OnEquipmentRemoved(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment);
    public static event OnEquipmentRemoved onEquipmentRemoved;
    public static void EquipmentRemoved(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment) { onEquipmentRemoved?.Invoke(equipment, allyRemovingTheEquipment, allyWhoHadTheEquipment); }
    // ********** **************** ********** //

    public static List<PlayingCard> GetAllAllies(bool bGetMyTeam) { return GameObject.FindFirstObjectByType<GameMaster>().GetAllAllies(bGetMyTeam); }
    public static Transform GetDiscardPileTransform(bool isPlayer1) { return GameObject.FindFirstObjectByType<GameMaster>().GetDiscardPilieTransform(isPlayer1); }
    public static void AddCardToDiscard(PlayingCard cardtoAdd) { GameObject.FindFirstObjectByType<GameMaster>().AddCardToDiscard(cardtoAdd); }
}
