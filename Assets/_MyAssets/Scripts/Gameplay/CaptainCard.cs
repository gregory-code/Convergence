using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public int GetBonusHealth()
    {
        int health = 0;
        foreach (PlayingCard card in EquipmentsAttached)
        {
            if (card.myCard is EquipmentCard equipment)
            {
                health += equipment.bonusHealthStatChange;
            }
        }
        return health;
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
            physical += card.myCard.physicalStatLinger;
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
            magic += card.myCard.magicStatLinger;
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
            defense += card.myCard.defenseStatLinger;
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

    public void AttachLinger(PlayingCard linger, PlayingCard allyDoingTheEquipping)
    {
        LingersInEffect.Add(linger);
    }

    public void RemoveLinger(PlayingCard linger, PlayingCard allyDoingTheUnEquipping)
    {
        LingersInEffect.Remove(linger);
    }

    public void TakeDamage(int damageDealt, bool bWasMagic, PlayingCard usedCard, PlayingCard allyDealingDamage, bool bTargetingEnemy, PlayingCard allyRecivingDamage)
    {
        currentHealth -= damageDealt;

        while(currentHealth < 0)
        {
            currentHealth++;
            damageDealt--;
        }

        allyRecivingDamage.SetHealthText(currentHealth, maxHealth);
        StaticGameplayDelegates.DealtDamage(damageDealt, bWasMagic, usedCard, allyDealingDamage, allyRecivingDamage);
        
        if(currentHealth <= 0)
        {
            allyRecivingDamage.Die();
            StaticGameplayDelegates.Killed(damageDealt, usedCard, allyDealingDamage, allyRecivingDamage);
        }
    }

    public void HealHealth(bool bPrediction, int healthHealed, PlayingCard thisPlayingCard, PlayingCard allyDoingTheHealing, PlayingCard allyBeingHealed)
    {

    }

    public void SetToFullHealth()
    {
        currentHealth = maxHealth + GetBonusHealth();
    }

    public override void Init(UserPlayer ownerPlayer)
    {
        currentHealth = maxHealth;
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
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
