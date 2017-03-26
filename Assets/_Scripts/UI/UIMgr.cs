using UnityEngine;
using System.Collections;
using com.ootii.Messages;
using System;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour {


    public GameObject uiPanel;
    public GameObject levelBtnPrefab;
	void OnEnable()
    {
        MessageDispatcher.AddListener(MsgTypes.ConfigurationLoadFinish, OnConfiFinishHandler);
    }

    void OnDisable()
    {
        MessageDispatcher.RemoveListener(MsgTypes.ConfigurationLoadFinish, OnConfiFinishHandler);
    }

    private void OnConfiFinishHandler(IMessage rMessage)
    {
        if (uiPanel==null||levelBtnPrefab==null)
        {
            return;
        }
        var btnNum = (int)rMessage.Data;
        for (int i = 0; i < btnNum; i++)
        {
            GenerateLevelBtn(i);
        }
    }

    private void GenerateLevelBtn(int i)
    {
        if (levelBtnPrefab != null)
        {
            var newBtn = GameObject.Instantiate<GameObject>(levelBtnPrefab);
            newBtn.transform.SetParent(uiPanel.transform);
            var script = newBtn.AddComponent<UILevelBtn>();
            script.Init(i);
            var text = newBtn.GetComponentInChildren<Text>();
            if (text) text.text = string.Format("Level{0}", i);
        }
    }
}
