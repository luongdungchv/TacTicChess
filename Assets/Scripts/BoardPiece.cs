﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardPiece : MonoBehaviour
{
    public static event EventHandler OnBoardReset;
    public static BoardPiece selectedPiece;

    public bool isSamplePiece;

    public GameObject currentChessPieceObj;
    [SerializeField]
    private ChessPiece _currentChessPiece;
    public ChessPiece currentChessPiece
    {
        get => _currentChessPiece;
        set
        {
            _currentChessPiece = value;
            if (_currentChessPiece != null) _currentChessPiece.currentBoardPiece = this;
        }
    }

    public static bool isInHighlightSelection;
    public bool isRoot;


    public int highlightType;


    public SpriteRenderer marker;

    public int barrierSide = -1;

    private void Start()
    {
        OnBoardReset += (s, e) =>
        {
            selectedPiece = null;
            highlightType = 0;
            GetComponent<SpriteRenderer>().color = Color.white;
            marker.gameObject.SetActive(false);
            isInHighlightSelection = false;
            if (isRoot) isRoot = false;
        };
        highlightType = 0;
        barrierSide = -1;
    }


    private void OnDestroy()
    {
        OnBoardReset = null;
    }
    private void OnMouseDown()
    {
        if (Board.ins.matchEnded) return;

        if (!Client.IsConnectedToInternet())
        {
            return;
        }
        Debug.Log("click");
        if (Player.ins.side == Board.ins.currentSide && !Board.ins.isEnd)
        {

            if (BarrierPlacer.isInPlacingMode)
            {
                bool isValidPiece = Player.ins.side == 0 ? GetCoordinate().x > 4 : GetCoordinate().x <= 4;
                if (currentChessPiece == null && isValidPiece)
                {
                    if (Player.ins.barrierCount > 0)
                    {
                        Player.ins.PlaceBarrierRequest(GetCoordinate());
                    }
                }
                BarrierPlacer.isInPlacingMode = false;
                OnBoardReset?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (BarrierPlacer.isInMovingMode)
            {
                bool isValidPiece = Player.ins.side == 0 ? GetCoordinate().x > 4 : GetCoordinate().x <= 4;
                if (currentChessPiece == null && isValidPiece)
                {
                    Player.ins.SetPieceRequest(BarrierPlacer.selectedPiece.GetCoordinate(), GetCoordinate());
                }
                BarrierPlacer.DisableMovingMode();
                OnBoardReset?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!isRoot && highlightType == 0 && currentChessPiece != null && !isInHighlightSelection && currentChessPiece.side == Player.ins.side)
            {

                if (currentChessPiece.GetType() == typeof(MainBase)) return;
                marker.color = Color.green;
                marker.gameObject.SetActive(true);
                if (currentChessPiece.GetType() == typeof(Barrier))
                {
                    BarrierPlacer.EnableMovingMode(GetCoordinate());
                    marker.gameObject.SetActive(true);
                    marker.color = Color.green;
                    return;
                }
                isInHighlightSelection = true;
                HighlightSelector.ins.Show(this);
                return;
            }
            if (isRoot)
            {
                isRoot = false;
                OnBoardReset?.Invoke(this, EventArgs.Empty);
                return;
            }

            SetBoardPiece();
        }


    }
    public void HighlightMove()
    {
        OnBoardReset?.Invoke(this, EventArgs.Empty);
        currentChessPiece.HighlightMove(GetCoordinate());
        selectedPiece = this;
        isRoot = true;
    }
    public void HighlightAtk()
    {
        OnBoardReset?.Invoke(this, EventArgs.Empty);
        currentChessPiece.HighLightAtk(GetCoordinate());
        selectedPiece = this;
        isRoot = true;
    }
    public Vector2Int GetCoordinate()
    {
        int width = Board.ins.boardPieces.GetLength(0);
        int height = Board.ins.boardPieces.GetLength(1);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (Board.ins.boardPieces[i, j].Equals(this))
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return -Vector2Int.one;
    }
    void SetBoardPiece()
    {
        if (highlightType != 0)
        {
            if (highlightType == 1)
            {
                Player.ins.SetPieceRequest(selectedPiece.GetCoordinate(), this.GetCoordinate());
            }
            if (highlightType == 2)
            {
                Player.ins.AttackTargetRequest(selectedPiece.GetCoordinate(), this.GetCoordinate());
            }
        }
        OnBoardReset?.Invoke(this, EventArgs.Empty);
    }

    public void SetBarrier(int side)
    {
        var barrier = Instantiate(Board.ins.barrierPrefab, transform.position, Quaternion.identity).GetComponent<Barrier>();
        barrier.side = side;
        currentChessPiece = barrier;
    }

    public static void ResetEvent()
    {
        OnBoardReset = null;
    }


}
