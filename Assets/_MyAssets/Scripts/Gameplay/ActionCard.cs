using UnityEngine;

public abstract class ActionCard : BaseCard
{
    public bool bAttacking;
    public int Damage;

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
