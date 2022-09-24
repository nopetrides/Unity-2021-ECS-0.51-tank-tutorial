using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponNonECS : PlayerWeapon
{
    protected override void Start()
    {
        // do nothing, skip Entity conversion stuff
    }

    public override void FireBullet()
    {
        FireBulletNonECS();
    }
}
