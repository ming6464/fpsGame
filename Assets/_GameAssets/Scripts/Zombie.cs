using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [Header("Info")]
    public string Name;

    [Header("References")]
    [SerializeField]
    private NavMeshAgent _navMeshAgent;

    private Collider m_collider;
    private ZombieDamageSender m_zombieDamageSender;
    private ZombieInfo m_zombieInfo;
    private Transform m_player;

    public float AttackRange => m_zombieInfo.AttackRange;
    public float DistanceStop => m_zombieInfo.DistanceStop;

    public float DetectionRadiusWhenChasing => m_zombieInfo.DetectionRadiusWhenChasing;
    public float DetectionRadiusWhenPatrolling => m_zombieInfo.DetectionRadiusWhenPatrolling;

    [HideInInspector]
    public bool UpdateRotate;

    private void Awake()
    {
        m_zombieInfo = GameConfig.Instance.GetZombieInfo(Name);
        _navMeshAgent = GetComponent<NavMeshAgent>();
        m_collider = GetComponent<Collider>();
        m_zombieDamageSender = GetComponent<ZombieDamageSender>();
        _navMeshAgent.updateRotation = false;
    }

    private void OnEnable()
    {
        if (m_collider)
        {
            m_collider.enabled = true;
        }

        UpdateRotate = true;
        m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (m_zombieInfo != null)
        {
            if (TryGetComponent(out DamageReceiver damageReceiver))
            {
                damageReceiver.UpdateHpInfo(m_zombieInfo.MaxHP);
            }

            if (TryGetComponent(out DamageSender damageSender))
            {
                damageSender.SetDamage(m_zombieInfo.Damage);
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
        if (!m_player)
        {
            return false;
        }

        Vector3 Point1 = transform.position + Vector3.up;
        Vector3 Point2 = m_player.transform.position + Vector3.up;
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
        if (!m_player || !m_zombieDamageSender)
        {
            return;
        }

        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
            new Vector2(m_player.position.x, m_player.position.z));

        if (distance <= m_zombieInfo.AttackRange)
        {
            m_zombieDamageSender.Send(m_player);
            Debug.Log("m_player is damaged");
        }
    }

    private void OnFootStep(int state)
    {
        return;
        if (!AudioManager.Instance || !m_player)
        {
            return;
        }

        float dis = Vector3.Distance(m_player.position, transform.position);
        float volume = GameConfig.Instance.Volume;
        float maxDis = GameConfig.Instance.MaxDistance;
        if (dis > maxDis)
        {
            return;
        }

        if (state == 0)
        {
            AudioManager.Instance.PlaySfx(KeySound.WalkFootStepStone, volume * (1 - dis / maxDis));
        }
        else
        {
            AudioManager.Instance.PlaySfx(KeySound.RunFootStepStone, volume * (1 - dis / maxDis));
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