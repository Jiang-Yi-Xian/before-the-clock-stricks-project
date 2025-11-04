using System.Collections.Generic;
using UnityEngine;

public class InteractionPointManager : MonoBehaviour
{
    public static InteractionPointManager Instance { get; private set; }

    private Dictionary<string, InteractionPoint> points = new Dictionary<string, InteractionPoint>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // 掃描所有互動點（場景中掛有 InteractionPoint 的物件）
        foreach (var ip in FindObjectsOfType<InteractionPoint>())
        {
            if (!points.ContainsKey(ip.pointName))
                points.Add(ip.pointName, ip);
            else
                Debug.LogWarning($"重複的互動點名稱：{ip.pointName}");
        }
    }

    public InteractionPoint GetPoint(string name)
    {
        if (points.TryGetValue(name, out var point))
            return point;

        Debug.LogWarning($"找不到互動點：{name}");
        return null;
    }
}
