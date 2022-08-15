using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class ChessPiece : MonoBehaviour
{
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
            startGeneIndex = value == 0 ? 0 : 4;
        }
    }
    [SerializeField] protected string atkAnimation, getHitAnimation, dieAnimation;
    [SerializeField] protected Transform bulletContainer;
    [SerializeField] protected float atkDelay;

    public bool isAI;
    private List<Vector2Int> moveList;
    private List<Vector2Int> atkList;
    protected int startGeneIndex;

    public BoardPiece currentBoardPiece;

    protected virtual void Start()
    {
        this.InitAppearance();
        bulletContainer = GameObject.Find("Bullets").transform;
    }
    public virtual void InitAppearance()
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
        var targetChessPiece = Board.GetPiece(coord).currentChessPiece;

        PerformAtk(targetChessPiece);
    }
    public virtual void PerformAtk(ChessPiece targetChessPiece)
    {

        Vector2 targetPosition = targetChessPiece.transform.position;

        var animation = GetComponentInChildren<Figure>();
        var targetAnimation = targetChessPiece.GetComponentInChildren<Figure>();
        var targetSkeleton = targetChessPiece.GetComponentInChildren<SkeletonAnimation>();
        animation.DoAtkAnim(this.atkAnimation, targetSkeleton, () =>
        {
            targetChessPiece.hp -= damage;
            if (targetChessPiece.hp <= 0)
            {
                targetAnimation.DoHitOrDieAnim(this.dieAnimation, () => targetChessPiece.Perish());

            }
            else targetAnimation.DoHitOrDieAnim(this.getHitAnimation);
            AnimationPlayer.DamagePopup(targetPosition, 1f, damage, "-");
        }, atkDelay);

        //AnimationPlayer.DamagePopup(targetPosition, 1f, damage, "-");
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
