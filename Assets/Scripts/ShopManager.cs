using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [System.Serializable]
    public class ShopItem
    {
        public string name;
        public string description;
        public int cost;
        public string statType; // "damage", "health", "bow"
    }

    public ShopItem[] shopItems = new ShopItem[2];

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Initialize shop items
        shopItems[0] = new ShopItem { name = "Damage Boost", description = "+10 Damage", cost = 5, statType = "damage" };
        shopItems[1] = new ShopItem { name = "Health Boost", description = "+100 HP", cost = 5, statType = "health" };
    }

    public bool BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shopItems.Length)
        {
            Debug.LogError("Invalid item index: " + itemIndex);
            return false;
        }

        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return false;
        }

        ShopItem item = shopItems[itemIndex];

        // Check if player has enough coins
        if (player.coins < item.cost)
        {
            Debug.Log("Not enough coins! Need " + item.cost + ", have " + player.coins);
            return false;
        }

        // Deduct coins
        player.coins -= item.cost;
        player.coinCount -= item.cost;  // Scad din coinCount care e de adevărat
        Debug.Log("Bought " + item.name + "! Coins remaining: " + player.coins);

        // Apply stat modification
        switch (item.statType)
        {
            case "damage":
                player.damagePerHit += 25;
                Debug.Log("Damage increased to " + player.damagePerHit);
                break;
            case "health":
                player.maxHealth += 100;
                player.currentHealth = player.maxHealth; // Full heal
                Debug.Log("Health increased to " + player.maxHealth);
                break;
        }

        return true;
    }
}
