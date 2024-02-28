using System;
using UnityEngine;

public class TestNav : MonoBehaviour
{
    public bool LookPlayer
    {
        set
        {
            if (!value)
            {
                return;
            }

            LookAtPlayer();
        }
    }

    public Transform player;

    private void Update()
    {
    }

    public void LookAtPlayer()
    {
        if (!player)
        {
            return;
        }

        Debug.Log($"Look 1  {player.name}");
        transform.LookAt(player);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }
}