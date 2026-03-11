using UnityEngine;

/// <summary>
/// Singleton quan ly tien cua nguoi choi.
/// Goi EconomyManager.Instance.AddMoney(so_tien) khi NPC tra tien.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Cai dat")]
    [SerializeField] private int startingMoney = 0;
    [SerializeField] private int pricePerPlate = 35000; // 35.000d / dia

    private int currentMoney;

    public int CurrentMoney => currentMoney;
    public int PricePerPlate => pricePerPlate;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentMoney = startingMoney;
    }

    /// <summary>Cong tien va cap nhat UI.</summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"[Economy] +{amount:N0}d | Tong: {currentMoney:N0}d");

        MoneyUI ui = FindObjectOfType<MoneyUI>();
        ui?.ShowEarned(amount);
        ui?.UpdateTotal(currentMoney);
    }
}
