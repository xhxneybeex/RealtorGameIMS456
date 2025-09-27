using UnityEngine;
using UnityEngine.AI;

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
    [Range(0f, 1f)] public float chanceToChaseItem = 0.6f;   // each scan
    [Range(0f, 1f)] public float chanceToPickUp = 0.7f;   // at item

    NavMeshAgent agent;
    float nextPathTime;
    Transform currentItem;

    enum State { Roam, ToItem }
    State state = State.Roam;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (state == State.Roam) TickRoam();
        else TickToItem();
    }

    void TickRoam()
    {
        // periodic repath to a random point
        if (Time.time >= nextPathTime && (!agent.hasPath || agent.remainingDistance < 0.3f))
        {
            nextPathTime = Time.time + repathInterval;
            agent.SetDestination(RandomPointOnNavmesh(transform.position, roamRadius));
        }

        // look for items and maybe decide to chase one
        Transform nearest = FindNearestTagged(interactibleTag, scanRadius);
        if (nearest != null && Random.value < chanceToChaseItem)
        {
            currentItem = nearest;
            agent.SetDestination(currentItem.position);
            state = State.ToItem;
        }
    }

    void TickToItem()
    {
        if (currentItem == null)
        {
            state = State.Roam;
            return;
        }

        // keep updating path toward moving targets or after small delays
        if (Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + repathInterval;
            agent.SetDestination(currentItem.position);
        }

        float d = Vector3.Distance(transform.position, currentItem.position);
        if (d <= pickupRange)
        {
            // roll to pick up
            if (Random.value < chanceToPickUp)
            {
                // delete the object
                Destroy(currentItem.gameObject);
            }
            // whether picked up or not, clear target and roam again
            currentItem = null;
            state = State.Roam;
        }
    }

    Transform FindNearestTagged(string tagName, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
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
            Vector3 p = origin + Random.insideUnitSphere * radius;
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

