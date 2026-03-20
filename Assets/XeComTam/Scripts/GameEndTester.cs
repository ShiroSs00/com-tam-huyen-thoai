using UnityEngine;

/// <summary>
/// Công cụ hỗ trợ test nhanh bảng kết thúc GameEndUI mà không cần đợi 10 ngày.
/// </summary>
public class GameEndTester : MonoBehaviour
{
    [Header("Mặc định nhấn phím F10 để bật bảng")]
    [Tooltip("Phím để test")]
    public KeyCode testKey = KeyCode.F10;

    [Tooltip("Số tiền giả lập để test mạch Thắng/Thua")]
    public int testMoneyAmount = 1500000;

    private void Update()
    {
        // Chờ nhấn phím F10 qua New Input System
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame)
        {
            var endUI = FindObjectOfType<GameEndUI>(true);
            if (endUI != null)
            {
                // Dừng game như đang ở bảng tổng kết
                Time.timeScale = 0f;

                // Tắt bảng tổng kết thường nếu đang hiện
                var summaryUI = FindObjectOfType<DaySummaryUI>(true);
                if (summaryUI != null) summaryUI.gameObject.SetActive(false);

                // Bật bảng Game End lên với số tiền do mình tự nhập
                endUI.ShowGameEnd(testMoneyAmount);

                Debug.Log($"[Tester] Bật UI kết thúc ngày 10 với số tiền ảo: {testMoneyAmount:N0}đ");
            }
            else
            {
                Debug.LogWarning("[Tester] Không tìm thấy GameEndUI trong Scene!");
            }
        }
    }
}
