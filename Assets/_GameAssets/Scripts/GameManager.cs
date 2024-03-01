using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsFinishGame;
    public int CurrentStageGame;

    [Header("Player and Zombie")]
    [SerializeField]
    private Transform _spawnPointPlayerTf;

    [Space(10)]
    [SerializeField]
    private SpawnZombieManager _spawnZombieManager;

    public override void Awake()
    {
        base.Awake();
        CurrentStageGame = -1;
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnStartGame, OnStartGame);
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
    }

    private void OnFinishGame(object obj)
    {
        IsFinishGame = true;
        Time.timeScale = 0.1f;
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnStartGame, OnStartGame);
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
    }

    public void OnStartGame(object obj)
    {
        Instantiate(GameConfig.Instance.GetAppointee(),
            _spawnPointPlayerTf.position, Quaternion.identity);
        PlayNextStage();
        Time.timeScale = 1f;
    }

    public void PlayNextStage()
    {
        CurrentStageGame++;
        if (GameConfig.Instance == null || GameConfig.Instance.CheckFinalStage(CurrentStageGame))
        {
            return;
        }

        _spawnZombieManager.SpawnStage(GameConfig.Instance.GetStage(CurrentStageGame));
    }
}