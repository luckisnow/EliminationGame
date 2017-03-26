using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using com.ootii.Messages;
using System;
using UnityEngine.Events;

public class UILevelBtn : MonoBehaviour {

    Button btn;
    int id;
    public void Init(int id)
    {
        this.id = id;
    }
	void OnEnable () {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        MessageDispatcher.SendMessage(this, MsgTypes.UI_OnLevelBtnClick, id, 0);
    }

    void OnDisable () {
        btn.onClick.RemoveListener(OnClick);
    }
}
