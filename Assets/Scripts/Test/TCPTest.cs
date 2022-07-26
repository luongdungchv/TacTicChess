using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.UI;
using System.Net.NetworkInformation;

public class TCPTest : MonoBehaviour
{
    TcpClient client;
    TcpListener server;
    NetworkStream stream;
    byte[] receiveBuffer;
    int dataBufferSize;

    public TextMeshProUGUI logText;
    public Button sendBtn;
    
    private void Start()
    {
        dataBufferSize = 4096;
        receiveBuffer = new byte[dataBufferSize];
        sendBtn.onClick.AddListener(() =>
        {
            SendData("data");
        });
        Debug.Log(SystemInfo.deviceName);
        int[,] a = new int[3, 3];
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                a[i, j] = j;
            }
        }
        Data d = new Data(a);
        string json = JsonUtility.ToJson(d);
        Debug.Log(json);
        //NetworkChange.NetworkAvailabilityChanged
    }
    [Serializable]
    public class Data
    {
        public int[,] data;
        public Data(int[,] input)
        {
            data = input;
        }
    }
    public bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    

    public async void Client()
    {
        try
        {
            client = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            logText.text = "Connecting";
            await client.ConnectAsync("192.168.1.138", 26950);
            logText.text = $"Connect successfully: {client.Client.RemoteEndPoint}";
            stream = client.GetStream();
            sendBtn.gameObject.SetActive(true);
            ReadAsync();
        }
        catch(Exception e)
        {
            logText.text = e.ToString();
        }
    }
    public async void Server()
    {
        server = new TcpListener(IPAddress.Any,26950);
        server.Start();
        logText.text = "Server started on port: 26950";
        client = await server.AcceptTcpClientAsync();
        Debug.Log($"Client: {client.Client.RemoteEndPoint}");
        sendBtn.gameObject.SetActive(true);
        stream = client.GetStream();
        ReadAsync();
    }
    async void ReadAsync()
    {
        if (!IsConnectedToInternet())
        {
            logText.text = "Not Connected";
            ReadAsync();
            return;
        }
        int dataLength = await stream.ReadAsync(receiveBuffer, 0, dataBufferSize);
        if(dataLength == 0)
        {
            Debug.Log("No data");
            ReadAsync();
            return;
        }
        byte[] dataBytes = new byte[dataLength];
        Array.Copy(receiveBuffer, dataBytes, dataLength);
        string msg = Encoding.ASCII.GetString(dataBytes);
        logText.text = msg;
        ReadAsync();
    }
    public async void SendData(string msg)
    {
        if (!CheckConnection(client.Client))
        {
            logText.text = "Not Connected";
            return;
        } 
        Debug.Log(client.Connected);
        try
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(msg);
            await stream.WriteAsync(dataBytes, 0, dataBytes.Length);
        }
        catch(Exception e)
        {
            logText.text =e.ToString();
        }
    }
    public bool CheckConnection(Socket socket)
    {
        if(socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
        {
            return false;
        }
        return true;
    }
    public void ReloadScene()
    {
        try
        {
            client.Close();
            server.Stop();
        }
        catch { }
        SceneManager.LoadScene(0);
    }
}
