using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleUI : MonoBehaviour
{
    private HexUnit _selectedUnit;
    private HexCell _currentCell;
    private Vector3 _lastPos = Vector3.zero;
    private enum Modes { Default, Move, Attack };
    private Modes _mode = Modes.Default;
    private HexDirection _direction;
    private Transform _unitActions;
    private Transform _terrainPreview;

    public HexGrid grid;

    void Awake()
    {
        _unitActions = transform.Find("Actions");
        _terrainPreview = transform.Find("TerrainPreview");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(_selectedUnit && _mode == Modes.Move)
                DoMove();

            if (_mode == Modes.Attack && !_selectedUnit.IsBusy && _currentCell.HasEnemyUnit(_selectedUnit.Team))
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

            if (_selectedUnit && _selectedUnit.ActPhase == ActPhase.WaitForCommand)
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

            if (_selectedUnit && _selectedUnit.ActPhase == ActPhase.WaitForCommand)
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
            grid.ClearMoves(_selectedUnit);
            grid.ClearAttacks(_selectedUnit);
            EnableOption("Move", false);
        }
    }

    void DoAttack()
    {
        _selectedUnit.IsBusy = true;
        _selectedUnit.Attack(_currentCell.Unit, _direction);
        ShowActions(false);
    }

    void ShowTerrainType()
    {
        if (_currentCell)
        {
            //Update text
            var text = _terrainPreview.Find("Name");
            text.gameObject.GetComponent<Text>().text = _currentCell.TerrainType.GetName();

            //Update img
            var img = _terrainPreview.Find("Preview");
            var sprites = _terrainPreview.GetComponent<TerrainPreview>().TerrainSprites;
            img.gameObject.GetComponent<Image>().sprite = sprites[(Int32)_currentCell.TerrainType];
        }
    }

    private void ShowActions(Boolean enable)
    {
        _unitActions.gameObject.SetActive(enable);

        if (_selectedUnit.AvailabeAttacks == null)
            _selectedUnit.AvailabeAttacks = grid.GetAvailableAttacks(grid.SelectedCell, _selectedUnit.AttackRange);

        EnableOption("Attack", _selectedUnit.AvailabeAttacks.Count > 0);
    }

    private void EnableOption(String option, Boolean enable)
    {
        _unitActions.Find(option).gameObject.SetActive(enable);
    }

    public void ShowAvailableAttacks()
    {
        if(_selectedUnit.AvailabeAttacks != null && _selectedUnit.AvailabeAttacks.Count > 0)
        {
            grid.ShowAllAvailableAttacks(_selectedUnit);
            _mode = Modes.Attack;
        }
    }

    public void EndTurn()
    {
        ShowActions(false);
        _selectedUnit.EndTurn();
    }

    public void ShowAvailableMoves()
    {
        grid.ShowAllAvailableMoves(grid.SelectedCell, _selectedUnit.Speed, _selectedUnit.AttackRange);
        _mode = Modes.Move;
    }
}
