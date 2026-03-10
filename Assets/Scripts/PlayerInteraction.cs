using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gắn lên Player — xử lý raycast phát hiện và tương tác với IInteractable.
/// Yêu cầu: Camera phải là con của cameraPivot (hoặc chính là cameraPivot).
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [Tooltip("Transform của camera (để lấy hướng nhìn)")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Khoảng cách tối đa player có thể tương tác (m)")]
    [SerializeField] private float interactDistance = 2.5f;

    [Tooltip("Layer mask chỉ gồm layer 'Interactable'")]
    [SerializeField] private LayerMask interactLayer;

    [Header("UI")]
    [SerializeField] private InteractionUI interactionUI;

    // ── Private state ─────────────────────────────────────────────────────────
    private IInteractable currentTarget;
    private bool wantInteract;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Update()
    {
        DetectTarget();

        if (wantInteract && currentTarget != null)
        {
            currentTarget.Interact();
            wantInteract = false;
        }
    }

    // ── Input System callback (cần PlayerInput component trên Player) ─────────
    /// <summary>Được gọi bởi PlayerInput khi nhấn phím Interact (E).</summary>
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            wantInteract = true;
    }

    // ── Logic ─────────────────────────────────────────────────────────────────
    private void DetectTarget()
    {
        // Nếu chưa gán camera thì dùng Camera.main
        Transform origin = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (origin == null) return;

        Ray ray = new Ray(origin.position, origin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                interactionUI?.Show(interactable.InteractName, interactable.InteractHint);
                return;
            }
        }

        // Không hit → ẩn UI và xóa target
        currentTarget = null;
        interactionUI?.Hide();
    }

    // ── Gizmo debug ──────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Transform origin = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (origin == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin.position, origin.forward * interactDistance);
    }
}
