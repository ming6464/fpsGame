using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneScript : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _loadUiCg;

    [SerializeField]
    private Image _slideImageLoad;

    private bool m_animLerp;
    private int indexSceneLoad;

    private void Awake()
    {
        OnResetLoad();
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnLoadScene, OnLoadScene);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnLoadScene, OnLoadScene);
    }


    private void OnLoadScene(object obj)
    {
        if (obj == null)
        {
            return;
        }

        _loadUiCg.interactable = false;
        _loadUiCg.blocksRaycasts = true;
        _loadUiCg.alpha = 0f;
        m_animLerp = true;
        _slideImageLoad.fillAmount = 0f;
        indexSceneLoad = (int)obj;
    }

    private void OnResetLoad()
    {
        _loadUiCg.interactable = true;
        _loadUiCg.blocksRaycasts = false;
        _loadUiCg.alpha = 0f;
        m_animLerp = false;
        indexSceneLoad = -1;
    }

    private IEnumerator LoadSceneSync(int index)
    {
        AsyncOperation sync = SceneManager.LoadSceneAsync(index);
        while (!sync.isDone)
        {
            _slideImageLoad.fillAmount = sync.progress;
            yield return new WaitForEndOfFrame();
        }

        OnResetLoad();
    }

    private void Update()
    {
        if (m_animLerp)
        {
            _loadUiCg.alpha = Mathf.Lerp(_loadUiCg.alpha, 1f, 10 * Time.deltaTime);
            if (_loadUiCg.alpha > 0.95f)
            {
                _loadUiCg.alpha = 1;
                m_animLerp = false;
                if (indexSceneLoad < 0)
                {
                    return;
                }

                StartCoroutine(LoadSceneSync(indexSceneLoad));
            }
        }
    }
}