using UnityEngine;
using UnityEngine.AI;

public class ZombieScreamState : StateMachineBehaviour
{
    private float m_detectionAreaRadius = -1;
    private Transform m_player;
    private NavMeshAgent m_agent;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (!m_agent)
        {
            m_agent = animator.transform.GetComponent<NavMeshAgent>();
        }

        if (m_detectionAreaRadius < 0)
        {
            m_detectionAreaRadius = animator.GetComponent<Zombie>().DetectionRadiusWhenChasing;
        }

        if (m_player && m_agent)
        {
            m_agent.SetDestination(m_player.position);
        }

        if (AudioManager.Instance)
        {
            float dis = m_detectionAreaRadius / 2f;

            if (m_player)
            {
                dis = Vector3.Distance(m_player.position, animator.transform.position);
            }

            AudioManager.Instance.PlaySfx(KeySound.Zombie_Scream, 1 - dis / m_detectionAreaRadius);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        if (stateInfo.normalizedTime > 0.85f)
        {
            float dis = Vector3.Distance(animator.transform.position, m_player.position);
            if (dis > m_detectionAreaRadius)
            {
                animator.SetBool("IsChasing", false);
            }
        }
    }
}