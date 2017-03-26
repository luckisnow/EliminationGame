using UnityEngine;
using System.Collections;

public class ManifestTest : TxtReader<LevelManifestData> {
    public void Start()
    {
        StartParse();
    }
    public override void ParseCell(string[] columnContent, ref LevelManifestData t)
    {
        //first column is ID,so the length have to subtract 1
        ElementColor[] cArr = new ElementColor[columnContent.Length - 1];
        int indexer = 0;
        t.id = int.Parse(columnContent[indexer]);
        for (int i = 1; i < columnContent.Length; i++)
        {
            cArr[i-1] = (ElementColor)int.Parse(columnContent[indexer++]);
        }
    }
    private string path;
    public void SetPath(string path)
    {
        this.path = path;
    }
    public override string GetPath()
    {
        return "Configuration/" + "Manifest1" + ".txt"; 
    }

    public override void PostParseFile()
    {
        print(Contents.Length);
        base.PostParseFile();
    }
}
