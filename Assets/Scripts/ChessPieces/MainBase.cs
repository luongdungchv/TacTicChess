using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBase : ChessPiece
{
    public override void Perish()
    {
        base.Perish();
        if (Board.gameMode == "Single")
        {
            if (side == 0) Client.ins.DecreaseBases();
            else
            {
                AI.ins.DecreaseBases();
            }

            return;
        }
        if (Client.ins.side == side)
        {
            Client.ins.DecreaseBases();
            Debug.Log("1 base destroyed");
        }
    }
}
