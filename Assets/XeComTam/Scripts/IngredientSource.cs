using UnityEngine;

/// <summary>
/// Gan len cac vat the nguon nguyen lieu.
/// Flow:
///   - Neu nguyen lieu phai nau truoc (ThitSong, TrungSong) → chi cam len, den station nau
///   - Neu nguyen lieu an duoc luon (Com, LatCaChua, LatDuaLeo) → cam dia E vao la them thang
///   - Neu tay trong → cam nguyen lieu len tay
/// </summary>
public class IngredientSource : MonoBehaviour, IInteractable
{
    [Header("Thong tin")]
    [SerializeField] private string sourceName = "Nguon nguyen lieu";
    [SerializeField] private string hint = "Nhan [E] de lay";

    [Header("Spawn")]
    [Tooltip("Prefab spawn khi lay nguyen lieu")]
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private IngredientType ingredientType = IngredientType.None;

    [Tooltip("Can phai nau truoc moi vao dia duoc (ThitSong, TrungSong)")]
    [SerializeField] private bool requiresCooking = false;

    [Tooltip("Scale cua vat khi cam tren tay (chinh de vua man hinh)")]
    [SerializeField] private float handScale = 0.3f;

    [Tooltip("Thoi gian hoi phuc giua 2 lan lay (giay)")]
    [SerializeField] private float cooldown = 0.5f;

    private float nextAvailableTime;

    public string InteractName => sourceName;
    public string InteractHint
    {
        get
        {
            if (requiresCooking) return $"Nhan [E] de lay {ingredientType} (can nau truoc)";
            PlayerInventory inv = FindObjectOfType<PlayerInventory>();
            if (inv != null && !inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
                return $"Nhan [E] de them {ingredientType} vao dia";
            return hint;
        }
    }

    public void Interact()
    {
        if (Time.time < nextAvailableTime) return;
        if (ingredientPrefab == null)
        {
            Debug.LogError($"[IngredientSource] {sourceName}: Chua gan ingredientPrefab!");
            return;
        }

        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null) return;

        // Nguyen lieu can nau → chi cam len tay, KHONG them vao dia
        if (requiresCooking)
        {
            if (!inv.IsEmpty)
            {
                Debug.Log($"[IngredientSource] {ingredientType} can nau truoc. Dat do dang cam xuong truoc.");
                return;
            }
            SpawnToHand(inv);
            return;
        }

        // Nguyen lieu an ngay (Com, LatCaChua, LatDuaLeo):
        //  - Dang cam Dia → them thang
        //  - Tay trong → cam len
        if (!inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
        {
            PlateItem plate = inv.HeldItem.GetComponent<PlateItem>();
            if (plate != null)
            {
                if (plate.TryAddIngredient(ingredientType))
                {
                    SpawnVisualOnPlate(inv.HeldItem.transform, plate.Ingredients.Count);
                    nextAvailableTime = Time.time + cooldown;
                    Debug.Log($"[IngredientSource] Da them {ingredientType} vao dia. ({plate.Ingredients.Count}/{plate.RequiredCount})");
                }
                else
                {
                    Debug.Log($"[IngredientSource] Dia day hoac da co {ingredientType} roi.");
                }
            }
            return;
        }

        // Tay trong
        if (inv.IsEmpty)
        {
            SpawnToHand(inv);
        }
        else
        {
            Debug.Log("[IngredientSource] Tay dang cam do. Dat xuong hoac cam dia truoc.");
        }
    }

    private void SpawnToHand(PlayerInventory inv)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject spawned = Instantiate(ingredientPrefab, spawnPos, Quaternion.identity);
        spawned.transform.localScale = Vector3.one * handScale; // Set scale khi cam

        IngredientPickup pickup = spawned.GetComponent<IngredientPickup>();
        if (pickup == null) pickup = spawned.AddComponent<IngredientPickup>();
        pickup.Initialize(ingredientType);

        inv.PickUp(pickup);
        nextAvailableTime = Time.time + cooldown;
        Debug.Log($"[IngredientSource] Da cam: {ingredientType} (scale={handScale})");
    }

    private void SpawnVisualOnPlate(Transform plateTransform, int count)
    {
        if (ingredientPrefab == null) return;
        GameObject visual = Instantiate(ingredientPrefab, plateTransform);

        // Xep nguyen lieu quanh tam dia theo hinh quat
        float angle = (count - 1) * 60f;
        float r = 0.3f; // world-relative radius tren mat dia
        visual.transform.localPosition = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * r,
            0.05f,
            Mathf.Sin(angle * Mathf.Deg2Rad) * r);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale    = Vector3.one * 0.8f;

        // Tat collider va script de khong bi pickup
        foreach (var col in visual.GetComponentsInChildren<Collider>())
            col.enabled = false;
        var p = visual.GetComponent<IngredientPickup>();
        if (p) p.enabled = false;
    }
}
