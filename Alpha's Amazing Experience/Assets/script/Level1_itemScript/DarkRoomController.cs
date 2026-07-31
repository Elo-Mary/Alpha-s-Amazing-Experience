using System.Collections.Generic;
using UnityEngine;

public class DarkRoomController : MonoBehaviour
{
    [Header("状态配置")]
    [Tooltip("向管家报备的全局暗号，例如：Level1_Storehouse_Lit")]
    public string roomLitStateKey = "Level1_Storehouse_Lit";

    [Header("表现配置")]
    [Tooltip("负责遮挡画面的黑暗UI或黑块实体")]
    public GameObject darknessOverlay;

    [Tooltip("黑暗中需要隐藏的物品列表 (把斧头、锤子等拖进来)")]
    public List<GameObject> hiddenItems;

    void Start()
    {
        // 1. 场景加载时，先问世界管家：这个房间以前被点亮过吗？
        if (WorldStateManager.Instance.GetState(roomLitStateKey) == "lit")
        {
            // 曾经亮过，直接维持明亮状态，结束判定
            ApplyLitState();
            return;
        }

        // 2. 如果房间还没亮过，检查玩家当前的背包里有没有点燃的油灯
        if (InventoryManager.Instance.HasItem("oilLight"))
        {
            Debug.Log("你携带的点燃油灯(oilLight)散发出温暖的光芒，驱散了房间里的黑暗！");

            // 改变表现层：驱散黑暗，显示物品
            ApplyLitState();

            // 写入永久暗号：告诉管家这个房间以后永远是亮的，哪怕玩家下次没带油灯进来
            WorldStateManager.Instance.SaveState(roomLitStateKey, "lit");
        }
        else
        {
            Debug.Log("房间里漆黑一片，你需要一个明亮的光源才能看清深处藏着什么...");
            // 维持黑暗状态
            ApplyDarkState();
        }
    }

    private void ApplyLitState()
    {
        // 隐藏黑幕
        if (darknessOverlay != null) darknessOverlay.SetActive(false);

        // 激活物品
        // 注意：这里一定要判空 (item != null)，因为如果玩家点亮房间后把斧头捡走了，
        // 斧头实体会被 Destroy。下次进房间时，列表里就会有 null 引用，不判空会报错！
        foreach (var item in hiddenItems)
        {
            if (item != null) item.SetActive(true);
        }
    }

    private void ApplyDarkState()
    {
        // 显示黑幕
        if (darknessOverlay != null) darknessOverlay.SetActive(true);

        // 隐藏物品 (雷达彻底扫不到)
        foreach (var item in hiddenItems)
        {
            if (item != null) item.SetActive(false);
        }
    }
}