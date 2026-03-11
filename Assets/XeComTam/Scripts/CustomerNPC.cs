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
    [SerializeField] private float eatTime = 4f;
    [SerializeField] private float payDelay = 1f;
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
        agent.SetDestination(assignedSeat.SitPoint.position);

        // Doi den noi
        while (agent.pathPending || agent.remainingDistance > 0.5f)
            yield return null;

        agent.enabled = false; // Dung de ngoi yen
        transform.position = assignedSeat.SitPoint.position;
        transform.rotation = assignedSeat.SitPoint.rotation;

        // --- Ngoi xuong ---
        SetState(State.Seated);
        yield return new WaitForSeconds(0.5f);

        // --- Goi mon ---
        SetState(State.ShowOrder);
        if (orderBubble) orderBubble.SetActive(true);
        Debug.Log("[CustomerNPC] Goi: 1 dia com tam.");

        // --- Cho do an ---
        SetState(State.WaitFood);
        yield return new WaitUntil(() => currentState == State.Eating);

        // --- An ---
        if (orderBubble) orderBubble.SetActive(false);
        yield return new WaitForSeconds(eatTime);

        // --- Tra tien ---
        SetState(State.Paying);
        yield return new WaitForSeconds(payDelay);

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddMoney(EconomyManager.Instance.PricePerPlate);

        // --- Roi di ---
        SetState(State.Leave);
        assignedSeat.Vacate();
        yield return new WaitForSeconds(leaveDelay);

        agent.enabled = true;
        // Di ra khoi man hinh (vi tri spawn hoac vi tri cach xa)
        agent.SetDestination(transform.position + -transform.forward * 10f);
        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Duoc goi boi CustomerSeat khi player dat dia du len ban.</summary>
    public void OnFoodReceived()
    {
        if (currentState == State.WaitFood)
        {
            SetState(State.Eating);
            Debug.Log("[CustomerNPC] Nhan do an, bat dau an...");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetState(State s)
    {
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
