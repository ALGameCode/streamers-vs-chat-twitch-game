using UnityEngine;

public static class Save
{

    public static void SaveConnectPlayerInfo(string username, string channelname)
    {
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("channelname", channelname);
        PlayerPrefs.Save();
    }

    public static void SaveOAuthPassword(string pass)
    {
        PlayerPrefs.SetString("password", pass);
        PlayerPrefs.Save();
    }

    public static void DeleteOAuthPassword()
    {
        PlayerPrefs.DeleteKey("password");
    }

    public static void DeleteAlSave()
    { 
        PlayerPrefs.DeleteAll(); 
    }
}
