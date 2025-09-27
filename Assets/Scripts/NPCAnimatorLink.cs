using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCAnimatorLink : MonoBehaviour
{
    public string speedParam = "Speed";
    public float speedMultiplier = 1f; // tune so walk looks right

    Animator anim;
    NavMeshAgent agent;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float speed = agent.velocity.magnitude * speedMultiplier;
        anim.SetFloat(speedParam, speed);
    }
}

