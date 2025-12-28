using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] BattleMenu battleMenu;
    [SerializeField] FirebasePlayerInfo FirebasePlayer;
    [SerializeField] TMP_InputField[] DeckNameInputs;
    [SerializeField] private int NumOfDecks = 3;

    [SerializeField] private CanvasGroup SelectDeckCanvasGroup;
    [SerializeField] private CanvasGroup EditDeckCanvasGroup;

    [SerializeField] private Image CaptainToggle;
    [SerializeField] private Image ActionToggle;
    [SerializeField] private Image ReactionToggle;
    [SerializeField] private Image EquipmentToggle;
    [SerializeField] private Image AllyToggle;

    [SerializeField] private Sprite checkedSprite;
    [SerializeField] private Sprite unCheckedSprite;

    [SerializeField] private VisibleCard VisibleCardPreview;
    [SerializeField] private List<BaseCard> CaptainLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CaptainCardsLibrary = new List<BaseCard>();
    [SerializeField] private List<BaseCard> CardLibrary = new List<BaseCard>();

    [SerializeField] private Transform CardLibraryTransform;

    [SerializeField] private LibraryCardPreview CardPreviewPrefab;
    private List<LibraryCardPreview> CardPreviewLibrary = new List<LibraryCardPreview>();

    [SerializeField] private Transform CardBarTransform;

    [SerializeField] private BarCardPreview BarCardPreviewPrefab;
    private List<BarCardPreview> CardPreviewBar = new List<BarCardPreview>();

    [SerializeField] TextMeshProUGUI CardsInDeckText;
    [SerializeField] TextMeshProUGUI CaptainsInDeckText;

    private int currentDeckIndex;
    private int cardsIndeck;
    private int captainsInDeck;
    private List<BaseCard> currentDeck = new List<BaseCard>();
    [SerializeField] private DeckLists deckLists;

    [System.Serializable]
    public struct DeckLists
    {
        public List<string> Deck1;
        public List<string> Deck2;
        public List<string> Deck3;

        public List<string> GetList(int index)
        {
            switch (index)
            {
                case 0: return Deck1;
                case 1: return Deck2;
                case 2: return Deck3;
                default:
                    Debug.LogError("Index out of range (0-2)");
                    return null;
            }
        }
    }

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
        DeleteCardLibrary();

        AddCardsToLibrary(CaptainLibrary);
        AddCardsToLibrary(CardLibrary);

        if (deckLists.GetList(currentDeckIndex).Count > 0)
        {
            foreach(string cardID in deckLists.GetList(currentDeckIndex))
            {
                if (AddCardFromID(cardID, CardLibrary))
                    continue;

                if(AddCardFromID(cardID, CaptainLibrary))
                    continue;

                if (AddCardFromID(cardID, CaptainCardsLibrary))
                    continue;
            }
        }
    }

    private void RemoveType(CardType typeToRemove)
    {
        for (int i = 0; i < CardPreviewLibrary.Count; i++)
        {
            if (CardPreviewLibrary[i].Card.Type.type == typeToRemove)
            {
                Destroy(CardPreviewLibrary[i].gameObject);
                CardPreviewLibrary.RemoveAt(i);
                i--;
            }
        }
    }

    private void AddType(CardType typeToAdd)
    {
        List<BaseCard> cardToAdd = new List<BaseCard>();
        for (int i = 0; i < CardLibrary.Count; i++)
        {
            if (CardLibrary[i].Type.type == typeToAdd)
            {
                cardToAdd.Add(CardLibrary[i]);
            }
        }

        for (int i = 0; i < currentDeck.Count; i++)
        {
            if (currentDeck[i].Type.type == CardType.Captain)
            {
                AddCardsToLibrary(GetCaptainsSignatureCards(currentDeck[i].Captain));
            }
        }
        AddCardsToLibrary(cardToAdd);
    }

    public void EditCaptainToggle()
    {
        if(CaptainToggle.sprite == checkedSprite) // was true
        {
            CaptainToggle.sprite = unCheckedSprite;
            RemoveType(CardType.Captain);
        }
        else // was false
        {
            CaptainToggle.sprite = checkedSprite;
            AddCardsToLibrary(CaptainLibrary);
        }
    }

    public void EditActionToggle()
    {
        if (ActionToggle.sprite == checkedSprite) // was true
        {
            ActionToggle.sprite = unCheckedSprite;
            RemoveType(CardType.Action);
        }
        else // was false
        {
            ActionToggle.sprite = checkedSprite;
            AddType(CardType.Action);
        }
    }

    public void EditReactionToggle()
    {
        if (ReactionToggle.sprite == checkedSprite) // was true
        {
            ReactionToggle.sprite = unCheckedSprite;
            RemoveType(CardType.Reaction);
        }
        else // was false
        {
            ReactionToggle.sprite = checkedSprite;
            AddType(CardType.Reaction);
        }
    }

    public void EditEquipmentToggle()
    {
        if (EquipmentToggle.sprite == checkedSprite) // was true
        {
            EquipmentToggle.sprite = unCheckedSprite;
            RemoveType(CardType.Equipment);
        }
        else // was false
        {
            EquipmentToggle.sprite = checkedSprite;
            AddType(CardType.Equipment);
        }
    }

    public void EditAllyToggle()
    {
        if (AllyToggle.sprite == checkedSprite) // was true
        {
            AllyToggle.sprite = unCheckedSprite;
            RemoveType(CardType.Ally);
        }
        else // was false
        {
            AllyToggle.sprite = checkedSprite;
            AddType(CardType.Ally);
        }
    }

    public void DeleteDeck(int index)
    {
        currentDeckIndex = index;

        deckLists.GetList(index).Clear();
        DeckNameInputs[index].text = "";

        StartCoroutine(FirebasePlayer.UpdateObject("DeckName" + currentDeckIndex, "")); // This will be decks 0 - 2 on firebase
        CloudUpdateDeck();
    }

    private bool AddCardFromID(string cardID, List<BaseCard> libraryToSearch)
    {
        foreach (BaseCard cardSearch in libraryToSearch)
        {
            if (cardID == cardSearch.CardName)
            {
                AddCard(cardSearch, false);
                return true;
            }
        }
        return false;
    }

    public void FinishEdittingDeck()
    {
        deckLists.GetList(currentDeckIndex).Clear();
        for (int i = 0; i < currentDeck.Count; i++)
        {
            deckLists.GetList(currentDeckIndex).Add(currentDeck[i].CardName);
        }

        for (int i = 0; i < currentDeck.Count; i++)
        {
            DestroyImmediate(currentDeck[i], true);
        }
        currentDeck.Clear();

        for (int i = 0; i < CardPreviewBar.Count; i++)
        {
            Destroy(CardPreviewBar[i].gameObject);
        }
        CardPreviewBar.Clear();

        CaptainsInDeckText.text = "0/3";
        CardsInDeckText.text = "0/40";
        captainsInDeck = 0;
        cardsIndeck = 0;

        battleMenu.SetDeckCaptains(0, GetCaptainPreviewsFromDeckList(deckLists.GetList(0)));
        battleMenu.SetDeckCaptains(1, GetCaptainPreviewsFromDeckList(deckLists.GetList(1)));
        battleMenu.SetDeckCaptains(2, GetCaptainPreviewsFromDeckList(deckLists.GetList(2)));

        SetMenuCanvasGroups(true);
    }

    public void OnEndEditDeckIndex(int index)
    {
        currentDeckIndex = index;
    }

    public void OnEndEditDeckName(string deckName)
    {
        StartCoroutine(FirebasePlayer.UpdateObject("DeckName" + currentDeckIndex, deckName)); // This will be decks 0 - 2 on firebase
        battleMenu.SetDeckNames(currentDeckIndex, deckName);
        //currentTeam.UpdateTeamName(deckName);
    }

    private void DeleteCardLibrary()
    {
        foreach(LibraryCardPreview cardPreview in CardPreviewLibrary)
        {
            Destroy(cardPreview.gameObject);
        }
        CardPreviewLibrary.Clear();
    }

    private void AddCardsToLibrary(List<BaseCard> cardGroup)
    {
        foreach (BaseCard card in cardGroup)
        {
            LibraryCardPreview newCaptain = Instantiate(CardPreviewPrefab, CardLibraryTransform);
            newCaptain.Init(this, card);
            CardPreviewLibrary.Add(newCaptain);
        }
    }

    private void RemoveCaptainCardsFromLibrary(List<BaseCard> cardGroup)
    {
        for(int i = 0; i < CardPreviewLibrary.Count; i++) // LibraryCardPreview cardPreview in CardPreviewLibrary)
        {
            for(int j = 0; j < cardGroup.Count; j++)
            {
                if (CardPreviewLibrary[i].MatchingName(cardGroup[j].CardName))
                {
                    Destroy(CardPreviewLibrary[i].gameObject);
                    CardPreviewLibrary.Remove(CardPreviewLibrary[i]);
                }
            }
        }
    }

    private List<BaseCard> GetCaptainsSignatureCards(CardCaptain captain)
    {
        List<BaseCard> newLibrary = new List<BaseCard>();

        foreach(BaseCard card in CaptainCardsLibrary)
        {
            if(card.Captain == captain)
            {
                switch(card.Type.type)
                {
                    case CardType.Action:
                        if(ActionToggle.sprite == checkedSprite)
                            newLibrary.Add(card);
                        break;

                    case CardType.Reaction:
                        if (ReactionToggle.sprite == checkedSprite)
                            newLibrary.Add(card);
                        break;

                    case CardType.Equipment:
                        if (EquipmentToggle.sprite == checkedSprite)
                            newLibrary.Add(card);
                        break;

                    case CardType.Ally:
                        if (AllyToggle.sprite == checkedSprite)
                            newLibrary.Add(card);
                        break;
                }
            }
        }

        return newLibrary;
    }

    private void RemoveCaptain(CardCaptain captain)
    {
        List<BaseCard> newCaptainCardLibrary = GetCaptainsSignatureCards(captain);
        RemoveCaptainCardsFromLibrary(newCaptainCardLibrary);

        for(int i = 0; i < currentDeck.Count; i++) // BaseCard card in currentDeck)
        {
            for (int j = 0; j < newCaptainCardLibrary.Count; j++)
            {
                if (currentDeck[i].CardName == newCaptainCardLibrary[j].CardName)
                {
                    RemoveCard(currentDeck[i]);
                }
            }
        }
    }

    public void AddCard(BaseCard cardToAdd, bool updateCloud)
    {
        int copies = 1;
        foreach(BaseCard card in currentDeck)
        {
            if(card.CardName == cardToAdd.CardName)
                copies++;
        }

        if (copies > cardToAdd.Rarity.maxCopies)
            return;

        if ((captainsInDeck >= 3 && cardToAdd.Type.type == CardType.Captain) || (cardsIndeck >= 40 && cardToAdd.Type.type != CardType.Captain))
            return; // Stop any cards from being added here

        BaseCard newCardCopy = ScriptableObject.Instantiate(cardToAdd);
        currentDeck.Add(newCardCopy);

        if(updateCloud)
            CloudUpdateDeck();

        if (cardToAdd.Type.type == CardType.Captain)
        {
            captainsInDeck++;
            CaptainsInDeckText.text = captainsInDeck + "/3";
            AddCardsToLibrary(GetCaptainsSignatureCards(cardToAdd.Captain));
        }
        else
        {
            cardsIndeck++;
            CardsInDeckText.text = cardsIndeck + "/40";
        }

        if (copies > 1)
        {
            foreach (BarCardPreview barCard in CardPreviewBar)
            {
                if (barCard.MatchingName(newCardCopy.CardName))
                {
                    barCard.UpdateCopes(copies);
                    return;
                }
            }
        }

        BarCardPreview newbarCard = Instantiate(BarCardPreviewPrefab, CardBarTransform);
        newbarCard.Init(this, newCardCopy, copies);

        if (cardToAdd.Type.type == CardType.Captain)
            newbarCard.transform.SetAsFirstSibling();

        CardPreviewBar.Add(newbarCard);
    }

    public void RemoveCard(BaseCard cardToRemove)
    {
        BarCardPreview barCardToRemove = null;
        foreach (BarCardPreview barCard in CardPreviewBar)
        {
            if (barCard.MatchingName(cardToRemove.CardName))
            {
                barCardToRemove = barCard;
                break;
            }
        }

        if(barCardToRemove != null)
        {
            Destroy(barCardToRemove.gameObject);
            CardPreviewBar.Remove(barCardToRemove);

            for (int i = 0; i < currentDeck.Count; i++)
            {
                if (currentDeck[i].CardName == cardToRemove.CardName)
                {
                    if (currentDeck[i].Type.type == CardType.Captain)
                    {
                        captainsInDeck--;
                        CaptainsInDeckText.text = captainsInDeck + "/3";
                        RemoveCaptain(currentDeck[i].Captain);
                    }
                    else
                    {
                        cardsIndeck--;
                        CardsInDeckText.text = cardsIndeck + "/40";
                    }
                    DestroyImmediate(currentDeck[i], true);
                    currentDeck.Remove(currentDeck[i]);
                    i--;
                }
            }
        }

        CloudUpdateDeck();
    }

    public void ShowCardEffect(BaseCard cardToShow)
    {
        VisibleCardPreview.SetCard(cardToShow);
    }

    private void CloudUpdateDeck()
    {
        List<string> cardIDs = new List<string>();
        for (int i = 0; i < currentDeck.Count; i++)
        {
            cardIDs.Add(currentDeck[i].CardName);
        }

        StartCoroutine(FirebasePlayer.UpdateObject("Deck" + currentDeckIndex, cardIDs));
    }

    private Sprite[] GetCaptainPreviewsFromDeckList(List<string> deckList)
    {
        Sprite[] captainSprites = new Sprite[3];
        captainSprites[0] = null;
        captainSprites[1] = null;
        captainSprites[2] = null;
        int index = 0;
        for(int i = 0; i < CaptainLibrary.Count; i++)
        {
            if (deckList.Contains(CaptainLibrary[i].CardName))
            {
                captainSprites[index] = CaptainLibrary[i].CardPreviewArt;
                index++;
                continue;
            }
        }
        return captainSprites;
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
                battleMenu.SetDeckNames(i, data.Child("DeckName" + i).Value.ToString());
            }

            if (data.Child("Deck" + i).Exists)
            {
                for (int j = 0; j < data.Child("Deck" + i).ChildrenCount; ++j)
                {
                    deckLists.GetList(i).Add(data.Child("Deck" + i).Child("" + j).Value.ToString());
                }
                battleMenu.SetDeckCaptains(i, GetCaptainPreviewsFromDeckList(deckLists.GetList(i)));
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }

}
