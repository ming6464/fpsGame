using TMPro;
using UnityEngine;

public class StageItemScript : MonoBehaviour
{
    public TMP_InputField NumberZombieV1;
    public TMP_InputField NumberZombieV2;
    public TMP_InputField NumberZombieV3;
    public TextMeshProUGUI STT;

    public StageGame GetStageData()
    {
        StageInfo[] StageInfos = new StageInfo[3];
        StageInfos[0].ZombieName = "ZombieV1";
        StageInfos[0].Count = int.Parse(NumberZombieV1.text);
        StageInfos[1].ZombieName = "ZombieV2";
        StageInfos[1].Count = int.Parse(NumberZombieV2.text);
        StageInfos[2].ZombieName = "ZombieV3";
        StageInfos[2].Count = int.Parse(NumberZombieV3.text);
        return new StageGame { StageInfos = StageInfos };
    }

    public void LoadData(StageGame stageGame, int stt)
    {
        NumberZombieV1.text = "0";
        NumberZombieV2.text = "0";
        NumberZombieV3.text = "0";
        STT.text = stt.ToString();
        if (stageGame == null)
        {
            return;
        }

        foreach (StageInfo info in stageGame.StageInfos)
        {
            switch (info.ZombieName.ToLower())
            {
                case "zombiev1":
                    NumberZombieV1.text = info.Count.ToString();
                    break;
                case "zombiev2":
                    NumberZombieV2.text = info.Count.ToString();
                    break;
                case "zombiev3":
                    NumberZombieV3.text = info.Count.ToString();
                    break;
            }
        }
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }

    public void OnChangeTextNumberZombieV1(string text)
    {
        if (!NumberZombieV1)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            NumberZombieV1.text = "0";
            return;
        }

        NumberZombieV1.text = Mathf.Abs(int.Parse(text)).ToString();
    }

    public void OnChangeTextNumberZombieV2(string text)
    {
        if (!NumberZombieV2)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            NumberZombieV2.text = "0";
            return;
        }

        NumberZombieV2.text = Mathf.Abs(int.Parse(text)).ToString();
    }

    public void OnChangeTextNumberZombieV3(string text)
    {
        if (!NumberZombieV3)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            NumberZombieV3.text = "0";
            return;
        }

        NumberZombieV3.text = Mathf.Abs(int.Parse(text)).ToString();
    }
}