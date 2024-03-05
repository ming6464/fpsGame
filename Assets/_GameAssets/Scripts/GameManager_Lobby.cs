using UnityEngine;

public class GameManager_Lobby : MonoBehaviour
{
    [SerializeField]
    private Camera _backgroundCam;


    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnPlayGame, OnPlayGame);
        EventDispatcher.Instance.RegisterListener(EventID.OnSelectCharacter, OnSelectCharacter);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnPlayGame, OnPlayGame);
        EventDispatcher.Instance.RemoveListener(EventID.OnSelectCharacter, OnSelectCharacter);
    }

    private void Start()
    {
        LoadBackground();
    }

    private void OnSelectCharacter(object obj)
    {
        LoadBackground();
    }

    private void LoadBackground()
    {
        _backgroundCam.cullingMask |= (1 << 10) | (1 << 11) | (1 << 12);
        switch (SaveManager.CharacterNameSelect.ToLower())
        {
            case "swat":
                _backgroundCam.cullingMask &= ~((1 << 11) | (1 << 12));
                break;
            case "atienza":
                _backgroundCam.cullingMask &= ~((1 << 10) | (1 << 12));
                break;
            case "gonza":
                _backgroundCam.cullingMask &= ~((1 << 11) | (1 << 10));
                break;
        }
    }

    private void OnPlayGame(object obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnLoadScene, 1);
    }
}