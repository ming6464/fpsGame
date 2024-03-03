using UnityEngine;

public static class SaveManager
{
    public static string CharacterNameSelect
    {
        get => PlayerPrefs.GetString("CharacterNameSelect");
        set => PlayerPrefs.SetString("CharacterNameSelect", value);
    }
}