using UnityEngine;

public class GlobalStateListener : MonoBehaviour
{
    [Header("监听配置")]
    [Tooltip("要向管家查询的全局暗号，例如 Global_RoofWireTied")]
    public string globalStateKey = "Global_RoofWireTied";

    [Tooltip("暗号对应的值等于这个时，触发显示")]
    public string expectedValue = "true";

    [Header("目标物体")]
    [Tooltip("需要被显示的隐藏物体 (如屋外圣诞树上的电线)")]
    public GameObject targetObject;

    void Start()
    {
        // 场景刚加载时，立刻向管家对暗号
        if (WorldStateManager.Instance.GetState(globalStateKey) == expectedValue)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                Debug.Log($"[全局监听器] 检测到暗号 {globalStateKey} 成立，已激活目标物体！");
            }
        }
        else
        {
            // 暗号不对，确保它保持隐藏
            if (targetObject != null) targetObject.SetActive(false);
        }
    }
}