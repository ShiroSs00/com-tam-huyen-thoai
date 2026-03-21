using UnityEngine;

/// <summary>
/// Singleton quản lý tiền của người chơi.
/// Hỗ trợ: multiplier (x2 event), giá bán thay đổi, tracking thu chi hàng ngày.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Cài đặt")]
    [SerializeField] private int startingMoney = 0;
    [SerializeField] private int pricePerPlate = 35000; // 35.000đ / đĩa
    [Tooltip("Chi phí vận hành / tiền vốn mỗi ngày (bị trừ lúc hết ngày)")]
    [SerializeField] private int dailyOperatingCost = 500000; 

    private int currentMoney;

    // ── Event system support ──
    private float moneyMultiplier = 1f;    // Hệ số nhân tiền (x2 event)
    private int currentPricePerPlate;      // Giá bán hiện tại (có thể bị giảm bởi event)

    // ── Tracking thu chi hàng ngày ──
    private int todayIncome;               // Tổng thu trong ngày
    private int todayPlatesSold;           // Số đĩa bán trong ngày
    private int todayExpenses;             // Tổng chi (phạt, bảo kê) trong ngày

    // ── Public properties ──
    public int CurrentMoney => currentMoney;
    public int PricePerPlate => currentPricePerPlate;
    public int BasePricePerPlate => pricePerPlate;
    public float MoneyMultiplier => moneyMultiplier;
    public int DailyOperatingCost => dailyOperatingCost;
    public int TodayIncome => todayIncome;
    public int TodayPlatesSold => todayPlatesSold;
    public int TodayExpenses => todayExpenses;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentMoney = startingMoney;
        currentPricePerPlate = pricePerPlate;
    }

    /// <summary>Cộng tiền (đã nhân multiplier) và cập nhật UI.</summary>
    public void AddMoney(int amount)
    {
        // Áp dụng hệ số nhân (x2 event)
        int finalAmount = Mathf.RoundToInt(amount * moneyMultiplier);
        currentMoney += finalAmount;
        todayIncome += finalAmount;
        todayPlatesSold++;

        Debug.Log($"[Economy] +{finalAmount:N0}đ (x{moneyMultiplier}) | Tổng: {currentMoney:N0}đ");

        MoneyUI ui = FindObjectOfType<MoneyUI>();
        ui?.ShowEarned(finalAmount);
        ui?.UpdateTotal(currentMoney);
    }

    /// <summary>Trừ tiền (dùng cho event phạt, bảo kê).</summary>
    public void DeductMoney(int amount)
    {
        currentMoney -= amount;
        todayExpenses += amount;
        Debug.Log($"[Economy] -{amount:N0}đ (phạt/bảo kê) | Tổng: {currentMoney:N0}đ");

        MoneyUI ui = FindObjectOfType<MoneyUI>();
        ui?.UpdateTotal(currentMoney);
    }

    /// <summary>Set hệ số nhân tiền (x2 event). Gọi 1f để reset.</summary>
    public void SetMoneyMultiplier(float multiplier)
    {
        moneyMultiplier = multiplier;
        Debug.Log($"[Economy] Hệ số nhân tiền: x{multiplier}");
    }

    /// <summary>Đổi giá bán tạm thời (event giảm giá). 0 = reset về gốc.</summary>
    public void SetCurrentPrice(int newPrice)
    {
        currentPricePerPlate = newPrice > 0 ? newPrice : pricePerPlate;
        Debug.Log($"[Economy] Giá bán hiện tại: {currentPricePerPlate:N0}đ");
    }

    /// <summary>Reset tracking + modifier đầu mỗi ngày mới.</summary>
    public void ResetDailyStats()
    {
        todayIncome = 0;
        todayPlatesSold = 0;
        todayExpenses = 0;
        moneyMultiplier = 1f;
        currentPricePerPlate = pricePerPlate;
        Debug.Log("[Economy] Đã reset stats ngày mới.");
    }
}
