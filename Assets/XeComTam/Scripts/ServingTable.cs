using UnityEngine;

/// <summary>
/// Gan len cac ban (BanInox, BanNhua).
/// Khi player dat dia len ban (nhan E trong khi cam dia) →
/// kiem tra xem co NPC nao dang cho khong → thong bao NPC bat dau an.
/// </summary>
public class ServingTable : MonoBehaviour, IInteractable
{
    [Header("Thong tin")]
    [SerializeField] private string tableName = "Ban phuc vu";

    [Header("Ghe NPC gan nhat")]
    [Tooltip("Gan CustomerSeat cua ghe ngoi ke ben ban nay")]
    [SerializeField] private CustomerSeat linkedSeat;

    [Header("Vi tri dat dia")]
    [Tooltip("Vi tri hien thi dia tren ban (de trong thi dung tam ban)")]
    [SerializeField] private Transform plateSlot;

    private PlateItem plateOnTable;

    // IInteractable
    public string InteractName => tableName;
    public string InteractHint
    {
        get
        {
            PlayerInventory inv = FindObjectOfType<PlayerInventory>();
            if (inv != null && !inv.IsEmpty
                && inv.HeldItem?.IngredientType == IngredientType.Dia)
                return "Nhan [E] de dat dia len ban";
            return plateOnTable != null ? "Ban dang co dia" : "Ban trong";
        }
    }

    public void Interact()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null || inv.IsEmpty) return;

        IngredientPickup held = inv.HeldItem;
        if (held == null || held.IngredientType != IngredientType.Dia)
        {
            Debug.Log("[ServingTable] Can cam Dia de dat len ban.");
            return;
        }

        PlateItem plate = held.GetComponent<PlateItem>();
        if (plate == null)
        {
            Debug.LogWarning("[ServingTable] Vat dang cam khong co PlateItem!");
            return;
        }

        if (!plate.IsFull)
        {
            Debug.Log($"[ServingTable] Dia chua du nguyen lieu. Con thieu: {plate.GetMissingIngredients()}");
            return;
        }

        // Dat dia len ban
        inv.Drop(destroy: false);
        Vector3 slotPos = plateSlot != null ? plateSlot.position : transform.position + Vector3.up * 0.1f;
        held.transform.SetParent(null);
        held.transform.position = slotPos;
        held.transform.rotation = Quaternion.identity;
        plateOnTable = plate;

        Debug.Log("[ServingTable] Da dat dia len ban!");

        // Thong bao NPC
        if (linkedSeat != null)
            linkedSeat.NotifyFoodServed(plate);
        else
            Debug.LogWarning("[ServingTable] Chua gan linkedSeat!");
    }

    /// <summary>Duoc goi boi CustomerNPC khi an xong, xoa dia khoi ban.</summary>
    public void ClearPlate()
    {
        if (plateOnTable != null)
        {
            Destroy(plateOnTable.gameObject);
            plateOnTable = null;
        }
    }
}
