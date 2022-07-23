using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : ChessPiece
{
    public void DestroyBarrier()
    {
        Destroy(gameObject);
    }
}
