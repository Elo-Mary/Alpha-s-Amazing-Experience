using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InteractableScanner : MonoBehaviour
{
    [Header("扫描参数")]
    public float scanRadius = 3f; // 扫描半径（米）

    // 暴露给外部（解析器和UI）的数据
    public List<IInteractable> CurrentInteractables { get; private set; } = new List<IInteractable>();
    public int SelectedIndex { get; private set; } = 0; // 当前选中的索引 (0代表列表第一项)

    // 向外广播的事件：当雷达列表发生变化，或者玩家按数字键切换了选中项时触发
    public static event Action<List<IInteractable>, int> OnScannerUpdated;

    void Update()
    {
        // 雷达时刻在扫描
        ScanEnvironment();

        // 只有在思考模式下，玩家才能用数字键 1-9 切换列表目标
        if (GameManager.Instance.CurrentState == GameState.Thinking)
        {
            HandleSelectionInput();
        }
    }

    void ScanEnvironment()
    {
        // 1. 画一个圆，找出里面所有的碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, scanRadius);
        List<IInteractable> foundItems = new List<IInteractable>();

        // 2. 筛选出带有 IInteractable 接口的物体
        foreach (var col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                foundItems.Add(interactable);
            }
        }

        // 3. 按照 ItemName 字典序 (A-Z) 排序
        foundItems = foundItems.OrderBy(i => i.ItemName).ToList();

        // 4. 检查列表内容是否发生了实质性变化（防止每帧重复触发UI刷新）
        bool listChanged = !foundItems.SequenceEqual(CurrentInteractables);
        CurrentInteractables = foundItems;

        // 5. 约束选中光标，防止越界（比如之前选中第3个，现在列表只剩2个了）
        int previousIndex = SelectedIndex;
        if (CurrentInteractables.Count == 0)
        {
            SelectedIndex = -1; // 附近没东西
        }
        else if (SelectedIndex >= CurrentInteractables.Count || SelectedIndex < 0)
        {
            SelectedIndex = 0; // 越界自动重置为第一个
        }

        // 6. 如果列表变了，或者光标被重置了，通知 UI 刷新
        if (listChanged || SelectedIndex != previousIndex)
        {
            OnScannerUpdated?.Invoke(CurrentInteractables, SelectedIndex);
        }
    }

    void HandleSelectionInput()
    {
        if (CurrentInteractables.Count == 0) return;

        // 监听数字键 1 到 9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                int targetIndex = i - 1; // 按下1，对应索引0
                // 确保按下的数字在当前列表长度范围内
                if (targetIndex < CurrentInteractables.Count)
                {
                    SelectedIndex = targetIndex;
                    OnScannerUpdated?.Invoke(CurrentInteractables, SelectedIndex);
                    Debug.Log($"[雷达] 选中目标切换为: {CurrentInteractables[SelectedIndex].ItemName}");
                }
            }
        }
    }

    // ================= 【新增：方向键切换逻辑】 =================

    // 向上切换：在UI上是往上走，对应索引 +1
    public void SelectNextTarget()
    {
        // 如果雷达里没有东西，或者只有一个东西，就不需要切换
        if (CurrentInteractables == null || CurrentInteractables.Count <= 1) return;

        // 循环递增逻辑：当前索引 + 1，达到最大值时取模绕回 0
        SelectedIndex = (SelectedIndex + 1) % CurrentInteractables.Count;

        // 触发事件广播，通知 UI 刷新高亮
        OnScannerUpdated?.Invoke(CurrentInteractables, SelectedIndex);
    }

    // 向下切换：在UI上是往下走，对应索引 -1
    public void SelectPreviousTarget()
    {
        if (CurrentInteractables == null || CurrentInteractables.Count <= 1) return;

        // 循环递减逻辑：当前索引 - 1，加上总数再取模，防止出现负数
        SelectedIndex = (SelectedIndex - 1 + CurrentInteractables.Count) % CurrentInteractables.Count;

        // 触发事件广播，通知 UI 刷新高亮
        OnScannerUpdated?.Invoke(CurrentInteractables, SelectedIndex);
    }

    // ================= 【修复：状态监听与重置】 =================
    void OnEnable()
    {
        // 订阅状态切换事件
        GameManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        // 注销事件，防止内存泄漏
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState newState)
    {
        // 每次进入思考模式时，强制重置选中索引为第一项
        if (newState == GameState.Thinking)
        {
            SelectedIndex = 0;
            // 【取消这里的注释】手动触发事件，强制让右下角的 UI 列表刷新光标位置！
            OnScannerUpdated?.Invoke(CurrentInteractables, SelectedIndex);
        }
    }

    // 在 Scene 窗口画一个绿色的圈，方便你在编辑器里直观地调节 scanRadius 扫描范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
