using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton quản lý vòng lặp 10 ngày.
/// Subscribe vào DayNightCycle.OnDayEnd để phát hiện hết ngày.
/// Pause game → hiện tổng kết → bấm Tiếp tục → random event → sang ngày mới.
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("Cài đặt")]
    [Tooltip("Tổng số ngày chơi")]
    [SerializeField] private int totalDays = 10;

    [Header("Tham chiếu")]
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private DaySummaryUI daySummaryUI;
    [SerializeField] private EventNotificationUI eventNotificationUI;
    [SerializeField] private GameEndUI gameEndUI;

    // ── State ──
    private int currentDay = 1;
    private bool gameEnded = false;

    public int CurrentDay => currentDay;
    public int TotalDays => totalDays;
    public bool GameEnded => gameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Tự tìm tham chiếu nếu chưa gán
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (daySummaryUI == null)
            daySummaryUI = FindObjectOfType<DaySummaryUI>(true);
        if (eventNotificationUI == null)
            eventNotificationUI = FindObjectOfType<EventNotificationUI>(true);
        if (gameEndUI == null)
            gameEndUI = FindObjectOfType<GameEndUI>(true);

        // Subscribe sự kiện hết ngày
        if (dayNightCycle != null)
            dayNightCycle.OnDayEnd += HandleDayEnd;

        // KIỂM TRA TUTORIAL
        // Nếu có UI Hướng dẫn đang bật trên Scene thì đợi TutorialUI gọi, KO StartNewDay!
        var tutorial = FindObjectOfType<TutorialUI>();
        if (tutorial == null || !tutorial.gameObject.activeInHierarchy)
        {
            // Nếu không xài Tutorial thì game tự chạy luôn
            StartFirstDay();
        }
    }

    /// <summary>Bắt đầu ngày 1 (Được gọi bởi TutorialUI lúc bấm phím T)</summary>
    public void StartFirstDay()
    {
        StartNewDay();
    }

    private void OnDestroy()
    {
        if (dayNightCycle != null)
            dayNightCycle.OnDayEnd -= HandleDayEnd;
    }

    /// <summary>Gọi khi DayNightCycle phát sự kiện hết ngày.</summary>
    private void HandleDayEnd()
    {
        if (gameEnded) return;

        Debug.Log($"[GameLoop] ═══ HẾT NGÀY {currentDay}/{totalDays} ═══");

        // Pause game
        Time.timeScale = 0f;

        // Unlock cursor để bấm UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hiện bảng tổng kết
        if (daySummaryUI != null)
        {
            var eco = EconomyManager.Instance;
            var evtMgr = DailyEventManager.Instance;

            // Thu chi phí vận hành cố định mỗi ngày trước khi tông kết
            if (eco != null)
            {
                eco.DeductMoney(eco.DailyOperatingCost);
            }

            daySummaryUI.ShowSummary(
                dayNumber: currentDay,
                totalDays: totalDays,
                todayIncome: eco != null ? eco.TodayIncome : 0,
                todayPlatesSold: eco != null ? eco.TodayPlatesSold : 0,
                todayExpenses: eco != null ? eco.TodayExpenses : 0,
                totalMoney: eco != null ? eco.CurrentMoney : 0,
                events: evtMgr != null ? new List<GameEventData>(evtMgr.TodayEvents) : new List<GameEventData>(),
                isLastDay: currentDay >= totalDays
            );
        }
    }

    /// <summary>Được gọi bởi DaySummaryUI khi bấm nút "Tiếp tục".</summary>
    public void OnContinuePressed()
    {
        if (currentDay >= totalDays)
        {
            // Game kết thúc sau ngày cuối
            gameEnded = true;
            Debug.Log("[GameLoop] ═══ GAME KẾT THÚC ═══");
            
            // Xóa UI tổng kết ngày hiện tại
            if (daySummaryUI != null) daySummaryUI.gameObject.SetActive(false);

            // Bật UI Game Over / Win
            if (gameEndUI != null && EconomyManager.Instance != null)
            {
                gameEndUI.ShowGameEnd(EconomyManager.Instance.CurrentMoney);
            }
            return;
        }

        // Chuyển sang ngày mới
        currentDay++;
        Debug.Log($"[GameLoop] ═══ BẮT ĐẦU NGÀY {currentDay}/{totalDays} ═══");

        // Resume game
        Time.timeScale = 1f;

        // Lock cursor lại
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset stats + event → random event mới
        StartNewDay();
    }

    private void StartNewDay()
    {
        // Reset economy stats
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.ResetDailyStats();

        // Reset + random event mới
        var evtMgr = DailyEventManager.Instance;
        if (evtMgr != null)
        {
            evtMgr.ResetEvents();
            List<GameEventData> events = evtMgr.GenerateAndApplyEvents(currentDay);

            // Hiện thông báo event — Coroutine chạy trên GameLoopManager (luôn active)
            if (eventNotificationUI != null)
            {
                eventNotificationUI.ShowEvents(currentDay, events);
                StartCoroutine(HideNotificationAfterDelay(eventNotificationUI.displayDuration));
            }
        }

        Debug.Log($"[GameLoop] Ngày {currentDay} bắt đầu!");
    }

    /// <summary>Coroutine ẩn thông báo — chạy trên GameLoopManager (luôn active, không bị lỗi)</summary>
    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (eventNotificationUI != null)
            eventNotificationUI.HidePanel();
    }
}
