using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using com.ootii.Messages;
using DG.Tweening;

public class UICell : MonoBehaviour
{

    public int row = -1, column = -1;

    Transform element;
    public UnityEngine.Transform Element
    {
        get { return element; }
        private set { element = value; }
    }
    public void SetNewData(CellData newData)
    {
        element = newData.uiCell.element;
        element.SetParent(transform, true);
    }

    public void Init(CellData data)
    {
        row = data.row;
        column = data.column;

        Element = transform.childCount > 0 ? transform.GetChild(0) : null;

    }







    public Tween DestroyElement()
    {
        if (Element)
        {
            var rectTrans = Element.GetComponent<RectTransform>();
            if (rectTrans)
                return rectTrans.DOSizeDelta(Vector2.zero, GameStatics.elementDestroyTime).OnComplete(OmDestroyElementComplete);
        }
        return null;
    }

    private void OmDestroyElementComplete()
    {
        if (Element != null)
        {
            Destroy(Element.gameObject);
            Element = null;
        }

    }
}
