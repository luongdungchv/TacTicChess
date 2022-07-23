using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shielder : ChessPiece
{
    public override void HighLightAtk(Vector2Int currentCoordinate)
    {
        for (int i = currentCoordinate.x - 1; i <= currentCoordinate.x + 1; i++)
        {
            if (i < 0 || i > 9) continue;
            for (int j = currentCoordinate.y - 1; j <= currentCoordinate.y + 1; j++)
            {
                var piece = BoardGenerator.GetPiece(i, j);
                if (piece.GetCoordinate() == currentCoordinate) continue;
                if (piece.currentChessPiece != null)
                {
                    if (piece.barrierSide == side) continue;
                    if (piece.currentChessPiece != null && piece.currentChessPiece.side == side) continue;
                    HighlightPieceAtk(piece);
                }
                
            }
        }
    }
    public override void HighlightMove(Vector2Int currentCoordinate)
    {
        for(int i = currentCoordinate.x - 1; i <= currentCoordinate.x + 1; i++)
        {
            if (i < 0 || i > 9) continue;
            for(int j = currentCoordinate.y - 1; j <= currentCoordinate.y + 1; j++)
            {
                var piece = BoardGenerator.GetPiece(i, j);
                if (piece.GetCoordinate() == currentCoordinate) continue;
                if (piece.currentChessPiece != null) continue;
                HighlightPieceMove(piece);
            }
        }
    }
    public override void PerformAtk(Vector2Int coord)
    {
        base.PerformAtk(coord);
    }
}
