using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform player;       // 指向玩家的 Transform 组件引用

    [Header("平滑参数")]
    public float smoothSpeed = 5f; // 数字越大跟得越紧，越小越平滑/延迟

    [Header("摄像机 X 轴边界")]
    public float minX; // 画面最左侧边界
    public float maxX; // 画面最右侧边界

    // 缓存摄像机初始的 Y 和 Z 坐标
    private float fixedY;
    private float fixedZ;

    void Start()
    {
        // 游戏开始时，记录摄像机当前的 Y 和 Z 坐标（通常 Z 是 -10）
        // 我们的需求是只在 X 轴移动，所以 Y 和 Z 保持不变
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    // 必须使用 LateUpdate 避免画面抖动
    void LateUpdate()
    {
        // 防空指针保护
        if (player == null) return;

        // 1. 获取玩家当前的 X 坐标
        float targetX = player.position.x;

        // 2. 核心逻辑：限制目标 X 坐标在 [minX, maxX] 范围内
        // 如果玩家超过了 maxX，targetX 就会停留在 maxX
        targetX = Mathf.Clamp(targetX, minX, maxX);

        // 3. 构建摄像机应该去的目标位置
        Vector3 targetPosition = new Vector3(targetX, fixedY, fixedZ);

        // 4. 平滑移动：从摄像机当前位置，平滑插值到目标位置
        // Time.deltaTime 确保移动速度不受帧率影响
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
