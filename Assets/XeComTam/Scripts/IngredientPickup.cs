using UnityEngine;

/// <summary>
/// Script gan len moi vat the nguyen lieu duoc spawn ra (ThitSong, TrungSong, Com...).
/// Cho phep PlayerInventory cam len, va PlateItem nhan vao dia.
/// </summary>
public class IngredientPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private IngredientType ingredientType;

    public IngredientType IngredientType => ingredientType;

    // IInteractable — nguoi choi nhan E de cam len khi tay trong
    public string InteractName => ingredientType.ToString();
    public string InteractHint
    {
        get
        {
            PlayerInventory inv = FindObjectOfType<PlayerInventory>();
            if (inv == null) return "Nhan [E]";
            if (!inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
                return "Nhan [E] de them vao dia";
            return inv.IsEmpty ? "Nhan [E] de cam len" : "Tay dang ban";
        }
    }

    public void Interact()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null) return;

        // Neu dang cam Dia → them nguyen lieu vao dia
        if (!inv.IsEmpty && inv.HeldItem?.IngredientType == IngredientType.Dia)
        {
            PlateItem plate = inv.HeldItem.GetComponent<PlateItem>();
            if (plate != null && plate.TryAddIngredient(ingredientType))
            {
                Debug.Log($"[IngredientPickup] Da them {ingredientType} vao dia.");
                Destroy(gameObject);
            }
            return;
        }

        // Tay trong → cam nguyen lieu len
        if (inv.IsEmpty)
            inv.PickUp(this);
    }

    /// <summary>Goi sau khi spawn de dat loai nguyen lieu.</summary>
    public void Initialize(IngredientType type)
    {
        ingredientType = type;
    }
}
