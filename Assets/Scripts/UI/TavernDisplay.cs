using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Simon Voss
//Displays and handles tavern and shipyards

public class TavernDisplay : MonoBehaviour
{
    #region Singleton
    public static TavernDisplay instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Another instance of : " + instance.ToString() + " was tried to be instanced, but was destroyed from gameobject: " + this.transform.name);
            GameObject.Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    [SerializeField] Text title = null;

    [Header("Main panel")]
    [SerializeField] GameObject mainPanel = null;

    [Header("Tavern")]
    [SerializeField] GameObject[] tavernDetails = null;

    [Header("All characters")]
    [SerializeField] GameObject[] characterBoxes = null;
    [SerializeField] GameObject[] characterButtons = null;

    [SerializeField] Text[] names = null;
    [SerializeField] Image[] portraits = null;
    [SerializeField] Image[] miniPortraits = null;

    [SerializeField] Image[] wantedPosters = null;


    [SerializeField] Text[] HPs = null;
    [SerializeField] Text[] meleeDamages = null;
    [SerializeField] Text[] rangedDamages = null;
    [SerializeField] Text[] crits = null;
    [SerializeField] Text[] priceTexts = null;
    [SerializeField] Text[] classTexts = null;


    [Header("Single choice")]
    [SerializeField] Text loreField = null;
    [SerializeField] Button purchaseCharacterButton = null;
    [SerializeField] Text purchaseCharacterButtonText = null;

    [Header("Shipyard")]
    [SerializeField] GameObject[] shipyardObjects = null;
    [SerializeField] Button repairShipButton = null;
    [SerializeField] Text repairShipButtonText = null;

    int repairCost = 25;
    int repairHP = 25;

    //Saved information for the current event
    List<CharacterData> availableCharacters = new List<CharacterData>();
    List<int> prices = new List<int>();
    int selectedCharacterIndex = 0;

    public void StartTavernEvent(TavernEvent tavernEvent)
    {
        mainPanel.SetActive(true);
        HideCharacterUI();

        ShowCharacterInfo(tavernEvent.RandomizeCharacters(out List<int> priceList), priceList);

        ShipyardStatus(tavernEvent.isShipyard);
        title.text = tavernEvent.isShipyard ? "Tavern & Shipyard" : "Tavern";
        SetUIButtonStatus();
    }

    public void EndEvent()
    {
        mainPanel.SetActive(false);
        ShipyardStatus(false);
    }

    private void RandomizeDetails()
    {
        foreach (var item in tavernDetails)
        {
            item.SetActive(Random.Range(0, 2) == 1);
        }
    }

    private void ShowCharacterInfo(List<CharacterData> characterDatas, List<int> priceList)
    {
        availableCharacters = characterDatas;
        prices = priceList;

        if (availableCharacters.Count == 0)
        {
            loreField.text = "There are no recruitable pirates in the tavern. \nThey must all be at sea already!";
            purchaseCharacterButtonText.text = "No pirates to hire";
        }
        else
        {
            purchaseCharacterButtonText.text = "Select a pirate";
        }

        for (int i = 0; i < characterDatas.Count; i++)
        {
            characterBoxes[i].SetActive(true);
            characterButtons[i].SetActive(true);

            if (names.Length >= i && names[i] != null)
            {
                names[i].text = characterDatas[i].characterName;
            }
            if (portraits.Length >= i && portraits[i] != null)
            {
                portraits[i].sprite = characterDatas[i].defaultCharacterImage;
                miniPortraits[i].sprite = characterDatas[i].defaultCharacterImage;
            }
            if (wantedPosters.Length >= i && wantedPosters[i] != null)
            {
                wantedPosters[i].sprite = characterDatas[i].customWantedPoster;
            }

            if (HPs.Length >= i && HPs[i] != null)
            {
                HPs[i].text = characterDatas[i].currentHP.ToString() + "/" + characterDatas[i].maxHP.ToString();
            }
            if (meleeDamages.Length >= i && meleeDamages[i] != null)
            {
                meleeDamages[i].text = characterDatas[i].meleeMinDMG.ToString() + "-" + characterDatas[i].meleeMaxDMG.ToString();
            }
            if (rangedDamages.Length >= i && rangedDamages[i] != null)
            {
                rangedDamages[i].text = characterDatas[i].rangedMinDMG.ToString() + "-" + characterDatas[i].rangedMaxDMG.ToString();
            }
            if (crits.Length >= i && crits[i] != null)
            {
                crits[i].text = Mathf.RoundToInt(characterDatas[i].critChance * 100).ToString() + "%";
            }
            if (priceTexts.Length >= i && priceTexts[i] != null)
            {
                priceTexts[i].text = priceList[i].ToString();
            }
            if (classTexts.Length >= i && classTexts[i] != null)
            {
                string characterType = "";
                if (characterDatas[i] is FighterData)
                {
                    characterType = "Fighter";
                }
                else if (characterDatas[i] is GunnerData)
                {
                    characterType = "Gunner";
                }
                else if (characterDatas[i] is ChefData)
                {
                    characterType = "Chef";
                }
                classTexts[i].text = characterType;
            }
        }
    }


    public void SelectCharacter(int index)
    {
        loreField.text = availableCharacters[index].backstory;
        selectedCharacterIndex = index;


        if (PlayerSession.instance.Dubloons >= prices[selectedCharacterIndex])
        {
            if (PlayerSession.instance.GetNumberOfAliveCharacters() < PlayerSession.instance.characterLimit)
            {
                purchaseCharacterButtonText.text = "Hire for " + prices[selectedCharacterIndex].ToString() + " dubloons";
                purchaseCharacterButton.interactable = true;
            }
            else
            {
                purchaseCharacterButtonText.text = "Cannot hire any pirates \nCrew Full!";
                purchaseCharacterButton.interactable = false;
            }
        }
        else
        {
            purchaseCharacterButtonText.text = "Cannot hire this pirate \nNot enough dubloons!";
            purchaseCharacterButton.interactable = false;
        }
    }

    public void PurchaseSelected()
    {
        if (PlayerSession.instance.SpendDubloons(prices[selectedCharacterIndex]))
        {
            PlayerSession.instance.AddNewCharacter(availableCharacters[selectedCharacterIndex]);
            availableCharacters.RemoveAt(selectedCharacterIndex);
            prices.RemoveAt(selectedCharacterIndex);
            HideCharacterUI();
            ShowCharacterInfo(availableCharacters, prices);
            SetUIButtonStatus();
            Debug.Log("Character bought");
        }
        else
        {
            Debug.Log("Not enough dubloons to pay for character");
        }
    }

    private void HideCharacterUI()
    {
        foreach (var item in characterBoxes)
        {
            item.SetActive(false);
        }
        foreach (var item in characterButtons)
        {
            item.SetActive(false);
        }
        loreField.text = "";
        purchaseCharacterButton.interactable = false;
    }

    private void ShipyardStatus(bool state)
    {
        foreach (var item in shipyardObjects)
        {
            item.SetActive(state);
        }
    }

    public void Repair()
    {
        if (PlayerSession.instance.SpendDubloons(repairCost))
        {
            PlayerSession.instance.ShipCurrentHP += repairHP;
            SetUIButtonStatus();
            Debug.Log("Ship repaired");
        }
        else
        {
            Debug.Log("Not enough dubloons to pay for repair");
        }
    }

    private void SetUIButtonStatus()
    {
        purchaseCharacterButton.interactable = false;
        repairShipButtonText.text = "Repair Ship (25 health) \n25 dubloons";

        if (PlayerSession.instance.Dubloons >= repairCost)
        {
            if (PlayerSession.instance.ShipCurrentHP < PlayerSession.instance.ShipMaxHP)
            {
                repairShipButton.interactable = true;
            }
            else
            {
                repairShipButtonText.text = "Cannot repair ship \nShip is at max health";
                repairShipButton.interactable = false;
            }
        }
        else
        {
            repairShipButtonText.text = "Cannot repair ship \nCost: 25 dubloons";
            repairShipButton.interactable = false;

        }
    }
}
