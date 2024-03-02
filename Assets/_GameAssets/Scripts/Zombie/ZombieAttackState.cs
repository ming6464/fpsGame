using UnityEngine;
using UnityEngine.AI;

public class ZombieAttackState : StateMachineBehaviour
{
    private float m_attackAreaRadius;

    private NavMeshAgent m_agent;

    private Transform m_player;

    private Zombie m_zombie;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        m_zombie = animator.GetComponent<Zombie>();
        m_agent = animator.GetComponent<NavMeshAgent>();
        m_attackAreaRadius = m_zombie.AttackRange;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player || !m_agent || !m_zombie)
        {
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsChasing", false);
            return;
        }

        m_agent.SetDestination(m_player.position);

        if (stateInfo.normalizedTime > 0.9f)
        {
            if (m_agent.remainingDistance > m_attackAreaRadius || !m_zombie.CheckCanAttack())
            {
                animator.SetBool("IsAttacking", false);
            }
        }

        m_zombie.UpdateRotate = m_zombie.CheckCanAttack();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_agent)
        {
            m_agent.ResetPath();
        }

        if (m_zombie)
        {
            m_zombie.UpdateRotate = true;
        }
    }
}