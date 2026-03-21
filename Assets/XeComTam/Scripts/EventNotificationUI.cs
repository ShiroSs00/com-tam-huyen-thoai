using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// UI thông báo event đầu mỗi ngày mới.
/// CHỈ cập nhật text, KHÔNG dùng Coroutine.
/// Việc ẩn/hiện do GameLoopManager điều khiển.
/// </summary>
public class EventNotificationUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject notificationPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI txtDayHeader;
    [SerializeField] private TextMeshProUGUI txtEvents;

    [Header("Cài đặt")]
    [Tooltip("Thời gian hiển thị thông báo (giây)")]
    public float displayDuration = 5f;

    /// <summary>Hiện thông báo event cho ngày mới.</summary>
    public void ShowEvents(int dayNumber, List<GameEventData> events)
    {
        // Bật panel lên
        if (notificationPanel != null) notificationPanel.SetActive(true);

        // Header
        if (txtDayHeader != null)
        {
            int totalDays = GameLoopManager.Instance != null ? GameLoopManager.Instance.TotalDays : 10;
            txtDayHeader.text = $"NGÀY {dayNumber}/{totalDays}";
        }

        // Danh sách event
        if (txtEvents != null)
        {
            string text = "";
            foreach (var evt in events)
            {
                string icon = evt.category == GameEventCategory.Good ? "🟢" : "🔴";
                text += $"{icon} {evt.eventName}: {evt.description}\n";
            }
            txtEvents.text = text.TrimEnd('\n');
        }
    }

    /// <summary>Ẩn panel thông báo.</summary>
    public void HidePanel()
    {
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }
}
