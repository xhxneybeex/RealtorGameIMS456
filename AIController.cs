using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshEnem;
    public float startTime = 4;
    public float timeRotate = 2;
    public float run = 9;
    public float walk =  6;

    public float viewRadius = 360;
    public float viewAngle = 180;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public float meshResolution = 1f;
    public int edgeIterations = 4;
    public float edgeDistance = 0.5f;

    public Transform[] waypoints;
    int m_CurrentWaypointIndex;
    Vector3 playerLastPosition = Vector3.zero;
    Vector3 m_PlayerPosition;

    float m_WaitTime;
    float m_TimeToRotate;
    bool m_PlayerInRange;
    bool m_PlayerNear;
    bool m_IsPatrol;
    bool m_CaughtPlayer;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_CaughtPlayer = false;
        m_PlayerInRange = false;
        m_WaitTime = startTime;
        m_TimeToRotate = timeRotate;

        m_CurrentWaypointIndex = 0;
        navMeshEnem = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        navMeshEnem.isStopped = false;
        navMeshEnem.speed = walk;
        navMeshEnem.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    // Update is called once per frame
    void Update()
    {
        EnvironmentView();

        if (!m_IsPatrol)
        {
            Chasing();
        }
        else
        {
            Patroling();

        }
    }

    private void Chasing()
    {
        m_PlayerNear = false;
        playerLastPosition = Vector3.zero;

        if (!m_CaughtPlayer)
        {
            Move(run);
            navMeshEnem.SetDestination(m_PlayerPosition);
        }
        if (navMeshEnem.remainingDistance <= navMeshEnem.stoppingDistance)
        {
            if (m_WaitTime <= 0 && !m_CaughtPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position)>= 6f)
            {
                m_IsPatrol = true;
                m_PlayerNear = false;
                Move(walk);
                m_TimeToRotate = timeRotate;
                m_WaitTime = startTime;
                navMeshEnem.SetDestination(waypoints [m_CurrentWaypointIndex].position);
            }
            else
            {
                if (Vector3.Distance(transform.position,GameObject.FindGameObjectWithTag("Player").transform.position)>= 2.5f)
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }

            }
        }
    }

    private void Patroling()
    {
        if (m_PlayerNear)
        {
            if(m_TimeToRotate <= 0)
            {
                Move(walk);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            m_PlayerNear = false;
            playerLastPosition = Vector3.zero;
            navMeshEnem.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            if (navMeshEnem.remainingDistance <= navMeshEnem.stoppingDistance)
            {
                if (m_WaitTime <= 0 )
                {
                    NextPoint();
                    Move(walk);
                    m_WaitTime = startTime;

                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    void Move(float speed)
    {
        anim.SetBool("walk", true);
        navMeshEnem.isStopped = false;
        navMeshEnem.speed = speed;
    }

    void Stop()
    {
        navMeshEnem.isStopped = true;
        navMeshEnem.speed = 0;
        anim.SetBool("walk", false);
    }

    public void NextPoint()
    {
        m_CurrentWaypointIndex = (m_CurrentWaypointIndex+1) % waypoints.Length;
        navMeshEnem.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    void CaughtPlayer()
    {
        m_CaughtPlayer = true;
    }

    void LookingPlayer(Vector3 player)
    {
        navMeshEnem.SetDestination(player);
        if(Vector3.Distance(transform.position, player) <= 0.3)
        {
            if(m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(walk);
                navMeshEnem.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                m_WaitTime = startTime;
                m_TimeToRotate = timeRotate;            
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }
    void EnvironmentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius,playerMask);
        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    m_PlayerInRange = true;
                    m_IsPatrol = false;
                }
                else
                {
                    m_PlayerInRange = false;

                }
            }
            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                m_PlayerInRange = false;
            }
        
        if (m_PlayerInRange)
        {
            m_PlayerPosition = player.transform.position;
        }
    }
    }
}

