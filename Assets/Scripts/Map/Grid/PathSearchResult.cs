using System;

public class PathSearchResult {

    public Boolean CanReach { get; private set; }
    public Boolean CanAttack { get; private set; }

    public PathSearchResult(Boolean canReach, Boolean canAttack)
    {
        CanReach = canReach;
        CanAttack = canAttack;
    }

    public PathSearchResult() { }

    public static implicit operator Boolean(PathSearchResult res)
    {
        return res.CanReach;
    }
}
