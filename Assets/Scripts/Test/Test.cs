using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    UdpClient udpServer;
    UdpClient udp;
    public TextMeshProUGUI receiveLogText, sendLogText;
    TextMeshPro t;
    public List<Toggle> castTypes;
    public Toggle isIPv6;
    private void Start()
    {
        Application.runInBackground = true;
        var ipList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        foreach(var i in ipList)
        {
            Debug.Log(i.ToString());
        }
        
    }
    async void ReceiveAsync()
    {
        var receivedData = await udpServer.ReceiveAsync();
        var msg = Encoding.ASCII.GetString(receivedData.Buffer);
        string logText = $"{receivedData.RemoteEndPoint}: {msg}";
        Debug.Log(logText);
        receiveLogText.text = logText;
        var newmsg = Encoding.ASCII.GetBytes("RECEIVE");
        var multicastAddr = IPAddress.Parse("224.100.0.1");
        var multicastEP = new IPEndPoint(multicastAddr, 8888);
        await udpServer.SendAsync(newmsg, newmsg.Length, receivedData.RemoteEndPoint);
        ReceiveAsync();
    }
    public async void Send()
    {
        var broadcastEP = new IPEndPoint(IPAddress.Broadcast, 8888);
        var multicastAddr = IPAddress.Parse("224.100.0.1");
        var multicastEP = new IPEndPoint(multicastAddr, 8888);
        var uniEP = new IPEndPoint(IPAddress.Parse("192.168.1.142"), 8888);

        udp = new UdpClient(AddressFamily.InterNetwork);
        if (isIPv6.isOn)
        {
            udp = new UdpClient(AddressFamily.InterNetworkV6);
            multicastAddr = IPAddress.Parse("FF01::1");
            multicastEP = new IPEndPoint(multicastAddr, 8888);
            uniEP = new IPEndPoint(IPAddress.Parse("2402:800:6131:362d::2"), 8888);
        }

        var selectedCast = new IPEndPoint(IPAddress.Any, 0);
        foreach (var i in castTypes)
        {
            if (i.isOn)
            {
                int index = castTypes.IndexOf(i);
                if (index == 0) selectedCast = broadcastEP;
                else if (index == 1) selectedCast = multicastEP;
                else selectedCast = uniEP;
                Debug.Log(selectedCast);
                break;
            }
        }

        udp.EnableBroadcast = true;
        udp.JoinMulticastGroup(multicastAddr);

        var msg = Encoding.ASCII.GetBytes("SEND");
        int sent = await udp.SendAsync(msg, msg.Length, selectedCast);
        Debug.Log(udp.Client.LocalEndPoint);
        sendLogText.text = $"Send Completed:{selectedCast}, {sent}";
        var data = await udp.ReceiveAsync();
        receiveLogText.text = Encoding.ASCII.GetString(data.Buffer);
        

    }
    public void Receive()
    {
        var multicastAddr = IPAddress.Parse("224.100.0.1");
        if (isIPv6.isOn)
        {
            udpServer = new UdpClient(8888, AddressFamily.InterNetworkV6);
            multicastAddr = IPAddress.Parse("FF01::1");
        }
        else udpServer = new UdpClient(8888);

        Debug.Log(udpServer.Client.LocalEndPoint);
        udpServer.EnableBroadcast = true;
        
        udpServer.JoinMulticastGroup(multicastAddr);
        receiveLogText.text = "Receiving";
        ReceiveAsync();
    }
    public void ReloadScene()
    {
        if (udpServer != null) udpServer.Close();
        SceneManager.LoadScene(0);
    }
}
