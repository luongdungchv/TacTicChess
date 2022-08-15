using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using AxieMixer.Unity;

public class Player : MonoBehaviour
{
    public static Player ins;
    public static event EventHandler OnSideChange;


    [SerializeField]
    private int _barrierCount, _baseCount;

    public int barrierCount
    {
        get => _barrierCount;
        set
        {
            _barrierCount = value;
            PlayerPrefs.SetInt("Barrier Count", value);
        }
    }
    public int baseCount
    {
        get => _baseCount;
        set
        {
            _baseCount = value;
            PlayerPrefs.SetInt("Base Count", value);
        }
    }
    public int side;
    [SerializeField] private float turnDuration;

    private bool isConnected;

    Action pendingProcessing;
    private Coroutine changeSideRoutine;

    void Start()
    {
        Application.runInBackground = true;
        if (ins == null) ins = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        //receiveBuffer = new byte[dataBufferSize];
        var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        string myIP = addressList[addressList.Length - 1].ToString();
    }
    public void ChangeSideCountdown()
    {
        IEnumerator changeSideEnum()
        {
            yield return new WaitForSeconds(turnDuration);
            ChangeSideRequest();
        }
        if (changeSideRoutine != null) StopCoroutine(this.changeSideRoutine);
        this.changeSideRoutine = StartCoroutine(changeSideEnum());
    }



    public void SetPieceRequest(Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (Board.gameMode == "Single")
        {
            SetPieceLocal(oldCoord, newCoord);
            if (Board.ins.moveCount == 0)
            {
                ChangeSideLocal();
                AI.ins.MoveOptimally();
            }
            return;
        }
        pendingProcessing = () =>
        {
            SetPieceLocal(oldCoord, newCoord);
            pendingProcessing = () => { };
        };
        Vector2Int[] args = { oldCoord, newCoord };
        //var dataPack = new DataPacket("sp", args);
        var dataPack = new DataPack("sp", args);
        SendData(dataPack);
    }
    void SetPieceLocal(Vector2Int oldCoord, Vector2Int newCoord)
    {
        Move move = new SetPieceMove(oldCoord, newCoord);
        move.Perform();
    }


    public void AttackTargetRequest(Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (Board.gameMode == "Single")
        {
            AttackTargetLocal(oldCoord, newCoord);
            return;
        }
        pendingProcessing = () =>
        {
            AttackTargetLocal(oldCoord, newCoord);
            pendingProcessing = () => { };
        };
        Vector2Int[] args = { oldCoord, newCoord };
        //var dataPack = new DataPacket("at", args);
        var dataPack = new DataPack("at", args);
        SendData(dataPack);
    }
    void AttackTargetLocal(Vector2Int attackerCoord, Vector2Int targetCoord)
    {
        Move move = new AttackMove(attackerCoord, targetCoord);
        move.Perform();
    }
    public void ChangeSideLocal()
    {
        OnSideChange?.Invoke(this, EventArgs.Empty);
    }

    public void PlaceBarrierRequest(Vector2Int coord)
    {
        if (Board.gameMode == "Single")
        {
            PlaceBarrierLocal(coord, 0);
            return;
        }
        pendingProcessing = () =>
        {
            PlaceBarrierLocal(coord, side);
            pendingProcessing = () => { };
        };

        Vector2Int[] args = { coord, new Vector2Int(side, side) };
        //var dataPack = new DataPacket("pb", args);
        var dataPack = new DataPack("pb", args);
        SendData(dataPack);
    }
    void PlaceBarrierLocal(Vector2Int coord, int side)
    {
        Move move = new PlaceBarrierMove(coord);
        move.Perform();
    }

    public void UndoRequest()
    {
        pendingProcessing = () => { };
        //var packet = new DataPacket("ur", null);
        var packet = new DataPack("ur", null);
        SendData(packet);
    }
    public void AcceptUndoRequest()
    {
        pendingProcessing = () =>
        {
            Board.ins.Undo();
            pendingProcessing = () => { };
        };
        //var packet = new DataPacket("ua", null);
        var packet = new DataPack("ua", null);
        SendData(packet);
    }
    public void ChangeSideRequest()
    {
        pendingProcessing = () =>
        {
            ChangeSideLocal();
            pendingProcessing = () => { };
        };
        SendData(new DataPack("cs", null));
    }

    IEnumerator DenyUndoCountdown(float duration, Transform slider)
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / duration;
            slider.localScale = new Vector3(t, slider.localScale.y, slider.localScale.z);
            yield return null;
        }
        if (t <= 0) UIManager.ins.DenyUndoRequest();
    }
    public void DecreaseBases()
    {
        baseCount--;
        if (baseCount == 0)
        {
            if (Board.gameMode == "Single")
            {
                ShowResult(0);
                return;
            }
            pendingProcessing = () =>
            {
                ShowResult(0);
                pendingProcessing = () => { };
            };
            Vector2Int[] args = { new Vector2Int(side, side) };
            //var dataPack = new DataPacket("sr", args);
            var dataPack = new DataPack("sr", args);
            SendData(dataPack);
        }
    }
    public void ShowResult(int resultType)
    {
        if (resultType == 0) Board.ins.resultText.text = "NGU!! =)))";
        else Board.ins.resultText.text = "WINNER WINNER CHICKEN DINNER !!!";
        Board.ins.isEnd = true;
    }


    private void SendData(DataPack pack)
    {
        ClientManager.ins.client[0].SendData(pack);
    }
    private void SendData(string msg)
    {
        ClientManager.ins.client[0].SendData(msg);
    }
    private void OnApplicationQuit()
    {
        ClientManager.ins.client[0].Disconnect();
    }


    public void Notify(string msg)
    {
        var deserializedData = new DataPack(msg);

        if (deserializedData.cmd.Length > 2)
        {
            //var roomId = int.Parse(msg.Substring(msg.IndexOf(" ") + 1, msg.LastIndexOf(" ") - msg.IndexOf(" ") - 1));
            var playerId = int.Parse(msg.Substring(msg.LastIndexOf(" ") + 1));
            ClientManager.ins.client[0].isFindingMatch = false;
            side = playerId;
            PlayerPrefs.SetString("Current Room", msg);
            SceneManager.LoadScene("SampleScene");
            return;
        }
        if (msg.Length == 1)
        {
            side = int.Parse(msg);
            //BoardGenerator.ins.GeneratePieces(side);
            SceneManager.LoadScene("SampleScene");
            return;
        }

        if (msg == "pr")
        {
            //logText.text = msg;
            try
            {
                pendingProcessing();
            }
            catch
            {
                //logText.text = e.ToString();
            }
            return;
        }

        Debug.Log(msg);
        //var deserializedData = JsonUtility.FromJson<DataPacket>(msg);

        string command = deserializedData.cmd;
        var args = deserializedData.args == null ? null : deserializedData.args;
        switch (command)
        {
            case "sp":
                SetPieceLocal(args[0], args[1]);
                break;
            case "at":
                AttackTargetLocal(args[0], args[1]);
                break;
            case "pb":
                PlaceBarrierLocal(args[0], args[1].x);
                break;
            case "sr":
                if (side == args[0].x) ShowResult(0);
                else ShowResult(1);
                break;
            case "ur":
                UIManager.ins.incomingUndoRequestPanel.SetActive(true);
                StartCoroutine(DenyUndoCountdown(7, UIManager.ins.slider.transform));
                break;
            case "ua":
                Board.ins.Undo();
                UIManager.ins.undoReqBtn.interactable = true;
                break;
            case "cs":
                ChangeSideLocal();
                break;
            default:
                Debug.Log("Wrong Command");
                break;
        }
    }
    public static void ResetEvent()
    {
        OnSideChange = null;
    }


}
[Serializable]

public class DataPack
{
    public string cmd;
    public Vector2Int[] args;
    public string data;
    public DataPack(string data)
    {

        var split = data.Trim().Split();
        cmd = split[0];
        List<Vector2Int> argList = new List<Vector2Int>();
        for (int i = 1; i < split.Length; i += 2)
        {
            Debug.Log(split[i]);
            int x = int.Parse(split[i]);
            int y = int.Parse(split[i + 1]);
            Vector2Int arg = new Vector2Int(x, y);
            argList.Add(arg);
        }
        args = argList.ToArray();
        this.data = data;
    }
    public DataPack(string cmd, Vector2Int[] args)
    {
        this.cmd = cmd;
        this.args = args;

        if (args == null)
        {
            this.data = cmd;
            return;
        }

        string data = "" + cmd + " ";
        foreach (var i in args)
        {
            data += $"{i.x} {i.y} ";
        }

        this.data = data;
    }
}
