using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using com.ootii.Messages;

public class UIElement : MonoBehaviour {

    Button btn;
	// Use this for initialization
	void OnEnable () {
        btn = GetComponent<Button>();
        if (btn)
            btn.onClick.AddListener(OnClickHandler);
    }

    private void OnClickHandler()
    {
        var cell = transform.parent.GetComponent<UICell>();
        if (cell)
        {
#if DEBUGMODE
            print(string.Format("Click Element At Cell:Row{0},Column{1}.", cell.row, cell.column));
#endif
            MessageDispatcher.SendMessage(this, MsgTypes.UI_OnElementClick, new Vector2(cell.row, cell.column), 0f);
        }
    }

    private void OnDisable()
    {
        if (btn)
            btn.onClick.RemoveListener(OnClickHandler);
    }
}
