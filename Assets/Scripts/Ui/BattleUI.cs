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
    private HexDirection _direction;

    public HexGrid grid;

    void Awake()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(_selectedUnit && _mode == Modes.Move)
                DoMove();

            if (_mode == Modes.Attack && _currentCell.HasEnemyUnit(_selectedUnit.Team))
                DoAttack();
            else
                DoSelection();
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

            if (_selectedUnit)
            {
                _direction = grid.SelectedCell.Coordinates.GetRelatedDirection(_currentCell.Coordinates);
                _selectedUnit.Rotate(_direction);
            }
               
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
                grid.ClearAttacks(_selectedUnit);
                ShowActions(false);
                _selectedUnit.ResetAnimation();
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
            grid.ClearAttacks(_selectedUnit);
        }
    }

    void DoAttack()
    {
        _selectedUnit.Attack(_currentCell.Unit, _direction);
    }

    void ShowTerrainType()
    {
        //if(_currentCell)
            //Debug.Log("Terrain type: " + _currentCell.TerrainType);
    }

    private void ShowActions(Boolean enable)
    {
        var ui = transform.Find("Actions");
        ui.gameObject.SetActive(enable);

        if(_selectedUnit.AvailabeAttacks == null)
            _selectedUnit.AvailabeAttacks = grid.GetAvailableAttacks(grid.SelectedCell, _selectedUnit.AttackRange);

        ui.Find("Attack").gameObject.SetActive(_selectedUnit.AvailabeAttacks.Count > 0);
    }

    public void ShowAvailableAttacks()
    {
        if(_selectedUnit.AvailabeAttacks != null && _selectedUnit.AvailabeAttacks.Count > 0)
        {
            grid.ShowAllAvailableAttacks(_selectedUnit);
            _mode = Modes.Attack;
        }
    }

    public void ShowAvailableMoves()
    {
        grid.ShowAllAvailableMoves(grid.SelectedCell, _selectedUnit.Speed, _selectedUnit.AttackRange);
        _mode = Modes.Move;
    }
}
