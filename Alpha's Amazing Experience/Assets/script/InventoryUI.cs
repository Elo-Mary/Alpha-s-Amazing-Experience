using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 引用 (必须按 1-8 顺序拖入)")]
    public Image[] slotBackgrounds = new Image[8]; // 8个格子的底图组件
    public Image[] slotIcons = new Image[8];       // 8个格子的物品Icon组件

    [Header("颜色设置")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.494f, 0.494f, 0.494f, 1f); // 16进制 7E7E7E 对应的 RGBA

    void OnEnable()
    {
        InventoryManager.OnInventoryUpdated += RefreshInventoryUI;
        InventoryManager.OnSlotSelectionChanged += RefreshSelectionUI;
    }

    void OnDisable()
    {
        InventoryManager.OnInventoryUpdated -= RefreshInventoryUI;
        InventoryManager.OnSlotSelectionChanged -= RefreshSelectionUI;
    }

    // 刷新 8 个格子里的物品图片
    void RefreshInventoryUI(ItemData[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemIcon != null)
            {
                slotIcons[i].sprite = slots[i].itemIcon;
                slotIcons[i].color = Color.white; // 恢复不透明
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1, 1, 1, 0); // 设为完全透明进行隐藏
            }
        }
    }

    // 刷新选中状态（底图颜色白底与灰底切换）
    void RefreshSelectionUI(int selectedIndex)
    {
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (i == selectedIndex)
            {
                slotBackgrounds[i].color = selectedColor;
            }
            else
            {
                slotBackgrounds[i].color = normalColor;
            }
        }
    }
}