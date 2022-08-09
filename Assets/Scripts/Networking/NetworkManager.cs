using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Test;
using Colyseus;
using System;
public class NetworkManager : ColyseusManager<NetworkManager>
{
    public static NetworkManager ins;
    private ColyseusRoom<dynamic> currentRoom;
    private Player player;
    public async override void InitializeClient()
    {
        base.InitializeClient();
        //client = new ColyseusClient("ws://localhost:2567");
        //var room1 = await client.JoinOrCreate("custom_lobby");
        currentRoom = await client.JoinOrCreate("match");

        currentRoom.OnMessage<string>("pair", async (e) =>
        {
            Debug.Log(e);
            var reservation = JsonUtility.FromJson<ColyseusMatchMakeResponse>(e);
            await currentRoom.Leave();
            currentRoom = await client.ConsumeSeatReservation<dynamic>(reservation);
            currentRoom.OnMessage<string>("message", (s) =>
            {
                Debug.Log(e);
                player.Notify(s);
            });
            Debug.Log(currentRoom.Name);
        });

        Debug.Log(currentRoom);
        //await room.Send("message", "sdafsad");
        //client.
    }
    public async void SendMsg(string msg)
    {
        if (currentRoom == null) return;
        await currentRoom.Send("message", msg);
    }
    public void SetPlayer(Player input)
    {
        this.player = input;
    }
    public async void Disconnect()
    {
        await currentRoom.Leave();
    }
    protected override void Start()
    {
        if (ins == null) ins = this;
        //this.InitializeClient();
        //Debug.Log(client.Settings.WebSocketEndpoint);
        DontDestroyOnLoad(this);
    }
    [System.Serializable]
    class Message
    {
        public string kind;
    }
}
