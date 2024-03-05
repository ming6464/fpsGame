using UnityEngine;

public class PlayerDamageReceiver : DamageReceiver
{
    [SerializeField]
    private MinimapPointScript _minimapPoint;

    private void Start()
    {
        PostEventChangeHp();
    }

    private void PostEventChangeHp()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnChangeHealth, _hp);
    }

    public override void Add(float add)
    {
        base.Add(add);
        PostEventChangeHp();
    }

    protected override void OnDead()
    {
        base.OnDead();
        EventDispatcher.Instance.PostEvent(EventID.OnFinishGame, false);
        if (_minimapPoint)
        {
            _minimapPoint.OnDead();
        }
    }

    public override void Reduct(float reduct)
    {
        base.Reduct(reduct);
        PostEventChangeHp();
    }
}