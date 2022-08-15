using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public static Slider ins;
    private void Start()
    {
        ins = this;
    }

    public void UpdateSlider(float t)
    {
        var scale = transform.localScale;
        scale.x = t;
        transform.localScale = scale;
    }
}
