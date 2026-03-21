using UnityEngine;

/// <summary>
/// Giao diện Hướng dẫn khi mới bắt đầu game.
/// Dừng thời gian, chờ người chơi đọc xong bấm nút T để bắt đầu.
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("UI Panel hướng dẫn")]
    [Tooltip("Kéo thả GameObject Panel chứa nội dung Hướng Dẫn vào đây")]
    [SerializeField] private GameObject tutorialPanel;

    private void Start()
    {
        // Hiện UI hướng dẫn
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        
        // Dừng thời gian game
        Time.timeScale = 0f;

        // Mở khoá chuột để người chơi có thể đọc/bấm menu nếu cần
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Chờ nhấn phím T qua New Input System
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            CloseTutorial();
        }
    }

    /// <summary>Có thể gọi hàm này từ OnClick của Button trên UI nếu không muốn bấm phím T</summary>
    public void CloseTutorial()
    {
        // Ẩn panel
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        // Resume game speed
        Time.timeScale = 1f;

        // Khóa chuột lại
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Báo cho GameLoopManager biết để bắt đầu Ngày 1 và hiện Event
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.StartFirstDay();
        }
        
        // Xóa luôn script/bảng này để giải phóng bộ nhớ (hoặc Disable gameObject)
        gameObject.SetActive(false);
    }
}
