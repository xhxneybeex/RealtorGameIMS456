using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCnav : MonoBehaviour
{
    private NavMeshAgent m_Agent;
    private Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (m_Agent.velocity.magnitude != 0f)
        {
            m_Animator.SetBool("Walking", true);
        }
        else
        {
            m_Animator.SetBool("Walking", false);
        }
    }
    void OnAnimatorMove()
    {
        if (m_Animator.GetBool("Walking"))
        {
            m_Agent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
        }
    }
}

