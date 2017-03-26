using UnityEngine;
using System.Collections;
using com.ootii.Messages;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    public GraphicRaycaster[] alterableUIs;
    private bool uiAvailable;
    public bool UIAvailable
    {
        get { return uiAvailable; }
        set
        {
            uiAvailable = value;
            if (alterableUIs!=null)
            {
                for (int i = 0; i < alterableUIs.Length; i++)
                {
                    alterableUIs[i].enabled = uiAvailable;
                }
            }
        }
    }
    private GameManager() { }

    public static GameManager Instance
    { get; private set; }
    private int currentLevelID;
    private void Awake()
    {
        Instance = this;
        currentLevelID = -1;
        UIAvailable = false;
    }
    private void OnEnable()
    {
        MessageDispatcher.AddListener(MsgTypes.UI_OnLevelBtnClick, OnLevelChangeHandler);
        MessageDispatcher.AddListener(MsgTypes.LevelChangeFinish, OnLevelChangeFinishHandler);
    }


    private void OnDisable()
    {
        MessageDispatcher.RemoveListener(MsgTypes.UI_OnLevelBtnClick, OnLevelChangeHandler);
        MessageDispatcher.RemoveListener(MsgTypes.LevelChangeFinish, OnLevelChangeFinishHandler);
    }

    private void OnLevelChangeFinishHandler(IMessage rMessage)
    {
        UIAvailable = true;
    }

    IEnumerator Start ()
    {
        while (LevelReader.Instance==null||!LevelReader.Instance.InitializeFinish)
        {
            yield return null;
        }
        UIAvailable = true;
        MessageDispatcher.SendMessage(this, MsgTypes.ConfigurationLoadFinish,LevelReader.Instance.Contents.Length,0);
	}
    private void OnLevelChangeHandler(IMessage rMessage)
    {
        
        var newLevelID = (int)rMessage.Data;
        if (newLevelID<0||newLevelID>=LevelReader.Instance.Contents.Length)
        {
            return;
        }
      
        if (currentLevelID!=newLevelID)
        {
            UIAvailable = false;
            currentLevelID = newLevelID;
            DoLevelChange(newLevelID);
        }
    }

    private void DoLevelChange(int newLevelID)
    {
        if (newLevelID < 0 || newLevelID >= LevelReader.Instance.Contents.Length)
        {
            return;
        }
        if (BoardManager.Instance!=null)
        {
            BoardManager.Instance.GenerateBoard(LevelReader.Instance.Contents[newLevelID].rowCount,
                                                  LevelReader.Instance.Contents[newLevelID].columnCount);
        }
    }


    public LevelData GetCurrentLevelData()
    {
        if (currentLevelID < 0 || currentLevelID >= LevelReader.Instance.Contents.Length)
        {
            return null;
        }
        return LevelReader.Instance.Contents[currentLevelID];
    }
}
