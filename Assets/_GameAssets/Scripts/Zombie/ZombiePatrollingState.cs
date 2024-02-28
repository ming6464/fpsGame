using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePatrollingState : StateMachineBehaviour
{
    [SerializeField]
    private float _timePatrol;

    [SerializeField]
    private float _detectionAreaRadius;

    private Transform player;
    private float startTime;
    private Transform patrolPoints;
    private NavMeshAgent agent;
    private Vector3 nextPatrolPoint;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        patrolPoints = GameObject.FindGameObjectWithTag("PatrolPoints")?.transform;
        startTime = Time.time;
        animator.transform.TryGetComponent(out agent);

        if (patrolPoints && agent)
        {
            nextPatrolPoint = animator.transform.position;

            while (Vector3.Distance(animator.transform.position, nextPatrolPoint) < 1.5f)
            {
                nextPatrolPoint = patrolPoints.GetChild(Random.Range(0, patrolPoints.childCount)).position;
            }

            agent.SetDestination(nextPatrolPoint);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player && Vector3.Distance(animator.transform.position, player.position) <= _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", true);
            animator.SetBool("IsPatrolling", false);
            return;
        }


        if (Time.time > startTime + _timePatrol || agent.remainingDistance <= agent.radius)
        {
            agent.ResetPath();
            animator.SetBool("IsPatrolling", false);
        }
    }
}