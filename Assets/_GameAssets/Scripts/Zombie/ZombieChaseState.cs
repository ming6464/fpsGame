using UnityEngine;
using UnityEngine.AI;

public class ZombieChaseState : StateMachineBehaviour
{
    [SerializeField]
    private float _detectionAreaRadius;

    private float m_distanceStop;

    private Transform m_player;

    private NavMeshAgent m_agent;

    private Zombie m_zombie;

    private bool m_isFirstFrameExecuted;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        m_agent = animator.GetComponent<NavMeshAgent>();
        m_zombie = animator.GetComponent<Zombie>();
        m_distanceStop = m_zombie.DistanceStop;
        m_isFirstFrameExecuted = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player || !m_agent)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        m_agent.SetDestination(m_player.position);
        if (!m_isFirstFrameExecuted)
        {
            m_isFirstFrameExecuted = true;
            return;
        }

        if (Vector3.Distance(animator.transform.position, m_player.position) > _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        if (m_agent.remainingDistance < m_distanceStop && m_zombie.CheckCanAttack())
        {
            animator.SetBool("IsAttacking", true);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_agent)
        {
            m_agent.ResetPath();
        }
    }
}