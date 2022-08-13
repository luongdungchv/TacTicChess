using System.Collections;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.Http;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] GameObject _startMsgGO;
        [SerializeField] Button _mixBtn;
        [SerializeField] InputField _idInput;
        [SerializeField] AxieFigure _birdFigure;

        bool _isPlaying = false;
        bool _isFetchingGenes = false;

        private void OnEnable()
        {
            _mixBtn.onClick.AddListener(OnMixButtonClicked);
        }

        private void OnDisable()
        {
            //_mixBtn.onClick.RemoveListener(OnMixButtonClicked);
        }

        // Start is called before the first frame update
        void Start()
        {
            //Time.timeScale = 0f;

            Debug.Log(Shader.Find("Custom/Texture Splatting Palette"));
            Mixer.Init();

            string axieId = PlayerPrefs.GetString("selectingId", "2727");
            string genes = PlayerPrefs.GetString("selectingGenes", "0x2000000000000300008100e08308000000010010088081040001000010a043020000009008004106000100100860c40200010000084081060001001410a04406");
            _idInput.text = axieId;
            _birdFigure.SetGenes(axieId, genes);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isPlaying)
            {
                _startMsgGO.SetActive((Time.unscaledTime % .5 < .2));
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _startMsgGO.SetActive(false);
                    _isPlaying = true;
                    Time.timeScale = 1f;
                }
            }
        }

        void OnMixButtonClicked()
        {
            if (string.IsNullOrEmpty(_idInput.text) || _isFetchingGenes) return;
            _isFetchingGenes = true;
            //StartCoroutine(GetAxiesGenes(_idInput.text));
            GetGenesString(_idInput.text);
            Debug.Log("click");
        }

        public IEnumerator GetAxiesGenes(string axieId)
        {
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            Debug.Log(jPayload.ToString());
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    PlayerPrefs.SetString("selectingId", axieId);
                    PlayerPrefs.SetString("selectingGenes", genesStr);
                    _idInput.text = axieId;
                    _birdFigure.SetGenes(axieId, genesStr);
                }
            }
            _isFetchingGenes = false;
        }
        public async void GetGenesString(string id)
        {
            using (var client = new HttpClient())
            {
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
                    PlayerPrefs.SetString("selectingId", id);
                    PlayerPrefs.SetString("selectingGenes", genesStr);
                    _idInput.text = id;
                    _birdFigure.SetGenes(id, genesStr);
                }
                _isFetchingGenes = false;
            }
        }
    }
}
