using UnityEngine;

// ═════════════════════════════════════════════════════════════════════
// Định nghĩa các loại sự kiện trong game
// ═════════════════════════════════════════════════════════════════════

public enum GameEventCategory
{
    Bad,    // Sự kiện xấu (công an phạt, bảo kê)
    Good    // Sự kiện tốt (x2 tiền, giảm giá)
}

public enum GameEventType
{
    // ── Xấu ──
    CongAnPhat,   // Công an phạt tiền vi phạm ATTP
    BaoKe,        // Nộp tiền bảo kê cho anh hai

    // ── Tốt ──
    X2Tien,       // Nhận x2 tiền mỗi đĩa trong ngày
    GiamGia       // Giảm giá → NPC đến nhanh hơn
}

[System.Serializable]
public class GameEventData
{
    public GameEventType type;
    public GameEventCategory category;
    public string eventName;
    [TextArea] public string description;
    public int moneyEffect;       // Số tiền ảnh hưởng (âm = trừ, dương = cộng)
    public float multiplier;      // Hệ số nhân tiền (1 = bình thường, 2 = x2)
    public int modifiedPrice;     // Giá bán mới (0 = không đổi)
    public Sprite icon;           // Icon hiển thị trên UI (tùy chọn)

    public GameEventData(GameEventType type, GameEventCategory category, 
                         string name, string desc, int moneyEffect = 0, 
                         float multiplier = 1f, int modifiedPrice = 0)
    {
        this.type = type;
        this.category = category;
        this.eventName = name;
        this.description = desc;
        this.moneyEffect = moneyEffect;
        this.multiplier = multiplier;
        this.modifiedPrice = modifiedPrice;
    }
}
