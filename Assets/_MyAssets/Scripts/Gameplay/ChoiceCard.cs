using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceCard : MonoBehaviour
{
    [SerializeField] VisibleCard visibleCard;
    [SerializeField] Image sparkAmount;

    private UserPlayer player;
    private PlayingCard myPlayingCard;
    private BaseCard myBaseCard;

    [HideInInspector]
    public bool bINeedMore;
    [HideInInspector]
    public bool bDestinyStopwatch;

    [HideInInspector]
    public PlayingCard usingCaptain;

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
        if (bINeedMore)
        {
            INeedMoreEffect();
            return;
        }

        if (bDestinyStopwatch)
        {
            DestinyStopwatchEffect();
            return;
        }

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

    private void INeedMoreEffect()
    {
        List<PlayingCard> targets = new List<PlayingCard>();
        targets.Add(usingCaptain);

        myPlayingCard.myCard.bSwift = true;
        player.RequestPlayCard(myPlayingCard, usingCaptain, false, targets, true);

        player.FinishUniqueChoice();
    }

    private void DestinyStopwatchEffect()
    {

    }
}
