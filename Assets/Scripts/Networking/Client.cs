﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class Client : MonoBehaviour
{
    public static Client ins;
    [SerializeField] private Player player;
    public string serverIp;
    public int port;
    [SerializeField]
    private int dataBufferSize = 4096;
    private TcpClient tcpSocket;

    private bool isConnected;
    private NetworkStream tcpStream;

    private byte[] receiveBuffer;

    void Start()
    {
        if (ins == null)
        {
            ins = this;
        }
        else
        {
            Destroy(ins.gameObject);
        }
        DontDestroyOnLoad(this);
        receiveBuffer = new byte[dataBufferSize];
    }
    public async void ConnectToServer(Action<Exception> ConnectFailCallback, Action ConnectSucceededCallback)
    {
        Debug.Log(serverIp);
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
            ConnectFailCallback(e);
            //BoardGenerator.ins.logText.text = e.ToString();
            return;
        }

    }
    public void ConnectToServer()
    {
        ConnectToServer((e) => { }, () => { });
    }
    public void ConnectToServer(Action<Exception> ConnectFailCallback)
    {
        ConnectToServer(ConnectFailCallback, () => { Debug.Log("Connect sucess"); });
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
        player.Notify(msg);
        ReadDataAsync();

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
            ConnectToServer((e) =>
            {
                Board.ins.attemptReconnectPanel.SetActive(false);
                Disconnect();
            }, () => Board.ins.attemptReconnectPanel.SetActive(false));
        }
        StartCoroutine(ReconnectCoroutine(7));
    }
    public static bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
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
        Player.ResetEvent();
        BoardPiece.ResetEvent();
        SceneManager.LoadScene("Main Menu");
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
    public bool CheckSocketConnection()
    {
        if (tcpSocket.Client.Poll(1000, SelectMode.SelectRead) && tcpSocket.Available == 0)
        {
            return false;
        }
        return true;
    }

}
