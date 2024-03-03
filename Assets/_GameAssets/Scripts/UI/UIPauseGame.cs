using System;
using UnityEngine;

public class UIPauseGame : MonoBehaviour
{
    public bool PauseGameState;

    [SerializeField]
    private CanvasGroup _uiPauseGameCg;

    [Space(10)]
    [SerializeField]
    private GameObject _warningHighLine;

    [SerializeField]
    private GameObject _settingHighLine;

    [SerializeField]
    private GameObject _replayHighLine;

    [SerializeField]
    private GameObject _homeHighLine;


    private bool m_isOpenPauseGame;
    private bool m_isClosePauseGame;

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnOpenPauseGamePanel, OnOpenPauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnClosePauseGamePanel, OnClosePauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnHandlePauseGamePanel, OnHandlePauseGamePanel);
    }

    private void OnHandlePauseGamePanel(object obj)
    {
        if (PauseGameState)
        {
            EventDispatcher.Instance.PostEvent(EventID.OnClosePauseGamePanel);
        }
        else
        {
            EventDispatcher.Instance.PostEvent(EventID.OnOpenPauseGamePanel);
        }
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnOpenPauseGamePanel, OnOpenPauseGamePanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnClosePauseGamePanel, OnClosePauseGamePanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnHandlePauseGamePanel, OnHandlePauseGamePanel);
    }

    private void Update()
    {
        if (m_isOpenPauseGame)
        {
            _uiPauseGameCg.alpha = Mathf.Lerp(_uiPauseGameCg.alpha, 1f, 10 * Time.deltaTime);
            if (_uiPauseGameCg.alpha >= 0.91f)
            {
                _uiPauseGameCg.alpha = 1f;
                m_isOpenPauseGame = false;
            }
        }
        else if (m_isClosePauseGame)
        {
            _uiPauseGameCg.alpha = Mathf.Lerp(_uiPauseGameCg.alpha, 0f, 10 * Time.deltaTime);
            if (_uiPauseGameCg.alpha <= 0.1f)
            {
                _uiPauseGameCg.alpha = 0f;
                m_isClosePauseGame = false;
            }
        }
    }

    private void OnOpenPauseGamePanel(object obj = null)
    {
        PauseGameState = true;
        _uiPauseGameCg.alpha = 0f;
        _uiPauseGameCg.interactable = true;
        m_isOpenPauseGame = true;
    }

    private void OnClosePauseGamePanel(object obj = null)
    {
        PauseGameState = false;
        _uiPauseGameCg.alpha = 1f;
        _uiPauseGameCg.interactable = false;
        m_isClosePauseGame = true;
    }

    public void Warning_button_on_click()
    {
        ClickButton(_warningHighLine);
    }

    public void Setting_button_on_click()
    {
        ClickButton(_settingHighLine);
    }

    public void Replay_button_on_click()
    {
        ClickButton(_replayHighLine);
        OnClosePauseGamePanel();
        EventDispatcher.Instance.PostEvent(EventID.OnLoadScene, 1);
    }

    public void Home_button_on_click()
    {
        OnClosePauseGamePanel();
        ClickButton(_homeHighLine);
        EventDispatcher.Instance.PostEvent(EventID.OnLoadScene, 0);
    }

    private void ClickButton(GameObject highLine)
    {
        _homeHighLine.SetActive(false);
        _settingHighLine.SetActive(false);
        _warningHighLine.SetActive(false);
        _replayHighLine.SetActive(false);
        highLine.SetActive(true);
    }
}