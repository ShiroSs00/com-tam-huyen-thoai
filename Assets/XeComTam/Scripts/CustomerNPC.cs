using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Script hợp nhất: Spawner + Walker + Customer.
/// 
/// CHẾ ĐỘ 1 — SPAWNER: Gắn lên Empty GameObject trên đường.
///   Xoay forward dọc theo đường. Tự spawn NPC đi bộ, một số rẽ vào ăn.
///   
/// CHẾ ĐỘ 2 — NPC: Được spawn tự động. Đi bộ trên đường,
///   có thể rẽ vào ăn cơm → ngồi → chờ → ăn → trả tiền → đi.
/// </summary>
public class CustomerNPC : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ROLE: Spawner hay NPC?
    // ═══════════════════════════════════════════════════════════════════════════

    public enum Role { Spawner, Walker, Customer }

    [Header("=== Vai trò ===")]
    [Tooltip("Spawner = đặt trên đường để spawn NPC. Walker/Customer sẽ tự gán khi spawn.")]
    [SerializeField] private Role role = Role.Spawner;

    // ═══════════════════════════════════════════════════════════════════════════
    // SPAWNER SETTINGS (chỉ dùng khi role = Spawner)
    // ═══════════════════════════════════════════════════════════════════════════

    [Header("=== Spawner Settings ===")]
    [Tooltip("Khoảng thời gian spawn 1 NPC mới (giây)")]
    [SerializeField] private float spawnInterval = 3f;

    [Tooltip("Số NPC tối đa cùng lúc")]
    [SerializeField] private int maxPedestrians = 15;

    [Tooltip("Chiều rộng đường")]
    [SerializeField] private float roadWidth = 6f;

    [Tooltip("Khoảng cách NPC đi trước khi biến mất (m)")]
    [SerializeField] private float walkDistance = 60f;

    [Tooltip("% xác suất NPC rẽ vào ăn (0-100)")]
    [SerializeField] private float chanceToEat = 30f;

    [Tooltip("Khoảng cách phát hiện ghế ăn (m)")]
    [SerializeField] private float seatDetectionRadius = 15f;

    [Header("=== Debug & Visuals ===")]
    [SerializeField] private bool showGizmos = true;
    
    [Tooltip("Danh sách các Prefab NPC 3D. Nếu trống, script sẽ tự sinh ra hình Capsule.")]
    [SerializeField] private GameObject[] npcPrefabs;

    private int currentSpawnCount = 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // NPC SETTINGS (dùng khi role = Walker hoặc Customer)
    // ═══════════════════════════════════════════════════════════════════════════

    public enum State { Idle, Walking, WalkToSeat, Seated, ShowOrder, WaitFood, Eating, Paying, Leave }

    [Header("=== NPC State (debug) ===")]
    [SerializeField] private State currentState = State.Idle;

    [Header("=== Tham số NPC ===")]
    [SerializeField] private float eatTime = 8f;
    [SerializeField] private float payDelay = 0.5f;
    [SerializeField] private float leaveDelay = 1.5f;

    [Header("=== Kiên nhẫn (Patience) ===")]
    [Tooltip("Tổng thời gian chờ đồ ăn (giây)")]
    [SerializeField] private float maxPatienceTime = 60f;

    [Tooltip("% tiền bị trừ khi phục vụ sau nửa thời gian")]
    [SerializeField] private float latePenaltyPercent = 50f;

    [Header("=== Model ===")]
    [SerializeField] private Transform modelRoot;

    [Header("=== Order Bubble ===")]
    [SerializeField] private GameObject orderBubble;

    [Header("=== Patience Bar UI ===")]
    [Tooltip("Prefab UI thanh chờ")]
    [SerializeField] private GameObject patienceBarPrefab;

    [Tooltip("Hoặc kéo trực tiếp GameObject thanh chờ")]
    [SerializeField] private GameObject patienceBarObject;

    [Tooltip("Image Fill của thanh chờ (tự tìm nếu để trống)")]
    [SerializeField] private Image patienceFillImage;

    // ── Internal state ────────────────────────────────────────────────────────

    private NavMeshAgent agent;
    private CustomerSeat assignedSeat;
    private CustomerNPC spawnerRef;
    private float patienceRemaining;
    private bool passedHalf;
    private Animator anim;

    // Walker state
    private Vector3 walkerDestination;
    private bool wantsToEat;
    private float seatDetectRadius;
    private bool hasBecomCustomer;

    // ═══════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        // Luôn lấy agent (dù role chưa được gán đúng lúc Awake chạy)
        agent = GetComponent<NavMeshAgent>();
        if (orderBubble) orderBubble.SetActive(false);

        // Lấy Animator từ con hoặc chính nó để điều khiển Animation (Đi bộ / Ngồi)
        anim = GetComponentInChildren<Animator>();
    }

    /// <summary>Khởi tạo visual + UI cho NPC (gọi khi bắt đầu routine)</summary>
    private void InitializeNPCVisual()
    {
        // Tạo Capsule nếu chưa có model
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            CreateCapsuleVisual(gameObject, new Color(
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f)
            ));
        }

        SetupPatienceBar();
    }

    private void Start()
    {
        if (role == Role.Spawner)
        {
            StartCoroutine(SpawnLoop());
            return;
        }

        if (role == Role.Walker)
        {
            StartCoroutine(WalkerRoutine());
        }
        else if (role == Role.Customer)
        {
            StartCoroutine(CustomerLifecycle());
        }
    }

    private void Update()
    {
        if (role == Role.Spawner) return;

        // Điều khiển Animation
        if (anim != null)
        {
            // Trạng thái Ngồi
            bool isSitting = (currentState >= State.Seated && currentState <= State.Eating);
            anim.SetBool("IsSeated", isSitting);

            // Tốc độ di chuyển (Đi bộ)
            if (agent != null && agent.enabled)
            {
                anim.SetFloat("Speed", agent.velocity.magnitude);
            }
        }

        // Thanh chờ quay về phía camera
        if (patienceBarObject != null && patienceBarObject.activeSelf)
        {
            Camera cam = Camera.main;
            if (cam != null)
                patienceBarObject.transform.forward = cam.transform.forward;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SPAWNER LOGIC
    // ═══════════════════════════════════════════════════════════════════════════

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (currentSpawnCount < maxPedestrians)
                SpawnPedestrian();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPedestrian()
    {
        float lateralOffset = Random.Range(-roadWidth / 2f, roadWidth / 2f);
        Vector3 spawnPos = transform.position + transform.right * lateralOffset;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
            return;

        GameObject npcGO;

        // Nếu có Prefab 3D từ người chơi thì sẽ sinh ra Prefab đó, ngược lại sinh Game Object trống (Capsule)
        if (npcPrefabs != null && npcPrefabs.Length > 0)
        {
            GameObject prefabToSpawn = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
            npcGO = Instantiate(prefabToSpawn, hit.position, Quaternion.identity);
            npcGO.name = "Customer NPC (3D)";
        }
        else
        {
            npcGO = new GameObject("Pedestrian (Auto)");
            npcGO.transform.position = hit.position;
        }

        // --- Cài đặt NavMesh Agent ---
        NavMeshAgent npcAgent = npcGO.GetComponent<NavMeshAgent>();
        if (npcAgent == null) npcAgent = npcGO.AddComponent<NavMeshAgent>();
        
        npcAgent.speed = Random.Range(2f, 4f);
        npcAgent.angularSpeed = 120f;
        npcAgent.stoppingDistance = 0.5f;
        npcAgent.radius = 0.3f;
        npcAgent.height = 2f;

        // --- Cài đặt Script CustomerNPC ---
        CustomerNPC npc = npcGO.GetComponent<CustomerNPC>();
        if (npc == null) npc = npcGO.AddComponent<CustomerNPC>();

        // Truyền các thông số vào để nó chạy
        npc.role = Role.Walker;
        npc.spawnerRef = this;
        npc.walkerDestination = spawnPos + transform.forward * walkDistance
                              + transform.right * Random.Range(-2f, 2f);
        npc.wantsToEat = Random.Range(0f, 100f) < chanceToEat;
        npc.seatDetectRadius = seatDetectionRadius;

        // Copy patience settings sang NPC mới
        npc.maxPatienceTime = maxPatienceTime;
        npc.latePenaltyPercent = latePenaltyPercent;
        npc.eatTime = eatTime;
        npc.patienceBarPrefab = patienceBarPrefab;

        currentSpawnCount++;
    }

    public void OnNPCDestroyed()
    {
        currentSpawnCount = Mathf.Max(0, currentSpawnCount - 1);
    }

    private static void CreateCapsuleVisual(GameObject parent, Color color)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Visual";
        capsule.transform.SetParent(parent.transform);
        capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = Vector3.one;

        var renderer = capsule.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        var col = capsule.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WALKER LOGIC
    // ═══════════════════════════════════════════════════════════════════════════

    private IEnumerator WalkerRoutine()
    {
        // Chờ 1 frame để NavMeshAgent khởi tạo xong
        yield return null;

        // Đảm bảo agent được lấy (Awake có thể chạy trước khi role được gán)
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        InitializeNPCVisual();

        SetState(State.Walking);

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPC] Không nằm trên NavMesh tại {transform.position}, tự hủy.");
            Destroy(gameObject);
            yield break;
        }

        agent.SetDestination(walkerDestination);

        while (this != null)
        {
            // Kiểm tra muốn ăn + có ghế trống gần
            if (wantsToEat && !hasBecomCustomer)
            {
                CustomerSeat seat = FindEmptySeatNearby();
                if (seat != null)
                {
                    hasBecomCustomer = true;
                    role = Role.Customer;
                    Debug.Log("[NPC] 🍚 Quyết định vào ăn cơm tấm!");
                    StartCoroutine(CustomerLifecycle());
                    yield break;
                }
            }

            // Đã đến đích → tự hủy
            if (!agent.pathPending && agent.remainingDistance < 1.5f)
            {
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private CustomerSeat FindEmptySeatNearby()
    {
        CustomerSeat[] seats = FindObjectsOfType<CustomerSeat>();
        float closestDist = float.MaxValue;
        CustomerSeat closestSeat = null;

        foreach (var seat in seats)
        {
            if (seat.IsOccupied) continue;
            float dist = Vector3.Distance(transform.position, seat.transform.position);
            if (dist < seatDetectRadius && dist < closestDist)
            {
                closestDist = dist;
                closestSeat = seat;
            }
        }
        return closestSeat;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CUSTOMER LOGIC
    // ═══════════════════════════════════════════════════════════════════════════

    private IEnumerator CustomerLifecycle()
    {
        // Đảm bảo agent sẵn sàng
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        InitializeNPCVisual();

        // --- Tìm ghế ---
        SetState(State.Idle);
        assignedSeat = FindEmptySeat();

        if (assignedSeat == null)
        {
            Debug.Log("[NPC] Không có ghế trống, bỏ đi.");
            Destroy(gameObject, 1f);
            yield break;
        }

        assignedSeat.TryOccupy(this);

        // --- Đi đến ghế ---
        SetState(State.WalkToSeat);

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("[NPC] NavMeshAgent không hợp lệ.");
            yield break;
        }

        agent.SetDestination(assignedSeat.SitPoint.position);

        float timeout = 30f;
        while (this != null && agent != null && (agent.pathPending || agent.remainingDistance > 0.5f))
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0f) break;
            yield return null;
        }
        if (this == null) yield break;

        agent.enabled = false;
        transform.position = assignedSeat.SitPoint.position;
        transform.rotation = assignedSeat.SitPoint.rotation;

        // --- Ngồi ---
        SetState(State.Seated);
        yield return new WaitForSeconds(0.5f);
        if (this == null) yield break;

        // --- Gọi món ---
        SetState(State.ShowOrder);
        if (orderBubble) orderBubble.SetActive(true);
        Debug.Log("[NPC] Gọi: 1 đĩa cơm tấm.");

        // --- Chờ đồ ăn (với thanh kiên nhẫn) ---
        SetState(State.WaitFood);
        SetupPatienceBar(); // Setup lại nếu chưa có
        patienceRemaining = maxPatienceTime;
        passedHalf = false;

        if (patienceBarObject != null)
            patienceBarObject.SetActive(true);

        while (this != null && currentState == State.WaitFood)
        {
            patienceRemaining -= Time.deltaTime;
            float ratio = patienceRemaining / maxPatienceTime;
            UpdatePatienceBar(ratio);

            if (!passedHalf && ratio <= 0.5f)
            {
                passedHalf = true;
                Debug.Log("[NPC] ⚠️ Quá nửa thời gian chờ! Sẽ bị trừ tiền.");
            }

            // Hết kiên nhẫn
            if (patienceRemaining <= 0f)
            {
                Debug.Log("[NPC] 💢 Hết kiên nhẫn! Bỏ đi không trả tiền!");
                if (patienceBarObject != null) patienceBarObject.SetActive(false);
                if (orderBubble) orderBubble.SetActive(false);

                SetState(State.Leave);
                if (assignedSeat != null) assignedSeat.Vacate();

                yield return LeaveRoutine();
                yield break;
            }

            yield return null;
        }
        if (this == null) yield break;

        // Ẩn thanh chờ
        if (patienceBarObject != null)
            patienceBarObject.SetActive(false);

        // --- Ăn ---
        if (orderBubble) orderBubble.SetActive(false);
        yield return new WaitForSeconds(eatTime);
        if (this == null) yield break;

        // --- Trả tiền ---
        SetState(State.Paying);

        // Tính toán giá trị đĩa ăn trước khi dọn
        int fullPrice = EconomyManager.Instance != null ? EconomyManager.Instance.PricePerPlate : 35000;
        if (assignedSeat != null && assignedSeat.LinkedTable != null)
        {
            PlateItem plate = assignedSeat.LinkedTable.GetComponentInChildren<PlateItem>();
            if (plate != null)
            {
                fullPrice = plate.CalculatePlateValue(fullPrice);
            }
        }

        CleanupPlates();

        yield return new WaitForSeconds(payDelay);
        if (this == null) yield break;

        if (EconomyManager.Instance != null)
        {
            if (passedHalf)
            {
                int penalty = Mathf.RoundToInt(fullPrice * latePenaltyPercent / 100f);
                int reduced = fullPrice - penalty;
                EconomyManager.Instance.AddMoney(reduced);
                Debug.Log($"[NPC] ⚠️ Phục vụ trễ! +{reduced:N0}đ (trừ {penalty:N0}đ) - Giá gốc: {fullPrice}");
            }
            else
            {
                EconomyManager.Instance.AddMoney(fullPrice);
                Debug.Log($"[NPC] ✅ Đúng giờ! +{fullPrice:N0}đ");
            }
        }

        // --- Rời đi ---
        SetState(State.Leave);
        if (assignedSeat != null) assignedSeat.Vacate();

        yield return LeaveRoutine();
    }

    private IEnumerator LeaveRoutine()
    {
        yield return new WaitForSeconds(leaveDelay);
        if (this == null) yield break;

        if (agent != null)
        {
            agent.enabled = true;
            agent.SetDestination(transform.position + -transform.forward * 10f);
        }
        yield return new WaitForSeconds(3f);

        if (this != null) Destroy(gameObject);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PATIENCE BAR
    // ═══════════════════════════════════════════════════════════════════════════

    private void SetupPatienceBar()
    {
        if (patienceBarPrefab != null && patienceBarObject == null)
        {
            patienceBarObject = Instantiate(patienceBarPrefab, transform);
            patienceBarObject.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        }

        if (patienceBarObject != null && patienceFillImage == null)
        {
            Transform fillT = patienceBarObject.transform.Find("Fill");
            if (fillT != null)
                patienceFillImage = fillT.GetComponent<Image>();

            if (patienceFillImage == null)
            {
                Image[] images = patienceBarObject.GetComponentsInChildren<Image>();
                if (images.Length > 1)
                    patienceFillImage = images[images.Length - 1];
                else if (images.Length == 1)
                    patienceFillImage = images[0];
            }
        }

        if (patienceBarObject != null)
            patienceBarObject.SetActive(false);
    }

    private void UpdatePatienceBar(float ratio)
    {
        if (patienceFillImage == null) return;

        patienceFillImage.fillAmount = Mathf.Clamp01(ratio);

        if (ratio > 0.5f)
            patienceFillImage.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
        else
            patienceFillImage.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════════════════════

    public void OnFoodReceived()
    {
        if (this == null) return;
        if (currentState == State.WaitFood)
        {
            SetState(State.Eating);
            Debug.Log("[NPC] Nhận đồ ăn, bắt đầu ăn...");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private void SetState(State s)
    {
        if (this == null) return;
        currentState = s;
        Debug.Log($"[NPC] → {s}");
    }

    private CustomerSeat FindEmptySeat()
    {
        foreach (var seat in FindObjectsOfType<CustomerSeat>())
            if (!seat.IsOccupied) return seat;
        return null;
    }

    private void CleanupPlates()
    {
        if (assignedSeat != null && assignedSeat.LinkedTable != null)
            assignedSeat.LinkedTable.ClearPlate();

        foreach (var table in FindObjectsOfType<ServingTable>())
            table.ClearPlate();

        foreach (var plate in FindObjectsOfType<PlateItem>())
        {
            if (plate != null && plate.gameObject != null)
                Destroy(plate.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (role != Role.Spawner && spawnerRef != null)
            spawnerRef.OnNPCDestroyed();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GIZMOS (chỉ cho Spawner)
    // ═══════════════════════════════════════════════════════════════════════════

    private void OnDrawGizmos()
    {
        if (role != Role.Spawner || !showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 left = transform.position - transform.right * (roadWidth / 2f);
        Vector3 right = transform.position + transform.right * (roadWidth / 2f);
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawLine(left, right);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * walkDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position + transform.forward * (walkDistance * 0.5f), seatDetectionRadius);
    }
}
