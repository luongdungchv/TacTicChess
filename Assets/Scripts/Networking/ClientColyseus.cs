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
        //base.ConnectToServer(connectFailCallback, connectSuccessCallback);
        manager.SetPlayer(player);
        manager.SetClient(this);
        manager.InitializeClient(connectSuccessCallback, connectFailCallback);
    }
    protected override void Start()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
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
        if (isConnected)
        {
            manager.Disconnect();
            isConnected = false;
        }
    }
    private void HandleMessage(string msg)
    {
        player.Notify(msg);
    }
}
