using System;
using UnityEngine;

public class UIPauseGame : MonoBehaviour
{
    public bool PauseGameState;

    [Serializable]
    public struct PanelGameInfo
    {
        public string Name;
        public GameObject PanelGame;
        public GameObject HighLineButton;
    }

    [SerializeField]
    private CanvasGroup _uiPauseGameCg;

    [Space(10)]
    [SerializeField]
    private PanelGameInfo[] _panelGameInfos;

    [Header("Warning game")]
    [SerializeField]
    private GameObject _warningGameWin;

    [SerializeField]
    private GameObject _warningGameLose;

    [Header("Audio")]
    [SerializeField]
    private GameObject _audioOn;

    [SerializeField]
    private GameObject _audioOff;

    private bool m_isOpenPauseGame;
    private bool m_isClosePauseGame;

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnOpenPauseGamePanel, OnOpenPauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnClosePauseGamePanel, OnClosePauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnHandlePauseGamePanel, OnHandlePauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnShowResultGame, OnShowResultGame);
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
        EventDispatcher.Instance.RegisterListener(EventID.OnShowResultGame, OnShowResultGame);
    }

    private void Start()
    {
        _warningGameWin.SetActive(false);
        _warningGameLose.SetActive(false);
        LoadAudio();
    }

    private void OnShowResultGame(object obj)
    {
        if (obj == null)
        {
            return;
        }

        _warningGameWin.SetActive((bool)obj);
        _warningGameLose.SetActive(!(bool)obj);
        EventDispatcher.Instance.PostEvent(EventID.OnOpenPauseGamePanel);
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
                Time.timeScale = 0f;
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
        Time.timeScale = 1f;
        m_isOpenPauseGame = true;
        Warning_button_on_click();
    }

    private void OnClosePauseGamePanel(object obj = null)
    {
        PauseGameState = false;
        _uiPauseGameCg.alpha = 1f;
        _uiPauseGameCg.interactable = false;
        Time.timeScale = 1f;
        m_isClosePauseGame = true;
    }

    public void Warning_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        ClickButton("warning");
    }

    public void Tutorial_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        ClickButton("tutorial");
    }

    public void Replay_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        ClickButton("replay");
        OnClosePauseGamePanel();
        EventDispatcher.Instance.PostEvent(EventID.OnLoadScene, 1);
    }

    public void Home_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        OnClosePauseGamePanel();
        ClickButton("home");
        EventDispatcher.Instance.PostEvent(EventID.OnLoadScene, 0);
    }

    private void ClickButton(string name)
    {
        name = name.ToLower();
        foreach (PanelGameInfo info in _panelGameInfos)
        {
            bool isActive = info.Name.ToLower() == name;
            if (info.PanelGame)
            {
                info.PanelGame.SetActive(isActive);
            }

            if (info.HighLineButton)
            {
                info.HighLineButton.SetActive(isActive);
            }
        }
    }

    public void Audio_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        SaveManager.AudioSound = !SaveManager.AudioSound;
        LoadAudio();
    }

    private void LoadAudio()
    {
        bool check = SaveManager.AudioSound;
        _audioOn.SetActive(check);
        _audioOff.SetActive(!check);
        if (AudioManager.Instance)
        {
            if (check)
            {
                AudioManager.Instance.ActiveSfx();
            }
            else
            {
                AudioManager.Instance.DisableSfx();
            }
        }
    }
}