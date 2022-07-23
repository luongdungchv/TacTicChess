using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public static Dictionary<int, ChessPiece> chessPiecesList;
    public int hp;
    public int damage;
    [SerializeField] private int _side;
    public int side
    {
        get => _side;
        set
        {
            _side = value;
            GetComponent<SpriteRenderer>().color = value == 0 ? Color.cyan : Color.blue;
        }
    }

    public int value;
    public bool isAI;
    public List<Vector2Int> moveList;
    public List<Vector2Int> atkList;
    public BoardPiece currentBoardPiece;

    private void Start()
    {
    }



    public virtual void HighlightMove(Vector2Int currentCoordinate)
    {

    }
    public virtual void HighLightAtk(Vector2Int currentCoordinate)
    {

    }
    public virtual void PerformAtk(Vector2Int coord)
    {
        var targetChessPiece = BoardGenerator.GetPiece(coord).currentChessPiece;
        targetChessPiece.hp -= damage;
        if (targetChessPiece.hp <= 0)
        {
            targetChessPiece.Perish();
        }
        Vector2 targetPosition = targetChessPiece.transform.position;
        AnimationPlayer.DamagePopup(targetPosition, 1f, damage, "-");
    }
    protected void HighlightPieceAtk(BoardPiece piece)
    {
        if (isAI)
        {
            moveList.Add(piece.GetCoordinate());
            return;
        }
        piece.highlightType = 2;
        piece.marker.color = Color.red;
        piece.marker.gameObject.SetActive(true);
    }
    protected void HighlightPieceMove(BoardPiece piece)
    {

        if (isAI)
        {
            atkList.Add(piece.GetCoordinate());
            return;
        }
        piece.highlightType = 1;
        piece.marker.color = Color.yellow;
        piece.marker.gameObject.SetActive(true);
    }
    public virtual void Perish()
    {
        gameObject.SetActive(false);
        currentBoardPiece.currentChessPiece = null;
    }
}
