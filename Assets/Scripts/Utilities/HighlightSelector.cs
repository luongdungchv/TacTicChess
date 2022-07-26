using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HighlightSelector : MonoBehaviour
{
    public static HighlightSelector ins;
    public Button moveBtn, atkBtn;
    private void Start()
    {
        ins = this;
        moveBtn.interactable = false;
        atkBtn.interactable = false;
        BoardPiece.OnBoardReset += (s, e) =>
        {
            moveBtn.onClick.RemoveAllListeners();
            atkBtn.onClick.RemoveAllListeners();
        };
    }
    public void Show(BoardPiece selectedPiece)
    {
        Debug.Log(selectedPiece);
        moveBtn.interactable = true;
        atkBtn.interactable = true;
        moveBtn.onClick.AddListener(() => 
        {           
            selectedPiece.HighlightMove();            
            moveBtn.interactable = false;
            atkBtn.interactable = false;
            moveBtn.onClick.RemoveAllListeners();
        });
        atkBtn.onClick.AddListener(() => 
        { 
            selectedPiece.HighlightAtk(); 
            atkBtn.interactable = false;
            moveBtn.interactable = false;
            atkBtn.onClick.RemoveAllListeners();
        });
    }
}
