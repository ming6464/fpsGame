using UnityEngine;

public class UIHome : MonoBehaviour
{
    [SerializeField]
    private GameObject _highLinePlayBtn;

    [SerializeField]
    private GameObject _highLineHomeBtn;

    [SerializeField]
    private GameObject _highLineCharacterBtn;

    [Header("Audio")]
    [SerializeField]
    private GameObject _audioOn;

    [SerializeField]
    private GameObject _audioOff;

    private void Start()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnHomePanel);
        ClickButton(_highLineHomeBtn);
        LoadAudio();
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
    }
}