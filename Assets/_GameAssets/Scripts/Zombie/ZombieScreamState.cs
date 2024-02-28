using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class ZombieScreamState : StateMachineBehaviour
{
    [SerializeField]
    private float _detectionAreaRadius;

    private Transform player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (animator.TryGetComponent(out NavMeshAgent agent))
        {
            if (player)
            {
                agent.SetDestination(player.position);
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        if (stateInfo.normalizedTime > 0.85f)
        {
            float dis = Vector3.Distance(animator.transform.position, player.position);
            if (dis > _detectionAreaRadius)
            {
                animator.SetBool("IsChasing", false);
            }
        }
    }
}