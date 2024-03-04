using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    private Transform m_targetFollow;
    private Vector3 m_posNext;
    private float m_posAxisy;
    private float m_rotationAxisY;
    private Vector3 m_rotation;


    public void FollowTarget(Transform target)
    {
        if (!target)
        {
            return;
        }

        m_targetFollow = target;
        m_posAxisy = transform.position.y;
        m_rotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_targetFollow)
        {
            m_posNext = m_targetFollow.position;
            m_posNext.y = m_posAxisy;

            m_rotationAxisY = m_targetFollow.rotation.eulerAngles.y;

            transform.position = m_posNext;
            transform.rotation = Quaternion.Euler(m_rotation.x, m_rotationAxisY, m_rotation.z);
        }
    }
}