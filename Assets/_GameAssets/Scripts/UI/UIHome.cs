using UnityEngine;

public class UIHome : MonoBehaviour
{
    [SerializeField]
    private GameObject _highLinePlayBtn;

    [SerializeField]
    private GameObject _highLineHomeBtn;

    [SerializeField]
    private GameObject _highLineBagBtn;

    [SerializeField]
    private GameObject _highLineCharacterBtn;

    private void Start()
    {
        Home_button_on_click();
    }

    public void Play_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnPlayGame);
        ClickButton(_highLinePlayBtn);
    }

    public void Bag_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnBagPanel);
        ClickButton(_highLineBagBtn);
    }

    public void Character_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnCharacterPanel);
        ClickButton(_highLineCharacterBtn);
    }

    public void Home_button_on_click()
    {
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
        _highLineBagBtn.SetActive(false);
        _highLineCharacterBtn.SetActive(false);
    }
}