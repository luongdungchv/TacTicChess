using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : ChessPiece
{
    public override void HighLightAtk(Vector2Int currentCoordinate) 
    {       
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
            for(int i = 0; i < 3; i++)
            {
                if (lockState[i] == 1) continue;
                var piece = pieces[i];
                if(count >= 5) piece.GetComponent<SpriteRenderer>().color = Color.cyan;
                if(piece.currentChessPiece != null || piece.barrierSide != -1)
                {
                    lockState[i] = 1;
                    if (piece.barrierSide == side) continue;
                    if (piece.currentChessPiece != null && piece.currentChessPiece.side == side) continue;
                    if (count < 5) continue;
                    HighlightPieceAtk(piece);
                    
                }
            }
            if(dir == 4) Highlight(coord + Vector2Int.one, lockState, dir, count + 1);
            if(dir == 2) Highlight(coord - Vector2Int.one , lockState, dir, count + 1);
            if(dir == 1) Highlight(coord + new Vector2Int(-1, 1), lockState, dir, count + 1);
            if(dir == 3) Highlight(coord + new Vector2Int(1, -1), lockState, dir, count + 1);
        }
        Highlight(currentCoordinate + Vector2Int.one, new int[] {0,0,0 }, 4, 1);
        Highlight(currentCoordinate - Vector2Int.one, new int[] { 0, 0, 0 }, 2, 1);
        Highlight(currentCoordinate + new Vector2Int(-1, 1), new int[] { 0, 0, 0 }, 1, 1);
        Highlight(currentCoordinate + new Vector2Int(1, -1), new int[] { 0, 0, 0 }, 3, 1);
        
    }
    public override void HighlightMove(Vector2Int currentCoordinate)
    {
        Debug.Log(currentCoordinate);
        for(int i = currentCoordinate.x - 1; i <= currentCoordinate.x + 1; i++)
        {
            var pieceToHighlight = BoardGenerator.GetPiece(i, currentCoordinate.y);
            if (pieceToHighlight.currentChessPiece != null && pieceToHighlight.GetCoordinate() != currentCoordinate) continue;
            if (i < 0 || i > 9 || i == currentCoordinate.x) continue;
            HighlightPieceMove(pieceToHighlight);
        }
        for(int i = currentCoordinate.y - 1; i <= currentCoordinate.y + 1; i++)
        {
            var pieceToHighlight = BoardGenerator.GetPiece(currentCoordinate.x, i);
            if (pieceToHighlight.currentChessPiece != null && pieceToHighlight.GetCoordinate() != currentCoordinate) break;
            if (i < 0 || i > 9 || i == currentCoordinate.y) continue;
            HighlightPieceMove(pieceToHighlight);
        }
    }
    public override void PerformAtk(Vector2Int coord)
    {
        base.PerformAtk(coord);
    }
}
