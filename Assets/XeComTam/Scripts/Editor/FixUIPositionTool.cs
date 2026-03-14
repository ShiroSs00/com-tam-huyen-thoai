using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;

public class FixUIPositionTool : EditorWindow
{
    [MenuItem("Tools/Sửa lỗi UI (Tiền và Tâm ngắm)")]
    public static void FixUI()
    {
        bool changed = false;

        // 1. Sửa Money UI
        MoneyUI moneyUI = FindObjectOfType<MoneyUI>(true);
        if (moneyUI != null)
        {
            RectTransform rect = moneyUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                Undo.RecordObject(rect, "Move Money UI");
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(25, -25);
                
                Debug.Log("✅ [Money UI] Đã chuyển lên góc trên cùng bên trái.");
                changed = true;
            }
        }
        else
        {
            Debug.LogWarning("❌ [Money UI] Không tìm thấy script MoneyUI trong Scene.");
        }

        // 2. Sửa Tâm ngắm (Interaction UI)
        InteractionUI interactionUI = FindObjectOfType<InteractionUI>(true);
        if (interactionUI != null)
        {
            Transform uiTransform = interactionUI.transform;
            
            // Xoá hoặc ẩn các Text chứa chữ hiển thị (ví dụ "Cơm Tấm")
            TextMeshProUGUI[] texts = uiTransform.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts)
            {
                Undo.RecordObject(txt.gameObject, "Hide Interaction Text");
                txt.gameObject.SetActive(false); // Ẩn luôn Game Object chứa text
            }

            // Sửa hình vuông thành chấm đen
            Image[] images = uiTransform.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != interactionUI.gameObject) // bỏ qua container root
                {
                    Undo.RecordObject(img, "Change Crosshair to Dot");
                    Undo.RecordObject(img.transform, "Change Crosshair to Dot Transform");
                    
                    img.color = Color.black;
                    img.sprite = null; // Xoá sprite viền hình vuông nếu có
                    
                    RectTransform imgRect = img.GetComponent<RectTransform>();
                    if (imgRect != null)
                    {
                        imgRect.sizeDelta = new Vector2(6, 6); // Dấu chấm nhỏ 6x6 pixel
                    }
                }
            }
            
            Debug.Log("✅ [Interaction UI] Đã tạo tâm ngắm dấu chấm đen nhỏ, ẩn các dòng chữ.");
            changed = true;
        }
        else
        {
            Debug.LogWarning("❌ [Interaction UI] Không tìm thấy script InteractionUI trong Scene.");
        }

        if (changed && !Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("🚀 Đã chỉnh sửa Scene. Vui lòng bấm Save (Ctrl+S) để lưu lại Scene.");
        }
    }
}
#endif
