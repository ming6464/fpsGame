using UnityEngine;

public class MinimapPointScript : MonoBehaviour
{
    public float PosAxisY;
    public bool UsePositionDefault;

    [Header("icon")]
    public SpriteRenderer MinimapIcon;

    public Sprite MinimapIconAlive;
    public Sprite MinimapIconDead;

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

        if (MinimapIcon && MinimapIconAlive)
        {
            MinimapIcon.sprite = MinimapIconAlive;
        }
    }

    private void LateUpdate()
    {
        Vector3 nextPos = transform.position;
        nextPos.y = m_posAxisY;
        transform.position = nextPos;
    }

    public void OnDead()
    {
        if (MinimapIcon && MinimapIconDead)
        {
            MinimapIcon.sprite = MinimapIconDead;
        }
    }
}