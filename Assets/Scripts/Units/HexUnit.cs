using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexUnit : MonoBehaviour {
    private Animator _animator;
    private List<HexCell> _pathToTravel;
    private HexCell _location;
    private const float _travelSpeed = 4f;
    private String _currentAnimation = "Idle";

    public Texture2D CursorTexture;

    public Int32 Speed;
    public Int32 AttackRange;

    private Dictionary<HexCell, PathSearchResult> _availableMoves;
    public Dictionary<HexCell, PathSearchResult> AvailableMoves
    {
        get
        {
            if (_availableMoves == null)
                _availableMoves = new Dictionary<HexCell, PathSearchResult>();

            return _availableMoves;
        }
        set
        {
            _availableMoves = value;
        }
    }

    public HexCell Location
    {
        get
        {
            return _location;
        }
        set
        {
            if (_location){
                _location.Unit = null;
            }
            _location = value;
            value.Unit = this;
            transform.localPosition = new Vector3(value.Position.x, value.Height, value.Position.z);
        }
    }

    public void Travel(List<HexCell> path)
    {
        Location = path[path.Count - 1];
        _pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    public void ValidateLocation()
    {
        transform.localPosition = _location.Position;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();    
    }

    private IEnumerator TravelPath()
    {
        Vector3 a, b, c = _pathToTravel[0].Position;
        HexCell baseCell, targetCell = null;

        float t = Time.deltaTime * _travelSpeed;
        for (int i = 1; i < _pathToTravel.Count; i++)
        {
            baseCell = _pathToTravel[i - 1];
            targetCell = _pathToTravel[i];
            a = c.SetY(baseCell.Height);
            b = baseCell.Position.SetY(baseCell.Height);
            c = ((b + targetCell.Position) * 0.5f).SetY(targetCell.Height);

            for (; t < 1f; t += Time.deltaTime * _travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Rotate(baseCell.Coordinates, targetCell.Coordinates);
                yield return null;
            }
            t -= 1f;
        }

        baseCell = _pathToTravel[_pathToTravel.Count - 1];
        a = c.SetY(baseCell.Height); ;
        b = baseCell.Position.SetY(baseCell.Height);
        c = b;

        for (; t < 1f; t += Time.deltaTime * _travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Rotate(baseCell.Coordinates, targetCell.Coordinates);
            yield return null;
        }

        transform.localPosition = _location.Position.SetY(targetCell.Height);

        SetAnimation(AnimationSet.IDLE);

        ListPool<HexCell>.Add(_pathToTravel);
        _pathToTravel = null;
    }

    public void Rotate(HexCoordinates from, HexCoordinates to)
    {
        if(from.Z < to.Z)
            SetAnimation(AnimationSet.WALK_NORTH);

        if (from.Z > to.Z)
            SetAnimation(AnimationSet.WALK_SOUTH);

        if (from.Y < to.Y && from.Z == to.Z)
            SetAnimation(AnimationSet.WALK_EAST);

        if (from.Y > to.Y && from.Z == to.Z)
            SetAnimation(AnimationSet.WALK_WEST);
    }

    private void SetAnimation(String animation)
    {
        if (_currentAnimation == animation) return;
        _animator.SetBool(_currentAnimation, false);
        _currentAnimation = animation;
        _animator.SetBool(_currentAnimation, true);
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        /*if (Input.GetMouseButtonDown(1))
        {
            _animator.SetTrigger("AttackSouth");
        }*/

        //Follow camera rotation
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }

    void OnEnable()
    {
        if (_location)
        {
            transform.localPosition = _location.Position;
        }
    }

    void OnMouseDown()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            var grid = GetComponent<HexGrid>();
            var g = 4;
        }*/
    }

    void OnMouseEnter()
    {
        Cursor.SetCursor(CursorTexture,Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Die()
    {
        _location.Unit = null;
        Destroy(gameObject);
    }

    public bool IsValidDestination(HexCell cell)
    {
        return cell.TerrainType != HexTerrainTypes.LAKE && !cell.Unit;
    }

    public Boolean CanReach(HexCell targetCell)
    {
        return _availableMoves.ContainsKey(targetCell) && _availableMoves[targetCell].CanReach;
    }

    public Boolean CanAttack(HexCell targetCell)
    {
        return _availableMoves.ContainsKey(targetCell) && _availableMoves[targetCell].CanAttack;
    }
}
