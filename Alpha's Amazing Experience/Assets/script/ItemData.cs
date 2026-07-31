using UnityEngine;

// 允许你在 Unity 编辑器中通过右键菜单直接创建物品数据文件
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("物品核心数据")]
    public string itemID;        // 用于指令识别，例如 "axe", "key"
    public Sprite itemIcon;      // 用于 UI 显示的图片素材
    public GameObject itemPrefab;// 用于被丢弃时在场景中生成的实体预制体
}