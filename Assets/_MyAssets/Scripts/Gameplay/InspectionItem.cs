using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionItem : MonoBehaviour
{
    [SerializeField] private Button ClickableButton;
    [SerializeField] private Image Icon;
    [SerializeField] private TextMeshProUGUI Description;

    private PlayingCard playingCard;
    private UserPlayer ownerPlayer;

    public void Init(UserPlayer ownerPlayer, PlayingCard card, bool DoIOwnThis)
    {
        this.ownerPlayer = ownerPlayer;
        playingCard = card;

        Description.text = card.myCard.DescriptionText;
        Icon.sprite = card.myCard.CardArt;

        ClickableButton.interactable = false;

        if (card.myCard is CaptainCard captain)
        {
            if(card.bEnergized == true && ownerPlayer.IsMyTurn() == true && card.myCard.bOncePerTurn == true && DoIOwnThis && card.myCard.bDead == false)
            {
                ClickableButton.interactable = captain.bActivateableAbility;
                ClickableButton.enabled = captain.bActivateableAbility;
            }
        }
    }

    public void Activate()
    {
        if(playingCard != null && ownerPlayer != null)
        {
            ownerPlayer.StopInspecting();

            List<PlayingCard> playingCards = new List<PlayingCard>();
            playingCards.Add(playingCard);

            ownerPlayer.RequestPlayCard(playingCard, playingCard, false, playingCards);
        }
    }
}
