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
        // Đặt layer đúng để PlayerInteraction raycast chính xác
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0)
            gameObject.layer = layer;
        else
            Debug.LogWarning("[InteractableObject] Chưa có layer 'Interactable'. " +
                             "Vào Edit → Project Settings → Tags and Layers để tạo.");
    }
}
