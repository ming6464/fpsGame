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

    [Header("Score")]
    [SerializeField]
    private TextMeshProUGUI _amountHero;

    [SerializeField]
    private TextMeshProUGUI _amountzombie;

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeTimeNextStage, OnChangeTimeNextStage);
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeAmountZombie, OnChangeAmountZombie);
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeAmountHero, OnChangeAmountHero);
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeTimeNextStage, OnChangeTimeNextStage);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeAmountZombie, OnChangeAmountZombie);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeAmountHero, OnChangeAmountHero);
    }

    private void OnChangeAmountZombie(object obj)
    {
        if (obj == null)
        {
            return;
        }

        _amountzombie.text = (int)obj + "";
    }

    private void OnChangeAmountHero(object obj)
    {
        if (obj == null)
        {
            return;
        }

        _amountHero.text = (int)obj + "";
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