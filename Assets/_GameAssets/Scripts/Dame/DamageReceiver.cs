using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField]
    protected float _maxHp;

    [SerializeField]
    protected float _hp;

    public virtual void Add(float add)
    {
        _hp += add;
        if (_hp > _maxHp)
        {
            _hp = _maxHp;
        }
    }

    public virtual void Reduct(float reduct)
    {
        _hp -= reduct;
        if (_hp <= 0)
        {
            _hp = 0;
            OnDead();
        }
    }

    public virtual bool IsDead()
    {
        return _hp == 0;
    }

    protected virtual void OnDead()
    {
    }

    public virtual void UpdateHpInfo(float _hp, float _maxHp)
    {
        this._hp = _hp > _maxHp ? _maxHp : _hp;
        this._maxHp = _maxHp;
    }
}