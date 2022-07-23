using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Chess Piece Types", menuName = "Chess Piece Types")]
public class ChessPieceTypes : ScriptableObject
{
    public List<ChessPiece> pieces;
   
    public int GetTypeIndex(ChessPiece piece)
    {
        if (piece == null) return -1;
        for(int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].GetType() == piece.GetType()) return i;
        }
        return -1;
    }
    public ChessPiece GetChessPiecePrefab(int index)
    {
        return pieces[index];
    }
}
