using System;

public class PlayerDamageReceiver : DamageReceiver
{
    private void Start()
    {
        PostEventChangeHp();
    }

    private void PostEventChangeHp()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnChangeHealth, _hp.ToString());
    }

    public override void Add(float add)
    {
        base.Add(add);
        PostEventChangeHp();
    }

    public override void Reduct(float reduct)
    {
        base.Reduct(reduct);
        PostEventChangeHp();
    }
}