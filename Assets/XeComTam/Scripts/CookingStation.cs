using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tram nau co timer: Lo Nuong (nuong thit), Chao (chien trung).
/// Flow: player dat nguyen lieu vao → timer → san pham hoan chinh.
/// Khi co output: player E vao → lay san pham vao tay HOAC vao dia.
/// </summary>
public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("Thong tin")]
    [SerializeField] private string stationName = "Tram nau";

    [Header("Nguyen lieu & San pham")]
    [Tooltip("Loai nguyen lieu chap nhan dau vao")]
    [SerializeField] private IngredientType acceptedInput;

    [Tooltip("San pham sau khi nau xong")]
    [SerializeField] private GameObject outputPrefab;

    [Tooltip("Loai nguyen lieu dau ra")]
    [SerializeField] private IngredientType outputType;

    [Tooltip("Scale san pham khi lay ra tay hoac dat len dia")]
    [SerializeField] private float handScale = 0.3f;

    [Tooltip("Thoi gian nau (giay)")]
    [SerializeField] private float cookTime = 5f;

    [Header("Hien thi trong the gioi")]
    [Tooltip("Vi tri hien thi nguyen lieu dang nau")]
    [SerializeField] private Transform cookingSlot;

    [Header("Progress Bar UI (World Space Canvas)")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject progressBarRoot;

    // State
    private bool isCooking;
    private bool hasOutput;
    private GameObject currentIngredientVisual;

    public string InteractName => stationName;
    public string InteractHint => GetHint();

    private string GetHint()
    {
        if (hasOutput)
        {
            PlayerInventory inv = FindObjectOfType<PlayerInventory>();
            if (inv != null && !inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
                return $"Nhan [E] de them {outputType} vao dia";
            return "Nhan [E] de lay san pham";
        }
        if (isCooking) return "Dang nau...";
        return $"Nhan [E] khi cam {acceptedInput} de dat vao";
    }

    public void Interact()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null) return;

        // === Lay san pham da nau xong ===
        if (hasOutput)
        {
            if (outputPrefab == null)
            {
                Debug.LogError($"[CookingStation] {stationName}: Chua gan outputPrefab!");
                return;
            }

            // TH1: Dang cam Dia → them thang vao dia
            if (!inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
            {
                PlateItem plate = inv.HeldItem.GetComponent<PlateItem>();
                if (plate != null && plate.TryAddIngredient(outputType))
                {
                    // Spawn visual tren dia
                    SpawnVisualOnPlate(inv.HeldItem.transform);
                    ClearOutput();
                    Debug.Log($"[CookingStation] Them {outputType} vao dia.");
                }
                return;
            }

            // TH2: Tay trong → lay san pham vao tay
            if (inv.IsEmpty)
            {
                Vector3 spawnPos = cookingSlot != null ? cookingSlot.position
                                  : transform.position + Vector3.up * 0.5f;
                GameObject output = Instantiate(outputPrefab, spawnPos, Quaternion.identity);
                output.transform.localScale = Vector3.one * handScale; // Fix scale

                IngredientPickup pickup = output.GetComponent<IngredientPickup>();
                if (pickup == null) pickup = output.AddComponent<IngredientPickup>();
                pickup.Initialize(outputType);

                inv.PickUp(pickup);
                ClearOutput();
                Debug.Log($"[CookingStation] Da lay: {outputType} (scale={handScale})");
            }
            else
            {
                Debug.Log("[CookingStation] Dat dia xuong hoac lay san pham truoc.");
            }
            return;
        }

        // === Dat nguyen lieu vao de nau ===
        if (!isCooking)
        {
            if (inv.IsEmpty)
            {
                Debug.Log($"[CookingStation] Can cam {acceptedInput} truoc.");
                return;
            }

            IngredientPickup held = inv.HeldItem;
            if (held == null || held.IngredientType != acceptedInput)
            {
                Debug.Log($"[CookingStation] Can {acceptedInput}, dang cam {held?.IngredientType}");
                return;
            }

            // Lay nguyen lieu khoi tay va dat len station
            inv.Drop(destroy: false);

            if (cookingSlot != null)
            {
                held.transform.SetParent(cookingSlot);
                held.transform.localPosition = Vector3.zero;
                held.transform.localRotation = Quaternion.identity;
                held.transform.localScale    = Vector3.one * handScale; // Hien thi tren lo
            }
            currentIngredientVisual = held.gameObject;

            StartCoroutine(CookRoutine());
        }
    }

    private IEnumerator CookRoutine()
    {
        isCooking = true;
        if (progressBarRoot) progressBarRoot.SetActive(true);
        if (progressSlider)  progressSlider.value = 0f;

        float elapsed = 0f;
        while (elapsed < cookTime)
        {
            elapsed += Time.deltaTime;
            if (progressSlider) progressSlider.value = elapsed / cookTime;
            yield return null;
        }

        // Xoa visual nguyen lieu song
        if (currentIngredientVisual) Destroy(currentIngredientVisual);
        currentIngredientVisual = null;
        isCooking  = false;
        hasOutput  = true;
        if (progressSlider)  progressSlider.value = 1f;
        Debug.Log($"[CookingStation] {stationName}: NAU XONG! Nhan E de lay {outputType}.");
    }

    private void ClearOutput()
    {
        hasOutput = false;
        if (progressBarRoot) progressBarRoot.SetActive(false);
    }

    /// <summary>Spawn visual nguyen lieu chinh len mat dia.</summary>
    private void SpawnVisualOnPlate(Transform plateTransform)
    {
        if (outputPrefab == null) return;
        int idx = plateTransform.childCount;
        GameObject visual = Instantiate(outputPrefab, plateTransform);
        float angle = idx * 60f;
        float r = 0.25f;
        visual.transform.localPosition = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * r, 0.05f,
            Mathf.Sin(angle * Mathf.Deg2Rad) * r);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale    = Vector3.one * handScale;

        foreach (var col in visual.GetComponentsInChildren<Collider>())
            col.enabled = false;
        var p = visual.GetComponent<IngredientPickup>();
        if (p) p.enabled = false;
    }

    private void Awake()
    {
        if (progressBarRoot) progressBarRoot.SetActive(false);
    }
}
