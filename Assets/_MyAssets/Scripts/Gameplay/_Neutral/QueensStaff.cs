using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/QueensStaff")]
public class QueensStaff : EquipmentCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override void PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, PlayingCard captainTargeting)
    {
        base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

    }

    public override void Cleanup()
    {
        base.Cleanup();

    }
}
