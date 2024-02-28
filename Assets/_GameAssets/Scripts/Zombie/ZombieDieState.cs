using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class ZombieDieState : StateMachineBehaviour
{
    private bool m_isDelayToPool;
    private float m_startTimeDelay;
    private const float m_timeDelay = 2f;

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
                GObj_pooling.Instance.Push(PoolKEY.Zombie, animator.transform);
            }
        }
    }
}