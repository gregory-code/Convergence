using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public abstract class CaptainCard : BaseCard
{
    public int maxHealth;
    public int currentHealth { get; private set; }

    public int maxEquipment;
    public bool bIsAllyCard;
    private List<PlayingCard> EquipmentsAttached = new List<PlayingCard>();
    private List<PlayingCard> LingersInEffect = new List<PlayingCard>();

    public bool bActivateableAbility;

    public List<PlayingCard> GetEquipments() { return EquipmentsAttached; }
    public List<PlayingCard> GetLingersInEffect() { return LingersInEffect; }

    public int GetSlotsInEffect()
    {
        int slots = 0;
        slots += EquipmentsAttached.Count;
        slots += LingersInEffect.Count;
        return slots;
    }

    public int GetPhysical()
    {
        int physical = 0;
        foreach (PlayingCard card in EquipmentsAttached)
        {
            if(card.myCard is EquipmentCard equipment)
            {
                physical += equipment.physicalStatChange;
            }
        }
        foreach (PlayingCard card in LingersInEffect)
        {
        }
        return physical;
    }

    public int GetMagic()
    {
        int magic = 0;
        foreach (PlayingCard card in EquipmentsAttached)
        {
            if (card.myCard is EquipmentCard equipment)
            {
                magic += equipment.magicStatChange;
            }
        }
        foreach (PlayingCard card in LingersInEffect)
        {

        }
        return magic;
    }

    public int GetDefense()
    {
        int defense = 0;
        foreach (PlayingCard card in EquipmentsAttached)
        {
            if (card.myCard is EquipmentCard equipment)
            {
                defense += equipment.defenseStatChange;
            }
        }
        foreach (PlayingCard card in LingersInEffect)
        {

        }
        return defense;
    }

    public void AttachEquipment(PlayingCard equipment, PlayingCard allyDoingTheEquipping)
    {
        EquipmentsAttached.Add(equipment);
    }

    public void RemoveEquipment(PlayingCard equipment, PlayingCard allyDoingTheUnEquipping)
    {
        EquipmentsAttached.Remove(equipment);
    }

    public void PredictOrDealDamage(bool bPrediction, int damageDealt, bool bWasMagic, PlayingCard thisPlayingCard, PlayingCard allyDealingDamage, bool bTargetingEnemy, PlayingCard allyRecivingDamage)
    {
        currentHealth -= damageDealt;
        allyRecivingDamage.SetHealthText(currentHealth, maxHealth);
        StaticGameplayDelegates.DealtDamage(damageDealt, bWasMagic, thisPlayingCard, allyDealingDamage, allyRecivingDamage);
    }

    public void PredictOrHealHealth(bool bPrediction, int healthHealed, PlayingCard thisPlayingCard, PlayingCard allyDoingTheHealing, PlayingCard allyBeingHealed)
    {

    }

    public override void Init(UserPlayer ownerPlayer)
    {
        currentHealth = maxHealth;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {

    }
}
