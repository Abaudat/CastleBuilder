using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : CollisionBlock
{
    protected override void OnPlayerCollision()
    {
        playManager.Success();
    }
}
