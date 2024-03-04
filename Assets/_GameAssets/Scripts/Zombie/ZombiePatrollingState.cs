using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePatrollingState : StateMachineBehaviour
{
    [SerializeField]
    private float _timePatrol;

    private float m_detectionAreaRadius = -1;
    private Transform m_player;
    private float m_startTime;
    private Transform m_patrolPoints;
    private NavMeshAgent m_agent;
    private Vector3 m_nextPatrolPoint;
    private bool m_isFirstFrameExecuted;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_isFirstFrameExecuted = false;
        if (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (!m_patrolPoints)
        {
            m_patrolPoints = GameObject.FindGameObjectWithTag("PatrolPoints")?.transform;
        }

        m_startTime = Time.time;

        if (!m_agent)
        {
            m_agent = animator.transform.GetComponent<NavMeshAgent>();
        }


        if (m_detectionAreaRadius < 0)
        {
            m_detectionAreaRadius = animator.GetComponent<Zombie>().DetectionRadiusWhenPatrolling;
        }

        if (m_patrolPoints && m_agent)
        {
            List<int> indexs = new();
            int index = -1;
            do
            {
                indexs.Add(index);
                index = Random.Range(0, m_patrolPoints.childCount);
                m_nextPatrolPoint = m_patrolPoints.GetChild(index).position;
            }
            while (indexs.Contains(index) || Vector3.Distance(animator.transform.position, m_nextPatrolPoint) < 15f);

            m_agent.SetDestination(m_nextPatrolPoint);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_isFirstFrameExecuted)
        {
            m_isFirstFrameExecuted = true;
            return;
        }

        if (m_player && Vector3.Distance(animator.transform.position, m_player.position) <= m_detectionAreaRadius)
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