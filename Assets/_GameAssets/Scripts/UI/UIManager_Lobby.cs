using UnityEngine;

public class UIManager_Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject _uiCharacterPenal;

    [SerializeField]
    private GameObject _uiTutorialPenal;

    [SerializeField]
    private GameObject _uiSettingPenal;


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
        EventDispatcher.Instance.RegisterListener(EventID.OnHomePanel, OnHomePanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnCharacterPanel, OnCharacterPanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnTutorialPanel, OnTutorialPanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnSettingPanel, OnSettingPanel);
    }

    private void OnSettingPanel(object obj)
    {
        DisableAllPanel();
        _uiSettingPenal.SetActive(true);
    }

    private void OnTutorialPanel(object obj)
    {
        DisableAllPanel();
        _uiTutorialPenal.SetActive(true);
    }

    private void OnCharacterPanel(object obj)
    {
        DisableAllPanel();
        _uiCharacterPenal.SetActive(true);
    }

    private void OnHomePanel(object obj)
    {
        DisableAllPanel();
    }

    private void DisableAllPanel()
    {
        _uiCharacterPenal.SetActive(false);
        _uiTutorialPenal.SetActive(false);
        _uiSettingPenal.SetActive(false);
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnHomePanel, OnHomePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnCharacterPanel, OnCharacterPanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnTutorialPanel, OnTutorialPanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnSettingPanel, OnSettingPanel);
    }

#endregion
}