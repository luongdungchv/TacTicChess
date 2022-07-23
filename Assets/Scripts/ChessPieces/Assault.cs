using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assault : ChessPiece
{
    public override void HighLightAtk(Vector2Int currentCoordinate)
    {
        int startRange = 1;
        void Highlight(Vector2Int coord, int[] lockState, int dir, int count)
        {
            var coordClone = coord;
            if (coordClone.x > 9 || coordClone.y > 9 || coordClone.x < 0 || coordClone.y < 0)
            {
                coordClone -= Vector2Int.one;
                lockState[2] = 1;
            }
            if ((coord.x < 0 || coord.x > 9) && (coord.y < 0 || coord.y > 9)) return;

            BoardPiece[] pieces =
           {
                BoardGenerator.GetPiece(currentCoordinate.x, coord.y),
                BoardGenerator.GetPiece(coord.x, currentCoordinate.y),
                BoardGenerator.GetPiece(coordClone.x, coordClone.y)
            };
            for (int i = 0; i < 3; i++)
            {
                if (lockState[i] == 1) continue;
                var piece = pieces[i];
                if(count >= 2) piece.GetComponent<SpriteRenderer>().color = Color.cyan;
                if (piece.currentChessPiece != null)
                {
                    lockState[i] = 1;
                    if (piece.barrierSide == side) continue;
                    if (piece.currentChessPiece != null && piece.currentChessPiece.side == side) continue;
                    if (count < 1) continue;
                    HighlightPieceAtk(piece);
                }
            }
            if (count == 5) return;
            if (dir == 4) Highlight(coord + Vector2Int.one, lockState, dir, count + 1);
            if (dir == 2) Highlight(coord - Vector2Int.one, lockState, dir, count + 1);
            if (dir == 1) Highlight(coord + new Vector2Int(-1, 1), lockState, dir, count + 1);
            if (dir == 3) Highlight(coord + new Vector2Int(1, -1), lockState, dir, count + 1);
        }
        Highlight(currentCoordinate + Vector2Int.one * startRange, new int[] { 0, 0, 0 }, 4, 0);
        Highlight(currentCoordinate - Vector2Int.one * startRange, new int[] { 0, 0, 0 }, 2, 0);
        Highlight(currentCoordinate + new Vector2Int(-1, 1) * startRange, new int[] { 0, 0, 0 }, 1, 0);
        Highlight(currentCoordinate + new Vector2Int(1, -1) * startRange, new int[] { 0, 0, 0 }, 3, 0);
    }
    public override void HighlightMove(Vector2Int currentCoordinate)
    {
        for (int i = currentCoordinate.x - 2; i <= currentCoordinate.x + 2; i++)
        {
            if (i < 0 || i > 9) continue;
            for (int j = currentCoordinate.y - 2; j <= currentCoordinate.y + 2; j++)
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
