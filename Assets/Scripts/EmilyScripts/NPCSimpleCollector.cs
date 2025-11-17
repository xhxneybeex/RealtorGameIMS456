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
    public float moveSpeed = 3.5f;
    public float accel = 20f;
    public float turnSpeed = 10f;
    public float stopEpsilon = 0.05f;
    public float stuckSeconds = 2f;

    [Header("Wall Avoidance")]
    public float wallCheckDistance = 1.0f;       // how far ahead we look
    public float avoidJumpDistance = 3.0f;       // how far to pick a new point when avoiding
    public LayerMask wallMask = ~0;              // usually your environment layers
    public float reavoidCooldown = 0.5f;         // small cooldown so we do not spam repaths

    NavMeshAgent agent;
    Rigidbody rb;
    CapsuleCollider cap;

    float nextPathTime;
    float nextAvoidTime;
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

        // agent plans, rb moves
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.autoBraking = true;
        agent.autoTraverseOffMeshLink = true;

        // size sync with collider
        agent.radius = Mathf.Max(agent.radius, cap.radius);
        agent.height = Mathf.Max(agent.height, cap.height);
        agent.baseOffset = cap.height * 0.5f;

        // physics
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // pickup stop
        agent.stoppingDistance = Mathf.Min(agent.stoppingDistance, pickupRange * 0.8f);

        lastProgressPos = transform.position;
        lastProgressTime = Time.time;
    }

    void Update()
    {
        if (state == State.Roam) TickRoam();
        else TickToItem();

        // keep agent aligned to rb
        agent.nextPosition = rb.position;
        agent.velocity = rb.velocity;

        // simple unstuck
        HandleProgressFailSafe();

        // check a short feeler for walls and turn away if needed
        TryAvoidWall();
    }

    void FixedUpdate()
    {
        // agent's intent
        Vector3 desiredVel = agent.desiredVelocity;
        Vector3 desiredPlanar = Vector3.ProjectOnPlane(desiredVel, Vector3.up);

        // target velocity
        Vector3 targetVel = Vector3.zero;
        if (desiredPlanar.sqrMagnitude > stopEpsilon * stopEpsilon)
            targetVel = desiredPlanar.normalized * moveSpeed;

        // accelerate rb
        Vector3 newVel = Vector3.MoveTowards(
            rb.velocity,
            new Vector3(targetVel.x, rb.velocity.y, targetVel.z),
            accel * Time.fixedDeltaTime
        );
        rb.velocity = newVel;

        // face move dir
        Vector3 faceDir = new Vector3(newVel.x, 0f, newVel.z);
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(faceDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    // roaming between random points, opportunistically chasing items
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

    // heading to an item, repathing on a cadence
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

    // distance using collider closest points
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

    // find nearest tagged within radius
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

    // random reachable point
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

    // draw scan radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }

    // short feeler to detect walls, then pick a new point away from hit
    void TryAvoidWall()
    {
        if (Time.time < nextAvoidTime) return;

        Vector3 origin = cap ? (transform.position + Vector3.up * Mathf.Clamp(cap.height * 0.5f, 0.5f, 1.5f))
                             : transform.position + Vector3.up * 0.9f;

        Vector3 fwd = rb.velocity;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = transform.forward;
        fwd.Normalize();

        float radius = cap ? Mathf.Max(0.1f, cap.radius * 0.9f) : 0.3f;
        if (Physics.SphereCast(origin, radius, fwd, out RaycastHit hit, wallCheckDistance, wallMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 away = Vector3.ProjectOnPlane(Vector3.Reflect(fwd, hit.normal), Vector3.up).normalized;
            if (away.sqrMagnitude < 0.25f) away = -fwd; // fallback

            Vector3 candidate = transform.position + away * avoidJumpDistance;

            if (NavMesh.SamplePosition(candidate, out var navHit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                nextPathTime = Time.time + 0.1f;
                nextAvoidTime = Time.time + reavoidCooldown;

                // if we were chasing an item and the wall is hard blocking, drop it
                if (state == State.ToItem && currentItem != null)
                {
                    // optional: keep chasing, but this drop helps them not nose into walls forever
                    currentItem = null;
                    state = State.Roam;
                }
            }
        }
    }

    // watch progress and repath if stuck
    void HandleProgressFailSafe()
    {
        if (agent.hasPath && !agent.pathPending)
        {
            float moved = Vector3.ProjectOnPlane(rb.position - lastProgressPos, Vector3.up).magnitude;
            if (moved > 0.1f)
            {
                lastProgressPos = rb.position;
                lastProgressTime = Time.time;
            }
            else if (Time.time - lastProgressTime > stuckSeconds)
            {
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
}
