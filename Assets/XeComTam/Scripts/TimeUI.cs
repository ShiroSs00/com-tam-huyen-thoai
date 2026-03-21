using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị giờ trong game lên UI.
/// </summary>
public class TimeUI : MonoBehaviour
{
    [Header("Cài đặt UI")]
    [Tooltip("TextMeshPro hiển thị thời gian (VD: 12:00)")]
    [SerializeField] private TextMeshProUGUI txtTime;

    [Header("Tham chiếu")]
    [Tooltip("Kéo object Directional Light (có gắn script DayNightCycle) vào đây")]
    [SerializeField] private DayNightCycle dayNightCycle;

    private void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();
    }

    private void Update()
    {
        if (dayNightCycle == null || txtTime == null) return;
        txtTime.text = dayNightCycle.GetFormattedTime();
    }
}
