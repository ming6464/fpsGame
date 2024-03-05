using UnityEngine;

public class UIHome : MonoBehaviour
{
    [SerializeField]
    private GameObject _highLinePlayBtn;

    [SerializeField]
    private GameObject _highLineHomeBtn;

    [SerializeField]
    private GameObject _highLineCharacterBtn;

    [SerializeField]
    private GameObject _highLineTutorialBtn;

    [SerializeField]
    private GameObject _highLineSettingBtn;


    [Header("Audio-Music")]
    [SerializeField]
    private GameObject _audioOn;

    [SerializeField]
    private GameObject _audioOff;

    [SerializeField]
    private GameObject _musicOn;

    [SerializeField]
    private GameObject _musicOff;

    private void Start()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnHomePanel);
        ClickButton(_highLineHomeBtn);
        LoadAudio();
        LoadMusic();
    }

    public void Play_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnPlayGame);
        ClickButton(_highLinePlayBtn);
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

    public void Music_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        SaveManager.MusicSound = !SaveManager.MusicSound;
        LoadMusic();
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

    private void LoadMusic()
    {
        bool check = SaveManager.MusicSound;
        _musicOn.SetActive(check);
        _musicOff.SetActive(!check);
        if (AudioManager.Instance)
        {
            if (check)
            {
                AudioManager.Instance.ActiveMusic();
                AudioManager.Instance.PlayMusic(KeySound.Theme);
            }
            else
            {
                AudioManager.Instance.DisableMusic();
            }
        }
    }

    public void Character_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnCharacterPanel);
        ClickButton(_highLineCharacterBtn);
    }

    public void Home_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnHomePanel);
        ClickButton(_highLineHomeBtn);
    }

    public void Tutorial_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnTutorialPanel);
        ClickButton(_highLineTutorialBtn);
    }

    public void Setting_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnSettingPanel);
        ClickButton(_highLineSettingBtn);
    }


    private void ClickButton(GameObject btn)
    {
        DisableAllHighLine();
        btn.SetActive(true);
    }

    private void DisableAllHighLine()
    {
        _highLinePlayBtn.SetActive(false);
        _highLineHomeBtn.SetActive(false);
        _highLineCharacterBtn.SetActive(false);
        _highLineTutorialBtn.SetActive(false);
        _highLineSettingBtn.SetActive(false);
    }
}