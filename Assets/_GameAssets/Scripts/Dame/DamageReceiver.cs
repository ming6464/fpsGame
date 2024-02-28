using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField]
    protected float maxHp;

    [SerializeField]
    protected float hp;

    public virtual void Add(float add)
    {
        hp += add;
        if (hp > maxHp)
        {
            hp = maxHp;
        }
    }

    public virtual void Reduct(float reduct)
    {
        hp -= reduct;
        if (hp <= 0)
        {
            hp = 0;
            OnDead();
        }
    }

    public virtual bool IsDead()
    {
        return hp == 0;
    }

    protected virtual void OnDead()
    {
    }

    public virtual void UpdateHpInfo(float hp, float maxHp)
    {
        this.hp = hp > maxHp ? maxHp : hp;
        this.maxHp = maxHp;
    }
}