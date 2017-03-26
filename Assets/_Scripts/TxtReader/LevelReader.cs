using UnityEngine;
using System.Collections;
using System;

public class LevelReader : TxtReader<LevelData>
{

    private LevelReader() { }
    private static readonly object locker = new object();
    private static LevelReader instance;
    public static LevelReader Instance
    {
       get { return instance; }
       private set { instance = value; }
    }
    void Awake()
    {
        Instance = this;    
    }
    private void Start()
    {
        StartParse();
    }

    public override void ParseCell(string[] columnContent, ref LevelData t)
    {
        int indexer = 0;
        t.id = int.Parse(columnContent[indexer]);
        ++indexer;
        t.rowCount = int.Parse(columnContent[indexer]);
        ++indexer;
        t.columnCount = int.Parse(columnContent[indexer]);
        ++indexer;
        t.direction = (LevelMoveDirection)int.Parse(columnContent[indexer]);
        ++indexer;
        t.manifestName = columnContent[indexer];
    }
    public override string GetPath()
    {
       return GameStatics.LevelConfigurationPath;
    }

    public override void PostParseFile()
    {
        StartCoroutine(IEParseEachManifest());
    }
    IEnumerator IEParseEachManifest()
    {
        for (int i = 0; i < Contents.Length; i++)
        {
            if (GameStatics.HaveLevelManifest(Contents[i].manifestName))
            {
                Contents[i].levelManifestData = ParseLevelManifest(Contents[i].manifestName);
            }
        }
        while (!IsIniliazeFinish())
        {
            yield return null;
        }
        initializeFinish = true;
    }

    private bool IsIniliazeFinish()
    {
        if (Contents==null)
        {
            return false;
        }
        for (int i = 0; i < Contents.Length; i++)
        {
            if (Contents[i].levelManifestData==null)
            {
                return false;
            }
            if (!Contents[i].levelManifestData.initializeFinish)
                return false;
        }
        return true;
    }
    private LevelManifestReader ParseLevelManifest(string manifestName)
    {
        var newLevelManifestReader = gameObject.AddComponent<LevelManifestReader>();
        newLevelManifestReader.SetPath(GameStatics.GetLevelManifestFullPath(manifestName));
        newLevelManifestReader.StartParse();
        return newLevelManifestReader;
    }
}

public class LevelData : ObjectData
{
    public int rowCount;
    public int columnCount;
    public LevelMoveDirection direction;
    public string manifestName;
    public LevelManifestReader levelManifestData;
}