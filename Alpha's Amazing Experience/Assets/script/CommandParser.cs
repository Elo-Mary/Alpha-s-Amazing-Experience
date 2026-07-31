using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandParser : MonoBehaviour
{
    [Header("系统引用")]
    public InteractableScanner scanner;      // 需要雷达提供当前目标
    public Transform playerTransform;        // 需要玩家位置来生成问号
    public GameObject questionMarkPrefab;    // 报错时的问号预制体

    // 同义词字典：Key是玩家输入的词，Value是标准核心指令
    private Dictionary<string, string> commandDictionary = new Dictionary<string, string>();

    void Awake()
    {
        InitializeDictionary();
    }

    // --- 事件订阅与注销 ---
    void OnEnable()
    {
        // 订阅 InputManager 广播的回车提交事件
        InputManager.OnCommandSubmitted += ParseAndExecute;
    }

    void OnDisable()
    {
        InputManager.OnCommandSubmitted -= ParseAndExecute;
    }

    // --- 初始化同义词词典 ---
    void InitializeDictionary()
    {
        // 拾取类
        commandDictionary.Add("pick", "pick");
        commandDictionary.Add("get", "pick");
        commandDictionary.Add("take", "pick");
        commandDictionary.Add("gain", "pick");

        // 砍伐类
        commandDictionary.Add("cut", "cut");
        commandDictionary.Add("chop", "cut");

        // 观察/调查类
        commandDictionary.Add("look", "look");
        commandDictionary.Add("check", "look");
        commandDictionary.Add("inspect", "look");

        // 你可以在这里无限扩展你的词典...
        // 布置类
        commandDictionary.Add("set", "set");
        commandDictionary.Add("place", "set");

        // 点火类
        commandDictionary.Add("fire", "fire");
        commandDictionary.Add("light", "fire");
        commandDictionary.Add("ignite", "fire");
        commandDictionary.Add("start", "fire");

        // 烹饪类
        commandDictionary.Add("cook", "cook");
        commandDictionary.Add("roast", "cook");

        // 剥皮类
        commandDictionary.Add("peel", "peel");
        commandDictionary.Add("skin", "peel");

        // 丢弃类
        commandDictionary.Add("put", "put");
        commandDictionary.Add("drop", "put");

        // 进入类
        commandDictionary.Add("enter", "enter");
        commandDictionary.Add("in", "enter");
        commandDictionary.Add("go", "enter");
        commandDictionary.Add("down", "enter");

        // 搜索类
        commandDictionary.Add("search", "search");
        commandDictionary.Add("find", "search");

        // 切换类
        commandDictionary.Add("switch", "switch");
        commandDictionary.Add("turn", "switch");
        commandDictionary.Add("open", "switch");


        // 破坏/暴力类
        commandDictionary.Add("kick", "kick");
        commandDictionary.Add("break", "kick");

        // 动作/转化类
        commandDictionary.Add("pump", "pump");
        commandDictionary.Add("extract", "pump");

        // 动作/交互类
        commandDictionary.Add("tie", "tie");
        commandDictionary.Add("bind", "tie");

        // 动作/生产类
        commandDictionary.Add("fix", "fix");
        commandDictionary.Add("repair", "fix");

        // 移动/攀爬类
        commandDictionary.Add("climb", "climb");

        // 动作/交互类
        commandDictionary.Add("add", "add");
    }

    // --- 核心解析逻辑 ---
    void ParseAndExecute(string rawInput)
    {
        // 去除首尾空格，其实 InputManager 里已经转小写了，这里保险起见再转一次
        string input = rawInput.Trim().ToLower();

        // 【第一步：同义词翻译】
        if (!commandDictionary.TryGetValue(input, out string coreCommand))
        {
            Debug.Log($"[大脑] 未知指令: {input}，字典中未收录。");
            TriggerErrorAndExit();
            return;
        }

        // ================= 【新增：全局独立指令拦截】 =================
        if (coreCommand == "put")
        {
            ItemData itemToDrop = InventoryManager.Instance.GetSelectedItem();
            if (itemToDrop != null)
            {
                // 计算丢弃坐标（玩家脚边）
                Vector3 dropPos = playerTransform.position + new Vector3(1.5f, 0f, 0f);

                // 调用物品栏的专门方法，传入当前选中的槽位索引和丢弃坐标
                InventoryManager.Instance.DropFromInventory(InventoryManager.Instance.CurrentSelectedIndex, dropPos);

                // 执行成功，退出思考模式
                if (GameManager.Instance.CurrentState == GameState.Thinking)
                {
                    GameManager.Instance.ToggleThinkingMode();
                }
            }
            else
            {
                Debug.Log("[大脑] 当前槽位为空，无法执行 put！");
                TriggerErrorAndExit();
            }
            return; // 拦截完成，直接结束函数，不往下走了
        }

        // 【第二步：目标获取与校验】
        if (scanner.CurrentInteractables.Count == 0 || scanner.SelectedIndex < 0)
        {
            Debug.Log("[大脑] 当前周围没有可交互的物体！");
            TriggerErrorAndExit();
            return;
        }

        IInteractable target = scanner.CurrentInteractables[scanner.SelectedIndex];

        // 【第三步：能力质询】
        if (!target.GetSupportedCommands().Contains(coreCommand))
        {
            Debug.Log($"[大脑] 目标 '{target.ItemName}' 不支持 '{coreCommand}' 指令！");
            TriggerErrorAndExit();
            return;
        }

        // 【第四步：下发执行】
        // 1. 向 InventoryManager 获取当前选中槽位的物品数据 (如果是空手，返回的是 null)
        ItemData handItemData = InventoryManager.Instance.GetSelectedItem();

        // 2. 获取玩家当前的绝对世界坐标 (用于灌木丛掉落树枝等需要生成新物体的操作)
        // 注意：我们在写 CommandParser 时已经声明了 public Transform playerTransform; 
        Vector3 currentPlayerPos = playerTransform.position;

        // 3. 严格按照全新的 IInteractable 接口签名，传入：核心指令、手持物品数据、玩家坐标
        bool success = target.ExecuteCommand(coreCommand, handItemData, currentPlayerPos);

        if (success)
        {
            // 执行成功，退出思考模式
            if (GameManager.Instance.CurrentState == GameState.Thinking)
            {
                GameManager.Instance.ToggleThinkingMode();
            }
        }
        else
        {
            // 执行失败（比如砍树但没拿斧头），报错并退出
            TriggerErrorAndExit();
        }
    }

    // --- 错误处理机制 ---
    void TriggerErrorAndExit()
    {
        // 在玩家头顶生成问号
        if (questionMarkPrefab != null && playerTransform != null)
        {
            Vector3 spawnPos = playerTransform.position + new Vector3(0, 3f, 0); // 暂定头顶上方 2 米处
            Instantiate(questionMarkPrefab, spawnPos, Quaternion.identity);
        }

        // 报错后退出思考模式
        if (GameManager.Instance.CurrentState == GameState.Thinking)
        {
            GameManager.Instance.ToggleThinkingMode();
        }
    }
}
