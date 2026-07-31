using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 定义游戏的两种核心状态
//public enum GameState { Action, Thinking }
// 确保你的 GameState 枚举包含 Cutscene
public enum GameState { Action, Thinking, Cutscene }

public class GameManager : MonoBehaviour
{

    // 在 GameManager.cs 中，添加这个公开方法：
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    // 单例模式，方便全局访问 GameManager.Instance
    public static GameManager Instance { get; private set; }

    // 当前状态，默认是行动模式
    public GameState CurrentState { get; private set; } = GameState.Action;

    // 状态改变时触发的事件（其它脚本可以监听这个事件）
    public static event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 如果你有多个场景，取消注释这行，让它跨场景不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 切换思考/行动模式的公开方法
    public void ToggleThinkingMode()
    {
        if (CurrentState == GameState.Action)
        {
            ChangeState(GameState.Thinking);
        }
        else
        {
            ChangeState(GameState.Action);
        }
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;

        // 核心：时间控制
        if (newState == GameState.Thinking)
        {
            Time.timeScale = 0f; // 时间完全停止
            Debug.Log(">>> 进入思考模式，时间停止");
        }
        else
        {
            Time.timeScale = 1f; // 时间恢复正常
            Debug.Log(">>> 进入行动模式，时间恢复");
        }

        // 广播状态改变的事件
        OnStateChanged?.Invoke(CurrentState);
    }
}
