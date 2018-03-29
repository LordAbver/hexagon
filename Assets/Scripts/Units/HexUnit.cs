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
        for (int i = 1; i < _pathToTravel.Count; i++)
        {
            var baseCell = _pathToTravel[i - 1];
            Vector3 a = new Vector3(baseCell.Position.x, baseCell.Height, baseCell.Position.z);

            var targetCell = _pathToTravel[i];
            Vector3 b = new Vector3(targetCell.Position.x, targetCell.Height, targetCell.Position.z);

            for (float t = 0f; t < 1f; t += Time.deltaTime * _travelSpeed)
            {
                transform.localPosition = Vector3.Lerp(a, b, t);
                yield return null;
            }
        }
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            _animator.SetTrigger("AttackSouth");
        }

        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetBool("WalkSouth", true);
        }

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
