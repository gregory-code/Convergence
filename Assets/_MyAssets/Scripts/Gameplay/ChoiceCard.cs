using UnityEngine;
using UnityEngine.UI;

public class ChoiceCard : MonoBehaviour
{
    [SerializeField] VisibleCard visibleCard;
    [SerializeField] Image sparkAmount;

    private UserPlayer player;
    private PlayingCard myPlayingCard;

    public void Init(UserPlayer player, PlayingCard myPlayingCard)
    {
        this.player = player;
        this.myPlayingCard = myPlayingCard;
        visibleCard.SetCard(myPlayingCard.myCard);

        if (myPlayingCard.bInDiscard)
            return;

        if(myPlayingCard.myCard is PrinceCard prince)
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

    public void SelectedThisEffect()
    {
        if (myPlayingCard.bInDiscard)
            return;

        player.StopAndWaitChoosingSomething();

        StartCoroutine(myPlayingCard.myCard.ActivateEffect(myPlayingCard));

        transform.SetParent(null); // this is awful, absolutely diabolical choice -GR
        transform.position = Vector3.zero;
    }
}
