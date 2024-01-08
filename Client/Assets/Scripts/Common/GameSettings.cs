 using System;
using UnityEngine;

public class GameSettings
{
    public string clientVersion="1.0.0";//给当前客户端版本号赋初值
    
    //存储数据
    public void SaveSettings()
    {
        //设置PlayerPrefs中的clientVersion版本号
        PlayerPrefs.SetString("clientVersion", clientVersion);
    }
    //检索数据
    public string LoadSettings()
    {
        string versionNumber= PlayerPrefs.GetString("clientVersion");
        return versionNumber;
    }

}


