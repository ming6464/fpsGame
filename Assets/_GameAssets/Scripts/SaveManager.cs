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
}