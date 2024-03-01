using UnityEngine;

public class ZombieDieState : StateMachineBehaviour
{
    private bool m_isDelayToPool;
    private float m_startTimeDelay;
    private const float m_timeDelay = 2f;
    private string m_zombieName;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_zombieName = animator.transform.GetComponent<Zombie>().Name;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 1)
        {
            if (!m_isDelayToPool)
            {
                m_isDelayToPool = true;
                m_startTimeDelay = Time.time;
            }
            else if (Time.time > m_startTimeDelay + m_timeDelay)
            {
                PoolKEY key = PoolKEY.ZombieV1;
                switch (m_zombieName.ToLower())
                {
                    case "zombiev2":
                        key = PoolKEY.ZombieV2;
                        break;
                    case "zombiev3":
                        key = PoolKEY.ZombieV3;
                        break;
                }

                GObj_pooling.Instance.Push(key, animator.transform);
            }
        }
    }
}