using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfo
{
    public Vector2 position { get; private set; }
    public int tier { get; private set; }


    public WeaponInfo(Vector2 position, int tier)
    {
        this.position = position;
        this.tier = tier;
    }
}
