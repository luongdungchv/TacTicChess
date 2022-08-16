using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public virtual void Perform()
    {

    }
    public virtual void Undo()
    {
        if (Board.ins.moveCount == 3)
        {
            Player.ins.ChangeSideLocal();
            Board.ins.moveCount = 1;
            return;
        }
        Board.ins.moveCount++;
    }

}
public class SetPieceMove : Move
{
    private Vector2Int fromCoord;
    private Vector2Int toCoord;

    public SetPieceMove(Vector2Int oldCoord, Vector2Int newCoord)
    {
        this.fromCoord = oldCoord;
        this.toCoord = newCoord;
    }
    public override void Perform()
    {
        var from = Board.GetPiece(fromCoord);
        var to = Board.GetPiece(toCoord);

        AnimationPlayer.LerpPosition(from.currentChessPiece.transform, to.transform.position, 0.25f);

        to.currentChessPiece = from.currentChessPiece;
        from.currentChessPiece = null;

        Board.ins.moveStack.Push(this);
        Board.ins.PerformMove();
    }
    public override void Undo()
    {
        base.Undo();
        var from = Board.GetPiece(toCoord);
        var to = Board.GetPiece(fromCoord);

        AnimationPlayer.LerpPosition(from.currentChessPiece.transform, to.transform.position, 0.25f);

        to.currentChessPiece = from.currentChessPiece;
        from.currentChessPiece = null;
    }
}
public class AttackMove : Move
{
    private ChessPiece attacker;
    private ChessPiece target;
    private Vector2Int attackerCoord, targetCoord;
    public AttackMove(Vector2Int attackerCoord, Vector2Int targetCoord)
    {
        this.target = Board.GetPiece(targetCoord).currentChessPiece;
        this.attacker = Board.GetPiece(attackerCoord).currentChessPiece;
        this.attackerCoord = attackerCoord;
        this.targetCoord = targetCoord;
    }
    public override void Perform()
    {
        // Debug.Log(attacker);
        if (attacker != null)
        {
            //target = BoardGenerator.GetPiece(pattern[1]).currentChessPiece;
            attacker.PerformAtk(target);

        }
        Board.ins.moveStack.Push(this);
        Board.ins.PerformMove();
    }
    public override void Undo()
    {
        base.Undo();
        var from = Board.GetPiece(attackerCoord);
        var to = Board.GetPiece(targetCoord);

        target.gameObject.SetActive(true);
        target.hp += from.currentChessPiece.damage;
        AnimationPlayer.DamagePopup(to.transform.position, 1f, from.currentChessPiece.damage, "+");
        to.currentChessPiece = target;
    }
}
public class PlaceBarrierMove : Move
{
    private Vector2Int placeCoord;
    public PlaceBarrierMove(Vector2Int placeCoord)
    {
        this.placeCoord = placeCoord;

    }
    public override void Perform()
    {
        var boardPiece = Board.GetPiece(placeCoord);
        boardPiece.SetBarrier(Board.ins.currentSide);
        Player.ins.barrierCount--;
        Board.ins.moveStack.Push(this);
        Board.ins.PerformMove();
    }
    public override void Undo()
    {
        base.Undo();
        var piece = Board.GetPiece(placeCoord);
        Player.ins.barrierCount++;
        piece.currentChessPiece.GetComponent<Barrier>().DestroyBarrier();
    }
}
