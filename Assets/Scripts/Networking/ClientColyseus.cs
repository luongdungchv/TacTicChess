using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using System.Text;

public class ClientColyseus : ClientBase
{
    [SerializeField] private NetworkManager manager;
    public override void ConnectToServer(Action<Exception> connectFailCallback, Action connectSuccessCallback)
    {
        base.ConnectToServer(connectFailCallback, connectSuccessCallback);
        manager.SetPlayer(player);
        manager.InitializeClient();
    }
    protected override void Start()
    {
        base.Start();
    }
    public override void SendData(DataPack pack)
    {
        if (!IsConnectedToInternet())
        {
            Debug.Log("Not Connected");
            return;
        }
        string msg = pack.data;
        manager.SendMsg(msg);

    }
    public override void SendData(string msg)
    {
        if (!IsConnectedToInternet())
        {
            Debug.Log("Not Connected");
            return;
        }
        manager.SendMsg(msg);
    }
    public override void Disconnect()
    {
        manager.Disconnect();
    }
    private void HandleMessage(string msg)
    {
        player.Notify(msg);
    }
}
