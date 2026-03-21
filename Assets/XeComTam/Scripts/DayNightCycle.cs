using System;
using UnityEngine;

/// <summary>
/// Script quản lý chu kỳ ngày đêm: xoay mặt trời đúng với đồng hồ,
/// hiện mặt trăng và sao ban đêm (19:00 → 06:00).
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Thời gian")]
    [Tooltip("Số giây thực tế cho 1 ngày trong game")]
    [SerializeField] private float dayDuration = 1200f;

    [Tooltip("Thời điểm bắt đầu (0=0:00, 0.25=6:00, 0.5=12:00, 0.75=18:00)")]
    [Range(0, 1)]
    [SerializeField] private float currentTime = 0.35f;

    [Header("Cường độ ánh sáng mặt trời")]
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float minIntensity = 0.02f;

    [Header("Mặt trăng & Sao")]
    [SerializeField] private bool autoCreateMoonAndStars = true;
    [SerializeField] private float moonOrbitRadius = 400f;
    [SerializeField] private float moonSize = 25f;
    [SerializeField] private int starCount = 250;

    // ── Internal ──────────────────────────────────────────────────────────────
    private Light sunLight;
    private Transform moonTransform;
    private Light moonLight;
    private GameObject starGO;        // Lưu ref để bật/tắt
    private Camera mainCam;       // Cache camera để tránh null

    // Intensity curve tự build trong Awake (tránh Unity serialize ghi đè)
    private AnimationCurve intensityCurve;

    public float CurrentTime => currentTime;

    /// <summary>Fire khi thời gian quay lại 00:00 (hết 1 ngày).</summary>
    public event Action OnDayEnd;
    private bool dayEndFired;

    public string GetFormattedTime()
    {
        float totalHours = currentTime * 24f;
        int h = Mathf.FloorToInt(totalHours);
        int m = Mathf.FloorToInt((totalHours - h) * 60f);
        return string.Format("{0:00}:{1:00}", h, m);
    }

    // ── Unity ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        sunLight = GetComponent<Light>();

        // Build intensity curve theo kiểu mặt trời thực: đỉnh 12h, tối 0h/24h
        intensityCurve = new AnimationCurve(
            new Keyframe(0.00f, 0.00f),
            new Keyframe(0.20f, 0.00f),  // 4:48 – đêm
            new Keyframe(0.25f, 0.05f),  // 6:00 – bình minh
            new Keyframe(0.35f, 0.60f),  // 8:24 – sáng dần
            new Keyframe(0.50f, 1.00f),  // 12:00 – trưa sáng nhất
            new Keyframe(0.65f, 0.60f),  // 15:36 – chiều
            new Keyframe(0.75f, 0.05f),  // 18:00 – hoàng hôn
            new Keyframe(0.82f, 0.00f),  // 19:41 – tối hẳn
            new Keyframe(1.00f, 0.00f)
        );

        // Cache camera (tag MainCamera hoặc bất kỳ cam nào trong scene)
        mainCam = Camera.main;
        if (mainCam == null)
            mainCam = FindObjectOfType<Camera>();

        if (autoCreateMoonAndStars)
        {
            CreateMoon();
            CreateStars();
        }
    }

    private void Update()
    {
        float prevTime = currentTime;
        currentTime += Time.deltaTime / dayDuration;
        if (currentTime >= 1f)
        {
            currentTime -= 1f;

            // Phát sự kiện hết ngày (chỉ 1 lần mỗi vòng)
            if (!dayEndFired)
            {
                dayEndFired = true;
                OnDayEnd?.Invoke();
            }
        }
        else
        {
            // Reset flag khi đã qua khỏi mốc 0
            if (currentTime > 0.05f) dayEndFired = false;
        }

        // Công thức đúng: 6h→0°, 12h→90°, 18h→180°, 0h→270°
        float sunAngle = currentTime * 360f - 90f;
        transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

        if (sunLight != null)
        {
            float t = intensityCurve.Evaluate(currentTime);
            sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            sunLight.shadows = IsNight() ? LightShadows.None : LightShadows.Soft;
        }

        UpdateMoon();
        UpdateStars();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    // Ban đêm: 19:41 (0.82) → 6:00 (0.25) wrap qua nửa đêm
    private bool IsNight() => (currentTime > 0.82f || currentTime < 0.25f);

    private float NightAlpha()
    {
        // Fade vào đêm: 0.78 → 0.82
        if (currentTime >= 0.78f && currentTime <= 0.82f)
            return Mathf.InverseLerp(0.78f, 0.82f, currentTime);

        // Fade ra ngày: 0.23 → 0.27
        if (currentTime >= 0.23f && currentTime <= 0.27f)
            return Mathf.InverseLerp(0.27f, 0.23f, currentTime);

        return IsNight() ? 1f : 0f;
    }

    private Vector3 CamPos() => mainCam != null ? mainCam.transform.position : Vector3.zero;

    // ── Moon ──────────────────────────────────────────────────────────────────

    private void CreateMoon()
    {
        GameObject moonGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moonGO.name = "Moon (Auto)";
        Destroy(moonGO.GetComponent<Collider>());
        moonTransform = moonGO.transform;
        moonTransform.localScale = Vector3.one * moonSize;

        var mat = new Material(Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_Color", new Color(0.95f, 0.95f, 0.85f));
        mat.SetColor("_EmissionColor", new Color(0.9f, 0.9f, 0.75f) * 2.5f);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0f);
        moonGO.GetComponent<Renderer>().material = mat;

        var lGO = new GameObject("MoonLight (Auto)");
        lGO.transform.SetParent(moonGO.transform);
        moonLight = lGO.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.intensity = 0.12f;
        moonLight.color = new Color(0.7f, 0.8f, 1f);
        moonLight.shadows = LightShadows.None;

        moonGO.SetActive(false);
    }

    private void UpdateMoon()
    {
        if (moonTransform == null) return;

        float alpha = NightAlpha();
        bool show = alpha > 0f;
        if (moonTransform.gameObject.activeSelf != show)
            moonTransform.gameObject.SetActive(show);
        if (!show) return;

        float moonAngle = currentTime * 360f - 90f + 180f;
        Quaternion moonRot = Quaternion.Euler(moonAngle, -30f, 0f);
        moonTransform.position = CamPos() + moonRot * Vector3.forward * moonOrbitRadius;
        moonTransform.LookAt(CamPos());

        if (moonLight != null) moonLight.intensity = 0.12f * alpha;
    }

    // ── Stars ─────────────────────────────────────────────────────────────────

    private void CreateStars()
    {
        starGO = new GameObject("Stars (Auto)");
        var ps = starGO.AddComponent<ParticleSystem>();

        // FIX: spawn với màu trắng đục (alpha=1), dùng SetActive để bật/tắt
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = starCount;
        main.startLifetime = float.MaxValue;
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startColor = new Color(1f, 1f, 1f, 1f); // Luôn trắng đục, KHÔNG alpha=0
        main.startSpeed = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, starCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 480f;
        shape.position = Vector3.zero;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // Ưu tiên shader Unlit (sáng ngay cả khi đêm tối)
        Shader unlit = Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Legacy Shaders/Particles/Additive");
        if (unlit != null)
        {
            var mat = new Material(unlit);
            mat.SetColor("_Color", Color.white);
            renderer.material = mat;
        }

        ps.Play();
        // FIX: tắt ngay khi tạo, chỉ bật khi IsNight()
        starGO.SetActive(false);
    }

    private void UpdateStars()
    {
        if (starGO == null) return;

        float alpha = NightAlpha();
        bool show = alpha > 0f;

        // FIX: Chỉ cần bật/tắt object — particle đã spawn với alpha=1 rồi
        if (starGO.activeSelf != show)
            starGO.SetActive(show);

        if (!show) return;

        // Trung tâm theo camera
        if (mainCam != null)
            starGO.transform.position = mainCam.transform.position;
    }
}
