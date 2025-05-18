using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Xml;

public class HintRequester : MonoBehaviour
{
    public Button AIHint;
    public string playerStatus = "�ڥ��b�ǲ߾ާ@";
    public string stage = "�s��о�";
    public TMPro.TextMeshProUGUI hintText;

    //�w�ϥδ��� ID �O��
    private List<string> usedIds = new List<string>();

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

        QueryRequest payload = new QueryRequest(playerStatus, stage, 3, usedIds);
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Color c = hintText.color;
            c.a = 1f;
            hintText.color = c;

            Debug.LogError("�d�ߥ���: " + request.responseCode + " " + request.error + "\n" + request.downloadHandler.text);
            hintText.text = "���ܨ��o����";
        }
        else
        {
            string jsonResult = request.downloadHandler.text;
            HintResponse response = JsonUtility.FromJson<HintResponse>(jsonResult);

            Color c = hintText.color;
            c.a = 1f;
            hintText.color = c;

            hintText.text = response.ai_generated_hint;
            Debug.Log("���ܵ��G" + response.ai_generated_hint);

            //�O���w���ܪ� hint_id�]�p�G���^
            if (!string.IsNullOrEmpty(response.hint_id) && !usedIds.Contains(response.hint_id))
            {
                usedIds.Add(response.hint_id);
            }

            StartCoroutine(DeleteText());
        }
    }

    IEnumerator DeleteText()
    {
        yield return new WaitForSeconds(5f); // ���� 5 ��A�}�l�H�X

        Debug.Log("���ܵ�����");
        LeanTween.value(gameObject, 1f, 0f, 2f)
            .setOnUpdate((float alpha) =>
            {
                Color c = hintText.color;
                c.a = alpha;
                hintText.color = c;
            });

        // ���H�X������M�Ť�r�]�i��^
        yield return new WaitForSeconds(2f);
        hintText.text = "";
    }

    [System.Serializable]
    public class QueryRequest
    {
        public string player_status;
        public string stage;
        public int n_results;
        public List<string> used_ids;

        public QueryRequest(string status, string stage, int results, List<string> used)
        {
            player_status = status;
            this.stage = stage;
            n_results = results;
            used_ids = used;
        }
    }

    [System.Serializable]
    public class HintResponse
    {
        public string[] raw_hints;
        public string ai_generated_hint;
        public string hint_id;
    }
}
