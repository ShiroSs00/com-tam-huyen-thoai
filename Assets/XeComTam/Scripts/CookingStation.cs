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

    [Tooltip("San pham sinh ra (vd: ThitChin)")]
    [SerializeField] private GameObject outputPrefab;

    [Tooltip("Model hien thi DANG NAU (vd: Trung dap vao chao). De trong neu dung chinh model nguyen lieu dau vao.")]
    [SerializeField] private GameObject cookingVisualPrefab;

    [Header("Visual Ajustments")]
    [Tooltip("Doi vi tri nguyen lieu SONG tren bep")]
    public Vector3 rawOffsetPosition = Vector3.zero;
    [Tooltip("Xoay nguyen lieu SONG tren bep")]
    public Vector3 rawOffsetRotation = Vector3.zero;

    [Tooltip("Doi vi tri nguyen lieu CHIN tren bep")]
    public Vector3 cookedOffsetPosition = Vector3.zero;
    [Tooltip("Xoay nguyen lieu CHIN tren bep")]
    public Vector3 cookedOffsetRotation = Vector3.zero;

    [Header("Nau An")]
    [Tooltip("Loai nguyen lieu dau ra")]
    [SerializeField] private IngredientType outputType;

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
                GameObject output = Instantiate(outputPrefab, spawnPos, outputPrefab.transform.rotation);
                TransformUtils.ForceEnableRenderers(output); // Fix mat mesh fbx

                IngredientPickup pickup = output.GetComponent<IngredientPickup>();
                if (pickup == null) pickup = output.AddComponent<IngredientPickup>();
                pickup.Initialize(outputType);

                inv.PickUp(pickup);
                ClearOutput();
                Debug.Log($"[CookingStation] Da lay: {outputType}");
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
            if (cookingVisualPrefab != null)
            {
                inv.Drop(destroy: true); // Xoa bo nguyen lieu goc
                
                Vector3 startPos = cookingSlot != null ? cookingSlot.position : transform.position;
                GameObject cookingVisual = Instantiate(cookingVisualPrefab, startPos, Quaternion.Euler(rawOffsetRotation));
                
                if (cookingSlot != null)
                {
                    TransformUtils.SetParentKeepWorldScale(cookingVisual.transform, cookingSlot);
                    Vector3 parentS = cookingSlot.lossyScale;
                    cookingVisual.transform.localPosition = new Vector3(
                        parentS.x != 0 ? rawOffsetPosition.x / parentS.x : rawOffsetPosition.x,
                        parentS.y != 0 ? rawOffsetPosition.y / parentS.y : rawOffsetPosition.y,
                        parentS.z != 0 ? rawOffsetPosition.z / parentS.z : rawOffsetPosition.z
                    );
                    cookingVisual.transform.localRotation = Quaternion.Euler(rawOffsetRotation);
                }
                currentIngredientVisual = cookingVisual;
            }
            else
            {
                inv.Drop(destroy: false); // Giu nguyen nguyen lieu goc
                
                if (cookingSlot != null)
                {
                    TransformUtils.SetParentKeepWorldScale(held.transform, cookingSlot);
                    // Trick de khi User dien 0.12 (m) ra Inspector, game se hieu do la do cao the gioi 
                    // vi Lò Nuong dang co do Scale khong lo (VD: 100) ma User khong hay biet.
                    Vector3 parentS = cookingSlot.lossyScale;
                    held.transform.localPosition = new Vector3(
                        parentS.x != 0 ? rawOffsetPosition.x / parentS.x : rawOffsetPosition.x,
                        parentS.y != 0 ? rawOffsetPosition.y / parentS.y : rawOffsetPosition.y,
                        parentS.z != 0 ? rawOffsetPosition.z / parentS.z : rawOffsetPosition.z
                    );
                    held.transform.localRotation = Quaternion.Euler(rawOffsetRotation);
                }
                currentIngredientVisual = held.gameObject;
            }

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

        // Sinh ra visual cua nguyen lieu chin tren bep luon de user thay doi tu song -> chin
        if (outputPrefab != null)
        {
            Vector3 visualPos = cookingSlot != null ? cookingSlot.position : transform.position + Vector3.up * 0.5f;
            currentIngredientVisual = Instantiate(outputPrefab, visualPos, Quaternion.Euler(cookedOffsetRotation));
            
            if (cookingSlot != null)
            {
                TransformUtils.SetParentKeepWorldScale(currentIngredientVisual.transform, cookingSlot);
                
                // Same trick: bu dap Parent Lossy Scale khong lo de thit ko bay len troi met
                Vector3 parentS = cookingSlot.lossyScale;
                currentIngredientVisual.transform.localPosition = new Vector3(
                    parentS.x != 0 ? cookedOffsetPosition.x / parentS.x : cookedOffsetPosition.x,
                    parentS.y != 0 ? cookedOffsetPosition.y / parentS.y : cookedOffsetPosition.y,
                    parentS.z != 0 ? cookedOffsetPosition.z / parentS.z : cookedOffsetPosition.z
                );
                currentIngredientVisual.transform.localRotation = Quaternion.Euler(cookedOffsetRotation);
            }
            TransformUtils.ForceEnableRenderers(currentIngredientVisual);

            // TAT TUONG TAC: De tranh bi loi tia raycast hoac tu dong roi Physics do collider 
            foreach (var col in currentIngredientVisual.GetComponentsInChildren<Collider>())
                col.enabled = false;
            var pickupScript = currentIngredientVisual.GetComponent<IngredientPickup>();
            if (pickupScript) pickupScript.enabled = false;

            Debug.Log($"[CookingStation] Da hien thi nguyen lieu chin tren bep: {outputPrefab.name}\n" +
                      $" - Toa do The Gioi (WorldPos): {currentIngredientVisual.transform.position}\n" +
                      $" - Toa do Bep (LocalPos): {currentIngredientVisual.transform.localPosition}\n" +
                      $" - The tich The Gioi (LossyScale): {currentIngredientVisual.transform.lossyScale}\n" +
                      $" - The tich rieng (LocalScale): {currentIngredientVisual.transform.localScale}");
        }
        else 
        {
            Debug.LogError($"[LOI NANG] Bep {stationName} CHUA DUOC GAN PREFAB THIT CHIN (Output Prefab) o Inspector! No se khong the hien hinh.");
        }

        isCooking  = false;
        hasOutput  = true;
        if (progressSlider)  progressSlider.value = 1f;
        Debug.Log($"[CookingStation] {stationName}: NAU XONG! Nhan E de lay {outputType}.");
    }

    private void ClearOutput()
    {
        hasOutput = false;
        if (progressBarRoot) progressBarRoot.SetActive(false);
        if (currentIngredientVisual) Destroy(currentIngredientVisual);
        currentIngredientVisual = null;
    }

    private void Awake()
    {
        if (progressBarRoot) progressBarRoot.SetActive(false);
    }
}
