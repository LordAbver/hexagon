using System;
using System.Collections.Generic;

public class HexCellPriorityQueue
{
    private List<HexCell> _list = new List<HexCell>();
    private Int32 _count = 0;
    private Int32 _minimum = Int32.MaxValue;

    public int Count
    {
        get
        {
            return _count;
        }
    }


    public void Enqueue(HexCell cell)
    {
        _count += 1;
        var priority = cell.SearchPriority;
        if (priority < _minimum)
        {
            _minimum = priority;
        }
        while (priority >= _list.Count)
        {
            _list.Add(null);
        }
        cell.NextWithSamePriority = _list[priority];
        _list[priority] = cell;
    }

    public HexCell Dequeue()
    {
        _count -= 1;
        for (; _minimum < _list.Count; _minimum++)
        {
            var cell = _list[_minimum];
            if (cell != null)
            {
                _list[_minimum] = cell.NextWithSamePriority;
                return cell;
            }
        }
        return null;
    }

    public void Change(HexCell cell, Int32 oldPriority)
    {
        HexCell current = _list[oldPriority];
        HexCell next = current.NextWithSamePriority;
        if (current == cell)
        {
            _list[oldPriority] = next;
        }
        else
        {
            while (next != cell)
            {
                current = next;
                next = current.NextWithSamePriority;
            }
            current.NextWithSamePriority = cell.NextWithSamePriority;
        }
        Enqueue(cell);
        _count -= 1;
    }

    public void Clear()
    {
        _list.Clear();
        _count = 0;
        _minimum = int.MaxValue;
    }
}
