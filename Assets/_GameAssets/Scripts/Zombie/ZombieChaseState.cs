using UnityEngine;
using UnityEngine.AI;

public class ZombieChaseState : StateMachineBehaviour
{
    [SerializeField]
    private float _detectionAreaRadius;

    [SerializeField]
    private float _attackAreaRadius;

    private Transform player;

    private NavMeshAgent agent;

    private Zombie zombie;

    private bool isFirstFrameExecuted;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator.transform.TryGetComponent(out agent);
        animator.TryGetComponent(out zombie);
        isFirstFrameExecuted = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player || !agent)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        agent.SetDestination(player.position);
        if (!isFirstFrameExecuted)
        {
            isFirstFrameExecuted = true;
            return;
        }

        if (Vector3.Distance(animator.transform.position, player.position) > _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", false);
            return;
        }

        if (agent.remainingDistance < _attackAreaRadius && zombie.CheckCanAttack())
        {
            animator.SetBool("IsAttacking", true);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent)
        {
            agent.ResetPath();
        }
    }
}