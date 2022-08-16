using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UsernameEnter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject container;
    [SerializeField] private Button multiPlayerBtn, localMatchBtn, editUsernameBtn;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("username", "") == "")
        {
            OpenUsernameInput();
        }
    }

    public void SubmitUsername()
    {
        PlayerPrefs.SetString("username", inputField.text);
        container.gameObject.SetActive(false);
        multiPlayerBtn.interactable = true;
        localMatchBtn.interactable = true;
        editUsernameBtn.interactable = true;
    }
    public void OpenUsernameInput()
    {
        var username = PlayerPrefs.GetString("username", "lonewolf");
        inputField.text = username;
        container.gameObject.SetActive(true);
        multiPlayerBtn.interactable = false;
        localMatchBtn.interactable = false;
        editUsernameBtn.interactable = false;
    }
}
