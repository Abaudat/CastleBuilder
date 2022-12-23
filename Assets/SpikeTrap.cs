using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : CollisionBlock
{
    protected override void OnPlayerCollision()
    {
        playManager.Die();
    }
}
