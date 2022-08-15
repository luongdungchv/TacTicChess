using System.Collections;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;


public class FigureManager : MonoBehaviour
{
    public static FigureManager ins;
    bool _isPlaying = false;
    bool _isFetchingGenes = false;

    public string shielderGenes, assaultGenes, sniperGenes, baseGenes;
    public string[] ids = { "2567", "2323", "1000", "1367", "2765", "1002", "2312", "1111" };
    private string[] genes = new string[8];
    // Start is called before the first frame update
    async void Start()
    {
        //Time.timeScale = 0f;
        if (ins == null) ins = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        Mixer.Init();

        // shielderGenes = PlayerPrefs.GetString("shielder", "");
        // assaultGenes = PlayerPrefs.GetString("assault", "");
        // sniperGenes = PlayerPrefs.GetString("sniper", "");
        // baseGenes = PlayerPrefs.GetString("base", "");

        // if (shielderGenes.Length == 0) shielderGenes = await GetGenesString("2567", "shielder");
        // if (assaultGenes.Length == 0) assaultGenes = await GetGenesString("2323", "assault");
        // if (sniperGenes.Length == 0) sniperGenes = await GetGenesString("1000", "sniper");
        // if (baseGenes.Length == 0) baseGenes = await GetGenesString("1367", "base");

        for (int i = 0; i < 8; i++)
        {
            genes[i] = PlayerPrefs.GetString("gene" + i.ToString(), "");
            if (genes[i].Length == 0) genes[i] = await GetGenesString(ids[i], "gene" + i.ToString());
        }

    }
    public string GetGene(int i)
    {
        return this.genes[i];
    }

    private async Task<string> GetGenesString(string id, string type)
    {
        var client = new HttpClient();
        string searchString = "{ axie (axieId:" + id + ") { id, genes, newGenes}}";
        JObject jPayload = new JObject();
        jPayload.Add(new JProperty("query", searchString));
        var jsonContent = new StringContent(jPayload.ToString(), System.Text.Encoding.UTF8, "application/json");
        var url = "https://graphql-gateway.axieinfinity.com/graphql";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Clear();

        request.Content = jsonContent;

        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        Debug.Log(result);
        if (!string.IsNullOrEmpty(result))
        {
            JObject jResult = JObject.Parse(result);
            string genesStr = (string)jResult["data"]["axie"]["newGenes"];
            _isFetchingGenes = false;
            PlayerPrefs.SetString(type, genesStr);
            return genesStr;
        }
        _isFetchingGenes = false;
        return "";

    }
}

