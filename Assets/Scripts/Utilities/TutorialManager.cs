using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DisplayItem[] items;
    [SerializeField] private RawImage displayImage;

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private GameObject window;
    [SerializeField] private Button nextBtn, prevBtn;

    private int index = 0;
    private void Start()
    {
        ShowItem();
    }

    public void NextItem()
    {
        if (index + 1 >= items.Length)
        {
            nextBtn.interactable = false;
            return;
        }
        Debug.Log(index);
        index++;
        ShowItem();
    }
    public void PrevItem()
    {
        if (index <= 0) { prevBtn.interactable = false; return; };
        index--;
        ShowItem();
    }
    public void ShowItem()
    {
        if (index + 1 < items.Length)
        {
            nextBtn.interactable = true;
        }
        if (index > 0) { prevBtn.interactable = true; };

        displayImage.texture = items[index].image;
        displayText.text = items[index].text;
    }
    public void ToggleTutorialWindow()
    {
        window.SetActive(!window.activeSelf);
    }


    [System.Serializable]
    class DisplayItem
    {
        public Texture2D image;
        [TextArea(1, 10)]
        public string text;
    }
}
