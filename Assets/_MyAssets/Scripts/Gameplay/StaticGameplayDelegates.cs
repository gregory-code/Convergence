using System.Collections.Generic;
using UnityEngine;

public static class StaticGameplayDelegates
{
    public delegate void OnInspect();
    public static event OnInspect onInspect;
    public static void Inspect() { onInspect?.Invoke(); }

    // ********** Hall of Delegates ********** //
    public delegate void OnTurnStarted(UserPlayer player, bool bPlayers1Turn);
    public static event OnTurnStarted onTurnStarted;
    public static void TurnStarted(UserPlayer player, bool bPlayers1Turn) { onTurnStarted?.Invoke(player, bPlayers1Turn); }

    public delegate void OnRemoveLingers(bool bPlayers1Turn);
    public static event OnRemoveLingers onRemoveLingers;
    public static void RemoveLingers(bool bPlayers1Turn) { onRemoveLingers?.Invoke(bPlayers1Turn); }

    public delegate void OnTurnEnded(bool bPlayers1Turn);
    public static event OnTurnEnded onTurnEnded;
    public static void TurnEnded(bool bPlayers1Turn) { onTurnEnded?.Invoke(bPlayers1Turn); }

    public delegate void OnKilled(int killingDamage, PlayingCard allyDoingTheKilling, PlayingCard cardThatWasUsed, PlayingCard allyKilled);
    public static event OnKilled onKilled;
    public static void Killed(int killingDamage, PlayingCard cardThatWasUsed, PlayingCard allyDoingTheKilling, PlayingCard allyKilled) { onKilled?.Invoke(killingDamage, cardThatWasUsed, allyDoingTheKilling, allyKilled); }

    public delegate void OnDealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage);
    public static event OnDealtDamage onDealtDamage;
    public static void DealtDamage(int damageDealt, bool bWasMagic, PlayingCard cardThatWasUsed, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage) { onDealtDamage?.Invoke(damageDealt, bWasMagic, cardThatWasUsed, allyDealingDamage, allyRecivingDamage); }

    public delegate void OnHealed(int healthHealed, PlayingCard allyDoingTheHealing, PlayingCard cardThatWasUsed, PlayingCard allyBeingHealed);
    public static event OnHealed onHealed;
    public static void Healed(int healthHealed, PlayingCard cardThatWasUsed, PlayingCard allyDoingTheHealing, PlayingCard allyBeingHealed) { onHealed?.Invoke(healthHealed, allyDoingTheHealing, cardThatWasUsed, allyBeingHealed); }

    public delegate void OnEquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment);
    public static event OnEquipmentAttached onEquipmentAttached;
    public static void EquipmentAttached(PlayingCard equipment, PlayingCard allyDoingTheEquipping, PlayingCard allyGettingTheEquipment) { onEquipmentAttached?.Invoke(equipment,allyDoingTheEquipping, allyGettingTheEquipment); }

    public delegate void OnEquipmentRemoved(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment);
    public static event OnEquipmentRemoved onEquipmentRemoved;
    public static void EquipmentRemoved(PlayingCard equipment, PlayingCard allyRemovingTheEquipment, PlayingCard allyWhoHadTheEquipment) { onEquipmentRemoved?.Invoke(equipment, allyRemovingTheEquipment, allyWhoHadTheEquipment); }
    // ********** **************** ********** //

    public static int GetAllySparkCount() { return GameObject.FindFirstObjectByType<GameMaster>().allySparkValue; }
    public static void RequestAttackPrediction(PlayingCard cardToPlay, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting) { GameObject.FindFirstObjectByType<GameMaster>().RequestPlayCard(cardToPlay, captainUsing, bTargetingEnemy, captainTargeting, true, false); }
    public static List<PlayingCard> GetAllAllies(bool bGetMyTeam, PlayingCard captainForReference) { return GameObject.FindFirstObjectByType<GameMaster>().GetAllAllies(bGetMyTeam, captainForReference); }
    public static Transform GetDiscardPileTransform(bool isPlayer1) { return GameObject.FindFirstObjectByType<GameMaster>().GetDiscardPilieTransform(isPlayer1); }
    public static void AddCardToDiscard(PlayingCard cardtoAdd, bool isPlayer1) { GameObject.FindFirstObjectByType<GameMaster>().AddCardToDiscard(cardtoAdd, isPlayer1); }
    public static Sprite[] GetNumberSpriteWholes() { return GameObject.FindFirstObjectByType<GameMaster>().GetNumberSpriteWholes(); }
}
