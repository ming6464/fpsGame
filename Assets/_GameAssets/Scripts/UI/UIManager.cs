using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _finishGamePanel;

    [SerializeField]
    private GameObject _startGamePanel;

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
    }

    private void Start()
    {
        _startGamePanel.SetActive(true);
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

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
    }
}