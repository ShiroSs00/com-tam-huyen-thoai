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
    public string InteractName => this == null ? "" : interactName;
    public string InteractHint => this == null ? "" : interactHint;

    public void Interact()
    {
        if (this == null) return;
        Debug.Log($"[Interact] {interactName}");
        onInteract?.Invoke();
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0)
        {
            // CHI set layer cho chinh GameObject nay va nhung child KHONG co IInteractable khac.
            // Tranh ghi de layer cua PlateSlot, GheNPC hoac cac object con co script rieng.
            SetLayerSafe(gameObject, layer);
        }
        else
        {
            Debug.LogWarning("[InteractableObject] Chua co layer 'Interactable'. " +
                             "Vao Edit → Project Settings → Tags and Layers de tao.");
        }
    }

    /// <summary>
    /// Set layer cho go va children, nhung KHONG ghi de neu child da co IInteractable rieng.
    /// Dieu nay tranh Raycast bi boi roi khi nhieu IInteractable chong lan nhau.
    /// </summary>
    private static void SetLayerSafe(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            // Neu child da co IInteractable rieng thi de no tu quan ly layer cua no
            bool childHasOwnInteractable = child.GetComponent<IInteractable>() != null
                                           && child.GetComponent<InteractableObject>() == null; 
            // InteractableObject chinh no implement IInteractable nen phai loai tru no ra
            // De don gian: chi check xem co COMPONENT khac implement IInteractable khong
            if (!childHasOwnInteractable)
                SetLayerSafe(child.gameObject, layer);
        }
    }
}
