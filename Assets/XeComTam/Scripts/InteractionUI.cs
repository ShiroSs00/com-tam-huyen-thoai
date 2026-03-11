using UnityEngine;
using TMPro;

/// <summary>
/// UI hiển thị tên vật thể và gợi ý phím khi player nhìn vào interactable.
/// Gắn lên một GameObject con của Canvas (WorldSpace hoặc ScreenSpace-Overlay đều được).
/// 
/// Hierarchy gợi ý:
///   Canvas (Screen Space - Overlay)
///   └── InteractionHUD
///       ├── Panel_BG         (Image, bán trong suốt)
///       └── InteractionUI    ← gắn script này
///           ├── Txt_Name     (TextMeshProUGUI)
///           └── Txt_Hint     (TextMeshProUGUI)
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("Tham chiếu Text")]
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtHint;

    [Header("Animation (tuỳ chọn)")]
    [Tooltip("Tốc độ fade in/out (0 = bật tắt ngay lập tức)")]
    [SerializeField] private float fadeSpeed = 8f;

    private CanvasGroup canvasGroup;
    private float targetAlpha;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (fadeSpeed <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            return;
        }
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha,
                                              fadeSpeed * Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// <summary>Hiện UI với tên và gợi ý phím.</summary>
    public void Show(string name, string hint)
    {
        if (txtName != null) txtName.text = name;
        if (txtHint != null) txtHint.text = hint;
        targetAlpha = 1f;
    }

    /// <summary>Ẩn UI (fade out).</summary>
    public void Hide()
    {
        targetAlpha = 0f;
    }
}
