using UnityEngine;

/// <summary>
/// Gan len moi ghe (GheNhuaCao, GheNhuaNho).
/// Quan ly trang thai: trong / co NPC / NPC da an xong.
/// Lien ket voi ServingTable gan nhat.
/// </summary>
public class CustomerSeat : MonoBehaviour
{
    [Header("Lien ket")]
    [Tooltip("Ban phuc vu ke ben ghe nay")]
    [SerializeField] private ServingTable linkedTable;

    [Header("Vi tri ngoi")]
    [Tooltip("Vi tri NPC se di chuyen den de ngoi")]
    [SerializeField] private Transform sitPoint;

    private CustomerNPC occupant;

    public bool IsOccupied => occupant != null;
    public ServingTable LinkedTable => linkedTable;
    public Transform SitPoint => sitPoint != null ? sitPoint : transform;

    /// <summary>NPC goi phuong thuc nay khi chon ghe va ngoi xuong.</summary>
    public bool TryOccupy(CustomerNPC npc)
    {
        if (IsOccupied) return false;
        occupant = npc;
        return true;
    }

    /// <summary>Duoc goi boi ServingTable khi player dat dia len ban.</summary>
    public void NotifyFoodServed(PlateItem plate)
    {
        if (occupant != null)
            occupant.OnFoodReceived();
    }

    /// <summary>NPC goi khi roi ghe.</summary>
    public void Vacate()
    {
        occupant = null;
    }
}
