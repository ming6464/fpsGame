using UnityEngine;

public class UIManager_Lobby : MonoBehaviour
{
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
    }

    private void DisableAllPanel()
    {
        _uiCharacterPenal.SetActive(false);
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnHomePanel, OnHomePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnCharacterPanel, OnCharacterPanel);
    }

#endregion
}