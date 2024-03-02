using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsFinishGame;
    public int CurrentStageGame;
    public int CurrentNumberZombie;
    public bool IsFinalStage;

    [Header("Player and Zombie")]
    public Transform SpawnPointPlayer;

    [Space(10)]
    public Transform[] SpawnPointsZombie;

    //
    private int m_indexSpawnPointZombie;
    private float m_TimePerStageDeltaTime;

    public override void Awake()
    {
        base.Awake();
        CurrentStageGame = -1;
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnStartGame, OnStartGame);
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
        EventDispatcher.Instance.RemoveListener(EventID.OnKilledZombie, OnKilledZombie);
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnStartGame, OnStartGame);
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
        EventDispatcher.Instance.RegisterListener(EventID.OnKilledZombie, OnKilledZombie);
    }

    public override void Update()
    {
        base.Update();
        if (m_TimePerStageDeltaTime > 0)
        {
            m_TimePerStageDeltaTime -= Time.deltaTime;
            EventDispatcher.Instance.PostEvent(EventID.OnChangeTimeNextStage, m_TimePerStageDeltaTime);
            if (m_TimePerStageDeltaTime <= 0)
            {
                PlayNextStage();
                EventDispatcher.Instance.PostEvent(EventID.OnChangeTimeNextStage);
            }
        }
    }

    private void OnKilledZombie(object obj)
    {
        CurrentNumberZombie--;
        if (CurrentNumberZombie <= 0)
        {
            CurrentNumberZombie = 0;
            if (IsFinalStage)
            {
                EventDispatcher.Instance.PostEvent(EventID.OnFinishGame, true);
                return;
            }

            if (GameConfig.Instance && GameConfig.Instance.TimePerStage > 10)
            {
                m_TimePerStageDeltaTime = 9;
            }
        }
    }

    private void OnFinishGame(object obj)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsFinishGame = true;
        Time.timeScale = 0.1f;
    }

    private void OnStartGame(object obj)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Instantiate(GameConfig.Instance.GetAppointee(),
            SpawnPointPlayer.position, Quaternion.identity);
        m_TimePerStageDeltaTime = 9;
        Time.timeScale = 1f;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void PlayNextStage()
    {
        CurrentStageGame++;
        if (GameConfig.Instance == null)
        {
            return;
        }

        IsFinalStage = GameConfig.Instance.CheckFinalStage(CurrentStageGame);

        InitStage(GameConfig.Instance.GetStage(CurrentStageGame));
        if (!IsFinalStage)
        {
            Invoke(nameof(DelayCountDownNextStage), 3f);
        }
    }

    private void DelayCountDownNextStage()
    {
        if (!GameConfig.Instance)
        {
            return;
        }

        m_TimePerStageDeltaTime = GameConfig.Instance.TimePerStage;
    }

    private void InitStage(StageGame stageGame)
    {
        Debug.Log("Play New Stage");
        foreach (StageInfo info in stageGame.StageInfos)
        {
            CurrentNumberZombie += info.Count;
            for (int i = 0; i < info.Count; i++)
            {
                SpawnZombie(info.ZombieName);
            }
        }
    }

    private void SpawnZombie(string name)
    {
        if (SpawnPointsZombie.Length == 0 || !GObj_pooling.Instance)
        {
            return;
        }

        GameObject zombie = GObj_pooling.Instance.Pull(GetPoolKeyZombie(name));
        if (zombie == null)
        {
            return;
        }

        m_indexSpawnPointZombie++;
        if (m_indexSpawnPointZombie >= SpawnPointsZombie.Length)
        {
            m_indexSpawnPointZombie = 0;
        }

        zombie.transform.position = SpawnPointsZombie[m_indexSpawnPointZombie].position;
        zombie.SetActive(true);
    }

    private PoolKEY GetPoolKeyZombie(string name)
    {
        PoolKEY key = PoolKEY.ZombieV1;
        switch (name.ToLower())
        {
            case "zombiev2":
                key = PoolKEY.ZombieV2;
                break;
            case "zombiev3":
                key = PoolKEY.ZombieV3;
                break;
        }

        return key;
    }
}