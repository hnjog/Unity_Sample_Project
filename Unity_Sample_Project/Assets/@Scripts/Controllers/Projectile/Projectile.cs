using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : BaseObject
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Projectile;

        return true;
    }
}
