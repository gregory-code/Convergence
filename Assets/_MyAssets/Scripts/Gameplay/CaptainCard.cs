using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CaptainCard : BaseCard
{
    public int maxHealth;

    [HideInInspector]
    public int currentHealth;

    public int maxEquipment;
    public bool bIsAllyCard;

    [HideInInspector]
    public PlayingCard CaptainWhoPlayedMe;

    private List<PlayingCard> EquipmentsAttached = new List<PlayingCard>();
    private List<PlayingCard> LingersInEffect = new List<PlayingCard>();

    private const int maxSlots = 6;
    Dictionary<int, PlayingCard> slots = new Dictionary<int, PlayingCard>(maxSlots);



    public bool bActivateableAbility;

    public List<PlayingCard> GetEquipments() { return EquipmentsAttached; }
    public List<PlayingCard> GetLingersInEffect() { return LingersInEffect; }

    public int GetNextAvailiableSlots()
    {
        for(int i = 0; i < maxSlots; i++)
        {
            if(slots.ContainsKey(i) == false)
                return i;
        }
        return 0;
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
        health += GetTeammateBonusHealthBuffs();
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
        physical += GetTeammatePhysicalBuffs();
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
        for (int i = 0; i < maxSlots; i++)
        {
            if (slots.ContainsKey(i) == false)
            {
                slots[i] = equipment;
                return;
            }
        }
    }

    public void RemoveEquipment(PlayingCard equipment, PlayingCard allyDoingTheUnEquipping)
    {
        EquipmentsAttached.Remove(equipment);
        for (int i = 0; i < maxSlots; i++)
        {
            if (slots.TryGetValue(i, out PlayingCard equip) && equip == equipment)
            {
                slots.Remove(i);
                return;
            }
        }
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
        allyRecivingDamage.DisplayHitDamageVFX(damageDealt, bWasMagic);
        StaticGameplayDelegates.DealtDamage(damageDealt, bWasMagic, usedCard, allyDealingDamage, allyRecivingDamage);
        
        if(currentHealth <= 0)
        {
            if(bIsAllyCard)
            {
                AllyDeath();
            }
            else
            {
                allyRecivingDamage.Die();
                StaticGameplayDelegates.Killed(damageDealt, usedCard, allyDealingDamage, allyRecivingDamage);
            }
        }
    }

    public void AllyDeath()
    {
        thisCard.BeginEnergize();
        thisCard.BeginPlayAndDiscard(thisCard);

        if (thisCard.DoIOwnThis())
        {
            FindFirstObjectByType<GameMaster>().RequestRemoveAllyFromBoard(thisCard);
        }

        bDead = true;
        Cleanup();
    }

    public void HealHealth(int healthHealed, bool bWasMagic, PlayingCard usedCard, PlayingCard allyDoingTheHealing, bool bTargetingEnemy, PlayingCard allyBeingHealed)
    {
        currentHealth += healthHealed;

        while (currentHealth > maxHealth + GetBonusHealth())
        {
            currentHealth--;
            healthHealed--;
        }

        allyBeingHealed.SetHealthText(currentHealth, maxHealth);
        allyBeingHealed.DisplayHealVFX(healthHealed);
        StaticGameplayDelegates.Healed(healthHealed, usedCard, allyDoingTheHealing, allyBeingHealed);
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
        if (thisCard == null)
            return;

        SetToFullHealth();
    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
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
