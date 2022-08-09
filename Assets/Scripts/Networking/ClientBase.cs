using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientBase : MonoBehaviour
{
    public static ClientBase ins;
    [SerializeField] protected Player player;
    // Start is called before the first frame update
    protected virtual void Start()
    {

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public virtual void ConnectToServer(Action<Exception> connectFailCallback, Action connectSuccessCallback)
    {

    }
    public void ConnectToServer()
    {
        ConnectToServer((e) => { Debug.Log("Connect Fail!"); }, () => { Debug.Log("Connect Success"); });
    }
    public void ConnectToServer(Action<Exception> connectFailCallback)
    {
        ConnectToServer(connectFailCallback, () => { Debug.Log("Connect Success"); });
    }
    public static bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    public virtual void Disconnect()
    {

    }
    public virtual void SendData(DataPack data)
    {

    }
    public virtual void SendData(string msg)
    {

    }
}
