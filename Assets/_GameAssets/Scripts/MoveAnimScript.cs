using UnityEngine;

public class MoveAnimScript : MonoBehaviour
{
    public Transform objMove;
    public Vector3 LocalPosition1;
    public Vector3 LocalPosition2;

    public float Speed;

    //
    private Vector3 m_targetPosition;

    // Start is called before the first frame update
    private void Start()
    {
        objMove.localPosition = LocalPosition1;
        m_targetPosition = LocalPosition2;
    }

    // Update is called once per frame
    private void Update()
    {
        PlayAnimReloadText();
    }

    private void PlayAnimReloadText()
    {
        objMove.localPosition = Vector3.Lerp(objMove.localPosition, m_targetPosition, Speed * Time.deltaTime);
        if (Vector3.Distance(m_targetPosition, objMove.localPosition) <= 0.2f)
        {
            if (m_targetPosition == LocalPosition1)
            {
                m_targetPosition = LocalPosition2;
            }
            else
            {
                m_targetPosition = LocalPosition1;
            }
        }
    }
}