using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    [SerializeField]
    [Range(1, 10)]
    private float _timeIdleMax;

    private float m_detectionAreaRadius = -1;
    private Transform m_player;
    private float m_startTime;
    private float m_timeIdle;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (m_detectionAreaRadius < 0)
        {
            m_detectionAreaRadius = animator.GetComponent<Zombie>().DetectionRadiusWhenPatrolling;
        }

        m_startTime = Time.time;
        m_timeIdle = Random.Range(1, _timeIdleMax);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsPatrolling", false);
        animator.SetBool("IsAttacking", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_player && Vector3.Distance(animator.transform.position, m_player.position) <= m_detectionAreaRadius)
        {
            animator.SetBool("IsChasing", true);
            return;
        }

        if (Time.time > m_startTime + m_timeIdle)
        {
            animator.SetBool("IsPatrolling", true);
        }
    }
}