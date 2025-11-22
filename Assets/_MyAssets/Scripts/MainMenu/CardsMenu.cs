using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardsMenu : MonoBehaviour, IDataPersistence
{
    [System.Serializable]
    public struct CardTypePair
    {
        public CardType type;
        public Sprite typeIcon;
        public Sprite typeOutline;
    }

    public CardTypePair[] CardSpritePairs;
    private Dictionary<CardType, CardTypePair> CardTypeDictionary;
    public Sprite GetIconFromType(CardType type) { return CardTypeDictionary[type].typeIcon; }
    public Sprite GetOutlineFromType(CardType type) { return CardTypeDictionary[type].typeOutline; }

    [System.Serializable]
    public struct CardRarityPair
    {
        public CardRarity rarity;
        public Sprite rairtyIcon;
        public int maxCopies;
    }

    public CardRarityPair[] CardRarityPairs;
    private Dictionary<CardRarity, CardRarityPair> CardRarityDictionary;

    public Sprite GetIconFromRarity(CardRarity rairty) { return CardRarityDictionary[rairty].rairtyIcon; }
    public int GetMaxCopiesFromRarity(CardRarity rairty) { return CardRarityDictionary[rairty].maxCopies; }

    [SerializeField] FirebasePlayerInfo FirebasePlayer;
    [SerializeField] TMP_InputField[] DeckNameInputs;
    [SerializeField] private int NumOfDecks = 3;

    [SerializeField] private CanvasGroup SelectDeckCanvasGroup;
    [SerializeField] private CanvasGroup EditDeckCanvasGroup;

    private int currentDeckIndex;

    public void Start()
    {
        CardTypeDictionary = new Dictionary<CardType, CardTypePair>();
        foreach (var pair in CardSpritePairs)
            CardTypeDictionary[pair.type] = pair;

        CardRarityDictionary = new Dictionary<CardRarity, CardRarityPair>();
        foreach (var pairRarity in CardRarityPairs)
            CardRarityDictionary[pairRarity.rarity] = pairRarity;
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
