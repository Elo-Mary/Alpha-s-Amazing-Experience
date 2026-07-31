using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class CGManager : MonoBehaviour
{
    public static CGManager Instance { get; private set; }

    [Header("UI 组件引用")]
    public GameObject cgCanvas;      // 过场动画的画布
    public RawImage videoScreen;     // 显示视频的屏幕
    public VideoPlayer videoPlayer;  // 视频播放器组件

    private Action onVideoCompleteCallback; // 播完后的回调函数

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始隐藏 CG 画布，并订阅视频播放结束的事件
        cgCanvas.SetActive(false);
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    // 公开的播放视频方法
    public void PlayVideo(VideoClip clip, Action onComplete)
    {
        onVideoCompleteCallback = onComplete;

        cgCanvas.SetActive(true);
        videoPlayer.clip = clip;
        videoPlayer.Play();

        Debug.Log($"[CG播放器] 开始播放视频: {clip.name}");
    }

    // 视频播放结束时的自动回调
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[CG播放器] 视频播放完毕。");
        cgCanvas.SetActive(false);

        // 执行传入的回调逻辑（比如切场景、恢复游戏等）
        onVideoCompleteCallback?.Invoke();
        onVideoCompleteCallback = null;
    }
}