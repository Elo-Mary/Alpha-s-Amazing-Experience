using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractablePlacedLadder : MonoBehaviour, IInteractable
{
    [Header("基础配置")]
    [Tooltip("雷达显示的名称 (填 ladderToAttic 或 ladderToRoof)")]
    public string ladderName = "ladderToAttic";

    [Header("表现配置")]
    [Tooltip("控制梯子显示的图片渲染器 (极其重要)")]
    public SpriteRenderer targetSpriteRenderer;
    [Tooltip("梯子的贴图")]
    public Sprite ladderSprite;

    [Header("传送配置")]
    public string targetSceneName;
    public string targetSpawnPointName;

    // 核心状态：梯子是否已经架设
    private bool isPlaced = false;

    public string ItemName => ladderName;

    void Start()
    {
        // 场景加载时，向管家查询这里是否架了梯子
        SceneObjectID soid = GetComponent<SceneObjectID>();
        if (soid != null && WorldStateManager.Instance.GetState(soid.id) == "placed")
        {
            ApplyPlacedState();
        }
        else
        {
            ApplyEmptyState();
        }
    }

    // 状态 A：已放置
    private void ApplyPlacedState()
    {
        isPlaced = true;
        if (targetSpriteRenderer != null && ladderSprite != null)
        {
            targetSpriteRenderer.sprite = ladderSprite;
            targetSpriteRenderer.enabled = true; // 显示图片
        }
    }

    // 状态 B：未放置 (隐藏空缺状态)
    private void ApplyEmptyState()
    {
        isPlaced = false;
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.enabled = false; // 仅仅隐藏图片，但碰撞体还在，雷达依然能扫到！
        }
    }

    public List<string> GetSupportedCommands()
    {
        // 【动态菜单】：没放梯子时只能 look 和 set；放了梯子后变成 look, climb, pick
        if (isPlaced)
            return new List<string> { "look", "climb", "pick" };
        else
            return new List<string> { "look", "set" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                if (isPlaced)
                    Debug.Log($"梯子({ladderName})已经稳稳地架好了。你可以攀爬(climb)上去，也可以取回(get)它。");
                else
                    Debug.Log($"这里有一个可以通往高处的入口({ladderName})，但是太高了。也许可以用梯子(Ladder)架设(set)上去。");
                return true;

            case "set":
                if (!isPlaced)
                {
                    // 校验是否拿着大写的 Ladder
                    if (heldItem != null && heldItem.itemID == "Ladder")
                    {
                        InventoryManager.Instance.ConsumeSelectedItem(); // 扣除背包里的梯子
                        ApplyPlacedState(); // 显示场景里的梯子

                        SceneObjectID soid = GetComponent<SceneObjectID>();
                        if (soid != null) WorldStateManager.Instance.SaveState(soid.id, "placed");

                        Debug.Log($"你稳稳地架好了梯子({ladderName})！现在可以攀爬(climb)了。");
                        return true;
                    }
                    else
                    {
                        Debug.Log("你需要手里拿着修好后的梯子(Ladder)才能架设！");
                        return false;
                    }
                }
                return false;

            case "climb":
                if (isPlaced && !string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log("你手脚并用地顺着梯子爬了上去...");
                    SceneConfig.TargetSpawnPointName = targetSpawnPointName;
                    SceneManager.LoadScene(targetSceneName);
                    return true;
                }
                return false;

            case "pick":
                if (isPlaced)
                {
                    // 1. 取回梯子：呼叫管家在玩家脚边生成一个 Ladder
                    WorldStateManager.Instance.SpawnAndRecord("Ladder", playerPosition);

                    // 2. 隐藏场景里的梯子
                    ApplyEmptyState();

                    // 3. 将状态覆盖为 empty
                    SceneObjectID soid = GetComponent<SceneObjectID>();
                    if (soid != null) WorldStateManager.Instance.SaveState(soid.id, "empty");

                    Debug.Log($"你把梯子({ladderName})收了回来，它掉落在了你的脚边。");
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}