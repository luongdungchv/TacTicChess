using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public static AI ins;
    public Dictionary<ChessPiece, Vector2Int> AIpieces;
    public Dictionary<ChessPiece, Vector2Int> playerPieces;

    

    public int baseCount;
    private void Start()
    {
        ins = this;
        AIpieces = new Dictionary<ChessPiece, Vector2Int>();
        playerPieces = new Dictionary<ChessPiece, Vector2Int>();
        //Dictionary<int, int> dict = new Dictionary<int, int>();
        //dict.Add(2, 4);
        //dict.Add(3, 5);
        //foreach(var i in dict.Keys)
        //{
        //    dict[i] = 6;
        //}
        //foreach(var i in dict.Keys)
        //{
        //    Debug.Log(dict[i]);
        //}
    }

    public void MoveOptimally()
    {
        StartCoroutine(MoveOptimallyEnum());
    }
    IEnumerator MoveOptimallyEnum()
    {
        for (int i = 0; i < 3; i++)
        {
            FindOptimalMove(AIpieces, playerPieces, null, null, 0, 1).Perform();
            if (i == 3)
            {
                BoardGenerator.ins.moveCount = 3;
                Client.ins.ChangeSideLocal();
            }
            yield return new WaitForSeconds(1);
        }
    }

    MovePattern FindOptimalMove(Dictionary<ChessPiece, Vector2Int> _aiPieces, Dictionary<ChessPiece, Vector2Int> _playerPieces,Vector2Int[] atkPattern,Vector2Int[] translatePattern, int count, int side)
    {
        return null;
    }
    
    public void DecreaseBases()
    {
        baseCount--;
        if (baseCount == 0) Client.ins.ShowResult(1);
    }
    public class MovePattern
    {
        public virtual int Evaluate()
        {
            return 0;
        }
        public virtual void Perform()
        {

        }
    }
    
}
