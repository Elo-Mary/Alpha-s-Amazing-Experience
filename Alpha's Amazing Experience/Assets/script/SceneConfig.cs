using UnityEngine;

public class SceneConfig : MonoBehaviour
{
    // 全局静态信使：记录玩家将要出现在的出生点名称
    public static string TargetSpawnPointName = "";

    [Header("当前场景摄像机边界")]
    public float cameraMinX = -10f;
    public float cameraMaxX = 10f;

    void Start()
    {
        // 1. 动态更新玩家位置
        if (!string.IsNullOrEmpty(TargetSpawnPointName))
        {
            // 找到名字匹配的出生点物体
            GameObject spawnPoint = GameObject.Find(TargetSpawnPointName);
            if (spawnPoint != null)
            {
                // 找到常驻的玩家实体，将其移动过去
                GameObject player = GameObject.Find("Player"); // 确保你的玩家物体名字叫 "Player"
                if (player != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log($"[场景配置] 已将玩家传送至出生点: {TargetSpawnPointName}");
                }
            }
            else
            {
                Debug.LogWarning($"[场景配置] 找不到名为 {TargetSpawnPointName} 的出生点！");
            }
        }

        // 2. 动态更新摄像机边界
        CameraController camController = FindObjectOfType<CameraController>();
        if (camController != null)
        {
            camController.minX = cameraMinX;
            camController.maxX = cameraMaxX;
            Debug.Log($"[场景配置] 摄像机边界已更新为: {cameraMinX} 到 {cameraMaxX}");
        }
    }
}