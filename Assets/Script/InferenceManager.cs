using UnityEngine;
using System.Collections.Generic;

public class InferenceManager : MonoBehaviour
{
    public static InferenceManager Instance;

    // 儲存已生成推論（可供查詢、回顯等）
    private List<string> generatedInferences = new List<string>();

    // 預先定義可成立的推論組合
    private Dictionary<(string, string), string> validPairs = new Dictionary<(string, string), string>();

    private void Awake()
    {
        Instance = this;

        validPairs.Add(("key", "gun"), "推論提示");
    }

    public bool TryGenerateInference(ClueCard a, ClueCard b)
    {
        string id1 = a.clueData.id;
        string id2 = b.clueData.id;

        // 無視順序（雙向檢查）
        string result;
        if (validPairs.TryGetValue((id1, id2), out result) || validPairs.TryGetValue((id2, id1), out result))
        {
            Debug.Log($"推論成立：{result}");

            // 儲存推論
            string inferenceText = $"{a.clueData.name} + {b.clueData.name} ➜ {result}";
            generatedInferences.Add(inferenceText);

            // 你也可以顯示在 UI 或記錄到日誌
            return true;
        }
        else
        {
            Debug.Log($"推論失敗：{a.clueData.id} 和 {b.clueData.id} 沒有定義推論");
            return false;
        }
    }

    public List<string> GetGeneratedInferences()
    {
        return generatedInferences;
    }
}
