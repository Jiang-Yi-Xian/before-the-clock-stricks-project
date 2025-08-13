using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [System.Serializable] // 讓此類別可以在 Inspector 中顯示
    public class NamedCamera
    {
        public string name;   // 相機名稱 (用來查找和切換)
        public Camera camera; // 對應的 Camera 物件
    }

    [Header("Camera 設定")]
    public List<NamedCamera> cameraList; // 設定相機清單

    // 快速查找相機的字典 (key: 相機名稱, value: Camera 物件)
    private Dictionary<string, Camera> cameraMap = new Dictionary<string, Camera>();

    // 紀錄當前啟用的相機名稱
    private string currentCameraName = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 場景切換時不刪除此物件
        DontDestroyOnLoad(gameObject);

        // 建立相機字典映射表
        BuildCameraMap();
    }

    void OnEnable()
    {
        // 訂閱場景載入事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 取消訂閱事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 檢查所有相機是否已經在場景中綁定
        foreach (var entry in cameraList)
        {
            if (entry.camera == null)
            {
                // 嘗試用名稱尋找場景中的 Camera
                Camera found = GameObject.Find(entry.name)?.GetComponent<Camera>();
                if (found != null)
                {
                    entry.camera = found;
                    cameraMap[entry.name] = found; // 更新字典
                }
                else
                {
                    Debug.LogWarning($"[CameraManager] 找不到相機：{entry.name}");
                }
            }
        }

        // 如果之前已經有啟用的相機，則切回該相機
        if (!string.IsNullOrEmpty(currentCameraName))
        {
            SwitchTo(currentCameraName);
        }
    }

    // 將 cameraList 轉換成字典以便快速查找
    private void BuildCameraMap()
    {
        cameraMap.Clear();
        foreach (var entry in cameraList)
        {
            if (entry.camera != null && !cameraMap.ContainsKey(entry.name))
            {
                cameraMap.Add(entry.name, entry.camera);
            }
        }
    }

    // 切換到指定名稱的相機
    public void SwitchTo(string cameraName)
    {
        // 檢查是否有對應名稱的相機
        if (!cameraMap.TryGetValue(cameraName, out Camera targetcam) || targetcam == null)
        {
            Debug.LogError($"[CameraManager] 找不到名稱為 {cameraName} 的相機，切換失敗");
            return;
        }

        // 先關閉所有相機與其 AudioListener
        foreach (var cam in cameraMap.Values)
        {
            if (cam != null)
            {
                cam.enabled = false; // 關閉畫面顯示

                AudioListener listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false; // 關閉該相機的音訊收音
            }
        }

        // 啟用目標相機
        targetcam.enabled = true;
        currentCameraName = cameraName;

        // 啟用目標相機的 AudioListener
        AudioListener targetListener = targetcam.GetComponent<AudioListener>();
        if (targetListener != null)
            targetListener.enabled = true;

        // 設定為當前渲染相機
        Camera.SetupCurrent(targetcam);

        // 通知 PlayerController 更新使用的攝影機
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateCamera(targetcam);
        }

        Debug.Log($"[CameraManager] 已切換至 {cameraName}，啟用 AudioListener。");
    }
}
