using System.Collections.Generic;
using UnityEngine;

public class InteractableWood : MonoBehaviour, IInteractable
{
    [Header("生成物配置")]
    [Tooltip("点燃后生成的篝火预制体")]
    public GameObject bonefirePrefab;

    public string ItemName => "wood";

    public List<string> GetSupportedCommands()
    {
        return new List<string> { "fire", "look" };
    }

    public bool ExecuteCommand(string command, ItemData heldItem, Vector3 playerPosition)
    {
        switch (command)
        {
            case "look":
                Debug.Log("这是一个布置好的柴堆(wood)。用打火机(lighter)输入 fire 可以点燃它。");
                return true;

            case "fire":
                if (heldItem != null && heldItem.itemID == "lighter")
                {
                    // 申请生成篝火
                    WorldStateManager.Instance.SpawnAndRecord("bonefire", transform.position);
                    Debug.Log("点火成功！变成了篝火(bonefire)。");

                    // 死亡报备并销毁
                    WorldStateManager.Instance.MarkAsDestroyed(gameObject);
                    Destroy(gameObject);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}