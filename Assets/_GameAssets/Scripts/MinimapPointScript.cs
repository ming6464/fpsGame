using UnityEngine;

public class MinimapPointScript : MonoBehaviour
{
    public float PosAxisY;
    public bool UsePositionDefault;

    private float m_posAxisY;

    private void OnEnable()
    {
        if (UsePositionDefault)
        {
            m_posAxisY = transform.position.y;
        }
        else
        {
            m_posAxisY = PosAxisY;
        }
    }

    private void LateUpdate()
    {
        Vector3 nextPos = transform.position;
        nextPos.y = m_posAxisY;
        transform.position = nextPos;
    }
}