using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;

public class MatchmakingServer : MonoBehaviour
{
    // Start is called before the first frame update
    public TcpListener server;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void StartLocalHost()
    {
        server = new TcpListener(IPAddress.Any, 26950);
        server.Start();
    }
}
