using System.Collections.Generic;
using UnityEngine;

public abstract class CaptainCard : BaseCard
{
    public int maxHealth;
    private int currentHealth;

    public int maxEquipment;
    private List<PlayingCard> EquipmentsAttached = new List<PlayingCard>();
    private List<PlayingCard> LingersInEffect = new List<PlayingCard>();

    public int GetPhysical()
    {
        int physical = 0;
        foreach (PlayingCard card in EquipmentsAttached)
        {

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

        }
        foreach (PlayingCard card in LingersInEffect)
        {

        }
        return defense;
    }

    public void PredictOrDealDamage(bool bPrediction, int damageDealt, bool bWasMagic, PlayingCard allyDealingDamage, PlayingCard allyRecivingDamage)
    {

    }

    public void PredictOrHealHealth(bool bPrediction, int healthHealed, PlayingCard allyDoingTheHealing, PlayingCard allyBeingHealed)
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
