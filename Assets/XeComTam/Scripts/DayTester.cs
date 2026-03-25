using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Công cụ test nhanh hệ thống Ngày Đêm mà không cần đợi 20 phút.
/// Gắn script này vào bất kỳ GameObject nào trong Scene (ví dụ: GameManagers).
///
/// HƯỚNG DẪN SỬ DỤNG (Các phím tắt):
/// ─────────────────────────────────────────────────────────────────
/// F5  = Tua nhanh thời gian (x10 tốc độ). Bấm lại để tắt.
/// F6  = Tua SIÊU nhanh (x50 tốc độ). Bấm lại để tắt.
/// F7  = Nhảy thẳng tới 23:00 (gần hết ngày) để test bảng tổng kết.
/// F8  = Cộng 100.000đ tiền ảo để test Economy.
/// F9  = Nhảy sang ngày kế tiếp (bỏ qua bảng tổng kết luôn).
/// F10 = Test bảng Game End (đã có sẵn trong GameEndTester.cs).
/// ─────────────────────────────────────────────────────────────────
/// </summary>
public class DayTester : MonoBehaviour
{
    [Header("Cài đặt Test")]
    [Tooltip("Tốc độ tua nhanh khi bấm F5")]
    [SerializeField] private float fastSpeed = 10f;

    [Tooltip("Tốc độ tua SIÊU nhanh khi bấm F6")]
    [SerializeField] private float superFastSpeed = 50f;

    private bool isFastForward = false;
    private bool isSuperFast = false;
    private float originalTimeScale = 1f;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // ═══ F5: Tua nhanh x10 ═══
        if (kb.f5Key.wasPressedThisFrame)
        {
            if (isSuperFast) // Tắt siêu nhanh trước
            {
                isSuperFast = false;
            }

            isFastForward = !isFastForward;
            Time.timeScale = isFastForward ? fastSpeed : originalTimeScale;
            Debug.Log($"[DayTester] Tua nhanh: {(isFastForward ? $"BẬT x{fastSpeed}" : "TẮT (x1)")}");
        }

        // ═══ F6: Tua SIÊU nhanh x50 ═══
        if (kb.f6Key.wasPressedThisFrame)
        {
            if (isFastForward) // Tắt tua nhanh thường trước
            {
                isFastForward = false;
            }

            isSuperFast = !isSuperFast;
            Time.timeScale = isSuperFast ? superFastSpeed : originalTimeScale;
            Debug.Log($"[DayTester] Tua SIÊU nhanh: {(isSuperFast ? $"BẬT x{superFastSpeed}" : "TẮT (x1)")}");
        }

        // ═══ F7: Nhảy tới 23:00 (gần hết ngày) ═══
        if (kb.f7Key.wasPressedThisFrame)
        {
            var dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                // 23:00 = 23/24 = 0.9583
                // Dùng reflection hoặc set trực tiếp nếu field là public
                // Vì currentTime là private, ta dùng trick: tính thời gian cần nhảy
                float target = 23f / 24f; // 0.9583
                float current = dnc.CurrentTime;
                float jump = target - current;
                if (jump < 0) jump += 1f; // Nếu đã qua 23h thì quay vòng

                // Nhảy bằng cách tăng thời gian thật nhanh trong 1 frame
                // Cách an toàn: set timeScale cực cao trong 1 frame
                float dayDuration = GetDayDuration(dnc);
                float realSecondsToJump = jump * dayDuration;

                Debug.Log($"[DayTester] Đang nhảy tới 23:00... (hiện tại: {dnc.GetFormattedTime()}, cần nhảy {realSecondsToJump:F1}s thực)");

                // Dùng coroutine để tua nhanh trong vài frame
                StartCoroutine(JumpTimeRoutine(realSecondsToJump));
            }
            else
            {
                Debug.LogWarning("[DayTester] Không tìm thấy DayNightCycle!");
            }
        }

        // ═══ F8: Cộng 100.000đ tiền ảo ═══
        if (kb.f8Key.wasPressedThisFrame)
        {
            var eco = EconomyManager.Instance;
            if (eco != null)
            {
                eco.AddMoney(100000);
                Debug.Log($"[DayTester] Đã cộng +100.000đ! Tổng: {eco.CurrentMoney:N0}đ");
            }
            else
            {
                Debug.LogWarning("[DayTester] Không tìm thấy EconomyManager!");
            }
        }

        // ═══ F9: Nhảy sang ngày kế tiếp (skip bảng tổng kết) ═══
        if (kb.f9Key.wasPressedThisFrame)
        {
            var glm = GameLoopManager.Instance;
            if (glm != null)
            {
                if (!glm.GameEnded)
                {
                    Debug.Log($"[DayTester] Bỏ qua ngày {glm.CurrentDay} → sang ngày kế!");

                    // Tắt bảng tổng kết nếu đang hiện
                    var summaryUI = FindObjectOfType<DaySummaryUI>(true);
                    if (summaryUI != null && summaryUI.gameObject.activeSelf)
                    {
                        summaryUI.gameObject.SetActive(false);
                    }

                    // Resume nếu đang pause
                    Time.timeScale = 1f;

                    // Gọi tiếp tục sang ngày mới
                    glm.OnContinuePressed();
                }
                else
                {
                    Debug.LogWarning("[DayTester] Game đã kết thúc rồi, không thể nhảy ngày!");
                }
            }
            else
            {
                Debug.LogWarning("[DayTester] Không tìm thấy GameLoopManager!");
            }
        }
    }

    /// <summary>Lấy dayDuration từ DayNightCycle bằng Reflection (vì field là private)</summary>
    private float GetDayDuration(DayNightCycle dnc)
    {
        var field = typeof(DayNightCycle).GetField("dayDuration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(dnc);
        return 1200f; // Fallback = 20 phút
    }

    private System.Collections.IEnumerator JumpTimeRoutine(float realSecondsToJump)
    {
        // Tua x100 trong vài frame để nhảy thời gian
        float savedTimeScale = Time.timeScale;
        Time.timeScale = 100f;

        float waited = 0f;
        while (waited < realSecondsToJump)
        {
            waited += Time.unscaledDeltaTime * 100f;
            yield return null;
        }

        Time.timeScale = savedTimeScale > 0 ? savedTimeScale : 1f;

        var dnc = FindObjectOfType<DayNightCycle>();
        if (dnc != null)
            Debug.Log($"[DayTester] Đã nhảy xong! Giờ hiện tại: {dnc.GetFormattedTime()}");
    }
}
