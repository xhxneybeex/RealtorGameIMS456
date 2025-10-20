using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class NPCSimpleCollector : MonoBehaviour
{
    [Header("Nav")]
    public float roamRadius = 10f;
    public float repathInterval = 0.5f;

    [Header("Scanning")]
    public float scanRadius = 12f;
    public string interactibleTag = "NPCInteractible";
    public float pickupRange = 1.5f;

    [Header("Behavior")]
    [Range(0f, 1f)] public float chanceToChaseItem = 0.6f;
    [Range(0f, 1f)] public float chanceToPickUp = 0.7f;

    [Header("Motion (physics-driven)")]
    public float moveSpeed = 3.5f;          // similar to agent.speed
    public float accel = 20f;               // how fast we reach target velocity
    public float turnSpeed = 10f;           // facing smoothing
    public float stopEpsilon = 0.05f;       // small deadzone
    public float stuckSeconds = 2f;         // fail-safe: repath if not progressing

    NavMeshAgent agent;
    Rigidbody rb;
    CapsuleCollider cap;

    float nextPathTime;
    Transform currentItem;

    Vector3 lastProgressPos;
    float lastProgressTime;

    enum State { Roam, ToItem }
    State state = State.Roam;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        cap = GetComponent<CapsuleCollider>();

        // Agent only plans, we move with physics.
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.autoBraking = true;
        agent.autoTraverseOffMeshLink = true; // optional, disable if you want custom link traversal

        // Keep agent dimensions in line with collider so corners/doors match expectations.
        agent.radius = Mathf.Max(agent.radius, cap.radius);
        agent.height = Mathf.Max(agent.height, cap.height);
        agent.baseOffset = cap.height * 0.5f;

        // Physics settings
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Pickups: stop close enough that DistanceTo() will succeed.
        agent.stoppingDistance = Mathf.Min(agent.stoppingDistance, pickupRange * 0.8f);

        lastProgressPos = transform.position;
        lastProgressTime = Time.time;
    }

    void Update()
    {
        if (state == State.Roam) TickRoam();
        else TickToItem();

        // Keep the agent “anchored” to the rigidbody for planning.
        agent.nextPosition = rb.position;
        agent.velocity = rb.velocity; // helps the agent think we are actually moving

        // Simple unstuck: if we haven't progressed toward the goal for a while, repath.
        if (agent.hasPath && !agent.pathPending)
        {
            // Only consider horizontal progress.
            float moved = Vector3.ProjectOnPlane(rb.position - lastProgressPos, Vector3.up).magnitude;
            if (moved > 0.1f)
            {
                lastProgressPos = rb.position;
                lastProgressTime = Time.time;
            }
            else if (Time.time - lastProgressTime > stuckSeconds)
            {
                // Rekick the same destination to force a fresh solve.
                if (agent.hasPath) agent.SetDestination(agent.destination);
                lastProgressTime = Time.time;
            }
        }
        else
        {
            lastProgressPos = rb.position;
            lastProgressTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        // 1) Get the agent's steering intent.
        Vector3 desiredVel = agent.desiredVelocity;         // world-space velocity the agent wants
        Vector3 desiredPlanar = Vector3.ProjectOnPlane(desiredVel, Vector3.up);

        // 2) Compute target velocity we want the rigidbody to have.
        Vector3 targetVel = Vector3.zero;
        if (desiredPlanar.sqrMagnitude > stopEpsilon * stopEpsilon)
            targetVel = desiredPlanar.normalized * moveSpeed;

        // 3) Accelerate the rigidbody toward that velocity.
        Vector3 newVel = Vector3.MoveTowards(rb.velocity, new Vector3(targetVel.x, rb.velocity.y, targetVel.z), accel * Time.fixedDeltaTime);
        rb.velocity = newVel;

        // 4) Face movement direction.
        Vector3 faceDir = new Vector3(newVel.x, 0f, newVel.z);
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(faceDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    void TickRoam()
    {
        if (!agent.pathPending && (Time.time >= nextPathTime) && (!agent.hasPath || agent.remainingDistance < 0.3f))
        {
            nextPathTime = Time.time + repathInterval;
            Vector3 roamTarget = RandomPointOnNavmesh(transform.position, roamRadius);
            agent.SetDestination(roamTarget);
        }

        Transform nearest = FindNearestTagged(interactibleTag, scanRadius);
        if (nearest != null && Random.value < chanceToChaseItem)
        {
            currentItem = nearest;
            if (NavMesh.SamplePosition(currentItem.position, out var hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                state = State.ToItem;
            }
        }
    }

    void TickToItem()
    {
        if (currentItem == null) { state = State.Roam; return; }

        if (!agent.pathPending && Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + repathInterval;
            if (NavMesh.SamplePosition(currentItem.position, out var hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        float dist = DistanceTo(currentItem);
        if (dist <= pickupRange)
        {
            if (Random.value < chanceToPickUp)
                Destroy(currentItem.root ? currentItem.root.gameObject : currentItem.gameObject);

            currentItem = null;
            state = State.Roam;
        }
    }

    float DistanceTo(Transform target)
    {
        Collider theirCol = target.GetComponent<Collider>();
        if (theirCol && cap)
        {
            Vector3 a = cap.ClosestPoint(target.position);
            Vector3 b = theirCol.ClosestPoint(a);
            return Vector3.Distance(a, b);
        }
        return Vector3.Distance(transform.position, target.position);
    }

    Transform FindNearestTagged(string tagName, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Ignore);
        Transform best = null;
        float bestSqr = float.PositiveInfinity;

        foreach (var h in hits)
        {
            if (!h.CompareTag(tagName)) continue;
            float sqr = (h.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = h.transform;
            }
        }
        return best;
    }

    static Vector3 RandomPointOnNavmesh(Vector3 origin, float radius)
    {
        for (int i = 0; i < 12; i++)
        {
            float r = Random.Range(0f, radius);
            float a = Random.Range(0f, Mathf.PI * 2f);
            Vector3 p = origin + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            if (NavMesh.SamplePosition(p, out var hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return origin;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
