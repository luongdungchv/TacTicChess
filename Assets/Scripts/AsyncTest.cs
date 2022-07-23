using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;

public class AsyncTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Test();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    async void Test()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        Debug.Log("Done!");
        Test();
    }
}
