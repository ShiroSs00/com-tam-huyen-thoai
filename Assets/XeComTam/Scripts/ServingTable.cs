using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gan len ban (BanInox, BanNhua).
/// Ho tro NHIEU cho ngoi: moi ghe co 1 plateSlot rieng.
/// Khi player dat dia len ban (nhan E) → tim slot trong dau tien → dat dia vao → thong bao NPC.
/// </summary>
public class ServingTable : MonoBehaviour, IInteractable
{
    [Header("Thong tin")]
    [SerializeField] private string tableName = "Ban phuc vu";

    [Header("Goc xoay dia khi dat len ban")]
    [Tooltip("Chinh trong Inspector neu mat dia chua huong len troi")]
    [SerializeField] private Vector3 plateRestRotation = new Vector3(-76.791f, -16.817f, 17.247f);

    [Header("Ghe va Vi tri Dat Dia")]
    [Tooltip("Gan CustomerSeat cua ghe ngoi ke ben ban nay")]
    [SerializeField] private CustomerSeat linkedSeat;
    [Tooltip("Transform vi tri dat dia (con cua ban)")]
    [SerializeField] private Transform plateSlot;

    // Trang thai hien tai tren ban
    private PlateItem plateOnTable;

    // IInteractable
    public string InteractName => tableName;
    public string InteractHint
    {
        get
        {
            if (this == null) return string.Empty; // Guard chong MissingReferenceException

            PlayerInventory inv = FindObjectOfType<PlayerInventory>();
            if (inv != null && !inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
            {
                return plateOnTable == null ? "Nhan [E] de dat dia len ban" : "Ban da co dia";
            }

            return plateOnTable == null ? "Ban trong" : "Ban da co dia";
        }
    }

    public void Interact()
    {
        if (this == null) return; // Guard chong MissingReferenceException

        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null || inv.IsEmpty) 
        {
            Debug.Log("[ServingTable] PlayerInventory null hoac dang trong tay.");
            return;
        }

        IngredientPickup held = inv.HeldItem;
        if (held == null || held.IngredientType != IngredientType.Dia)
        {
            Debug.Log($"[ServingTable] Can cam Dia de dat len ban. Dang cam: {held?.IngredientType}");
            return;
        }

        PlateItem plate = held.GetComponent<PlateItem>();
        if (plate == null)
        {
            Debug.LogWarning("[ServingTable] Vat dang cam co type Dia nhung khong co PlateItem script!");
            return;
        }

        if (!plate.IsReadyToServe)
        {
            string currentIngs = string.Join(", ", plate.Ingredients);
            Debug.Log($"[ServingTable] Dia chua du nguyen lieu. Con thieu: {plate.GetMissingIngredients()} | Dang co: {currentIngs}");
            return;
        }

        if (plateOnTable != null)
        {
            Debug.Log("[ServingTable] Ban da co dia, khong the dat them!");
            return;
        }

        // Dat dia len ban:
        // Do PlayerInventory.Drop se modify transform.position/scale/rotation,
        // Ta lay ra component truoc tien
        Transform plateTransform = held.transform;
        
        // Drop de inventory clear reference
        inv.Drop(destroy: false);

        if (plateSlot != null)
        {
            // Drop da dua plateTransform tro ve the gioi thuc (Scale 20, 20, 20).
            // Bay gio dua no vao ban (Scale 100, 100, 100).
            plateTransform.SetParent(plateSlot, true);
            
            // Xoa bo moi li le lech toa do ma SetParent gay ra
            plateTransform.localPosition = Vector3.zero;
            
            // Ép góc xoay
            plateTransform.localRotation = Quaternion.Euler(plateRestRotation);

            // Bắt buộc scale cục bộ phải bù trừ cho mớ hỗn độn của Bàn
            // Bàn tỉ lệ 100 -> con phải là 0.2 thì mới hiển thị đúng kích cỡ 20 gốc của Đĩa
            plateTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
        else
        {
            held.transform.SetParent(null);
            held.transform.position = transform.position + Vector3.up * 0.85f;
            held.transform.rotation = Quaternion.identity;
        }

        plateOnTable = plate;

        Debug.Log("[ServingTable] Da dat dia len ban!");

        // Thong bao NPC
        if (linkedSeat != null)
            linkedSeat.NotifyFoodServed(plate);
        else
            Debug.LogWarning("[ServingTable] Ban chua gan linkedSeat!");
    }

    /// <summary>Duoc goi boi CustomerNPC khi an xong hoac khi can don ban.</summary>
    public void ClearPlate()
    {
        if (plateOnTable != null)
        {
            Destroy(plateOnTable.gameObject);
            plateOnTable = null;
        }

        // Failsafe: Dọn sạch mọi object còn sót lại đang làm con của plateSlot
        // Đảm bảo đĩa cơm biến mất 100% về mặt thị giác dù có bị lỗi reference.
        if (plateSlot != null)
        {
            for (int i = plateSlot.childCount - 1; i >= 0; i--)
            {
                Destroy(plateSlot.GetChild(i).gameObject);
            }
        }
        
        Debug.Log("[ServingTable] Da don dia khoi ban.");
    }
}

