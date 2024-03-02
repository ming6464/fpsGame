using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _finishGamePanel;

    [SerializeField]
    private GameObject _startGamePanel;

    [SerializeField]
    private GameObject _crossHair;

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
        EventDispatcher.Instance.RegisterListener(EventID.OnHandleCrossHair, OnHandleCrossHair);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
        EventDispatcher.Instance.RemoveListener(EventID.OnHandleCrossHair, OnHandleCrossHair);
    }


    private void Start()
    {
        _startGamePanel.SetActive(true);
    }

    private void OnHandleCrossHair(object obj)
    {
        _crossHair.SetActive((bool)obj);
    }

    private void OnFinishGame(object obj)
    {
        _finishGamePanel.SetActive(true);
    }

    public void OnStartGame()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnStartGame);
        _startGamePanel.SetActive(false);
    }

    public void RePlay()
    {
        SceneManager.LoadScene(0);
    }
}