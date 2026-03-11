using UnityEngine;

/// <summary>
/// Quan ly vat player dang cam (1 vat tai 1 thoi diem).
/// Hien vat tai vi tri truoc mat camera (holdPoint).
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Vi tri cam vat")]
    [Tooltip("Empty child cua Camera, vi tri hien thi vat dang cam")]
    [SerializeField] private Transform holdPoint;

    [Tooltip("Khoang cach truoc mat camera")]
    [SerializeField] private float holdDistance = 0.5f;

    private IngredientPickup heldItem;

    public bool IsEmpty => heldItem == null;
    public IngredientPickup HeldItem => heldItem;

    private void Awake()
    {
        // Tu tao holdPoint neu chua co
        if (holdPoint == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                GameObject hp = new GameObject("HoldPoint");
                hp.transform.SetParent(cam.transform);
                hp.transform.localPosition = new Vector3(0f, -0.15f, holdDistance);
                hp.transform.localRotation = Quaternion.identity;
                holdPoint = hp.transform;
            }
        }
    }

    private void LateUpdate()
    {
        // Giup vat luon hien o truoc mat cam
        if (heldItem != null && holdPoint != null)
        {
            heldItem.transform.position = holdPoint.position;
            heldItem.transform.rotation = holdPoint.rotation;
        }
    }

    /// <summary>Cam vat len. Gan vao holdPoint, tat collider de tranh ray lan.</summary>
    public void PickUp(IngredientPickup item)
    {
        if (item == null) return;

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        // Tat collider de khong bat raycast len chinh vat dang cam
        foreach (var col in heldItem.GetComponentsInChildren<Collider>())
            col.enabled = false;

        Debug.Log($"[PlayerInventory] Dang cam: {item.IngredientType}");
    }

    /// <summary>Bo vat khoi tay. Neu destroy=true thi xoa luon.</summary>
    public void Drop(bool destroy = false)
    {
        if (heldItem == null) return;

        if (destroy)
        {
            Destroy(heldItem.gameObject);
        }
        else
        {
            heldItem.transform.SetParent(null);

            // Bat lai collider
            foreach (var col in heldItem.GetComponentsInChildren<Collider>())
                col.enabled = true;

            // Dat vat xuong dat truoc mat player
            heldItem.transform.position = transform.position
                + transform.forward * 0.6f
                + Vector3.up * 0.5f;
        }

        Debug.Log($"[PlayerInventory] Da bo: {heldItem?.IngredientType}");
        heldItem = null;
    }
}
