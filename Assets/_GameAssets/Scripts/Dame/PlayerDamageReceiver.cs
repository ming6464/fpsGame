using System;

public class PlayerDamageReceiver : DamageReceiver
{
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
    }

    public override void Reduct(float reduct)
    {
        base.Reduct(reduct);
        PostEventChangeHp();
    }
}