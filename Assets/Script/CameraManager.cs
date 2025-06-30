using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [System.Serializable]
    public class NamedCamera
    {
        public string name;
        public Camera camera;
    }

    [Header("Camera 設定")]
    public List<NamedCamera> cameraList;

    private Dictionary<string, Camera> cameraMap = new Dictionary<string, Camera>();
    private string currentCameraName = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCameraMap();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var entry in cameraList)
        {
            if (entry.camera == null)
            {
                Camera found = GameObject.Find(entry.name)?.GetComponent<Camera>();
                if (found != null)
                {
                    entry.camera = found;
                    cameraMap[entry.name] = found;
                }
                else
                {
                    Debug.LogWarning($"[CameraManager] 找不到相機：{entry.name}");
                }
            }
        }

        // 載入後自動切換回上一個視角（如果有設定）
        if (!string.IsNullOrEmpty(currentCameraName))
        {
            SwitchTo(currentCameraName);
        }
    }

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

    public void SwitchTo(string cameraName)
    {
        foreach (var cam in cameraMap.Values)
        {
            if (cam != null)
                cam.enabled = false;
        }

        if (cameraMap.TryGetValue(cameraName, out Camera targetcam) && targetcam != null)
        {
            targetcam.enabled = true;
            Camera.SetupCurrent(targetcam);
            currentCameraName = cameraName;

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.UpdateCamera(targetcam);
            }
        }
        else
        {
            Debug.LogError($"[CameraManager] 找不到名稱為 {cameraName} 的相機，切換失敗");
        }
    }
}
