using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC Khach hang — dung Capsule placeholder (ModelRoot de them model sau).
/// State Machine: Idle → WalkToSeat → Seated → ShowOrder → WaitFood → Eating → Paying → Leave
/// Yeu cau: NavMesh phai duoc Bake trong scene.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerNPC : MonoBehaviour
{
    public enum State { Idle, WalkToSeat, Seated, ShowOrder, WaitFood, Eating, Paying, Leave }

    [Header("State hien tai (debug)")]
    [SerializeField] private State currentState = State.Idle;

    [Header("Tham so")]
    [SerializeField] private float eatTime = 1.5f; // Đã giảm từ 4f xuống 1.5f
    [SerializeField] private float payDelay = 0.5f; // Giảm luôn thời gian thanh toán một chút cho nhanh
    [SerializeField] private float leaveDelay = 1.5f;

    [Header("Model (them sau khi co FBX)")]
    [Tooltip("Keo model FBX vao day, sau do tat Capsule MeshRenderer")]
    [SerializeField] private Transform modelRoot;

    [Header("Order Bubble")]
    [Tooltip("Icon hien tren dau NPC khi goi mon")]
    [SerializeField] private GameObject orderBubble;

    private NavMeshAgent agent;
    private CustomerSeat assignedSeat;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (orderBubble) orderBubble.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LifecycleRoutine());
    }

    // ── State machine chinh ───────────────────────────────────────────────────

    private IEnumerator LifecycleRoutine()
    {
        // --- Tim ghe trong ---
        SetState(State.Idle);
        assignedSeat = FindEmptySeat();

        if (assignedSeat == null)
        {
            Debug.Log("[CustomerNPC] Khong co ghe trong, NPC bo di.");
            Destroy(gameObject, 1f);
            yield break;
        }

        assignedSeat.TryOccupy(this);

        // --- Di den ghe ---
        SetState(State.WalkToSeat);

        // Guard: kiem tra doi tuong van ton tai truoc khi dung NavMeshAgent
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("[CustomerNPC] NavMeshAgent khong hop le. Huy routine.");
            yield break;
        }

        agent.SetDestination(assignedSeat.SitPoint.position);

        // Doi den noi (co timeout 30s de tranh vong lap vo tan neu NavMesh loi)
        float timeout = 30f;
        while (this != null && agent != null && (agent.pathPending || agent.remainingDistance > 0.5f))
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0f)
            {
                Debug.LogWarning("[CustomerNPC] Timeout khi di den ghe.");
                break;
            }
            yield return null;
        }

        // Guard: NPC co the da bi Destroy giua chung
        if (this == null || gameObject == null) yield break;

        agent.enabled = false; // Dung de ngoi yen
        transform.position = assignedSeat.SitPoint.position;
        transform.rotation = assignedSeat.SitPoint.rotation;

        // --- Ngoi xuong ---
        SetState(State.Seated);
        yield return new WaitForSeconds(0.5f);
        if (this == null) yield break;

        // --- Goi mon ---
        SetState(State.ShowOrder);
        if (orderBubble) orderBubble.SetActive(true);
        Debug.Log("[CustomerNPC] Goi: 1 dia com tam.");

        // --- Cho do an ---
        SetState(State.WaitFood);

        // Cho toi khi doi sang Eating hoac bi huy (co timeout)
        float waitTimeout = 300f; // 5 phut
        while (this != null && currentState != State.Eating)
        {
            waitTimeout -= Time.deltaTime;
            if (waitTimeout <= 0f)
            {
                Debug.LogWarning("[CustomerNPC] Timeout cho do an. NPC bo di.");
                break;
            }
            yield return null;
        }
        if (this == null) yield break;

        // --- An ---
        if (orderBubble) orderBubble.SetActive(false);
        yield return new WaitForSeconds(eatTime);
        if (this == null) yield break;

        // --- Tra tien ---
        SetState(State.Paying);
        
        // Ngay khi ăn xong và bắt đầu trả tiền, dọn đĩa lập tức
        bool plateCleared = false;
        if (assignedSeat != null && assignedSeat.LinkedTable != null)
        {
            assignedSeat.LinkedTable.ClearPlate();
            plateCleared = true;
        }

        // FALLBACK: Nếu ghế không được link với bàn đúng cách trong Scene,
        // dọn sạch mọi PlateItem nằm khơi khơi gần NPC (bán kính 3m, quét mọi Layer)
        if (!plateCleared || true) // Chạy luôn fallback cho chắc ăn
        {
            // Plates thường nằm ở layer "Interactable", nên quét toàn bộ Ignore.
            Collider[] hits = Physics.OverlapSphere(transform.position, 3f);
            foreach (var hit in hits)
            {
                PlateItem nearbyPlate = hit.GetComponentInParent<PlateItem>();
                if (nearbyPlate == null) nearbyPlate = hit.GetComponent<PlateItem>();
                
                if (nearbyPlate != null)
                {
                    // Nếu đĩa này đang nằm trên ServingTable, gỡ reference ra khỏi bàn trước
                    ServingTable table = nearbyPlate.GetComponentInParent<ServingTable>();
                    if (table != null) table.ClearPlate();
                    
                    if (nearbyPlate != null && nearbyPlate.gameObject != null)
                    {
                        Destroy(nearbyPlate.gameObject);
                        Debug.Log("[CustomerNPC] Fallback: Đã xoá đĩa cơm rơi vãi/trên bàn gần đó bằng OverlapSphere.");
                    }
                }
            }
        }

        yield return new WaitForSeconds(payDelay);
        if (this == null) yield break;

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddMoney(EconomyManager.Instance.PricePerPlate);

        // --- Roi di ---
        SetState(State.Leave);
        if (assignedSeat != null)
        {
            assignedSeat.Vacate();
        }

        yield return new WaitForSeconds(leaveDelay);
        if (this == null) yield break;

        if (agent != null)
        {
            agent.enabled = true;
            agent.SetDestination(transform.position + -transform.forward * 10f);
        }
        yield return new WaitForSeconds(3f);

        if (this != null)
            Destroy(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Duoc goi boi CustomerSeat khi player dat dia du len ban.</summary>
    public void OnFoodReceived()
    {
        if (this == null) return; // Guard chong MissingReferenceException

        if (currentState == State.WaitFood)
        {
            SetState(State.Eating);
            Debug.Log("[CustomerNPC] Nhan do an, bat dau an...");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetState(State s)
    {
        if (this == null) return; // Guard chong MissingReferenceException
        currentState = s;
        Debug.Log($"[CustomerNPC] → {s}");
    }

    private CustomerSeat FindEmptySeat()
    {
        CustomerSeat[] seats = FindObjectsOfType<CustomerSeat>();
        foreach (var seat in seats)
            if (!seat.IsOccupied) return seat;
        return null;
    }
}
