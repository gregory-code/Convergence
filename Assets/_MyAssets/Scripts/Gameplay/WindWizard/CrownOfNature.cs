using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/WindWizard/CrownOfNature")]
public class CrownOfNature : EquipmentCard
{
    private UserPlayer player;

    public PlayingCard crownsOwner {  get; private set; }

    public override void Init(UserPlayer ownerPlayer)
    {
        base.Init(ownerPlayer);

    }

    public override IEnumerator PlayCard(PlayingCard thisPlayingCard, PlayingCard captainUsing, bool bTargetingEnemy, List<PlayingCard> captainTargeting)
    {
        yield return base.PlayCard(thisPlayingCard, captainUsing, bTargetingEnemy, captainTargeting);

        List<PlayingCard> allies = StaticGameplayDelegates.GetAllAllies(true, captainUsing);
        if (thisPlayingCard.DoIOwnThis())
        {
            FindFirstObjectByType<UserPlayer>().DoUniqueChoice(thisPlayingCard);
        }

        crownsOwner = captainTargeting[0];

        foreach(PlayingCard ally in allies)
        {
            if(ally.myCard is CaptainCard captainCard)
            {
                if(captainCard.bIsAllyCard)
                {
                    if(captainCard.CaptainWhoPlayedMe == captainTargeting[0])
                    {
                        captainCard.currentHealth += 1;
                        ally.SetHealthText(captainCard.currentHealth, captainCard.maxHealth);
                    }
                }
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public override void Cleanup()
    {
        base.Cleanup();

        List<PlayingCard> allies = StaticGameplayDelegates.GetAllAllies(true, thisCard);
        for(int i = 0; i < allies.Count; i++)
        {
            if (allies[i].myCard is CaptainCard captainCard)
            {
                if (captainCard.bIsAllyCard && captainCard.CaptainWhoPlayedMe == crownsOwner)
                {
                    captainCard.currentHealth -= 1;
                    allies[i].SetHealthText(captainCard.currentHealth, captainCard.maxHealth);

                    if(captainCard.currentHealth <= 0)
                    {
                        i--;
                        captainCard.AllyDeath();
                    }
                }
            }
        }
    }
}
