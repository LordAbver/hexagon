using System;
using UnityEngine;
using System.Collections.Generic;

public class HexCell : MonoBehaviour
{
    [SerializeField]
    HexCell[] Neighbors;

    public Transform hexPrefab;

    public RectTransform UiRect;

    public HexCoordinates Coordinates;

    public HexUnit Unit { get; set; }
    public Color Color;
    public HexCell PathFrom { get; set; }
    public Int32 SearchHeuristic { get; set; }

    private float _height;
    public float Height {
        get
        {
            return _height + 1.2f;
        }
        set
        {
            _height = value;
        }
    }

    public int SearchPriority
    {
        get
        {
            return _distance + SearchHeuristic;
        }
    }

    public HexCell NextWithSamePriority { get; set; }

    private Guid _cellID;

    private HashSet<HexDirection> _roads;

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    public Int32 SearchPhase { get; set; }

    public HashSet<HexDirection> Roads
    {
        get
        {
            return _roads;
        }
        set
        {
            if (value != null && value.Count > 0)
                _terrainType = HexTerrainTypes.ROAD;

            _roads = value;
        }
    }

    private String _terrainType;
    public String TerrainType
    {
        get
        {
            return _terrainType ?? HexTerrainTypes.FLAT;
        }
        set
        {
            _terrainType = value;
        }
    }

    private Int32 _distance;
    public int Distance
    {
        get
        {
            return _distance;
        }
        set
        {
            _distance = value;
        }
    }

    private Int32 _absoluteDistance;
    public int AbsoluteDistance
    {
        get
        {
            return _absoluteDistance;
        }
        set
        {
            _absoluteDistance = value;
        }
    }

    private Int32 _attackDistance;
    public int AttackDistance
    {
        get
        {
            return _attackDistance;
        }
        set
        {
            _attackDistance = value;
        }
    }

    public HexCell()
    {
        _cellID = Guid.NewGuid();
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return Neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public Boolean HasRoad(HexDirection direction)
    {
        return Roads != null && Roads.Contains(direction);
    }

    public void SetVisualStyle(String styleName, Boolean enable)
    {
        var mesh = UiRect.Find(styleName);
        mesh.gameObject.SetActive(enable);
    }

    public void SetLabel(String text)
    {
        var label = UiRect.GetComponent<TextMesh>();
        label.text = text; ;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(HexCell) && Equals((HexCell)obj);
    }

    public bool Equals(HexCell other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(other._cellID, _cellID);
    }

    public static bool operator ==(HexCell a, HexCell b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(HexCell a, HexCell b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return _cellID.GetHashCode();
        }
    }
}