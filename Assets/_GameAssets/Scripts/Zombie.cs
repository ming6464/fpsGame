using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [Header("Info")]
    public string Name;

    [Header("References")]
    [SerializeField]
    private NavMeshAgent _navMeshAgent;

    private Collider collider;
    private ZombieDamageSender zombieDamageSender;
    private ZombieInfo zombieInfo;
    private Transform player;
    public bool UpdateRotate;

    private void Awake()
    {
        zombieInfo = GameConfig.Instance.GetZombieInfo(Name);
        if (TryGetComponent(out _navMeshAgent))
        {
            _navMeshAgent.updateRotation = false;
        }

        TryGetComponent(out collider);
        TryGetComponent(out zombieDamageSender);
    }

    private void OnEnable()
    {
        if (collider)
        {
            collider.enabled = true;
        }

        UpdateRotate = true;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (zombieInfo != null)
        {
            if (TryGetComponent(out DamageReceiver damageReceiver))
            {
                damageReceiver.UpdateHpInfo(zombieInfo.HP, zombieInfo.MaxHP);
            }

            if (TryGetComponent(out DamageSender damageSender))
            {
                damageSender.SetDamage(zombieInfo.Damage);
            }
        }
    }

    private void Update()
    {
        if (UpdateRotate && _navMeshAgent && _navMeshAgent.hasPath)
        {
            Transform tf = transform;
            Vector3 dir = (_navMeshAgent.steeringTarget - tf.position).normalized;
            Quaternion rotateTowards =
                Quaternion.RotateTowards(tf.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, rotateTowards.eulerAngles.y, 0);
        }
    }

    public bool CheckCanAttack()
    {
        if (!player)
        {
            return false;
        }

        Vector3 Point1 = transform.position + Vector3.up;
        Vector3 Point2 = player.transform.position + Vector3.up;
        Vector3 dir = Point2 - Point1;
        RaycastHit[] result = new RaycastHit[3];
        Physics.RaycastNonAlloc(Point1, dir, result, dir.magnitude);

        foreach (RaycastHit hit in result)
        {
            if (!hit.transform || !hit.transform.CompareTag("Environment"))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void OnAttack()
    {
        if (!player || !zombieDamageSender)
        {
            return;
        }

        // float angleY = Quaternion.FromToRotation(player.position - transform.position, transform.forward).eulerAngles.y;
        // if (angleY > 180f)
        // {
        //     angleY -= 360f;
        // }
        //
        // angleY = Mathf.Abs(angleY);
        // if (angleY > 35f)
        // {
        //     return;
        // }

        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z));

        if (distance <= zombieInfo.AttackRange)
        {
            zombieDamageSender.Send(player);
            Debug.Log("player is damaged");
        }
    }


    private void OnDrawGizmos()
    {
        if (!_navMeshAgent || !_navMeshAgent.hasPath)
        {
            return;
        }

        Vector3[] paths = _navMeshAgent.path.corners;
        for (int i = 0; i < paths.Length - 1; i++)
        {
            Debug.DrawLine(paths[i], paths[i + 1], Color.blue);
        }
    }
}