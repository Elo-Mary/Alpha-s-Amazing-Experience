using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    // 物品的名称标识（用于 UI 列表排序和指令识别）
    string ItemName { get; }

    // 声明该物体支持哪些核心指令（如 "cut", "pick", "peel"）
    List<string> GetSupportedCommands();

    // 核心交互逻辑
    // command: 解析后的标准指令
    // heldItem: 玩家当前选中的物品栏数据 (空手时为 null)
    // playerPosition: 玩家当前的绝对世界坐标 (用于偏移生成新物品)
    bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition);
}