using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    [SerializeField]
    [Range(1, 10)]
    private float _timeIdleMax;

    [SerializeField]
    private float _detectionAreaRadius;

    private Transform m_player;
    private float m_startTime;
    private float m_timeIdle;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        m_startTime = Time.time;
        m_timeIdle = Random.Range(1, _timeIdleMax);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsPatrolling", false);
        animator.SetBool("IsAttacking", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_player && Vector3.Distance(animator.transform.position, m_player.position) <= _detectionAreaRadius)
        {
            animator.SetBool("IsChasing", true);
            return;
        }

        if (Time.time > m_startTime + m_timeIdle)
        {
            animator.SetBool("IsPatrolling", true);
            Debug.Log("IsPatrolling : true");
        }
    }
}