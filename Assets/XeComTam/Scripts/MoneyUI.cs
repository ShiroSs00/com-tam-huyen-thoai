using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Hien thi tien tren UI.
/// Gan len Canvas chua:
///   - Txt_Total: TextMeshPro hien tong tien goc tren
///   - Txt_Earned: TextMeshPro hien "+35.000d" fade out
/// </summary>
public class MoneyUI : MonoBehaviour
{
    [Header("Text references")]
    [SerializeField] private TextMeshProUGUI txtTotal;
    [SerializeField] private TextMeshProUGUI txtEarned;

    [Header("Hieu ung earned")]
    [SerializeField] private float earnedDisplayTime = 2f;
    [SerializeField] private float earnedFadeSpeed = 2f;

    private void Start()
    {
        if (txtEarned != null)
        {
            var c = txtEarned.color;
            c.a = 0f;
            txtEarned.color = c;
        }
        UpdateTotal(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentMoney : 0);
    }

    public void UpdateTotal(int amount)
    {
        if (txtTotal != null)
            txtTotal.text = $"{amount:N0} d";
    }

    public void ShowEarned(int amount)
    {
        if (txtEarned == null) return;
        txtEarned.text = $"+{amount:N0} d";
        StopAllCoroutines();
        StartCoroutine(FadeEarned());
    }

    private IEnumerator FadeEarned()
    {
        // Hien ngay
        var c = txtEarned.color;
        c.a = 1f;
        txtEarned.color = c;

        yield return new WaitForSeconds(earnedDisplayTime);

        // Fade out
        while (c.a > 0f)
        {
            c.a -= earnedFadeSpeed * Time.deltaTime;
            txtEarned.color = c;
            yield return null;
        }
    }
}
