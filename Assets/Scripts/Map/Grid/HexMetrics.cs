using System;
using UnityEngine;

public static class HexMetrics
{
    public const float OUTER_RADIUS = 5f;
    public const float PARTICLE_SIZE = 0.2f;
    public const float SOLID_FACTOR = 0.98f;

    public const float BLEND_FACTOR = 1f - SOLID_FACTOR;
    public const float INNER_RADIUS = OUTER_RADIUS * 0.866025404f;

    private static Vector3[] Corners = {
        new Vector3(0f, 0f, OUTER_RADIUS),
        new Vector3(INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
        new Vector3(INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
        new Vector3(0f, 0f, -OUTER_RADIUS),
        new Vector3(-INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
        new Vector3(-INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
        new Vector3(0f, 0f, OUTER_RADIUS)
    };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return Corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return Corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return Corners[(int)direction] * SOLID_FACTOR;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return Corners[(int)direction + 1] * SOLID_FACTOR;
    }
}