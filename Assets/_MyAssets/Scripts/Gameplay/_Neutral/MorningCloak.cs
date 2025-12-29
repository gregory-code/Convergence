using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Neutral/MorningCloak")]
public class MorningCloak : EquipmentCard
{
    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    private PlayingCard equippedPlayer;

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        equippedPlayer = captainTargeting[0];

        if (captainTargeting[0].myCard is CaptainCard usingMorningCloak)
        {
            usingMorningCloak.currentHealth += 3;
            captainTargeting[0].SetHealthText();
        }

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

        if (equippedPlayer == null)
            return;

        if (equippedPlayer.myCard is CaptainCard equippedCharacter)
        {
            equippedCharacter.currentHealth -= 3;
            equippedPlayer.SetHealthText();

            if(equippedCharacter.currentHealth <= 0)
            {
                equippedPlayer.Die();
                StaticGameplayDelegates.Killed(0, thisCard, equippedPlayer, equippedPlayer);
            }

        }
    }
}
