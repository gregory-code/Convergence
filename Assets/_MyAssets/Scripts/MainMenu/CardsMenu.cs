using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] FirebasePlayerInfo FirebasePlayer;
    [SerializeField] TMP_InputField[] DeckNameInputs;
    [SerializeField] private int NumOfDecks = 3;

    [SerializeField] private CanvasGroup SelectDeckCanvasGroup;
    [SerializeField] private CanvasGroup EditDeckCanvasGroup;

    [SerializeField] private VisibleCard VisibleCardPreview;
    [SerializeField] private BaseCard[] CaptainLibrary;
    [SerializeField] private BaseCard[] CardLibrary;

    [SerializeField] private Transform CardLibraryTransform;

    [SerializeField] private LibraryCardPreview CardPreviewPrefab;
    private List<LibraryCardPreview> CardPreviewLibrary = new List<LibraryCardPreview>();

    private int currentDeckIndex;
    private List<BaseCard> currentDeck = new List<BaseCard>();

    private void SetMenuCanvasGroups(bool showSelectDeckMenu)
    {
        SelectDeckCanvasGroup.alpha = (showSelectDeckMenu) ? 1 : 0;
        SelectDeckCanvasGroup.interactable = showSelectDeckMenu;
        SelectDeckCanvasGroup.blocksRaycasts = showSelectDeckMenu;

        EditDeckCanvasGroup.alpha = (showSelectDeckMenu) ? 0 : 1;
        EditDeckCanvasGroup.interactable = !showSelectDeckMenu;
        EditDeckCanvasGroup.blocksRaycasts = !showSelectDeckMenu;
    }

    public void EditDeck(int index)
    {
        currentDeckIndex = index;
        SetMenuCanvasGroups(false);
        UpdateCardLibrary();
    }
    public void OnEndEditDeckIndex(int index)
    {
        currentDeckIndex = index;
    }

    public void OnEndEditDeckName(string deckName)
    {
        StartCoroutine(FirebasePlayer.UpdateObject("DeckName" + currentDeckIndex, deckName)); // This will be decks 0 - 2 on firebase
        //currentTeam.UpdateTeamName(deckName);
    }

    private void UpdateCardLibrary()
    {
        foreach(LibraryCardPreview cardPreview in CardPreviewLibrary)
        {
            Destroy(cardPreview.gameObject);
        }
        CardPreviewLibrary.Clear();

        AddCardsToLibrary(CaptainLibrary);
        AddCardsToLibrary(CardLibrary);
    }

    private void AddCardsToLibrary(BaseCard[] cardGroup)
    {
        foreach (BaseCard card in cardGroup)
        {
            LibraryCardPreview newCaptain = Instantiate(CardPreviewPrefab, CardLibraryTransform);
            newCaptain.Init(this, card);
            CardPreviewLibrary.Add(newCaptain);
        }
    }

    public void ShowCardEffect(BaseCard cardToShow)
    {
        VisibleCardPreview.SetCard(cardToShow);
    }

    public IEnumerator LoadData(DataSnapshot data)
    {

        /*for (int i = 0; i < data.Child("Deck" + teamSelectIndex).ChildrenCount; ++i)
        {
            GetMonsterPref(i).DeseralizePref(data.Child("team" + teamSelectIndex).Child("" + i).Value.ToString());
        }*/

        for (int i = 0; i < NumOfDecks; ++i)
        {
            if (data.Child("DeckName" + i).Exists)
            {
                DeckNameInputs[i].text = data.Child("DeckName" + i).Value.ToString();
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }
}
