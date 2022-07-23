using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator ins;
    public Transform startPos, startPos1;

    public ChessPieceTypes chessPieceTypes;

    public GameObject boardPiecePrefab;
    public BoxCollider2D baseSize;

    public GameObject test;
    public BoardPiece[,] boardPieces = new BoardPiece[10, 10];
    public Vector2Int testSet;

    public GameObject sniperPrefab, shielderPrefab, assaultPrefab, barrierPrefab, basePrefab;
    public List<ChessPiece> pieces;

    public int currentSide;
    public int moveCount;

    public TextMeshProUGUI resultText;
    public bool matchEnded;

    public TextMeshProUGUI turnIdentifier;
    public TextMeshProUGUI logText;

    public bool isEnd;
    public static string gameMode;

    public GameObject attemptReconnectPanel;

    public Stack<MovePattern> moveStack;

    void Start()
    {
        Application.runInBackground = true;
        moveStack = new Stack<MovePattern>();
        ins = this;
        GeneratePieces(Client.ins.side);
        Client.OnSideChange += (s, e) =>
        {
            currentSide = currentSide == 0 ? 1 : 0;
            moveCount = 3;
            BarrierPlacer.ins.barrierSelector.interactable = Client.ins.side == currentSide;
            CheckTurn();
        };
    }

    public void GeneratePieces(int input)
    {
        Debug.Log(input);
        Vector2 currentPos;
        float unit = baseSize.bounds.size.x;
        if (input == 0)
        {
            currentPos = startPos.position;
        }
        else { currentPos = startPos1.position;unit = -unit; BarrierPlacer.ins.barrierSelector.interactable = false; }
        Vector2 tempPos = currentPos;
        for (int i = 0; i < 10; i++)
        {
            tempPos.x = currentPos.x;
            
            for (int j = 0; j < 10; j++)
            {
                var boardPiece = Instantiate(boardPiecePrefab, tempPos, Quaternion.identity);
                boardPieces[i, j] = boardPiece.GetComponent<BoardPiece>();
                tempPos.x += unit;
            }

            tempPos.y -= unit;
        }
        GenerateChessPieces();
        turnIdentifier.gameObject.SetActive(true);
        CheckTurn();
    }
    void GenerateChessPieces()
    {
        //Generate shielders
        for(int i = 2; i <= 7; i += 5)
        {
            for(int j = 0; j <= 9; j++)
            {
                var shielderPiece = Instantiate(shielderPrefab).GetComponent<Shielder>();
                pieces.Add(shielderPiece);
                shielderPiece.side = i == 2 ? 1 : 0;
                GetPiece(i, j).currentChessPiece = shielderPiece;
                shielderPiece.transform.position = GetPiece(i, j).transform.position;
                if(gameMode == "Single")
                {
                    if (shielderPiece.side == 1)
                    {
                        shielderPiece.isAI = true;
                        AI.ins.AIpieces.Add(shielderPiece, new Vector2Int(i, j));
                    }
                    else AI.ins.playerPieces.Add(shielderPiece, new Vector2Int(i, j));
                }
            }
        }
        //Generate snipers
        Vector2Int[] spawnPositions =
        {
            new Vector2Int(9, 0), new Vector2Int(8, 0), new Vector2Int(8, 9), new Vector2Int(9, 9),
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 9), new Vector2Int(0, 9)
        };
        foreach(var i in spawnPositions)
        {
            var sniperPiece = Instantiate(sniperPrefab).GetComponent<Sniper>();
            pieces.Add(sniperPiece);
            sniperPiece.side = (i.x == 0 || i.x == 1) ? 1 : 0;
            GetPiece(i).currentChessPiece = sniperPiece;
            sniperPiece.transform.position = GetPiece(i).transform.position;
            if (gameMode == "Single")
            {
                if (sniperPiece.side == 1)
                {
                    sniperPiece.isAI = true;
                    AI.ins.AIpieces.Add(sniperPiece, i);
                }
                else AI.ins.playerPieces.Add(sniperPiece, i);
            }
        }
        //Generate assaults
        for(int n = 0; n <= 9; n += 9)
        {
            int i = n;
            for (int j = 1; j <= 8; j++)
            {
                var assaultPiece = Instantiate(assaultPrefab).GetComponent<Assault>();
                pieces.Add(assaultPiece);
                assaultPiece.side = (i == 1 || i == 0) ? 1 : 0;
                GetPiece(i, j).currentChessPiece = assaultPiece;
                assaultPiece.transform.position = GetPiece(i, j).transform.position;
                if (i == 0) i = 1;
                else if(i == 1)
                {
                    if (j == 4) continue;
                    i = 0;
                }

                if (i == 9) i = 8;
                else if(i == 8)
                {
                    if (j == 4) continue;
                    i = 9;
                }

                if (gameMode == "Single")
                {
                    if (assaultPiece.side == 1)
                    {
                        assaultPiece.isAI = true;
                        AI.ins.AIpieces.Add(assaultPiece, new Vector2Int(i, j));
                    }
                    else AI.ins.playerPieces.Add(assaultPiece, new Vector2Int(i, j));
                }
            }
        }
        //Generate bases
        for(int i = 0; i <= 9; i += 9)
        {
            for(int j = 4; j <= 5; j++)
            {
                var basePiece = Instantiate(basePrefab).GetComponent<MainBase>();
                pieces.Add(basePiece);
                basePiece.side = i == 0 ? 1 : 0;
                var boardPiece = GetPiece(i, j);
                boardPiece.currentChessPiece = basePiece;
                basePiece.transform.position = boardPiece.transform.position;
                if (gameMode == "Single")
                {
                    if (basePiece.side == 1)
                    {
                        basePiece.isAI = true;
                        AI.ins.AIpieces.Add(basePiece, new Vector2Int(i, j));
                    }
                    else AI.ins.playerPieces.Add(basePiece, new Vector2Int(i, j));
                }
            }
            
        }
    }
    
    
    public static BoardPiece GetPiece(int x, int y)
    {
        x = Mathf.Clamp(x, 0, 9);
        y = Mathf.Clamp(y, 0, 9);
        return ins.boardPieces[x, y];
    }
    public static BoardPiece GetPiece(Vector2Int coord)
    {
        return GetPiece(coord.x, coord.y);
    }
    public void PerformMove()
    {
        moveCount--;
        UIManager.ins.undoReqBtn.interactable = true;
        if(moveCount < 3 && Client.ins.side != currentSide) UIManager.ins.undoReqBtn.interactable = false;
        if (moveCount == 0)
        {
            Client.ins.ChangeSideLocal();
        }
        
    }
    public void CheckTurn()
    {
        if (Client.ins.side == currentSide)
        {
            turnIdentifier.text = "Your Turn";
            turnIdentifier.color = Color.green;
        }
        else
        {
            turnIdentifier.color = Color.red;
            turnIdentifier.text = "Opponent's Turn";
        }
    }
    public void Undo()
    {
        if(moveStack.Count > 0)
            moveStack.Pop().Undo();
    }
   
}
public class MovePattern
{
    public string type;
    public Vector2Int[] pattern;

    ChessPiece target;
    public MovePattern(string _type, Vector2Int[] _pattern)
    {
        type = _type;
        pattern = _pattern;
    }
    public MovePattern(string _type, Vector2Int from, Vector2Int to)
    {
        type = _type;
        pattern = new Vector2Int[] { from, to };
    }
    public void Perform()
    {
        if (type == "sp")
        {
            var from = BoardGenerator.GetPiece(pattern[0]);
            var to = BoardGenerator.GetPiece(pattern[1]);

            AnimationPlayer.LerpPosition(from.currentChessPiece.transform, to.transform.position, 0.25f);

            to.currentChessPiece = from.currentChessPiece;
            from.currentChessPiece = null;
        }
        if (type == "at")
        {
            var oldPiece = BoardGenerator.GetPiece(pattern[0]);

            if (oldPiece.currentChessPiece != null)
            {
                var selectedChessPiece = oldPiece.currentChessPiece;
                target = BoardGenerator.GetPiece(pattern[1]).currentChessPiece;
                selectedChessPiece.PerformAtk(pattern[1]);
            }
        }
        if(type == "pb")
        {
            var piece = BoardGenerator.GetPiece(pattern[0]);
            piece.SetBarrier(BoardGenerator.ins.currentSide);
            Client.ins.barrierCount--;
        }
        BoardGenerator.ins.moveStack.Push(this);
       
        BoardGenerator.ins.PerformMove();
    }
    public void Undo()
    {
        BoardGenerator.ins.moveCount++;
        if (BoardGenerator.ins.moveCount > 3)
        {
            Client.ins.ChangeSideLocal();
            BoardGenerator.ins.moveCount = 1;
        }
        if (type == "sp")
        {
            var from = BoardGenerator.GetPiece(pattern[1]);
            var to = BoardGenerator.GetPiece(pattern[0]);

            AnimationPlayer.LerpPosition(from.currentChessPiece.transform, to.transform.position, 0.25f);

            to.currentChessPiece = from.currentChessPiece;
            from.currentChessPiece = null;
        }
        if (type == "at")
        {
            var from = BoardGenerator.GetPiece(pattern[0]);
            var to = BoardGenerator.GetPiece(pattern[1]);

            target.gameObject.SetActive(true);
            target.hp += from.currentChessPiece.damage;
            AnimationPlayer.DamagePopup(to.transform.position, 1f, from.currentChessPiece.damage, "+");
            to.currentChessPiece = target;          
        }
        if (type == "pb")
        {
            var piece = BoardGenerator.GetPiece(pattern[0]);
            Client.ins.barrierCount++;
            piece.currentChessPiece.GetComponent<Barrier>().DestroyBarrier();
        }
    }
}
