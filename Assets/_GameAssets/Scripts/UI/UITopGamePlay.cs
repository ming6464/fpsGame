using System;
using TMPro;
using UnityEngine;

public class UITopGamePlay : MonoBehaviour
{
    [Header("Time Stage")]
    [SerializeField]
    private TextMeshProUGUI _timeSpawnNextStageText;

    [SerializeField]
    private TextMeshProUGUI _CountDownHighLine;

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeTimeNextStage, OnChangeTimeNextStage);
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeTimeNextStage, OnChangeTimeNextStage);
    }

    private void OnChangeTimeNextStage(object obj)
    {
        if (obj == null)
        {
            _timeSpawnNextStageText.gameObject.SetActive(false);
            return;
        }

        _timeSpawnNextStageText.gameObject.SetActive(true);

        int time = Mathf.RoundToInt((float)obj);
        if (time < 10)
        {
            _CountDownHighLine.gameObject.SetActive(true);
            _CountDownHighLine.text = time + "s";
            _timeSpawnNextStageText.text = "countdown time for the next zombie wave";
        }
        else
        {
            _CountDownHighLine.gameObject.SetActive(false);
            _timeSpawnNextStageText.text = $"countdown time for the next zombie wave : {time}s";
        }
    }
}