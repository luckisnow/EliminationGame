using System;
using UnityEngine;
[Serializable]
public class CellData
{
    public int row;
    public int column;

    public CellData preCell, nextCell;

    public ElementColor? elementColor;

    public UICell uiCell;

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        var t = obj as CellData;
        if (t == null)
            return false;   
        if (this.row == t.row && this.column == t.column)
            return true;
        return false;
    }
    public override int GetHashCode()
    {
        return row.GetHashCode()&column.GetHashCode();
    }

    internal void SetNewData(CellData cellData)
    {
        if (cellData == this)
            return;
        if (cellData == null)
            return;
        var tmp = elementColor;
        elementColor = cellData.elementColor;
        cellData .elementColor= tmp;
        if (uiCell) uiCell.SetNewData(cellData);
    }
}
