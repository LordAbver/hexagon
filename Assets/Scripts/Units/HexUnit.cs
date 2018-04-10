using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexUnit : MonoBehaviour {
    private Animator _animator;
    private List<HexCell> _pathToTravel;
    private HexCell _location;
    private HexDirection _direction;
    private const float _travelSpeed = 4f;
    private String _currentAnimation = AnimationSet.IDLE;
    private HexUnit _enemy;

    public void SetEnemy(HexUnit enemy)
    {
        _enemy = enemy;
    }

    public Boolean CanCounter {
        get
        {
            return _enemy && ActPhase == ActPhase.Wait && _enemy.Location.Coordinates.DistanceTo(Location.Coordinates) <= AttackRange;
        }
    }

    private ActPhase _actPhase;
    public ActPhase ActPhase
    {
        get
        {
            return _actPhase;
        }
        set
        {
            _animator.SetBool(AnimationSet.IS_ACTIVE, value == ActPhase.WaitForCommand);
            _actPhase = value;
        }
    }

    public Int32 Speed;
    public Int32 AttackRange;
    public UnitTeams Team { get; set; }

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

    public HashSet<HexCell> AvailabeAttacks { get; set; }

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
        ActPhase = ActPhase.Walk;
        Location = path[path.Count - 1];
        _pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
        ActPhase = ActPhase.WaitForCommand;
    }

    public void ValidateLocation()
    {
        transform.localPosition = _location.Position;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        ActPhase = ActPhase.WaitForCommand;
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
                var dir = baseCell.Coordinates.GetRelatedDirection(targetCell.Coordinates);
                Rotate(dir);
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
            var dir = baseCell.Coordinates.GetRelatedDirection(targetCell.Coordinates);
            Rotate(dir);
            yield return null;
        }

        transform.localPosition = _location.Position.SetY(targetCell.Height);

        ResetAnimation();

        ListPool<HexCell>.Add(_pathToTravel);
        _pathToTravel = null;
    }

    public void Damage(Int32 hp)
    {
        var hpBar = transform.Find("Hp");
        var chars = new List<Char> (hp.ToString());
        chars.Reverse();
        var sprites = GridResources.HpNumberSprites;
        for(var i = 0; i < chars.Count; i++)
        {
            var cur = Convert.ToInt16(chars[i].ToString());
            var img = hpBar.GetChild(i).GetComponent<SpriteRenderer>();
            img.sprite = sprites[cur];
            hpBar.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void Rotate(HexDirection direction)
    {
        _direction = direction;
       SetAnimation(String.Format("{0}{1}", AnimationSet.WALK, direction.GetName()));
    }

    public void Attack(HexUnit enemy, HexDirection direction, Int32 dmg)
    {
        SetEnemy(enemy);
        enemy.Damage(dmg);
        enemy.SetEnemy(this);
        _animator.SetBool(String.Format("{0}{1}", AnimationSet.WALK, direction.GetName()), false);
        _animator.SetTrigger(String.Format("{0}{1}", AnimationSet.ATTACK, direction.GetName()));
    }

    public void TakeDamage()
    {
        _animator.SetTrigger(AnimationSet.TAKE_DAMAGE);
    }

    public void EndTurn()
    {
        ActPhase = ActPhase.EndTurn;
        ResetAnimation();
    }

    public void ResetAnimation()
    {
        _animator.SetBool(_currentAnimation, false);
        _animator.Play(AnimationSet.IDLE);
    }

    private void ResetHpBar()
    {
        var hpBar = transform.Find("Hp");
        var sprites = GridResources.HpNumberSprites;
        for (var i = 0; i < hpBar.childCount; i++)
        {
            var img = hpBar.GetChild(i).GetComponent<SpriteRenderer>();
            img.sprite = sprites[0];
            hpBar.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void SetAnimation(String animation)
    {
        if (_currentAnimation == animation) return;

        if(_currentAnimation != AnimationSet.IDLE)
            _animator.SetBool(_currentAnimation, false);

        _currentAnimation = animation;
        _animator.SetBool(_currentAnimation, true);
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }

    void OnEnable()
    {
        if (_location)
        {
            transform.localPosition = _location.Position;
        }
    }

    void OnMouseEnter()
    {
        //Debug.Log(_actPhase);
    }

    void OnMouseExit()
    {
        //
    }

    void OnAttackEnd()
    {
        if(_enemy)
        {
            _enemy.TakeDamage();
        }
    }

    void OnTakeDamageEnd()
    {
        if (_enemy != null && CanCounter)
        {
            Attack(_enemy, _direction.Opposite(), 105);
        }
        else
        {
            EndTurn();
        }
    }

    void OnHpBarChange()
    {
        ResetHpBar();
    }

    public void Die()
    {
        _location.Unit = null;
        Destroy(gameObject);
    }

    public bool IsValidDestination(HexCell cell)
    {
        return cell.TerrainType != HexTerrainTypes.Lake && !cell.Unit;
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
