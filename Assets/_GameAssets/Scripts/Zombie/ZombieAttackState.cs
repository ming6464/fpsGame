using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class ZombieAttackState : StateMachineBehaviour
{
    [SerializeField]
    private float _attackAreaRadius;

    private NavMeshAgent agent;

    private Transform player;

    private Zombie zombie;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator.TryGetComponent(out zombie);
        animator.TryGetComponent(out agent);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!player || !agent || !zombie)
        {
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsChasing", false);
            return;
        }

        agent.SetDestination(player.position);

        if (stateInfo.normalizedTime > 0.9f)
        {
            if (agent.remainingDistance > _attackAreaRadius || !zombie.CheckCanAttack())
            {
                animator.SetBool("IsAttacking", false);
            }
        }

        zombie.UpdateRotate = zombie.CheckCanAttack();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent)
        {
            agent.ResetPath();
        }

        if (zombie)
        {
            zombie.UpdateRotate = true;
        }
    }
}