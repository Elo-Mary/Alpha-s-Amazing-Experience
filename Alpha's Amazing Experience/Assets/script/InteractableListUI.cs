using UnityEngine;
using TMPro; // 引入 TextMeshPro
using System.Collections.Generic;

public class InteractableListUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject container;       // 就是那个 InteractableListContainer
    public GameObject itemTextPrefab;  // 刚才做的 ListItemTemplate 预制体
    public Transform contentRoot;      // 生成文字的父节点（也是容器自身）

    [Header("视觉设置")]
    public Color normalColor = Color.grey;
    public Color selectedColor = Color.black; // 选中的高亮颜色
    public string selectedPrefix = " ";       // 选中的前缀符号

    // 用于缓存生成的 UI 实例，方便后续清理
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        // 游戏开始时默认隐藏
        container.SetActive(false);
    }

    void OnEnable()
    {
        // 严格遵循我们制定的事件订阅机制
        GameManager.OnStateChanged += HandleStateChanged;
        InteractableScanner.OnScannerUpdated += UpdateListUI;
    }

    void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
        InteractableScanner.OnScannerUpdated -= UpdateListUI;
    }

    void HandleStateChanged(GameState newState)
    {
        // 只有进入思考模式时才显示列表
        container.SetActive(newState == GameState.Thinking);
    }

    // 接收雷达传来的数据
    void UpdateListUI(List<IInteractable> interactables, int selectedIndex)
    {
        // 1. 清理上一帧/上一次生成的旧文本
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        // 2. 根据最新的物体列表生成新文本
        for (int i = 0; i < interactables.Count; i++)
        {
            // 实例化 Prefab
            GameObject newItem = Instantiate(itemTextPrefab, contentRoot);
            spawnedItems.Add(newItem);

            //TextMeshProUGUI textComp = newItem.GetComponent<TextMeshProUGUI>();
            // 【修改这里】因为文本现在是背景图的子物体，所以要用 GetComponentInChildren
            TextMeshProUGUI textComp = newItem.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                // i + 1 是为了显示 1, 2, 3，对应玩家键盘上的数字键
                if (i == selectedIndex)
                {
                    // 选中的项：变色 + 加前缀
                    textComp.text = $"{i + 1}{selectedPrefix}{interactables[i].ItemName}";
                    textComp.color = selectedColor;
                }
                else
                {
                    // 未选中的项：普通显示
                    textComp.text = $"{i + 1}{interactables[i].ItemName}";
                    textComp.color = normalColor;
                }
            }
        }
    }
}