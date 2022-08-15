using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBase : ChessPiece
{
    protected override void Start()
    {
        base.Start();
    }
    public override void Perish()
    {
        base.Perish();
        if (Board.gameMode == "Single")
        {
            if (side == 0) Player.ins.DecreaseBases();
            else
            {
                AI.ins.DecreaseBases();
            }

            return;
        }
        if (Player.ins.side == side)
        {
            Player.ins.DecreaseBases();
            Debug.Log("1 base destroyed");
        }
    }
    public override void InitAppearance()
    {
        var figure = GetComponentInChildren<Figure>();
        var figureManager = FigureManager.ins;
        var gene = figureManager.GetGene(this.startGeneIndex + 3);
        figure.SetGenes("1000", gene);
    }
}
