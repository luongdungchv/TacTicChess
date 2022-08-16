using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatHandler : MonoBehaviour
{
    public static ChatHandler ins;
    public static bool isChatOpen;
    [SerializeField] private GameObject chatContainer, scrollView;
    [SerializeField] private TextMeshProUGUI chatTextPrefab, incomingChatText;
    [SerializeField] private int incomingChatDuration;
    [SerializeField] private TMP_InputField chatInput;
    private Coroutine showIncomingChatRoutine;


    // Start is called before the first frame update
    private void Start()
    {
        ins = this;
        isChatOpen = false;
    }
    public void AddChatText(string text)
    {
        var textClone = Instantiate(chatTextPrefab);
        var displayText = text.Substring(3);
        textClone.text = displayText;
        textClone.transform.SetParent(chatContainer.transform, false);

    }
    public void ShowIncomingChat(string text)
    {
        IEnumerator ShowIncomingChatEnum(string msg)
        {
            incomingChatText.text = msg;
            incomingChatText.transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(incomingChatDuration);
            incomingChatText.transform.parent.gameObject.SetActive(false);
        }
        if (isChatOpen) return;
        if (showIncomingChatRoutine != null)
        {
            StopCoroutine(showIncomingChatRoutine);
        }
        showIncomingChatRoutine = StartCoroutine(ShowIncomingChatEnum(text.Substring(3)));
    }

    public void ToggleChatWindow()
    {
        scrollView.SetActive(!scrollView.activeSelf);
        isChatOpen = scrollView.activeSelf;
        if (showIncomingChatRoutine != null) StopCoroutine(showIncomingChatRoutine);
        incomingChatText.transform.parent.gameObject.SetActive(false);
    }
    public void SendChatData()
    {
        //ClientManager.ins.client[0].SendData("ct " + chatInput.text.Trim());
        var username = PlayerPrefs.GetString("username", "lonewolf");
        Player.ins.SendChatRequest($"ct {username}: {chatInput.text.Trim()}");
        chatInput.text = "";
    }
}