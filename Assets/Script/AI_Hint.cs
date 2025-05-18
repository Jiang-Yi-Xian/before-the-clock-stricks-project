using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class HintRequester : MonoBehaviour
{
    public Button AIHint;
    public string playerStatus = "我在走廊找不到鑰匙，門打不開";
    public string stage = "走廊";
    public TMPro.TextMeshProUGUI hintText;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            RequestHint();
        }
    }

    public void RequestHint()
    {
        StartCoroutine(SendHintRequest());
    }

    IEnumerator SendHintRequest()
    {
        string url = "http://172.20.10.2:8000/query_hint/";
        Debug.Log("送出的 URL：" + url);


        string json = JsonUtility.ToJson(new QueryRequest(playerStatus, stage, 3));
        Debug.Log("送出的 JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("查詢失敗: " + request.responseCode + " " + request.error + "\n" + request.downloadHandler.text);
            hintText.text = "提示取得失敗";
        }
        else
        {
            string jsonResult = request.downloadHandler.text;
            HintResponse response = JsonUtility.FromJson<HintResponse>(jsonResult);
            hintText.text = response.ai_generated_hint;
        }
    }

    [System.Serializable]
    public class QueryRequest
    {
        public string player_status;
        public string stage;
        public int n_results;

        public QueryRequest(string status, string stage, int results)
        {
            player_status = status;
            this.stage = stage;
            n_results = results;
        }
    }

    [System.Serializable]
    public class HintResponse
    {
        public string[] raw_hints;
        public string ai_generated_hint;
    }
}