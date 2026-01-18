using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel;
    public Button[] buyButtons = new Button[2];
    public Text[] itemTexts = new Text[2];
    public Text coinsDisplay;

    private bool shopOpen = false;

    void Start()
    {
        Debug.Log("ShopUI Start() called!");
        
        // Setup buy buttons
        for (int i = 0; i < 2; i++)
        {
            int index = i; // Local copy for closure
            if (buyButtons[i] != null)
                buyButtons[i].onClick.AddListener(() => BuyButtonClicked(index));
        }

        if (shopPanel != null)
            shopPanel.SetActive(false);  // Start invisible, toggle with E
    }

    void Update()
    {
        // Toggle shop with E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            shopOpen = !shopOpen;
            if (shopPanel != null)
                shopPanel.SetActive(shopOpen);
        }

        // Update coins display
        Player player = FindObjectOfType<Player>();
        if (player != null && coinsDisplay != null)
        {
            coinsDisplay.text = "Coins: " + player.coins;
        }

        // Update item descriptions
        if (shopOpen && ShopManager.instance != null)
        {
            for (int i = 0; i < 2; i++)
            {
                if (itemTexts[i] != null && i < ShopManager.instance.shopItems.Length)
                {
                    ShopManager.ShopItem item = ShopManager.instance.shopItems[i];
                    itemTexts[i].text = item.name + "\n" + item.description + "\nCost: " + item.cost;
                }
            }
        }
    }

    public void BuyButtonClicked(int itemIndex)
    {
        if (ShopManager.instance != null)
        {
            ShopManager.instance.BuyItem(itemIndex);
        }
    }
}
