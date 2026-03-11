using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gan len Player — xu ly raycast phat hien va tuong tac voi IInteractable.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    [Tooltip("Transform cua camera (de lay huong nhin)")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Khoang cach toi da player co the tuong tac (m)")]
    [SerializeField] private float interactDistance = 4f;

    [Tooltip("Layer mask chi gom layer 'Interactable'. De 0 = tat ca layer de debug")]
    [SerializeField] private LayerMask interactLayer;

    [Header("UI")]
    [SerializeField] private InteractionUI interactionUI;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    // Private state
    private IInteractable currentTarget;
    private bool wantInteract;

    private void Update()
    {
        DetectTarget();

        if (wantInteract && currentTarget != null)
        {
            currentTarget.Interact();
            wantInteract = false;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            wantInteract = true;
            if (showDebugLog)
                Debug.Log($"[PlayerInteraction] E pressed. Target = {(currentTarget != null ? currentTarget.InteractName : "null")}");
        }
    }

    private void DetectTarget()
    {
        Transform origin = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (origin == null) return;

        Ray ray = new Ray(origin.position, origin.forward);
        RaycastHit hit;

        // Neu interactLayer = 0 (chua set) → raycast khong filter layer de debug
        bool useLayerMask = interactLayer.value != 0;
        bool didHit = useLayerMask
            ? Physics.Raycast(ray, out hit, interactDistance, interactLayer)
            : Physics.Raycast(ray, out hit, interactDistance);

        if (didHit)
        {
            if (showDebugLog)
                Debug.Log($"[Raycast] Hit: {hit.collider.gameObject.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                interactionUI?.Show(interactable.InteractName, interactable.InteractHint);
                return;
            }
            else if (showDebugLog)
            {
                Debug.Log($"[Raycast] Hit '{hit.collider.gameObject.name}' nhung KHONG co IInteractable! Check script.");
            }
        }

        currentTarget = null;
        interactionUI?.Hide();
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (origin == null) return;

        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(origin.position, origin.forward * interactDistance);
    }
}
