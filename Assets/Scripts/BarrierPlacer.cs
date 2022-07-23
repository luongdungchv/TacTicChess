using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BarrierPlacer : MonoBehaviour
{
    private static bool isInPlacingModeRoot;
    public static BarrierPlacer ins;
    public Button barrierSelector;
    public static bool isInPlacingMode {
        get => isInPlacingModeRoot;
        set{
            isInPlacingModeRoot = value;
            if (isInPlacingModeRoot) ins.barrierSelector.GetComponent<Image>().color = Color.yellow;
            else ins.barrierSelector.GetComponentInChildren<Image>().color = Color.white;
        }
    }
    public static bool isInMovingMode;
    public static BoardPiece selectedPiece;
    // Start is called before the first frame update
    void Start()
    {
        ins = this;
        barrierSelector = GetComponent<Button>();
        
        
        barrierSelector.onClick.AddListener(() =>
        {
            if (!Client.IsConnectedToInternet())
            {
                return;
            }
            isInPlacingMode = !isInPlacingMode;
        });
    }

    public static void EnableMovingMode(Vector2Int coord)
    {
        isInMovingMode = true;
        selectedPiece = BoardGenerator.GetPiece(coord);
    }
    public static void DisableMovingMode()
    {
        isInMovingMode = false;
        selectedPiece = null;
    }
}
