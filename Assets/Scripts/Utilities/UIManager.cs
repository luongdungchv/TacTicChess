using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    public GameObject incomingUndoRequestPanel, slider;
    public Button undoReqBtn;
    public Transform gameListContainer;
    public Button joinMatchBtnPrefab;
    public GameObject localMatchPanel;
    public Button stopHostBtn, startHostBtn, findMatchBtn, cancelBtn;
    //public TextMeshProUGUI logText;

    private void Start()
    {
        ins = this;
        if (undoReqBtn != null)
        {
            undoReqBtn.onClick.AddListener(() =>
            {
                Undo();
                undoReqBtn.interactable = false;
            });
            undoReqBtn.interactable = false;
        }

    }
    public void ClearRoomData()
    {
        PlayerPrefs.SetString("Current Room", "");
    }
    public void Multiplayer(TextMeshProUGUI caller)
    {
        PlayerPrefs.SetString("Current Room", "");
        Button btn = caller.GetComponentInParent<Button>();
        cancelBtn.gameObject.SetActive(true);
        ClientManager.ins.client[0].ConnectToServer((e) =>
        {
            Debug.Log("Cannot connect to server");
            btn.interactable = true;
            caller.text = "Multiplayer";
            cancelBtn.gameObject.SetActive(false);
        });
        btn.interactable = false;
        caller.text = "Finding Match";
    }
    public void Disconnect()
    {
        ClientManager.ins.client[0].Disconnect();
    }
    public void SinglePlayer()
    {
        Board.gameMode = "Single";
        Board.ins.GeneratePieces(0);
    }
    public void Undo()
    {
        Player.ins.UndoRequest();
    }
    public void AcceptUndoRequest()
    {
        Player.ins.AcceptUndoRequest();
        incomingUndoRequestPanel.SetActive(false);
    }
    public void DenyUndoRequest()
    {
        incomingUndoRequestPanel.SetActive(false);
    }
    public void StartLocalHost(TextMeshProUGUI caller)
    {
        stopHostBtn.gameObject.SetActive(true);
        LocalServer.ins.StartHost();
        var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        string myIP = addressList[addressList.Length - 1].ToString();
        ConnectToLocalHost(caller, myIP);
    }
    public void StopLocalHost()
    {
        LocalServer.ins.StopHost();
        stopHostBtn.gameObject.SetActive(false);
        startHostBtn.interactable = true;
        findMatchBtn.interactable = true;
    }
    public void ConnectToLocalHost(TextMeshProUGUI caller, string ip)
    {
        caller.GetComponentInParent<Button>().interactable = false;
        findMatchBtn.interactable = false;
        (ClientManager.ins.client[0] as Client).serverIp = ip;
        //logText.text = ip;
        ClientManager.ins.client[0].ConnectToServer((e) =>
        {
            StopLocalHost();
            //logText.text = e.ToString();
            caller.GetComponentInParent<Button>().interactable = true;
        });
    }
    public void SwitchLocalMatchPanelState()
    {
        localMatchPanel.SetActive(!localMatchPanel.activeSelf);
    }
    public async void FindLocalMatch()
    {
        using (UdpClient broadcaster = new UdpClient(AddressFamily.InterNetwork))
        {
            try
            {
                broadcaster.EnableBroadcast = true;
                var address = IPAddress.Parse("224.100.0.1");
                broadcaster.JoinMulticastGroup(address);
                var multicastEP = new IPEndPoint(address, 8888);
                var broadcastEP = new IPEndPoint(IPAddress.Broadcast, 8888);

                var datagram = Encoding.ASCII.GetBytes("fm");

                await broadcaster.SendAsync(datagram, datagram.Length, multicastEP);
                var result = await broadcaster.ReceiveAsync();
                var msg = Encoding.ASCII.GetString(result.Buffer);
                Debug.Log(msg);
                AddNewGameToList(msg);
            }
            catch (Exception e)
            {
                //slogText.text = e.ToString();
            }
        }

    }

    public void AddNewGameToList(string ip)
    {
        while (gameListContainer.childCount > 0)
        {
            Destroy(gameListContainer.GetChild(gameListContainer.childCount));

        }
        var joinMatchBtn = Instantiate(joinMatchBtnPrefab);
        joinMatchBtn.transform.SetParent(gameListContainer, false);
        var text = joinMatchBtn.GetComponentInChildren<TextMeshProUGUI>();
        text.text = ip;
        joinMatchBtn.onClick.AddListener(() =>
        {
            joinMatchBtn.interactable = false;
            ConnectToLocalHost(text, ip);
        });
    }
    public void CancelFindMatch(Button multiplayerBtn)
    {
        var currentClient = ClientManager.ins.client[0];
        if (currentClient.isFindingMatch)
        {
            currentClient.Disconnect();
            currentClient.isFindingMatch = false;
            cancelBtn.gameObject.SetActive(false);
            multiplayerBtn.interactable = true;
            multiplayerBtn.GetComponentInChildren<TextMeshProUGUI>().text = "MultiPlayer";
        }
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
