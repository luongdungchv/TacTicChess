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

public class Client : MonoBehaviour
{
    public static Client ins;
    public static event EventHandler OnSideChange;

    public string serverIp;
    public int port;

    public int _barrierCount;
    public int _baseCount;

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
    [SerializeField]
    private int dataBufferSize;
    private TcpClient tcpSocket;

    private bool isConnected;

    Action pendingProcessing;

    NetworkStream tcpStream;

    byte[] receiveBuffer;



    void Start()
    {
        Application.runInBackground = true;
        if (ins != null) Destroy(ins.gameObject);
        ins = this;
        DontDestroyOnLoad(this);
        receiveBuffer = new byte[dataBufferSize];
        var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        string myIP = addressList[addressList.Length - 1].ToString();
        Debug.Log(Dns.GetHostEntry("https://www.google.com").AddressList[0]);
    }
    public async void ConnectToServer(Action ConnectFailCallback, Action ConnectSucceededCallback)
    {
        tcpSocket = new TcpClient
        {
            ReceiveBufferSize = dataBufferSize,
            SendBufferSize = dataBufferSize
        };
        try
        {
            await tcpSocket.ConnectAsync(serverIp, port);
            tcpStream = tcpSocket.GetStream();
            string currentRoom = PlayerPrefs.GetString("Current Room", "");
            if (currentRoom != "")
            {
                var dataBytes = Encoding.ASCII.GetBytes(currentRoom);
                await tcpStream.WriteAsync(dataBytes, 0, dataBytes.Length);
            }
            else
            {
                var dataBytes = Encoding.ASCII.GetBytes($"{string.Concat(SystemInfo.deviceName.Where(c => !Char.IsWhiteSpace(c)))} -1 -1");
                await tcpStream.WriteAsync(dataBytes, 0, dataBytes.Length);
            }
            ConnectSucceededCallback();
            isConnected = true;
            ReadDataAsync();
        }
        catch (Exception e)
        {
            ConnectFailCallback();
            //BoardGenerator.ins.logText.text = e.ToString();
            return;
        }

    }
    public void ConnectToServer()
    {
        ConnectToServer(() => { }, () => { });
    }
    public void ConnectToServer(Action ConnectFailCallback)
    {
        ConnectToServer(ConnectFailCallback, () => { });
    }
    public async void ReadDataAsync()
    {
        int dataLength;
        try
        {
            dataLength = await tcpStream.ReadAsync(receiveBuffer, 0, dataBufferSize);
        }
        catch
        {
            AttemptToReconnect();
            return;
        }

        if (dataLength <= 0)
        {
            Disconnect();
            return;
        }
        byte[] data = new byte[dataLength];
        Array.Copy(receiveBuffer, data, dataLength);

        string msg = Encoding.ASCII.GetString(data);
        Debug.Log(msg);
        var deserializedData = new DataPack(msg);

        if (deserializedData.cmd.Length > 2)
        {
            //var roomId = int.Parse(msg.Substring(msg.IndexOf(" ") + 1, msg.LastIndexOf(" ") - msg.IndexOf(" ") - 1));
            var playerId = int.Parse(msg.Substring(msg.LastIndexOf(" ") + 1));
            side = playerId;
            PlayerPrefs.SetString("Current Room", msg);
            SceneManager.LoadScene("SampleScene");
            ReadDataAsync();
            return;
        }
        if (msg.Length == 1)
        {
            side = int.Parse(msg);
            //BoardGenerator.ins.GeneratePieces(side);
            SceneManager.LoadScene("SampleScene");
            Debug.Log(tcpStream);
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
            ReadDataAsync();
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

            default:
                Debug.Log("Wrong Command");
                break;
        }
        ReadDataAsync();
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

    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            Debug.Log("disconnect");
            tcpSocket.Close();
            tcpSocket = null;
            LocalServer.ins.StopHost();
        }
        PlayerPrefs.SetString("Current Room", "");
        OnSideChange = null;
        BoardPiece.ResetEvent();
        SceneManager.LoadScene("Main Menu");
    }
    public void AttemptToReconnect()
    {
        if (!isConnected) return;
        Debug.Log("dfadsf");
        IEnumerator ReconnectCoroutine(float duration)
        {
            float t = 0;
            Board.ins.attemptReconnectPanel.SetActive(true);
            while (t < duration)
            {
                t += Time.deltaTime;
                if (IsConnectedToInternet())
                {
                    StartCoroutine(ReconnectDelay());
                    break;
                }
                yield return null;
            }
            if (t >= duration) Disconnect();
        }
        IEnumerator ReconnectDelay()
        {
            yield return new WaitForSeconds(1);
            ConnectToServer(() =>
            {
                Board.ins.attemptReconnectPanel.SetActive(false);
                Disconnect();
            }, () => Board.ins.attemptReconnectPanel.SetActive(false));
        }
        StartCoroutine(ReconnectCoroutine(7));
    }
    public async void SendData(DataPack pack)
    {
        if (!IsConnectedToInternet())
        {
            Debug.Log("Not Connected");
            return;
        }
        Debug.Log(tcpSocket);
        string msg = pack.data;
        byte[] databytes = Encoding.ASCII.GetBytes(msg);
        await tcpStream.WriteAsync(databytes, 0, databytes.Length);
    }
    public async void SendData(string msg)
    {
        if (!IsConnectedToInternet())
        {
            Debug.Log("Not Connected");
            return;
        }
        var dataBytes = Encoding.ASCII.GetBytes(msg);
        await tcpStream.WriteAsync(dataBytes, 0, dataBytes.Length);
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    public static bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    public bool CheckSocketConnection()
    {
        if (tcpSocket.Client.Poll(1000, SelectMode.SelectRead) && tcpSocket.Available == 0)
        {
            return false;
        }
        return true;
    }


}
[Serializable]
public class DataPacket
{
    public string cmd;
    public Vector2Int[] args;
    public DataPacket(string cmd, Vector2Int[] args)
    {
        this.cmd = cmd;
        this.args = args;
    }
}
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
