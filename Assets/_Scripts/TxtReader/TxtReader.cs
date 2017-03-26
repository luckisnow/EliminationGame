using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public abstract  class TxtReader<T> : MonoBehaviour, IInitializer where T:ObjectData,new()
{

    public abstract string GetPath();


    private static string[] escapeStr = {"\r\n","\r", " ", "\\", "//", "@" };
    private static char[] splitChar = { ' ' ,'\t','\r'};

    private  T[] contents;
    public bool initializeFinish;
    public  T[] Contents
    {
        get { return contents; }
        protected set { contents = value; }
    }

    public bool InitializeFinish
    {
        get
        {
            return initializeFinish;
        }

         set
        {
            initializeFinish = value;
        }
    }

    /// <summary>
    /// All the text file Must in StreamingAssets File!
    /// </summary>
    /// <param name="path"></param>
    private T[] ParseTextFile(string[] lines)
    {
            List<T> l = new List<T>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim())) continue;
                bool jump = false;
                for (int j = 0; j < escapeStr.Length; j++)
                {
                    if (lines[i].StartsWith(escapeStr[j]))
                    {
                        jump = true;
                        break;
                    }
                }
                if (jump) continue;
                var cells = lines[i].Split(splitChar,StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length == 0) continue;
                int id;
                if (!int.TryParse(cells[0], out id)) continue;
                l.Add(ParseRow(cells));


            }
            return l.ToArray();
    }
    protected virtual void OnEnable()
    {
        InitializeFinish = false;
    }

    public void StartParse()
    {
        StartCoroutine(IEParse());
    }
    IEnumerator IEParse()
    {
        
        string tmpPath = "";
#if UNITY_ANDROID&&!UNITY_EDITOR
        tmpPath = Application.streamingAssetsPath + "/" + GetPath() + (GetPath().Contains(".txt")?"":".txt");
#else
        tmpPath = "file://" + Application.streamingAssetsPath + "/" + GetPath() + (GetPath().Contains(".txt")?"":".txt");
#endif
        WWW www = new WWW(tmpPath);
        yield return www;
        Contents = ParseTextFile(www.text.Split('\n'));
        yield return Contents;
        PostParseFile();
    }

    public virtual void PostParseFile()
    {
        InitializeFinish = true;
    }
    public  T ParseRow(string[] cellRow)
    {
        T t = new T();

        ParseCell( cellRow, ref t);
        
        return t;
    }

    public  bool Contain(T t)
    {
        for (int i = 0; i < Contents.Length; i++)
        {
            if (Contents[i]==t)
            {
                return true;
            }
        }
        return false;
    }
    public  bool Contain(int ID)
    {
        for (int i = 0; i < Contents.Length; i++)
        {
            if (Contents[i].id == ID)
            {
                return true;
            }
        }
        return false;
    }

    public  T GetData(int ID)
    {
        for (int i = 0; i < Contents.Length; i++)
        {
            if (Contents[i].id == ID)
            {
                return contents[i];
            }
        }
        return null;
    }
    public abstract  void ParseCell( string[] columnContent, ref T t);

   
}

public  class ObjectData
{
    public int id;
}
public interface IInitializer
{
     bool InitializeFinish { get;  set; }
}