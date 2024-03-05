using UnityEngine;

public static class SaveManager
{
    public static string CharacterNameSelect
    {
        get => PlayerPrefs.GetString("CharacterNameSelect", "swat");
        set => PlayerPrefs.SetString("CharacterNameSelect", value);
    }

    public static bool AudioSound
    {
        get => PlayerPrefs.GetInt("AudioSound", 1) == 1;
        set => PlayerPrefs.SetInt("AudioSound", value ? 1 : 0);
    }

    public static bool FirstOpen
    {
        get => PlayerPrefs.GetInt("FirstOpen", 1) == 1;
        set => PlayerPrefs.SetInt("FirstOpen", value ? 1 : 0);
    }

    public static string DataGame
    {
        get => PlayerPrefs.GetString("DataGame", null);
        set => PlayerPrefs.SetString("DataGame", value);
    }
}