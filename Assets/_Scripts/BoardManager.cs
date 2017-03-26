using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using com.ootii.Messages;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    private BoardManager() { }
    private static BoardManager instance;
    public static BoardManager Instance
    {
        get { return instance; }
        private set { instance = value; }
    }
    private GameObject CellPrefab;
    private string cellPath = "Prefabs/Cell";

    public int column, row;


    private RectTransform board;
    private Vector2 boardPanelSize, eachCellSize;

    public GameObject redElement, orangeElement, yellowElement, blueElement, greenElement, purpleElement;


    private CellData[,] allElementData;
    /// <summary>
    /// First index is row,Second index is Column
    /// </summary>
    public CellData[,] AllElementData
    {
        get { return allElementData; }
        private set { allElementData = value; }
    }

    private List<CellData> lastClickElements;
    private ElementColor lastClickColor;

    private List<CellData> movingCells;
    private Sequence seq;
    private WaitForSeconds waitForDestroyElements, waitForMoveElements;
    private void Awake()
    {
        DOTween.defaultAutoPlay = AutoPlay.None;
        Instance = this;
        CellPrefab = Resources.Load<GameObject>(cellPath);
        lastClickElements = new List<CellData>();
        movingCells = new List<CellData>();
        seq = DOTween.Sequence();
        waitForDestroyElements = new WaitForSeconds(GameStatics.elementDestroyTime);
        waitForMoveElements = new WaitForSeconds(GameStatics.elementMoveTime);
        if (CellPrefab == null)
            Debug.LogErrorFormat("Can't find Cell Prefab!At the folder: Resources/{0}!", cellPath);

    }

    private void OnEnable()
    {
        MessageDispatcher.AddListener(MsgTypes.UI_OnElementClick, OnElementClickHandler);
    }
    private void OnDisable()
    {
        MessageDispatcher.RemoveListener(MsgTypes.UI_OnElementClick, OnElementClickHandler);
    }
    private void OnElementClickHandler(IMessage rMessage)
    {
        GameManager.Instance.UIAvailable = false;
        int clickedRow = (int)((Vector2)rMessage.Data).x;
        int clickedColumn = (int)((Vector2)rMessage.Data).y;
        if (clickedRow >= 0 && clickedRow < AllElementData.GetLength(0) &&
            clickedColumn >= 0 && clickedColumn < AllElementData.GetLength(1)
            )
        {
            lastClickElements.Clear();
            lastClickColor = AllElementData[clickedRow, clickedColumn].elementColor.Value;
            movingCells.Clear();
            GetLastClickElements(clickedRow, clickedColumn);
#if DEBUGMODE
            for (int i = 0; i < lastClickElements.Count; i++)
            {
                print(string.Format("LastClickElements contains: row{0},column{1}", lastClickElements[i].row, lastClickElements[i].column));
            }
#endif
            if (lastClickElements.Count > 1)
            {
                StartCoroutine(IEProcessElements());

            }
            else
            {
                GameManager.Instance.UIAvailable = true;
            }
        }
    }
    private IEnumerator IEProcessElements()
    {
        yield return StartCoroutine(IEDestroyClickedElements());
        yield return StartCoroutine(IEMovingElements());
        yield return StartCoroutine(CreateNewElement());
        GameManager.Instance.UIAvailable = true;
    }
    private IEnumerator IEDestroyClickedElements()
    {
        if (lastClickElements != null && lastClickElements.Count > 0)
        {
            for (int i = 0; i < lastClickElements.Count; i++)
            {
                seq.Join(lastClickElements[i].uiCell.DestroyElement());
                lastClickElements[i].elementColor = null;
            }
            OnDestroyClickElementsComplete();
            yield return waitForDestroyElements;

        }
    }

    private IEnumerator IEMovingElements()
    {
        while (movingCells != null && movingCells.Count > 0)
        {
            List<CellData> animationCells = new List<CellData>();
            for (int i = movingCells.Count - 1; i >= 0; i--)
            {
                if (NeedMove(movingCells[i].row, movingCells[i].column))
                {
                    movingCells[i].nextCell.SetNewData(movingCells[i]);
                    animationCells.Add(movingCells[i].nextCell);
                }
            }
            if (animationCells.Count > 0)
            {
                for (int j = 0; j < animationCells.Count; j++)
                {
                    seq.Join(animationCells[j].uiCell.Element.DOLocalMove(Vector3.zero, GameStatics.elementMoveTime));
                }
            }
            AdjustMovingCells();
            yield return waitForMoveElements;
        }
        yield return null;

    }
    private void AdjustMovingCells()
    {
        if (movingCells != null && movingCells.Count > 0)
        {
            for (int i = movingCells.Count - 1; i >= 0; i--)
            {
                if (movingCells[i].nextCell != null)
                {
                    movingCells[i] = movingCells[i].nextCell;
                    if (!NeedMove(movingCells[i].row, movingCells[i].column))
                        movingCells.RemoveAt(i);
                }
                else
                {
                    movingCells.RemoveAt(i);
                }
            }
            movingCells.Sort(MovingCellsSortor);
        }
    }

    private bool NeedMove(int row, int column)
    {
        var level = GameManager.Instance.GetCurrentLevelData();
        if (level == null) return false;
        switch (level.direction)
        {
            case LevelMoveDirection.None:
                return false;
            case LevelMoveDirection.FromUpToDown:
                bool result = false;
                for (int i = row; i < level.rowCount; i++)
                {
                    if (AllElementData[i, column].elementColor == null)
                        return true;
                }
                return result;
            case LevelMoveDirection.FromLeftToRight:
                return false;
            case LevelMoveDirection.SPath:
                return false;
            case LevelMoveDirection.Spiral:
                return false;
            default:
                return false;
        }
    }
    private void OnDestroyClickElementsComplete()
    {
        if (lastClickElements != null && lastClickElements.Count > 0)
        {
            for (int i = 0; i < lastClickElements.Count; i++)
            {
                GetPreviousElements(lastClickElements[i]);
            }
            movingCells.Sort(MovingCellsSortor);
#if DEBUGMODE
            for (int i = 0; i < movingCells.Count; i++)
            {
                print(string.Format("MovingCells contains: row{0},column{1}", movingCells[i].row, movingCells[i].column));
            }
#endif
        }
    }

    private int MovingCellsSortor(CellData x, CellData y)
    {
        var levelData = GameManager.Instance.GetCurrentLevelData();
        if (levelData == null)
            return 0;
        switch (levelData.direction)
        {
            case LevelMoveDirection.None:
                return 0;
            case LevelMoveDirection.FromUpToDown:
                if (x.column != y.column)
                    return x.column.CompareTo(y.column);
                else
                {
                    return x.row.CompareTo(y.row);
                }
            case LevelMoveDirection.FromLeftToRight:
                if (x.row != y.row)
                    return x.row.CompareTo(y.row);
                else
                {
                    return x.column.CompareTo(y.column);
                }
            case LevelMoveDirection.SPath:
                return 0;
            case LevelMoveDirection.Spiral:
                return 0;
            default:
                return 0;
        }
    }
    private IEnumerator CreateNewElement()
    {
        List<CellData> needCreateCells = new List<CellData>();
        for (int i = 0; i < AllElementData.GetLength(0); i++)
        {
            for (int j = 0; j < AllElementData.GetLength(1); j++)
            {
                if (AllElementData[i, j].elementColor == null)
                    needCreateCells.Add(AllElementData[i, j]);
            }
        }
        for (int i = 0; i < needCreateCells.Count; i++)
        {
            CreateNewElement(needCreateCells[i].row, needCreateCells[i].column, GameStatics.GetRandomColor());
        }
        SetAllElementsRelationData();
        yield return null;
    }
    private void GetPreviousElements(CellData cellData)
    {
        if (cellData.preCell != null && cellData.preCell.elementColor != null &&
            !movingCells.Contains(cellData.preCell)
            )
        {
            movingCells.Add(cellData.preCell);
            GetPreviousElements(cellData.preCell);
        }

    }

    private void GetLastClickElements(int clickedRow, int clickedColumn)
    {
        if (!lastClickElements.Contains(AllElementData[clickedRow, clickedColumn]) &&
             AllElementData[clickedRow, clickedColumn].elementColor != null &&
            AllElementData[clickedRow, clickedColumn].elementColor.Value == lastClickColor
            )
        {
            lastClickElements.Add(AllElementData[clickedRow, clickedColumn]);
        }
        else
        {
            return;
        }
        if (clickedColumn > 0)
        {
            GetLastClickElements(clickedRow, clickedColumn - 1);
        }
        if (clickedColumn < AllElementData.GetLength(1) - 1)
        {
            GetLastClickElements(clickedRow, clickedColumn + 1);
        }

        if (clickedRow > 0)
        {
            GetLastClickElements(clickedRow - 1, clickedColumn);
        }
        if (clickedRow < AllElementData.GetLength(0) - 1)
        {
            GetLastClickElements(clickedRow + 1, clickedColumn);
        }
    }

    public void GenerateBoard(int rowCount, int columnCount)
    {
        row = rowCount;
        column = columnCount;
        board = GetComponent<RectTransform>();
        boardPanelSize = board.sizeDelta;
        eachCellSize = new Vector2(boardPanelSize.x / (float)column, boardPanelSize.y / (float)row);
        StartCoroutine(IECreateNewBoard());

    }

    private IEnumerator IECreateNewBoard()
    {
        //Destroy Old
        while (board.childCount > 0)
        {
            Destroy(board.GetChild(0).gameObject);
            yield return null;
        }
        AllElementData = new CellData[row, column];

        //examine
        if (CellPrefab == null || row <= 0 || column <= 0)
        {
            yield return null;
        }

        //create
        else
        {
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    yield return GenerateOneCell(r, c);
                }
            }
        }

        SetAllElementsRelationData();

        MessageDispatcher.SendMessage(MsgTypes.LevelChangeFinish);
    }

    private void SetAllElementsRelationData()
    {
        var levelData = GameManager.Instance.GetCurrentLevelData();
        if (levelData != null)
        {
            switch (levelData.direction)
            {
                case LevelMoveDirection.None:
                    break;
                case LevelMoveDirection.FromUpToDown:
                    for (int r = 0; r < AllElementData.GetLength(0); r++)
                    {
                        for (int c = 0; c < allElementData.GetLength(1); c++)
                        {
                            if (r == 0) AllElementData[r, c].preCell = null;
                            else AllElementData[r, c].preCell = AllElementData[r - 1, c];
                            if (r == AllElementData.GetLength(0) - 1) AllElementData[r, c].nextCell = null;
                            else AllElementData[r, c].nextCell = AllElementData[r + 1, c];
                        }
                    }
                    break;
                case LevelMoveDirection.FromLeftToRight:
                    for (int r = 0; r < AllElementData.GetLength(0); r++)
                    {
                        for (int c = 0; c < allElementData.GetLength(1); c++)
                        {
                            if (c == 0) AllElementData[r, c].preCell = null;
                            else AllElementData[r, c].preCell = AllElementData[r, c - 1];
                            if (c == AllElementData.GetLength(1) - 1) AllElementData[r, c].nextCell = null;
                            else AllElementData[r, c].nextCell = AllElementData[r, c + 1];
                        }
                    }
                    break;
                case LevelMoveDirection.SPath:
                    break;
                case LevelMoveDirection.Spiral:
                    break;
                default:
                    break;
            }
        }
    }

    private void SetElementRelationData(int r, int c)
    {
        var levelData = GameManager.Instance.GetCurrentLevelData();
        if (levelData != null)
        {
            switch (levelData.direction)
            {
                case LevelMoveDirection.None:
                    break;
                case LevelMoveDirection.FromUpToDown:
                    if (r == 0) AllElementData[r, c].preCell = null;
                    else AllElementData[r, c].preCell = AllElementData[r - 1, c];
                    if (r == AllElementData.GetLength(0) - 1) AllElementData[r, c].nextCell = null;
                    else AllElementData[r, c].nextCell = AllElementData[r + 1, c];
                    break;
                case LevelMoveDirection.FromLeftToRight:
                    if (c == 0) AllElementData[r, c].preCell = null;
                    else AllElementData[r, c].preCell = AllElementData[r, c - 1];
                    if (c == AllElementData.GetLength(1) - 1) AllElementData[r, c].nextCell = null;
                    else AllElementData[r, c].nextCell = AllElementData[r, c + 1];
                    break;
                case LevelMoveDirection.SPath:
                    break;
                case LevelMoveDirection.Spiral:
                    break;
                default:
                    break;
            }
        }
    }

    private GameObject GenerateOneCell(int r, int c)
    {
        var newCell = GameObject.Instantiate(CellPrefab);
        newCell.name = string.Format("Cell_r{0}_c{1}", r, c);
        var newRectTransform = newCell.GetComponent<RectTransform>();
        newRectTransform.SetParent(board.transform, false);
        //our board coordinate is upper left  (0,0), bottom right (max,max)
        var XPos = -boardPanelSize.x / 2 + eachCellSize.x / 2 + c * eachCellSize.x;
        var YPos = boardPanelSize.y / 2 - eachCellSize.y / 2 - r * eachCellSize.y;
        newRectTransform.localPosition = new Vector3(XPos, YPos);
        newRectTransform.sizeDelta = eachCellSize;

        var lvlData = GameManager.Instance.GetCurrentLevelData();
        var elementCol = lvlData.levelManifestData[c, r];
        if (elementCol != ElementColor.None)
        {
            if (elementCol == ElementColor.Random)
                elementCol = GameStatics.GetRandomColor();

            CreateNewElement(r, c, elementCol, newCell.transform);
        }
        return newCell;
    }
    private GameObject CreateNewElement(int r, int c, ElementColor color, Transform parent = null)
    {
        if (r >= 0 && r < AllElementData.GetLength(0) &&
            c >= 0 && c < AllElementData.GetLength(1) &&
            color != ElementColor.None
            )
        {
            var newElement = CreateElement(color);

            if (parent != null)
            {
                newElement.transform.SetParent(parent, true);

            }
            else if (AllElementData[r, c] == null ||
                AllElementData[r, c].uiCell == null ||
                AllElementData[r, c].uiCell.transform == null)
            {
                newElement.transform.SetParent(parent, true);
            }
            else
            {
                newElement.transform.SetParent(AllElementData[r, c].uiCell.transform, true);
            }
            newElement.transform.localPosition = Vector3.zero;
            var elementScript = newElement.GetComponent<UIElement>();
            if (elementScript == null)
                elementScript = newElement.gameObject.AddComponent<UIElement>();
            UICell script = null;
            if (AllElementData[r, c] != null && AllElementData[r, c].uiCell != null)
            {
                script = AllElementData[r, c].uiCell.GetComponent<UICell>();
                if (script == null)
                {
                    script = AllElementData[r, c].uiCell.gameObject.AddComponent<UICell>();
                }
            }
            else
            {
                script = parent.GetComponent<UICell>();
                if (script == null)
                {
                    script = parent.gameObject.AddComponent<UICell>();
                }
            }
            CellData data = new CellData() { row = r, column = c, elementColor = color, uiCell = script };
            AllElementData[r, c] = data;
            script.Init(data);
            return newElement;
        }
        return null;
    }

    public GameObject CreateElement(ElementColor color)
    {
        switch (color)
        {
            case ElementColor.None:
                return null;

            case ElementColor.Red:
                if (redElement)
                    return GameObject.Instantiate(redElement);
                break;
            case ElementColor.Orange:
                if (orangeElement)
                    return GameObject.Instantiate(orangeElement);
                break;
            case ElementColor.Yellow:
                if (yellowElement)
                    return GameObject.Instantiate(yellowElement);
                break;
            case ElementColor.Green:
                if (greenElement)
                    return GameObject.Instantiate(greenElement);
                break;
            case ElementColor.Blue:
                if (blueElement)
                    return GameObject.Instantiate(blueElement);
                break;
            case ElementColor.Purple:
                if (purpleElement)
                    return GameObject.Instantiate(purpleElement);
                break;
        }
        return null;
    }
}
