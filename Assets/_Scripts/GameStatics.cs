using UnityEngine;
using System.Collections;
using System.IO;
public static class GameStatics
{
    public static string LevelConfigurationPath = "Configuration/Level";


    public static float elementDestroyTime = 1f;

    public static float elementMoveTime = 1f;
    public static bool HaveLevelManifest(string manifestName)
    {
        string tmpPath;
#if UNITY_ANDROID
        tmpPath = Application.streamingAssetsPath + "/Configuration/" + manifestName + ".txt";
#else
        tmpPath = Application.streamingAssetsPath + "/Configuration/" + manifestName + ".txt";
#endif
        var b = File.Exists(tmpPath);
        return b;
    }

    public static string GetLevelManifestFullPath(string fileName)
    {
        return "/Configuration/" + fileName + ".txt";
         
    }

    public static ElementColor GetRandomColor()
    {
        return (ElementColor)UnityEngine.Random.Range((int)1, (int)7);
    }
}
public enum LevelMoveDirection
{
    None=0,
    FromUpToDown=1,
    FromLeftToRight=2,
    SPath=3,
    Spiral=4,
}

/// <summary>
/// 1.红 2橙 3.黄 4.绿 5 蓝 6 紫 0随机
/// </summary>
public enum ElementColor
{
    None = 0,
    Red = 1,
    Orange = 2,
    Yellow = 3,
    Green = 4,
    Blue=5,
    Purple=6,
    Random=7,
}


public static class MsgTypes
{
    
    //business
    public static string ConfigurationLoadFinish = "ConfigurationLoadFinish";
    public static string LevelChangeFinish = "LevelChangeFinish";

    //ui
    public static string UI_OnLevelBtnClick = "UI_OnLevelBtnClick";
    public static string UI_OnElementClick = "UI_OnElementClick";
}