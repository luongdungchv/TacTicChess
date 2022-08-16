using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Test;
using Colyseus;
using System;
using UnityEngine.SceneManagement;
public class NetworkManager : ColyseusManager<NetworkManager>
{
    public static NetworkManager ins;
    private ColyseusRoom<dynamic> currentRoom;
    private Player player;
    private ClientColyseus playerClient;
    public async void InitializeClient(Action successCallback, Action<Exception> failCallback)
    {
        base.InitializeClient();
        playerClient.isFindingMatch = true;
        //client = new ColyseusClient("ws://localhost:2567");
        //var room1 = await client.JoinOrCreate("custom_lobby");
        try
        {
            currentRoom = await client.JoinOrCreate("match");
            await currentRoom.Send("name", "lonewolf");
            playerClient.isConnected = true;
            successCallback();
        }
        catch (Exception e)
        {
            Debug.Log("Cannot connect");
            failCallback(e);
            playerClient.isFindingMatch = false;
            return;
        }

        currentRoom.OnMessage<string>("pair", async (e) =>
        {
            Debug.Log(e);
            var reservation = JsonUtility.FromJson<ColyseusMatchMakeResponse>(e);
            await currentRoom.Leave();
            currentRoom = await client.ConsumeSeatReservation<dynamic>(reservation);
            currentRoom.OnMessage<string>("message", (s) =>
            {
                Debug.Log(s);
                player.Notify(s);
            });
            currentRoom.OnLeave += (i) =>
            {
                Debug.Log("player left");
                SceneManager.LoadScene("Main Menu");
            };

            currentRoom.OnMessage<string>("leave", (s) =>
             {
                 Debug.Log("Game Abolished");
                 Disconnect();
                 //SceneManager.LoadScene("Main Menu");
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
    public void SetClient(ClientColyseus input)
    {
        this.playerClient = input;
    }
    public async void Disconnect()
    {
        await currentRoom.Leave();
    }
    protected override void Start()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
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
