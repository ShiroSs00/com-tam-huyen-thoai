using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component gắn lên mọi vật thể tương tác được (dụng cụ nấu ăn, nguyên liệu...).
/// Tự động thêm vào đúng layer "Interactable" khi Awake.
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Thông tin hiển thị")]
    [SerializeField] private string interactName = "Vật thể";
    [SerializeField] private string interactHint = "Nhấn [E] để xem";

    [Header("Sự kiện khi tương tác")]
    public UnityEvent onInteract;

    // ── IInteractable ────────────────────────────────────────────────────────
    public string InteractName => interactName;
    public string InteractHint => interactHint;

    public void Interact()
    {
        Debug.Log($"[Interact] {interactName}");
        onInteract?.Invoke();
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0)
        {
            // Set layer cho root VÀ tất cả children (vì FBX mesh thường nằm ở child)
            SetLayerRecursive(gameObject, layer);
        }
        else
        {
            Debug.LogWarning("[InteractableObject] Chua co layer 'Interactable'. " +
                             "Vao Edit → Project Settings → Tags and Layers de tao.");
        }
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
