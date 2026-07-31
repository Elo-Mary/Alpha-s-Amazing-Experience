using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    // 定义三个向外广播的事件
    public static event Action<string> OnInputTextChanged;      // 通知 UI 更新输入框的文字
    public static event Action<string> OnCommandSubmitted;      // 通知解析器去查字典执行指令
    public static event Action<int> OnInventorySlotSelected;    // 通知选中了第几个物品栏(1-8)
    public InteractableScanner scanner;

    private string currentInput = ""; // 存放玩家在思考模式下敲击的字符串

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) return;
        // 1. 随时监听 Ctrl 键切换状态
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            GameManager.Instance.ToggleThinkingMode();

            // 如果刚切入思考模式，清空之前的残留输入
            if (GameManager.Instance.CurrentState == GameState.Thinking)
            {
                currentInput = "";
                OnInputTextChanged?.Invoke(currentInput);
            }
        }

        // 【新增】：无论处于 Action 还是 Thinking 模式，随时都支持左右键切换物品栏
        HandleInventoryNavigation();

        // 2. 根据当前状态，将键盘输入分流
        if (GameManager.Instance.CurrentState == GameState.Thinking)
        {
            HandleThinkingInput();
        }
        else
        {
            HandleActionInput();
        }
    }

    //void HandleActionInput()
    //{
    //    // 行动模式下，数字键 1-8 用于切换物品栏
    //    for (int i = 1; i <= 8; i++)
    //    {
    //        if (Input.GetKeyDown(i.ToString()))
    //        {
    //            Debug.Log($"行动模式：选中物品栏 [{i}]");
    //            OnInventorySlotSelected?.Invoke(i);
    //        }
    //    }
    //}
    void HandleActionInput()
    {
        // 行动模式下，数字键 1-8 用于切换物品栏
        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                // i 的值是 1-8，但数组索引是 0-7，所以传 i - 1
                InventoryManager.Instance.SelectSlot(i - 1);
                OnInventorySlotSelected?.Invoke(i); // 保留原有广播接口
            }
        }
    }

    // ================= 【新增：物品栏左右循环切换】 =================
    void HandleInventoryNavigation()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int currentIndex = InventoryManager.Instance.CurrentSelectedIndex;
            // 循环向左：当前索引减1，小于0时加上8绕回最后一位(7)
            int newIndex = (currentIndex - 1 + 8) % 8;
            InventoryManager.Instance.SelectSlot(newIndex);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int currentIndex = InventoryManager.Instance.CurrentSelectedIndex;
            // 循环向右：当前索引加1，大于7时取模绕回第一位(0)
            int newIndex = (currentIndex + 1) % 8;
            InventoryManager.Instance.SelectSlot(newIndex);
        }
    }

    void HandleThinkingInput()
    {
        // ================= 1. 优先处理方向键 (移出 foreach 循环！) =================
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (scanner != null) scanner.SelectNextTarget();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (scanner != null) scanner.SelectPreviousTarget();
        }

        // ================= 2. 处理键盘打字输入 =================
        // Input.inputString 包含了这一帧用户敲击的所有字符
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // 处理退格键 (Backspace)
            {
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    OnInputTextChanged?.Invoke(currentInput);
                }
            }
            else if (c == '\n' || c == '\r') // 处理回车键 (Enter)
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    Debug.Log($"玩家提交了指令: {currentInput}");
                    OnCommandSubmitted?.Invoke(currentInput);

                    // 提交后清空输入框
                    currentInput = "";
                    OnInputTextChanged?.Invoke(currentInput);
                }
            }
            else if (char.IsLetter(c)) // 只接收英文字母
            {
                currentInput += c.ToString().ToLower();
                OnInputTextChanged?.Invoke(currentInput);
            }
        }

        // ================= 3. 处理数字键直接选择目标 =================
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                int targetIndex = i - 1;
                if (scanner != null && targetIndex < scanner.CurrentInteractables.Count)
                {
                    // 这里不能只写 Debug，要真正去调用 Scanner 的选中逻辑！
                    // 【注意】这里我们先暂且留着，下一轮我们可以在 Scanner 里开个公开方法让这里调
                    Debug.Log($"思考模式：试图选中环境列表第 [{i}] 项");
                }
            }
        }
    }
}