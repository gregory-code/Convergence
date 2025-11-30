
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] private VisualDeck visualDeck;
    [SerializeField] private UserPlayer player;

    [SerializeField] private Vector3 hiddenHand;
    [SerializeField] private Vector3 hoverHand;

    private bool bBlockHand = false;
    private bool bInMulligan = false;


    [SerializeField] private PlayingCard playingCardPrefab;
    private List<PlayingCard> PlayingCardsInHand = new List<PlayingCard>();
    private List<PlayingCard> CardsToMulligan = new List<PlayingCard>();

    public void AddCard(BaseCard newCardToadd)
    {
        bBlockHand = true;
        horizontalLayoutGroup.enabled = true;

        PlayingCard newPlayingCard = Instantiate(playingCardPrefab, this.transform);
        newPlayingCard.Init(this, newCardToadd);
        newPlayingCard.transform.localPosition = new Vector3(0, -190, 0);
        PlayingCardsInHand.Add(newPlayingCard);
        StartCoroutine(newPlayingCard.MoveCardUpToPlace());

        StartCoroutine(ReenableHand());
    }

    private IEnumerator ReenableHand()
    {
        yield return new WaitForSeconds(1.0f);
        bBlockHand = false;
        horizontalLayoutGroup.enabled = false;
    }

    private IEnumerator MoveHand(Vector3 handPos)
    {
        float duration = 0.8f;
        while(duration > 0)
        {
            duration -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, handPos, Time.deltaTime * 15.0f);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = handPos;
    }

    public void AddOrRemoveCardToMulligan(PlayingCard card, bool bMullgianThis)
    {
        if(bMullgianThis)
        {
            CardsToMulligan.Add(card);
        }
        else if(bMullgianThis == false && CardsToMulligan.Contains(card))
        {
            CardsToMulligan.Remove(card);
        }
    }

    public void BlockHand(bool bState)
    {
        bBlockHand = bState;
        if(bState)
        {
            StopAllCoroutines();
            StartCoroutine(MoveHand(hiddenHand));
        }
    }

    public bool GetBlockHand()
    {
        return bBlockHand;
    }

    public int ConfirmMulligan()
    {
        bBlockHand = true;
        horizontalLayoutGroup.enabled = true;
        SetMulligan(false);
        StartCoroutine(MullgianWrapUp());
        return CardsToMulligan.Count;
    }

    private IEnumerator MullgianWrapUp()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < CardsToMulligan.Count; i++)
        {
            Destroy(CardsToMulligan[i].gameObject);
            PlayingCardsInHand.Remove(CardsToMulligan[i]);
            player.AddCard(CardsToMulligan[i].GetCard());
            yield return new WaitForSeconds(0.3f);
        }

        StartCoroutine(ReenableHand());
        CardsToMulligan.Clear();

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(visualDeck.ShuffleAnimation());
    }

    public void SetMulligan(bool bState)
    {
        bInMulligan = bState;
    }

    public bool GetMulligan()
    {
        return bInMulligan;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bBlockHand)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHand(hoverHand));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bBlockHand)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHand(hiddenHand));
    }
}
