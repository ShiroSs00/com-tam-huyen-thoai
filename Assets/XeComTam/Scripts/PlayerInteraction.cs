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

        if (wantInteract && IsValid(currentTarget))
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
                Debug.Log($"[PlayerInteraction] E pressed. Target = {(IsValid(currentTarget) ? currentTarget.InteractName : "null")}");
        }
    }

    private void DetectTarget()
    {
        // Xoa stale reference neu object da bi destroy (interface check khong qua Unity null override)
        if (!IsValid(currentTarget))
            currentTarget = null;

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

            // QUAN TRONG 2: Phai set includeInactive = true. Neu false, Unity se kiem tra .enabled
            // de loc object. Kiem tra .enabled tren object da bi destroy se gay ra MissingReferenceException
            // tu ben duoi C++ engine cua Unity!
            IInteractable[] interactables = hit.collider.GetComponentsInParent<IInteractable>(true);
            
            foreach (var interactable in interactables)
            {
                // QUAN TRONG: Phai cast sang Component de Unity null override hoat dong dung
                // Interface != null se TRUE ngay ca khi Unity script instance da bi destroy!
                if (IsValid(interactable))
                {
                    // Bo qua nhung object bi tat (activeSelf == false hoac enabled == false)
                    var comp = interactable as Behaviour;
                    if (comp != null && !comp.isActiveAndEnabled) continue;

                    currentTarget = interactable;
                    interactionUI?.Show(interactable.InteractName, interactable.InteractHint);
                    return;
                }
            }

            if (showDebugLog)
            {
                Debug.Log($"[Raycast] Hit '{hit.collider.gameObject.name}' nhung KHONG co IInteractable hop le! Check script.");
            }
        }

        currentTarget = null;
        interactionUI?.Hide();
    }

    /// <summary>
    /// Kiem tra an toan cho IInteractable: phai qua Unity null check (khong dung interface != null).
    /// Interface ne null check KHONG di qua Unity operator override → MissingReferenceException.
    /// </summary>
    private static bool IsValid(IInteractable target)
    {
        if (target == null) return false; // C# reference is null
        
        // Nếu object là MonoBehaviour/Component, phải dùng Unity null check để biết nó bị destroy chưa
        if (target is UnityEngine.Object unityObj)
        {
            return unityObj != null; // unityObj != null trả về FALSE nếu script đã bị destroy
        }
        
        return true; // Là pure C# class (không phải Unity object)
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (origin == null) return;

        Gizmos.color = IsValid(currentTarget) ? Color.green : Color.yellow;
        Gizmos.DrawRay(origin.position, origin.forward * interactDistance);
    }
}
