using UnityEngine;

public class SupplyPoint : MonoBehaviour
{
    [Header("Reload Info")]
    public float ReloadTime;

    [Space(10)]
    public MeshRenderer MeshRenderer;

    //
    private float m_reloadTimeDelta;
    private Color m_normalColor = Color.gray;
    private Color m_color1 = Color.red;
    private Color m_color2 = Color.green;
    private ReceiveSupplyBullet m_receiveSupplyBullet;


    // Update is called once per frame
    private void Update()
    {
        HandleSupply();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleSupply()
    {
        if (!MeshRenderer)
        {
            return;
        }

        if (m_receiveSupplyBullet == null)
        {
            MeshRenderer.material.color = m_normalColor;
            m_reloadTimeDelta = 0f;
            return;
        }

        if (m_receiveSupplyBullet.CheckNeedSupply() == false)
        {
            MeshRenderer.material.color = m_color2;
            m_reloadTimeDelta = 0f;
            return;
        }

        MeshRenderer.material.color = m_color1;
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