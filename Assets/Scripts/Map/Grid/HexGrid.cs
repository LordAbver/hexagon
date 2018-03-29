using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    private Canvas _gridCanvas;
    private HexMesh _hexMesh;
    private HexCell[] _cells;
    private HexCell _highLightedCell;
    private HexCell _selectedCell;
    private HexCellPriorityQueue _searchFrontier;
    private Int32 _searchFrontierPhase;
    private HexCell _currentPathFrom, _currentPathTo;
    private Boolean _currentPathExists;

    private HashSet<HexCoordinates> _lakes;
    private HashSet<HexCoordinates> _hills;
    private HashSet<HexCoordinates> _forest;
    private Dictionary<HexCoordinates, HashSet<HexDirection>> _roads;
    private Dictionary<HexCoordinates, String> _units;


    public Int16 Width = 6;
    public Int16 Height = 6;

    public HexCell CellPrefab;
    public Text CellLabelPrefab;
    public Color DefaultColor = Color.white;
    public Boolean HasPath
    {
        get
        {
            return _currentPathExists;
        }
    }

    public HexCell SelectedCell{
        get
        {
            return _selectedCell;
        }
    }

    void Awake()
    {
        _gridCanvas = GetComponentInChildren<Canvas>();
        _hexMesh = GetComponentInChildren<HexMesh>();

        LoadLakes();
        LoadHills();
        LoadForest();
        LoadRoads();
        LoadUnits();

        _cells = new HexCell[Height * Width];

        for (int z = 0, i = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void Start()
    {
        _hexMesh.Triangulate(_cells);
    }

    void Update()
    {

    }

    void CreateCell(Int32 x, Int32 z, Int32 i)
    {

        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.INNER_RADIUS * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.OUTER_RADIUS * 1.5f);

        var cell = _cells[i] = Instantiate(CellPrefab);
        var coords = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.Coordinates = coords;
        cell.Color = DefaultColor;

        if (_lakes.Contains(coords))
            cell.TerrainType = HexTerrainTypes.LAKE;
        else if (_hills.Contains(coords))
            cell.TerrainType = HexTerrainTypes.HILL;
        else if (_forest.Contains(coords))
            cell.TerrainType = HexTerrainTypes.FOREST;

        if (_units.ContainsKey(coords))
            AddUnit(cell, _units[coords]);

        cell.Roads = _roads.ContainsKey(coords) ? _roads[coords] : null;

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, _cells[i - Width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - Width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, _cells[i - Width]);
                if (x < Width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - Width + 1]);
                }
            }
        }

        var label = Instantiate(CellLabelPrefab);
        label.rectTransform.SetParent(_gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        cell.UiRect = label.rectTransform;

        label.GetComponent<TextMesh>().text = coords.ToStringOnSeparateLines();
        label.name = coords.ToString();
        cell.name = coords.ToString();
    }

    public void SelectCell(HexCell cell)
    {
        if (cell == null) return;

        if (_selectedCell != null && _selectedCell != cell)
            _selectedCell.SetVisualStyle(HexVisualStates.SELECTED, false);

        if (_selectedCell != cell)
            cell.SetVisualStyle(HexVisualStates.SELECTED, true);

        _selectedCell = cell;
    }

    public void HighlightCell(HexCell cell)
    {
        if (cell == null) return;

        if (_highLightedCell != null && _highLightedCell != cell)
            _highLightedCell.SetVisualStyle(HexVisualStates.HIGHLIGHTED, false);

        if (cell != _highLightedCell)
            cell.SetVisualStyle(HexVisualStates.HIGHLIGHTED, true);

        _highLightedCell = cell;
    }

    public void ShowAllAvailableMoves(HexCell fromCell, Int32 speed, Int32 attackRange)
    {
        var unit = fromCell.Unit;
        ClearMoves(unit);
        foreach(var cell in _cells)
        {
            var res = Search(fromCell, cell, speed, attackRange);
            if (res.CanReach)
            {
                cell.SetVisualStyle(HexVisualStates.PATH, true);
                unit.AvailableMoves.Add(cell, res);
            }
            if(!res.CanReach && res.CanAttack)
            {
                cell.SetVisualStyle(HexVisualStates.ATTACK, true);
                unit.AvailableMoves.Add(cell, res);
            }
            ClearPath();
        }
    }

    public void FindPath(HexCell fromCell, HexCell toCell, Int32 speed)
    {
        ClearPath();
        if (!fromCell.Unit.CanReach(toCell))
            return; // Route does not exist!

        _currentPathFrom = fromCell;
        _currentPathTo = toCell;
        _currentPathExists = Search(fromCell, toCell, speed);
        //ShowPath(speed);
    }

    private PathSearchResult Search(HexCell fromCell, HexCell toCell, Int32 speed, Int32 attackRange = 0)
    {
        if (!fromCell || !toCell) return new PathSearchResult();
        _searchFrontierPhase += 2;
        if (_searchFrontier == null)
        {
            _searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }
        fromCell.SearchPhase = _searchFrontierPhase;
        fromCell.Distance = 0;
        _searchFrontier.Enqueue(fromCell);

        var canReach = true;
        var canAttack = false;

        while (_searchFrontier.Count > 0)
        {
            var current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;
            Int32 currentTurn = (current.Distance - 1) / speed;
            var absoluteTurn = current.AbsoluteDistance - 1;

            if (currentTurn > 0)
            {
                canReach = false;
                canAttack = current.AttackDistance <= attackRange;
            }
                
            if (current == toCell)
                return new PathSearchResult(canReach, canAttack);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);

                if (neighbor == null || neighbor.SearchPhase > _searchFrontierPhase) continue;
                if (neighbor.TerrainType == HexTerrainTypes.LAKE || neighbor.Unit) continue;

                Int32 moveCost = 0;

                if (current.HasRoad(d))
                {
                    moveCost = 1;
                }
                else
                {
                    switch (neighbor.TerrainType)
                    {
                        case HexTerrainTypes.FLAT:
                            moveCost += 5;
                            break;
                        case HexTerrainTypes.HILL:
                            moveCost += 10;
                            break;
                        case HexTerrainTypes.FOREST:
                            moveCost += 7;
                            break;
                        case HexTerrainTypes.ROAD:
                            moveCost += 5;
                            break;
                        default:
                            moveCost += 10;
                            break;
                    }
                }

                Int32 distance = (current.Distance - 1) + moveCost;
                Int32 absoluteDistance = (current.AbsoluteDistance -1) + 1;
                Int32 attackDistance = 1;

                Int32 turn = distance / speed;
                var absoluteStep = absoluteDistance;

                if (absoluteStep > absoluteTurn)
                {
                    absoluteDistance = absoluteStep + 1;
                    if(!canReach)
                        attackDistance = current.AttackDistance + 1;
                }

                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }
                if (neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.AbsoluteDistance = absoluteDistance;
                    neighbor.AttackDistance = attackDistance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(toCell.Coordinates);
                    _searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    var oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.AttackDistance = attackDistance;
                    neighbor.AbsoluteDistance = absoluteDistance;
                    neighbor.PathFrom = current;
                    _searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
        return new PathSearchResult();
    }

    public List<HexCell> GetPath()
    {
        if (!_currentPathExists)
        {
            return null;
        }
        List<HexCell> path = ListPool<HexCell>.Get();
        for (HexCell c = _currentPathTo; c != _currentPathFrom; c = c.PathFrom)
        {
            path.Add(c);
        }
        path.Add(_currentPathFrom);
        path.Reverse();
        return path;
    }

    private void ShowPath(Int32 speed)
    {
        if (_currentPathExists)
        {
            var current = _currentPathTo;
            while (current != _currentPathFrom)
            {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel(turn.ToString());
                current.SetVisualStyle(HexVisualStates.PATH, true);
                current = current.PathFrom;
            }
        }
    }

    public void ClearMoves(HexUnit unit)
    {
        foreach (var cell in unit.AvailableMoves)
        {
            cell.Key.SetVisualStyle(HexVisualStates.PATH, false);
            cell.Key.SetVisualStyle(HexVisualStates.ATTACK, false);
        }

        unit.AvailableMoves.Clear();
    }

    public void ClearPath()
    {
        if (_currentPathExists)
        {
            HexCell current = _currentPathTo;
            while (current != _currentPathFrom)
            {
                current.SetLabel(null);
                //current.SetVisualStyle(HexVisualStates.PATH, false);
                current = current.PathFrom;
            }
            //current.SetVisualStyle(HexVisualStates.PATH, false);
            _currentPathExists = false;
        }
        _currentPathFrom = _currentPathTo = null;
    }

    private HexCell GetCellByPosition(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = HexCoordinates.FromPosition(position);
        var index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;

        return index < _cells.Length ? _cells[index] : null;
    }

    public HexCell GetCell(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return GetCellByPosition(hit.point);
        }
        return null;
    }

    public void AddUnit(HexCell cell, String unitName)
    {
        if (cell && !cell.Unit)
        {
            var prefab = Instantiate(Resources.Load(String.Format("Units\\{0}", unitName)), new Vector3(0, 0, 0), Quaternion.Euler(0, 180, 0)) as GameObject;
            var unit = prefab.GetComponent<HexUnit>();
            unit.transform.SetParent(gameObject.transform, false);
            unit.Location = cell;
        }
    }

    void DestroyUnit(HexCell cell)
    {
        if (cell && cell.Unit)
        {
            cell.Unit.Die();
        }
    }

    private void LoadLakes()
    {
        _lakes = new HashSet<HexCoordinates>() { new HexCoordinates(0, 5) };
    }

    private void LoadUnits()
    {
        _units = new Dictionary<HexCoordinates, String>() {
            { new HexCoordinates(5, 1), "Bishop" }
        };
    }

    private void LoadHills()
    {
        _hills = new HashSet<HexCoordinates>() {
            new HexCoordinates(5, 3),
            new HexCoordinates(5, 4),
            new HexCoordinates(6, 3),
            new HexCoordinates(6, 4)
        };
    }

    private void LoadForest()
    {
        _forest = new HashSet<HexCoordinates>() {
            new HexCoordinates(3, 7),
            new HexCoordinates(4, 7),
            new HexCoordinates(4, 6)
        };
    }

    private void LoadRoads()
    {
        _roads = new Dictionary<HexCoordinates, HashSet<HexDirection>>
        {
            { new HexCoordinates(2, 4), new HashSet<HexDirection>() { HexDirection.NE } },
            { new HexCoordinates(2, 5), new HashSet<HexDirection>() { HexDirection.NE, HexDirection.SW } },
            { new HexCoordinates(2, 6), new HashSet<HexDirection>() { HexDirection.NW, HexDirection.SW } },
            { new HexCoordinates(1, 7), new HashSet<HexDirection>() { HexDirection.SE, HexDirection.NE } },
            { new HexCoordinates(1, 8), new HashSet<HexDirection>() { HexDirection.SW, HexDirection.NW } },
            { new HexCoordinates(0, 9), new HashSet<HexDirection>() { HexDirection.SE, HexDirection.NE } }
        };
    }
}