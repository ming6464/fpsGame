using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class ZombieDieState : StateMachineBehaviour
{
    private bool isDelayToPool;
    private float startTimeDelay;
    private const float timeDelay = 2f;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 1)
        {
            if (!isDelayToPool)
            {
                isDelayToPool = true;
                startTimeDelay = Time.time;
            }
            else if (Time.time > startTimeDelay + timeDelay)
            {
                GObj_pooling.Instance.Push(PoolKEY.Zombie, animator.transform);
            }
        }
    }
}