using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý random + áp dụng event mỗi ngày.
/// Mỗi ngày random tối thiểu 1 event tốt + 1 event xấu.
/// </summary>
public class DailyEventManager : MonoBehaviour
{
    public static DailyEventManager Instance { get; private set; }

    [Header("Cài đặt Event Xấu")]
    [Tooltip("Số tiền công an phạt")]
    [SerializeField] private int congAnPhatAmount = 50000;

    [Tooltip("Số tiền bảo kê")]
    [SerializeField] private int baoKeAmount = 30000;

    [Header("Cài đặt Event Tốt")]
    [Tooltip("Giá bán khi có event Giảm Giá")]
    [SerializeField] private int discountPrice = 25000;

    // ── Danh sách event đã xảy ra trong ngày (để hiện tổng kết) ──
    private List<GameEventData> todayEvents = new List<GameEventData>();
    public IReadOnlyList<GameEventData> TodayEvents => todayEvents;

    // ── Pool event ──
    private List<GameEventData> badEventPool;
    private List<GameEventData> goodEventPool;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        BuildEventPools();
    }

    private void BuildEventPools()
    {
        badEventPool = new List<GameEventData>
        {
            new GameEventData(
                GameEventType.CongAnPhat, GameEventCategory.Bad,
                "Công an phạt",
                $"Bị phạt vi phạm vệ sinh ATTP! Mất {congAnPhatAmount:N0}đ.",
                moneyEffect: -congAnPhatAmount
            ),
            new GameEventData(
                GameEventType.BaoKe, GameEventCategory.Bad,
                "Đóng tiền bảo kê",
                $"Anh hai khu phố đến thu tiền! Mất {baoKeAmount:N0}đ.",
                moneyEffect: -baoKeAmount
            )
        };

        goodEventPool = new List<GameEventData>
        {
            new GameEventData(
                GameEventType.X2Tien, GameEventCategory.Good,
                "x2 Tiền nhận vào",
                "Khách review 5 sao! Đông khách gấp đôi! Mỗi đĩa nhận x2 tiền!",
                multiplier: 2f
            ),
            new GameEventData(
                GameEventType.GiamGia, GameEventCategory.Good,
                "Giảm giá cơm",
                $"Ngày hội giảm giá! Giá bán {discountPrice:N0}đ nhưng khách đến nhanh hơn!",
                modifiedPrice: discountPrice
            )
        };
    }

    /// <summary>
    /// Gọi đầu mỗi ngày mới: sinh duy nhất 1 event (hoặc không có) theo lịch trình.
    /// Lịch:
    /// - Bảo kê: cách 2 ngày mới thu (vd: ngày 3, 6, 9)
    /// - X2 Tiền: ngày 1, và random từ ngày 5 trở đi
    /// - Công an phạt: ngày 5 và ngày 9
    /// - Giảm giá: random từ ngày 5 tới ngày 9 (tỉ lệ 15%)
    /// => Ưu tiên event cố định cho các ngày đặc biệt, nếu trùng thì ưu tiên cái nặng hơn hoặc chia ra theo logic.
    /// Để đơn giản, thiết kế 1 chuỗi quyết định (if-else) từ độ ưu tiên cao nhất xuống thấp nhất.
    /// </summary>
    public List<GameEventData> GenerateAndApplyEvents(int dayNumber)
    {
        todayEvents.Clear();
        GameEventData chosenEvent = null;

        // ƯU TIÊN 1: Các event CỐ ĐỊNH theo kịch bản (gắn liền với ngày)
        if (dayNumber == 1)
        {
            // Ngày 1: Luôn X2 Tiền
            chosenEvent = goodEventPool.Find(e => e.type == GameEventType.X2Tien);
        }
        else if (dayNumber == 5 || dayNumber == 9)
        {
            // Ngày 5, 9: Luôn Công an phạt
            chosenEvent = badEventPool.Find(e => e.type == GameEventType.CongAnPhat);
        }
        else if (dayNumber % 2 == 0) // Hai ngày thu 1 lần (ngày 2, 4, 6, 8, 10...)
        {
            // Ngày 2, 4, 6...: Bảo kê
            chosenEvent = badEventPool.Find(e => e.type == GameEventType.BaoKe);
        }
        
        // ƯU TIÊN 2: Nếu ngày trống (chưa có event cố định), xét đến event RANDOM (từ ngày 5 trở đi)
        if (chosenEvent == null && dayNumber >= 5)
        {
            // Tung xúc xắc cho Giảm Giá (15%)
            if (Random.value <= 0.15f)
            {
                chosenEvent = goodEventPool.Find(e => e.type == GameEventType.GiamGia);
            }
            // Tung xúc xắc cho X2 Tiền (VD cho 15% luôn để không ra quá nhiều)
            else if (Random.value <= 0.15f)
            {
                chosenEvent = goodEventPool.Find(e => e.type == GameEventType.X2Tien);
            }
        }

        // Áp dụng event nếu có
        if (chosenEvent != null)
        {
            todayEvents.Add(chosenEvent);
            ApplyEvent(chosenEvent);
        }

        return todayEvents;
    }

    private void ApplyEvent(GameEventData evt)
    {
        var eco = EconomyManager.Instance;
        if (eco == null)
        {
            Debug.LogError("[DailyEventManager] Không tìm thấy EconomyManager!");
            return;
        }

        switch (evt.type)
        {
            case GameEventType.CongAnPhat:
            case GameEventType.BaoKe:
                // Trừ tiền ngay lập tức
                eco.DeductMoney(Mathf.Abs(evt.moneyEffect));
                Debug.Log($"[Event] 🔴 {evt.eventName}: -{Mathf.Abs(evt.moneyEffect):N0}đ");
                break;

            case GameEventType.X2Tien:
                // Set multiplier x2 cho ngày hôm nay
                eco.SetMoneyMultiplier(evt.multiplier);
                Debug.Log($"[Event] 🟢 {evt.eventName}: x{evt.multiplier} tiền!");
                break;

            case GameEventType.GiamGia:
                // Đổi giá bán trong ngày
                eco.SetCurrentPrice(evt.modifiedPrice);
                Debug.Log($"[Event] 🟢 {evt.eventName}: Giá bán = {evt.modifiedPrice:N0}đ");
                break;
        }
    }

    /// <summary>Tính tổng tiền bị trừ bởi event xấu trong ngày.</summary>
    public int GetTodayEventLosses()
    {
        int total = 0;
        foreach (var evt in todayEvents)
            if (evt.moneyEffect < 0) total += evt.moneyEffect; // moneyEffect đã âm
        return total; // Trả về số âm
    }

    /// <summary>Reset event cuối ngày (để chuẩn bị cho ngày mới).</summary>
    public void ResetEvents()
    {
        todayEvents.Clear();
    }
}
