using System;
using UnityEngine;

public class UICharacterPanel : MonoBehaviour
{
    [Serializable]
    public struct EffectCardInfo
    {
        public string CardName;
        public GameObject EffectCard;
    }

    [SerializeField]
    private EffectCardInfo[] _effectCardInfos;

    private void Awake()
    {
        LoadData();
    }

    private void LoadData()
    {
        ChosenEff(SaveManager.CharacterNameSelect);
    }

    public void Card_button_on_click(string cardName)
    {
        ChosenEff(cardName);
        SaveManager.CharacterNameSelect = cardName;
        EventDispatcher.Instance.PostEvent(EventID.OnSelectCharacter);
    }

    private void ChosenEff(string name)
    {
        name = name.ToLower();
        foreach (EffectCardInfo info in _effectCardInfos)
        {
            info.EffectCard.SetActive(info.CardName.ToLower() == name);
        }
    }
}