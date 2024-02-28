using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    [SerializeField]
    [Range(1, 10)]
    private float _timeIdleMax;

    [SerializeField]
    private float _detectionAreaRadius;

    private Transform player;
    private float startTime;
    private float timeIdle;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startTime = Time.time;
        timeIdle = Random.Range(1, _timeIdleMax);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsPatrolling", false);
        animator.SetBool("IsAttacking", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player && Vector3.Distance(animator.transform.position, player.position) <= _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", true);
            return;
        }

        if (Time.time > startTime + timeIdle)
        {
            animator.SetBool("IsPatrolling", true);
        }
    }
}