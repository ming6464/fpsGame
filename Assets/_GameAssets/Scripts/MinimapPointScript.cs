using UnityEngine;

public class MinimapPointScript : MonoBehaviour
{
    public float PosAxisY;
    public bool UsePositionDefault;
    public Transform Object;

    [Header("icon")]
    public SpriteRenderer MinimapIcon;

    public Sprite MinimapIconAlive;
    public Sprite MinimapIconDead;

    private float m_posAxisY;
    private Transform m_pointTf;
    private Transform m_minimapCam;
    private float m_minimapRadius;
    private bool m_isDead;

    private void OnEnable()
    {
        m_isDead = false;
        m_minimapCam = GameObject.FindGameObjectWithTag("CamMinimap").transform;

        if (m_minimapCam.TryGetComponent(out Camera camera))
        {
            m_minimapRadius = camera.orthographicSize;
        }

        m_pointTf = MinimapIcon.transform;
        if (UsePositionDefault)
        {
            m_posAxisY = transform.position.y;
        }
        else
        {
            m_posAxisY = PosAxisY;
        }

        if (MinimapIcon && MinimapIconAlive)
        {
            MinimapIcon.sprite = MinimapIconAlive;
        }
    }

    private void LateUpdate()
    {
        if (!m_minimapCam || !m_pointTf || !Object || m_isDead)
        {
            return;
        }

        Vector3 nextPos = Object.position;
        Vector3 posMiniCam = m_minimapCam.position;
        posMiniCam.y = m_posAxisY;
        nextPos.y = m_posAxisY;

        float dis = Vector3.Distance(posMiniCam, nextPos);
        if (dis >= m_minimapRadius)
        {
            Vector3 dir = nextPos - posMiniCam;
            nextPos = dir.normalized * (m_minimapRadius - 0.15f) + posMiniCam;
        }

        m_pointTf.position = nextPos;
    }

    public void OnDead()
    {
        if (MinimapIcon && MinimapIconDead)
        {
            MinimapIcon.sprite = MinimapIconDead;
        }

        m_isDead = true;
    }
}