using UnityEngine;
using UnityEngine.AI;

public class ZombieChaseState : StateMachineBehaviour
{
    private float m_detectionAreaRadius = -1;

    private float m_distanceStop = -1;

    private Transform m_player;

    private NavMeshAgent m_agent;

    private Zombie m_zombie;

    private bool m_isFirstFrameExecuted;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (!m_agent)
        {
            m_agent = animator.GetComponent<NavMeshAgent>();
        }

        if (!m_zombie)
        {
            m_zombie = animator.GetComponent<Zombie>();
        }

        if (m_zombie && m_detectionAreaRadius < 0)
        {
            m_detectionAreaRadius = m_zombie.DetectionRadiusWhenChasing;
        }

        if (m_zombie && m_distanceStop < 0)
        {
            m_distanceStop = m_zombie.DistanceStop;
        }

        m_isFirstFrameExecuted = false;

        animator.SetBool("IsPatrolling", false);

        CallsZombiesAround(animator.transform.position);
    }

    private void CallsZombiesAround(Vector3 pos)
    {
        foreach (Collider col in Physics.OverlapSphere(pos, 5))
        {
            if (col.transform.TryGetComponent(out Zombie zombie))
            {
                zombie.CheckDistanceFromPlayer();
            }
        }
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

        if (Vector3.Distance(animator.transform.position, m_player.position) > m_detectionAreaRadius)
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