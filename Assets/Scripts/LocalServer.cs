using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class LocalServer : MonoBehaviour
{
    public static LocalServer ins;
    public TcpListener server;
    public UdpClient udpListener;
    List<PlayerClient> clientList;
    public bool isHosting;

    private void Start()
    {
        if (ins != null) Destroy(ins.gameObject);
        ins = this;
        DontDestroyOnLoad(this);
    }
    public void StartHost(int port)
    {
        server = new TcpListener(IPAddress.Any, port);
        clientList = new List<PlayerClient>();
        server.Start();
        Debug.Log("Server started on port " + port.ToString());
        server.BeginAcceptTcpClient(ClientConnectCallback, null);

        udpListener = new UdpClient(8888, AddressFamily.InterNetwork);
        udpListener.EnableBroadcast = true;
        var address = IPAddress.Parse("224.100.0.1");
        udpListener.JoinMulticastGroup(address);

        UdpReceiveAsync();
        isHosting = true;
    }
    public void StopHost()
    {
        if (isHosting)
        {
            clientList.ForEach(n => n.Disconnect());
            server.Stop();
            udpListener.Close();
            isHosting = false;
        }
    }
    
    async void UdpReceiveAsync()
    {
        try
        {
            var receivedData = await udpListener.ReceiveAsync();
            var msg = Encoding.ASCII.GetString(receivedData.Buffer);
            if (msg == "fm")
            {
                var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                string myIP = addressList[addressList.Length - 1].ToString();

                var datagram = Encoding.ASCII.GetBytes(myIP);

                await udpListener.SendAsync(datagram, datagram.Length, receivedData.RemoteEndPoint);
            }
            if (clientList.Count != 2) UdpReceiveAsync();
        }
        catch
        {
            Debug.Log("Socket Discoonected");
        }
    }
    async void ClientConnectCallback(IAsyncResult result)
    {
        var incomingConnection = server.EndAcceptTcpClient(result);
        Debug.Log($"Incoming connection from {incomingConnection.Client.RemoteEndPoint}");
        
        server.BeginAcceptTcpClient(ClientConnectCallback, 0);
        PlayerClient client = new PlayerClient(incomingConnection);

        var buffer = new byte[4096];
        int dataLength = await incomingConnection.GetStream().ReadAsync(buffer, 0, 4096);
        var data = new byte[dataLength];
        Array.Copy(buffer, data, dataLength);
        var msg = Encoding.ASCII.GetString(data);
        try
        {
            client.name = msg.Substring(0, msg.IndexOf(" "));

            var roomId = int.Parse(msg.Substring(msg.IndexOf(" ") + 1, msg.LastIndexOf(" ") - msg.IndexOf(" ") - 1));
            if (roomId != -1)
            {
                var playerId = int.Parse(msg.Substring(msg.LastIndexOf(" ") + 1));
                if (clientList[playerId].name == client.name)
                {
                    SetDisconnectedPlayer(playerId, client);
                    return;
                }

            }
            if (clientList.Count == 2) return;
        }
        catch
        {

        }
        clientList.Add(client);
        Debug.Log(clientList.Count);
        if(clientList.Count == 2)
        {
            clientList[0].BeginCommunicate(clientList[1]);
            clientList[1].BeginCommunicate(clientList[0]);

            clientList[0].SendData($"{clientList[0].name} 0 0");
            clientList[1].SendData($"{clientList[1].name} 0 1");
        }
    }
    private void OnApplicationQuit()
    {
        StopHost();        
    }
    void SetDisconnectedPlayer(int id, PlayerClient sub)
    {
        clientList[id] = sub;
        clientList[id == 0 ? 1 : 0].partner = clientList[id];
        clientList[id].BeginCommunicate(clientList[id == 0 ? 1 : 0]);
    }
    public class PlayerClient
    {
        public TcpClient tcpSocket;
        public string name;
        public PlayerClient partner;
        NetworkStream stream;
        byte[] receiveBuffer;
        int dataBufferSize = 4096;

        public PlayerClient(TcpClient socket)
        {
            tcpSocket = socket;
            stream = socket.GetStream();
            receiveBuffer = new byte[dataBufferSize];
        }
        public void BeginCommunicate(PlayerClient _partner)
        {
            partner = _partner;
            try
            {
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
                partner.Disconnect();
                //partner.SendData("disconnect");
            }

        }
        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int dataLength = stream.EndRead(result);
                if (dataLength <= 0)
                {
                    Disconnect();
                    partner.Disconnect();
                    return;
                }
                byte[] dataByte = new byte[dataLength];
                Array.Copy(receiveBuffer, dataByte, dataLength);
                string msg = Encoding.ASCII.GetString(dataByte);
                SendData("pr");
                partner.SendData(msg);
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
                partner.Disconnect();
            }
        }
        public async void SendData(string msg)
        {
            byte[] dataByte = Encoding.ASCII.GetBytes(msg);
            try { await stream.WriteAsync(dataByte, 0, dataByte.Length); }
            catch { }
            
        }
        public void Disconnect()
        {
            try
            {
                tcpSocket.Client.Disconnect(false);
                tcpSocket.Close();
                
                Debug.Log("Server Disconnected");
            }
            catch
            {
                Debug.Log("Server Disconnected");
            }
        }
    }
}
