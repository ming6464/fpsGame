using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UISettingPanel : MonoBehaviour
{
    [SerializeField]
    private UIPauseGame.PanelGameInfo[] _panelGameInfos;

    [Header("Bag tab")]
    [Space(10)]
    [SerializeField]
    private TMP_InputField _numberBulletRifile;

    [SerializeField]
    private TMP_InputField _numberBulletShotgun;

    [SerializeField]
    private TMP_InputField _numberBulletPistol;


    [Header("Stage tab")]
    [Space(10)]
    [SerializeField]
    private Transform _parentStage;

    [SerializeField]
    private StageItemScript _itemStagePrefab;

    [Header("Message")]
    [SerializeField]
    private GameObject _messageSuccess;

    private BagInfo m_bagInfo = new();
    private List<StageGame> stageGames = new();
    private List<StageItemScript> stageItemScripts;

    private void Start()
    {
        LoadData();
        UploadData();
    }

    private void UploadData()
    {
        if (_numberBulletRifile)
        {
            _numberBulletRifile.text = m_bagInfo.TotalBulletRifle.ToString();
        }

        if (_numberBulletPistol)
        {
            _numberBulletPistol.text = m_bagInfo.TotalBulletPistol.ToString();
        }

        if (_numberBulletShotgun)
        {
            _numberBulletShotgun.text = m_bagInfo.TotalBulletShotgun.ToString();
        }

        if (_parentStage && _itemStagePrefab)
        {
            stageItemScripts = new List<StageItemScript>();
            for (int i = 0; i < stageGames.Count; i++)
            {
                StageItemScript item = Instantiate(_itemStagePrefab, _parentStage);
                item.LoadData(stageGames[i], i);
                stageItemScripts.Add(item);
            }
        }
    }

    private void LoadData()
    {
        if (!GameConfig.Instance)
        {
            return;
        }

        //load data
        stageGames = GameConfig.Instance.GetAllStage().ToList();
        m_bagInfo = GameConfig.Instance.GetBagInfo();
    }

    public void ClickTabButton(string name)
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        name = name.ToLower();
        foreach (UIPauseGame.PanelGameInfo info in _panelGameInfos)
        {
            bool isActive = info.Name.ToLower() == name;
            if (info.PanelGame)
            {
                info.PanelGame.SetActive(isActive);
            }

            if (info.HighLineButton)
            {
                info.HighLineButton.SetActive(isActive);
            }
        }
    }

    public void Save_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        if (!GameConfig.Instance)
        {
            return;
        }

        List<StageGame> stageGames = new();
        foreach (StageItemScript stageItemScript in stageItemScripts)
        {
            stageGames.Add(stageItemScript.GetStageData());
        }

        int numberRifle = int.Parse(_numberBulletRifile.text);
        int numberPistol = int.Parse(_numberBulletPistol.text);
        int numberShogun = int.Parse(_numberBulletShotgun.text);

        GameConfig.Instance.UpdateData(numberRifle, numberShogun, numberPistol, stageGames.ToArray());

        _messageSuccess.SetActive(true);
        Invoke(nameof(DelayDisableMessage), 1.5f);
    }

    private void DelayDisableMessage()
    {
        _messageSuccess.SetActive(false);
    }

    public void Add_new_stage_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        StageItemScript item = Instantiate(_itemStagePrefab, _parentStage);
        item.LoadData(null, stageItemScripts.Count);
        stageItemScripts.Add(item);
    }

    public void Remove_stage_button_on_click()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(KeySound.UI);
        }

        if (stageItemScripts.Count <= 1)
        {
            return;
        }

        int index = stageItemScripts.Count - 1;
        stageItemScripts[index].OnDestroy();
        stageItemScripts.RemoveAt(index);
    }

    public void OnChangeTextNumberBulletRifle(string text)
    {
        if (!_numberBulletRifile)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _numberBulletRifile.text = "0";
            return;
        }

        _numberBulletRifile.text = Mathf.Abs(int.Parse(text)).ToString();
    }

    public void OnChangeTextNumberBulletShotgun(string text)
    {
        if (!_numberBulletShotgun)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _numberBulletShotgun.text = "0";
            return;
        }

        _numberBulletShotgun.text = Mathf.Abs(int.Parse(text)).ToString();
    }

    public void OnChangeTextNumberBulletPistol(string text)
    {
        if (!_numberBulletPistol)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _numberBulletPistol.text = "0";
            return;
        }

        _numberBulletPistol.text = Mathf.Abs(int.Parse(text)).ToString();
    }
}