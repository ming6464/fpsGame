using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _finishGamePanel;

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

    private void OnHandleCrossHair(object obj)
    {
        _crossHair.SetActive((bool)obj);
    }

    private void OnFinishGame(object obj)
    {
        _finishGamePanel.SetActive(true);
    }

    private void Start()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnClosePauseGamePanel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventDispatcher.Instance.PostEvent(EventID.OnHandlePauseGamePanel);
        }
    }
}