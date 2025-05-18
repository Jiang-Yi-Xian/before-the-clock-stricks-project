using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class HintRequester : MonoBehaviour
{
    public Button AIHint;
    public string playerStatus = "�ڦb���Y�䤣���_�͡A�������}";
    public string stage = "���Y";
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
        Debug.Log("�e�X�� URL�G" + url);


        string json = JsonUtility.ToJson(new QueryRequest(playerStatus, stage, 3));
        Debug.Log("�e�X�� JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("�d�ߥ���: " + request.responseCode + " " + request.error + "\n" + request.downloadHandler.text);
            hintText.text = "���ܨ��o����";
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