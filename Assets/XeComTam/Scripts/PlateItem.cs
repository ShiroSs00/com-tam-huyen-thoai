using System.Collections.Generic;
using UnityEngine;

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

    private readonly HashSet<IngredientType> addedIngredients = new HashSet<IngredientType>();

    public bool IsFull => addedIngredients.IsSupersetOf(FULL_RECIPE);
    public IReadOnlyCollection<IngredientType> Ingredients => addedIngredients;
    public int RequiredCount => FULL_RECIPE.Count;

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
        Debug.Log($"[PlateItem] Them {type}. Con thieu: {GetMissingIngredients()}");

        if (IsFull)
            Debug.Log("[PlateItem] Dia day du! Dat len ban phuc vu.");

        return true;
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
