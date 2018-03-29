using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleUI : MonoBehaviour
{
    private HexUnit _selectedUnit;
    private HexCell _currentCell;
    private Vector3 _lastPos = Vector3.zero;
    private enum Modes { Default, Move, Attack };
    private Modes _mode = Modes.Default;

    public HexGrid grid;

    void Awake()
    {

    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(_mode == Modes.Default)
                DoSelection();
            else if(_selectedUnit && _mode == Modes.Move)
            {
                DoMove();
            } 
        }
        else if (_selectedUnit && _mode == Modes.Move)
        {
            DoPathfinding();
        }

        var pos = Input.mousePosition;
        if (_lastPos != pos)
        {
            DoHover();
        }
        
        _lastPos = pos;
    }

    bool UpdateCurrentCell()
    {
        var cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (cell != _currentCell)
        {
            _currentCell = cell;
            return true;
        }
        return false;
    }

    void DoHover()
    {
        UpdateCurrentCell();
        if (_currentCell)
        {
            ShowTerrainType();
            grid.HighlightCell(_currentCell);
        }
    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (_currentCell)
        {
            if (!_currentCell.Unit && _selectedUnit)
            {
                grid.ClearMoves(_selectedUnit);
                ShowActions(false);
                _mode = Modes.Default;
            }
                
            _selectedUnit = _currentCell.Unit;
            grid.SelectCell(_currentCell);

            if (_selectedUnit)
                ShowActions(true);
        }
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (_currentCell && _selectedUnit.IsValidDestination(_currentCell))
            {
                grid.FindPath(_selectedUnit.Location, _currentCell, _selectedUnit.Speed);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    void DoMove()
    {
        if (grid.HasPath)
        {
            _selectedUnit.Travel(grid.GetPath());
            grid.ClearPath();
            grid.SelectCell(_currentCell);
            grid.ShowAllAvailableMoves(_currentCell, _selectedUnit.Speed, _selectedUnit.AttackRange);
        }
    }

    void ShowTerrainType()
    {
        if(_currentCell)
            Debug.Log("Terrain type: " + _currentCell.TerrainType);
    }

    private void ShowActions(Boolean enable)
    {
        var ui = transform.Find("Actions");
        ui.gameObject.SetActive(enable);
    }

    public void ShowAvailableMoves()
    {
        grid.ShowAllAvailableMoves(grid.SelectedCell, _selectedUnit.Speed, _selectedUnit.AttackRange);
        _mode = Modes.Move;
    }
}
