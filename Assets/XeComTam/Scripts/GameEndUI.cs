using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Giao diện kết thúc game sau 10 ngày.
/// Kiểm tra xem người chơi phá sản hay đạt mục tiêu.
/// </summary>
public class GameEndUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject gameEndPanel;

    [Header("Cài đặt Mục Tiêu")]
    [Tooltip("Số tiền mục tiêu cần đạt được để chiến thắng sau 10 ngày")]
    [SerializeField] private int targetProfit = 1000000; // 1 triệu

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI txtResultTitle;     // CHÚC MỪNG / PHÁ SẢN
    [SerializeField] private TextMeshProUGUI txtFinalMoney;      // Hiện số tiền cuối cùng
    [SerializeField] private TextMeshProUGUI txtPerformance;     // Nhận xét

    [Header("Nút bấm")]
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnQuit;

    [Header("Âm thanh (Mới Thêm)")]
    [Tooltip("Kéo file âm thanh Game Over (Phá Sản) vào đây")]
    [SerializeField] private AudioClip gameOverSound;
    [Tooltip("Kéo file âm thanh Chúc Mừng vào đây (không bắt buộc)")]
    [SerializeField] private AudioClip winSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(false);

        if (btnRestart != null) btnRestart.onClick.AddListener(RestartGame);
        if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);

        // Tự động gắn AudioSource nếu game object chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>Được gọi bởi GameLoopManager khi qua ngày thứ 10.</summary>
    public void ShowGameEnd(int finalMoney)
    {
        if (gameEndPanel == null) return;

        // Bật panel
        gameEndPanel.SetActive(true);

        // Mở khóa chuột
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Kiểm tra thắng thua
        if (finalMoney >= targetProfit)
        {
            // THẮNG
            if (txtResultTitle != null) 
            {
                txtResultTitle.text = "CHÚC MỪNG!";
                txtResultTitle.color = Color.green;
            }
            if (txtPerformance != null) 
                txtPerformance.text = "Bạn đã kinh doanh vô cùng thành công!";
            
            // Phát nhạc win nếu có
            if (winSound != null) audioSource.PlayOneShot(winSound);
        }
        else if (finalMoney > 0 && finalMoney < targetProfit)
        {
            // KHÔNG ĐẠT MỤC TIÊU
            if (txtResultTitle != null)
            {
                txtResultTitle.text = "CHƯA ĐẠT MỤC TIÊU";
                txtResultTitle.color = Color.yellow;
            }
            if (txtPerformance != null) 
                txtPerformance.text = $"Mục tiêu là {targetProfit:N0}đ. Hãy cố gắng hơn vào lần sau!";
            
            // Có thể chơi nhạc Game Over nhè nhẹ ở đây luôn nếu thích
            if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound);
        }
        else
        {
            // PHÁ SẢN (Tiền <= 0)
            if (txtResultTitle != null)
            {
                txtResultTitle.text = "BẠN ĐẢ PHÁ SẢN!";
                txtResultTitle.color = Color.red;
            }
            if (txtPerformance != null) 
                txtPerformance.text = "Bán cơm tấm mà lỗ... Xin chia buồn cùng gia đình.";

            // Phát nhạc Game Over
            if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound);
        }

        // Hiện số dư
        if (txtFinalMoney != null)
            txtFinalMoney.text = $"Tổng Tài Sản: {finalMoney:N0}đ";
    }

    [Tooltip("Tên của Scene Main Menu để quay về khi bấm nút Thoát")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void RestartGame()
    {
        // Resume game speed and reload current scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        Debug.Log($"Quay về Main Menu: {mainMenuSceneName}");
        Time.timeScale = 1f;
        
        // Quá trình build game cần add scene này vào Build Settings
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
