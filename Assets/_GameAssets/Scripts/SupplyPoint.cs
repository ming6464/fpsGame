using UnityEngine;

public class SupplyPoint : MonoBehaviour
{
    [Header("Reload Info")]
    public float ReloadTime;

    //
    private float m_reloadTimeDelta;
    private Color m_normalColor = Color.yellow;
    private Color m_color1 = Color.red;
    private Color m_color2 = Color.green;
    private MeshRenderer m_meshRenderer;
    private ReceiveSupplyBullet m_receiveSupplyBullet;

    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        HandleSupply();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleSupply()
    {
        if (m_receiveSupplyBullet == null)
        {
            m_meshRenderer.material.color = m_normalColor;
            m_reloadTimeDelta = 0f;
            return;
        }

        if (m_receiveSupplyBullet.CheckNeedSupply() == false)
        {
            m_meshRenderer.material.color = m_color2;
            m_reloadTimeDelta = 0f;
            return;
        }

        m_meshRenderer.material.color = m_color1;
        if (m_reloadTimeDelta <= 0f)
        {
            m_reloadTimeDelta = ReloadTime;
        }

        m_reloadTimeDelta -= Time.deltaTime;
        if (m_reloadTimeDelta <= 0f)
        {
            m_receiveSupplyBullet.Supply();
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateSupplyProgress);
        }
        else
        {
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateSupplyProgress,
                1.0f - m_reloadTimeDelta / ReloadTime);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        m_receiveSupplyBullet = other.GetComponent<ReceiveSupplyBullet>();
    }

    private void OnTriggerExit(Collider other)
    {
        m_receiveSupplyBullet = null;
        m_reloadTimeDelta = 0f;
        EventDispatcher.Instance.PostEvent(EventID.OnUpdateSupplyProgress);
    }
}