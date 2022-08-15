using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public static ClientManager ins;
    [SerializeField]
    private List<ClientBase> clientList;
    [SerializeField]
    private int index;
    public List<ClientBase> client { get => clientList; }
    // Start is called before the first frame update
    void Start()
    {
        if (ins == null) ins = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
