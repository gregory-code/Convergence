using UnityEngine;
using UnityEngine.UI;

public class ChoiceCard : MonoBehaviour
{
    [SerializeField] VisibleCard visibleCard;
    [SerializeField] Image sparkAmount;

    private UserPlayer player;
    private PlayingCard myPlayingCard;
    private BaseCard myBaseCard;

    public void Init(UserPlayer player, PlayingCard myPlayingCard, BaseCard cardEffect)
    {
        this.player = player;
        if(myPlayingCard == null) // A deck ability
        {
            myBaseCard = cardEffect;
            visibleCard.SetCard(cardEffect);
        }
        else // the card exsists
        {
            this.myPlayingCard = myPlayingCard;
            visibleCard.SetCard(myPlayingCard.myCard);

            if (myPlayingCard.bInDiscard)
                return;

            if (myPlayingCard.myCard is PrinceCard prince)
            {
                sparkAmount.gameObject.SetActive(true);
                sparkAmount.sprite = StaticGameplayDelegates.GetNumberSpriteWholes()[prince.sparkAmount];
            }

            if (myPlayingCard.myCard is WindWizard windy)
            {
                sparkAmount.gameObject.SetActive(true);
                sparkAmount.sprite = StaticGameplayDelegates.GetNumberSpriteWholes()[windy.sparkAmount];
            }
        }
    }

    public void SelectedThisEffect()
    {
        if (myPlayingCard == null)
        {
            DeckEffect();
            return;
        }

        if (myPlayingCard.bInDiscard)
            return;

        player.StopAndWaitChoosingSomething();

        StartCoroutine(myPlayingCard.myCard.ActivateEffect(myPlayingCard));

        transform.SetParent(null); // this is awful, absolutely diabolical choice -GR
        transform.position = Vector3.zero;
    }

    private void DeckEffect()
    {
        if(myBaseCard.Type.type == CardType.Ally)
        {
            StartCoroutine(player.DrawCardFromDeck(myBaseCard));
            player.FinishUniqueChoice();
        }
    }
}
