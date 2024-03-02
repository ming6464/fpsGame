using UnityEngine;

public class UIManager_Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject _uiHomePenal;

    [SerializeField]
    private GameObject _uiBagPenal;

    [SerializeField]
    private GameObject _uiCharacterPenal;

    private void OnEnable()
    {
        LinkEvent();
    }

    private void OnDisable()
    {
        UnLinkEvent();
    }

#region Event

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnBagPanel, OnBagPanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnHomePanel, OnHomePanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnCharacterPanel, OnCharacterPanel);
    }

    private void OnCharacterPanel(object obj)
    {
        DisableAllPanel();
        _uiCharacterPenal.SetActive(true);
    }

    private void OnHomePanel(object obj)
    {
        DisableAllPanel();
        _uiHomePenal.SetActive(true);
    }

    private void OnBagPanel(object obj)
    {
        DisableAllPanel();
        _uiBagPenal.SetActive(true);
    }

    private void DisableAllPanel()
    {
        _uiBagPenal.SetActive(false);
        _uiCharacterPenal.SetActive(false);
        _uiHomePenal.SetActive(false);
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnBagPanel, OnBagPanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnHomePanel, OnHomePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnCharacterPanel, OnCharacterPanel);
    }

#endregion
}