using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : Singleton<CrossHair>
{
    public override void Update()
    {
        base.Update();
        var cam = Camera.main.transform;
        transform.transform.position = cam.position + cam.forward.normalized;
    }
}