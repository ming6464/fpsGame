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

    private Transform m_player;
    private float m_startTime;
    private Transform m_patrolPoints;
    private NavMeshAgent m_agent;
    private Vector3 m_nextPatrolPoint;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        m_patrolPoints = GameObject.FindGameObjectWithTag("PatrolPoints")?.transform;
        m_startTime = Time.time;
        animator.transform.TryGetComponent(out m_agent);

        if (m_patrolPoints && m_agent)
        {
            m_nextPatrolPoint = animator.transform.position;

            while (Vector3.Distance(animator.transform.position, m_nextPatrolPoint) < 1.5f)
            {
                m_nextPatrolPoint = m_patrolPoints.GetChild(Random.Range(0, m_patrolPoints.childCount)).position;
            }

            m_agent.SetDestination(m_nextPatrolPoint);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_player && Vector3.Distance(animator.transform.position, m_player.position) <= _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", true);
            animator.SetBool("IsPatrolling", false);
            return;
        }


        if (Time.time > m_startTime + _timePatrol || m_agent.remainingDistance <= m_agent.radius)
        {
            m_agent.ResetPath();
            animator.SetBool("IsPatrolling", false);
        }
    }
}