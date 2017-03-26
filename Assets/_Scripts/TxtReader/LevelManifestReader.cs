using UnityEngine;
using System.Collections;
using System;

public class LevelManifestReader : TxtReader<LevelManifestData> {
    public ElementColor this[int row,int column]
    {
        get
        {
            if (row < 0 || row >= Contents.Length ||
                column < 0 || column >= Contents[row].colorsInOneRow.Length
                )
            {
                return ElementColor.None;
            }
            return Contents[row].colorsInOneRow[column];
        }
    }
    public override void ParseCell(string[] columnContent, ref LevelManifestData t)
    {
        //first column is ID,so the length have to subtract 1
        ElementColor[] cArr = new ElementColor[columnContent.Length - 1];
        int indexer = 0;
        t.id = int.Parse(columnContent[indexer]);
        for (int i = 1; i < columnContent.Length; i++)
        {
            cArr[i-1] = (ElementColor)int.Parse(columnContent[++indexer]);
        }
        t.colorsInOneRow = cArr;
    }
    private string path;
    public void SetPath(string path)
    {
        this.path = path;
    }
    public override string GetPath()
    {
        return path;
    }


}

public class LevelManifestData:ObjectData
{
    
    public ElementColor[] colorsInOneRow;
}
