using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    [SerializeField]
    private float _smoothSpeed;

    private Transform m_targetFollow;
    private Vector3 m_posNext;
    private float m_posAxisZ;
    private float m_rotationAxisY;
    private Vector3 m_rotation;


    public void FollowTarget(Transform target)
    {
        if (!target)
        {
            return;
        }

        m_targetFollow = target;
        m_posNext = target.position;
        m_posAxisZ = transform.position.z;
        m_posNext.z = m_posAxisZ;
        m_rotation = transform.rotation.eulerAngles;
        m_rotationAxisY = target.rotation.eulerAngles.y;
        transform.position = m_posNext;
        transform.rotation = Quaternion.Euler(m_rotation.x, m_rotationAxisY, m_rotation.z);
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_targetFollow)
        {
            m_posNext = Vector3.Lerp(m_posNext, m_targetFollow.position, _smoothSpeed * Time.deltaTime);
            m_rotationAxisY = Mathf.Lerp(m_rotationAxisY, m_targetFollow.rotation.eulerAngles.y,
                _smoothSpeed * Time.deltaTime);
            m_posNext.z = m_posAxisZ;
            transform.position = m_posNext;
            transform.rotation = Quaternion.Euler(m_rotation.x, m_rotationAxisY, m_rotation.z);
        }
    }
}