using System;

public enum UnitTeams
{
    Blue,
    Brown,
    Green,
    Pink,
    Purple,
    Teal
}

public class UnitMeta
{
    private Int32[] _blue = new[] { 0, 1, 2 };
    private Int32[] _brown = new[] { 3, 4, 5 };
    private Int32[] _green = new[] { 6, 7, 8 };
    private Int32[] _pink = new[] { 9, 10, 11 };
    private Int32[] _purple = new[] { 12, 13, 14 };
    private Int32[] _teal = new[] { 15, 16, 17 };
    
    public UnitTeams Team { get; private set; }
    public Int32 SpriteIdx { get; private set; }
    public String Name { get; private set; }

    public UnitMeta(String name, UnitTeams team, Int32 idx)
    {
        Name = name;
        Team = team;
        switch (team)
        {
            case UnitTeams.Blue:
                SpriteIdx = _blue[idx];
            break;
            case UnitTeams.Brown:
                SpriteIdx = _brown[idx];
            break;
            case UnitTeams.Green:
                SpriteIdx = _green[idx];
            break;
            case UnitTeams.Pink:
                SpriteIdx = _pink[idx];
            break;
            case UnitTeams.Purple:
                SpriteIdx = _purple[idx];
            break;
            case UnitTeams.Teal:
                SpriteIdx = _teal[idx];
            break;
        }
    }
}
