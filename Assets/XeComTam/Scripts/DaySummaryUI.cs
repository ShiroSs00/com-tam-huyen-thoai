using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI bảng tổng kết cuối ngày.
/// Hiện: ngày, tổng thu, số đĩa bán, event, lợi nhuận ròng, tổng tài sản.
/// Nút "Tiếp tục" để sang ngày mới.
/// 
/// Hierarchy gợi ý:
///   Canvas (Screen Space - Overlay)
///   └── DaySummaryPanel (Panel full màn hình, bán trong suốt)
///       ├── SummaryBox (Panel trung tâm)
///       │   ├── Txt_Title         "📋 TỔNG KẾT NGÀY 1/10"
///       │   ├── Txt_Income        "💰 Tổng thu: +140.000đ"
///       │   ├── Txt_PlatesSold    "📋 Số đĩa bán: 4 đĩa"
///       │   ├── Txt_EventsHeader  "── Sự kiện trong ngày ──"
///       │   ├── Txt_EventsList    (nội dung event, mỗi event 1 dòng)
///       │   ├── Txt_NetProfit     "💵 Lợi nhuận ròng: +110.000đ"
///       │   ├── Txt_TotalMoney    "🏦 Tổng tài sản: 180.000đ"
///       │   └── Btn_Continue      [TIẾP TỤC →]
///       └── (hoặc Txt_GameOver nếu là ngày cuối)
/// </summary>
public class DaySummaryUI : MonoBehaviour
{
    [Header("Panel chính")]
    [SerializeField] private GameObject summaryPanel;

    [Header("Text hiển thị")]
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtIncome;
    [SerializeField] private TextMeshProUGUI txtPlatesSold;
    [SerializeField] private TextMeshProUGUI txtEventsHeader;
    [SerializeField] private TextMeshProUGUI txtEventsList;
    [SerializeField] private TextMeshProUGUI txtNetProfit;
    [SerializeField] private TextMeshProUGUI txtTotalMoney;

    [Header("Nút bấm")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private TextMeshProUGUI txtBtnContinue;

    private void Awake()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);

        if (btnContinue != null)
            btnContinue.onClick.AddListener(OnContinueClicked);
    }

    /// <summary>Hiện bảng tổng kết.</summary>
    public void ShowSummary(int dayNumber, int totalDays, int todayIncome, 
                            int todayPlatesSold, int todayExpenses, int totalMoney,
                            List<GameEventData> events, bool isLastDay)
    {
        if (summaryPanel == null) return;
        summaryPanel.SetActive(true);

        // Tiêu đề
        if (txtTitle != null)
            txtTitle.text = $"📋 TỔNG KẾT NGÀY {dayNumber}/{totalDays}";

        // Tổng thu
        if (txtIncome != null)
            txtIncome.text = $"💰 Tổng thu: +{todayIncome:N0}đ";

        // Số đĩa bán
        if (txtPlatesSold != null)
            txtPlatesSold.text = $"🍚 Số đĩa bán: {todayPlatesSold} đĩa";

        // Event header
        if (txtEventsHeader != null)
            txtEventsHeader.text = "── Sự kiện trong ngày ──";

        // Danh sách event
        if (txtEventsList != null)
        {
            int opCost = EconomyManager.Instance != null ? EconomyManager.Instance.DailyOperatingCost : 500000;
            string evtText = $"🛒 Chi phí vận hành:   -{opCost:N0}đ\n";
            
            foreach (var evt in events)
            {
                string icon = evt.category == GameEventCategory.Good ? "🟢" : "🔴";
                string moneyStr = "";

                if (evt.moneyEffect != 0)
                    moneyStr = $"  {evt.moneyEffect:N0}đ";
                else if (evt.multiplier > 1f)
                    moneyStr = $"  x{evt.multiplier} tiền";
                else if (evt.modifiedPrice > 0)
                    moneyStr = $"  Giá: {evt.modifiedPrice:N0}đ";

                evtText += $"{icon} {evt.eventName}: {moneyStr}\n";
            }
            txtEventsList.text = evtText.TrimEnd('\n');
        }

        // Lợi nhuận ròng
        int netProfit = todayIncome - todayExpenses;
        if (txtNetProfit != null)
        {
            string sign = netProfit >= 0 ? "+" : "";
            txtNetProfit.text = $"💵 Lợi nhuận ròng: {sign}{netProfit:N0}đ";
        }

        // Tổng tài sản
        if (txtTotalMoney != null)
            txtTotalMoney.text = $"🏦 Tổng tài sản: {totalMoney:N0}đ";

        // Nút tiếp tục
        if (txtBtnContinue != null)
            txtBtnContinue.text = isLastDay ? "KẾT THÚC" : "TIẾP TỤC →";
    }

    private void OnContinueClicked()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);

        if (GameLoopManager.Instance != null)
            GameLoopManager.Instance.OnContinuePressed();
    }
}
