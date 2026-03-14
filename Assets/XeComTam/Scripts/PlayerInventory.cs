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

    // Luu lai world scale va rotation GOC truoc khi pickup.
    // De khi Drop() tra vat ve dung kich thuoc + goc xoay ban dau (quan trong cho dat len ban).
    private Vector3    originalWorldScale;
    private Quaternion originalWorldRotation;

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
        // Vi vat da duoc gan lam con (SetParent) cua holdPoint o ham PickUp,
        // no se tu dong chay theo Camera.
        // Tuyet doi KHONG gan de transform.position o day nua,
        // neu khong moi chinh sua localPosition se bi reset ve 0.
    }

    /// <summary>Cam vat len. Gan vao holdPoint, tat collider de tranh ray lan.</summary>
    public void PickUp(IngredientPickup item)
    {
        if (item == null) return;

        heldItem = item;

        // Luu world scale va rotation GOC cua vat TRUOC khi gan vao holdPoint.
        // Se duoc dung de restore chinh xac khi Drop() dat vat xuong ban/san.
        originalWorldScale    = heldItem.transform.lossyScale;
        originalWorldRotation = heldItem.transform.rotation;

        if (heldItem.IngredientType == IngredientType.Dia)
        {
            // Dia: gan vao holdPoint va set scale/rot/pos thu cong de nhin dep tren tay
            heldItem.transform.SetParent(holdPoint);
            heldItem.transform.localScale    = new Vector3(30f, 30f, 30f);
            // Nghieng dia cho ngua thang len troi (-78 do)
            heldItem.transform.localRotation = Quaternion.Euler(-78f, 0f, 0f);
            // Dua ra phia truoc tam mat
            heldItem.transform.localPosition = new Vector3(0f, 0f, 0.35f);
        }
        else
        {
            // QUAN TRONG: Dung SetParentKeepWorldScale de giu nguyen kich thuoc
            // that su cua vat pham (world scale). Tranh vat bi be xiu khi
            // model goc co scale lon (vd: ThitSong scale=100) nhung holdPoint scale=1.
            TransformUtils.SetParentKeepWorldScale(heldItem.transform, holdPoint);
            heldItem.transform.localRotation = Quaternion.identity;
            heldItem.transform.localPosition = Vector3.zero;
        }

        // Tat collider de khong bat raycast len chinh vat dang cam
        foreach (var col in heldItem.GetComponentsInChildren<Collider>())
            col.enabled = false;

        Debug.Log($"[PlayerInventory] Dang cam: {item.IngredientType} | WorldScale goc: {originalWorldScale}");
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

            // QUAN TRONG: Restore world scale va rotation ve gia tri GOC
            // (truoc khi player cam vat). Dam bao vat dat len ban/san dung kich thuoc va huong.
            heldItem.transform.rotation   = originalWorldRotation;
            heldItem.transform.localScale = originalWorldScale; // parent = null → localScale = worldScale
        }

        Debug.Log($"[PlayerInventory] Da bo: {heldItem?.IngredientType}");
        heldItem = null;
    }
}
