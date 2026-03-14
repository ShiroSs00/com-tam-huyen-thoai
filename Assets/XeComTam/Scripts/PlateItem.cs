using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct PlateModelMapping
{
    [Tooltip("Tên gợi nhớ trong Inspector (VD: Đĩa Cơm Thịt Trứng)")]
    public string modelName;
    [Tooltip("Danh sách món ăn cần có mặt trên đĩa để hiển thị model này")]
    public List<IngredientType> ingredients;
    [Tooltip("Model (Prefab) sẽ được hiển thị với tổ hợp món ăn trên")]
    public GameObject visualPrefab;
    [Tooltip("Tùy chỉnh vị trí của model này để vừa vặn với đĩa")]
    public Vector3 localOffset;
    [Tooltip("Tùy chỉnh góc xoay của model này để vừa vặn với đĩa")]
    public Vector3 localRotationOffset;
}

/// <summary>
/// Gan len prefab Dia.
/// Theo doi danh sach nguyen lieu da xep vao dia.
/// Khi du cong thuc → sang trang thai "Dia day".
/// </summary>
public class PlateItem : MonoBehaviour
{
    // Cong thuc day du mot dia com tam
    private static readonly HashSet<IngredientType> FULL_RECIPE = new HashSet<IngredientType>
    {
        IngredientType.Com,
        IngredientType.ThitChin,
        IngredientType.TrungChien,
        IngredientType.LatCaChua,
        IngredientType.LatDuaLeo
    };

    private HashSet<IngredientType> addedIngredients = new HashSet<IngredientType>();

    [Header("Visuals (Model của tổ hợp món ăn)")]
    [Tooltip("Kéo thả các Model Prefabs tương ứng (VD: Prefab Cơm+Thịt) từ Project vào đây")]
    [SerializeField] private List<PlateModelMapping> modelMappings = new List<PlateModelMapping>();

    private GameObject currentVisualInstance;

    public bool IsFull => addedIngredients.IsSupersetOf(FULL_RECIPE);
    public IReadOnlyCollection<IngredientType> Ingredients => addedIngredients;
    public int RequiredCount => FULL_RECIPE.Count;

    private void Awake()
    {
        addedIngredients.Clear();
        UpdateVisuals();
    }

    /// <summary>Them nguyen lieu vao dia. Tra ve false neu da co hoac khong hop le.</summary>
    public bool TryAddIngredient(IngredientType type)
    {
        if (type == IngredientType.Dia || type == IngredientType.None)
        {
            Debug.LogWarning("[PlateItem] Khong the them loai nay vao dia.");
            return false;
        }

        if (addedIngredients.Contains(type))
        {
            Debug.Log($"[PlateItem] Da co {type} trong dia roi.");
            return false;
        }

        addedIngredients.Add(type);
        UpdateVisuals();
        Debug.Log($"[PlateItem] Them {type}. Con thieu: {GetMissingIngredients()}");

        if (IsFull)
            Debug.Log("[PlateItem] Dia day du! Dat len ban phuc vu.");

        return true;
    }

    private void UpdateVisuals()
    {
        // 1. Xóa model hiện tại nếu có
        if (currentVisualInstance != null)
        {
            Destroy(currentVisualInstance);
            currentVisualInstance = null;
        }

        // 2. Tìm model có số nguyên liệu trùng nhiều nhất với đĩa hiện tại
        // Đảm bảo model đó KHÔNG yêu cầu nguyên liệu mà đĩa CHƯA CÓ.
        PlateModelMapping bestMatch = default;
        int maxMatchCount = -1;

        for (int i = 0; i < modelMappings.Count; i++)
        {
            var mapping = modelMappings[i];
            bool isValid = true;
            if (mapping.ingredients == null)
            {
                mapping.ingredients = new List<IngredientType>();
                modelMappings[i] = mapping;
            }
            
            // Kiểm tra xem tổ hợp này có đòi hỏi món nào đĩa chưa có không
            foreach (var req in mapping.ingredients)
            {
                if (!addedIngredients.Contains(req))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                if (mapping.ingredients.Count > maxMatchCount)
                {
                    maxMatchCount = mapping.ingredients.Count;
                    bestMatch = mapping;
                }
            }
        }

        // 3. Spawn model khớp nhất lên
        if (bestMatch.visualPrefab != null)
        {
            currentVisualInstance = Instantiate(bestMatch.visualPrefab, transform);
            currentVisualInstance.transform.localPosition = bestMatch.localOffset;
            currentVisualInstance.transform.localRotation = Quaternion.Euler(bestMatch.localRotationOffset);
            currentVisualInstance.transform.localScale = Vector3.one; // Đảm bảo scale phụ thuộc vào DiaCom base
        }
    }

    public string GetMissingIngredients()
    {
        var missing = new List<string>();
        foreach (var req in FULL_RECIPE)
            if (!addedIngredients.Contains(req))
                missing.Add(req.ToString());
        return missing.Count == 0 ? "Khong co" : string.Join(", ", missing);
    }
}
