using UnityEngine;
using UnityEngine.AI;

public class ZombieScreamState : StateMachineBehaviour
{
    [SerializeField]
    private float _detectionAreaRadius;

    private Transform m_player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator.TryGetComponent(out NavMeshAgent agent))
        {
            if (m_player)
            {
                agent.SetDestination(m_player.position);
            }
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
            if (dis > _detectionAreaRadius)
            {
                animator.SetBool("IsChasing", false);
            }
        }
    }
}