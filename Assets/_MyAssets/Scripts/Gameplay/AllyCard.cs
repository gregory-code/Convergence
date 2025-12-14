using UnityEngine;

public abstract class AllyCard : BaseCard
{
    public int maxHealth;
    private int currentHealth;

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
