using UnityEngine;
using static UserPlayer;

public abstract class AllyCard : BaseCard
{
    public int maxHealth;
    public int currentHealth { get; private set; }

    public int GetPhysical()
    {
        int physical = 0;
        /*foreach (PlayingCard card in LingersInEffect)
        {
        }*/
        return physical;
    }

    public int GetMagic()
    {
        int magic = 0;
        /*foreach (PlayingCard card in LingersInEffect)
        {

        }*/
        return magic;
    }

    public int GetDefense()
    {
        int defense = 0;
        /*foreach (PlayingCard card in LingersInEffect)
        {

        }*/
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
        currentHealth = maxHealth;
    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {

    }

    public override void Cleanup()
    {

    }
}
