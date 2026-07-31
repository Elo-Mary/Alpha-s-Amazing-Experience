using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionMarkEffect : MonoBehaviour
{
    [Header("显示时长 (秒)")]
    public float lifetime = 2f;

    void Start()
    {
        // 开启一个基于真实时间的协程
        StartCoroutine(DestroyAfterRealtime());
    }

    IEnumerator DestroyAfterRealtime()
    {
        // WaitForSecondsRealtime 会无视 Time.timeScale = 0 的影响
        yield return new WaitForSecondsRealtime(lifetime);
        Destroy(gameObject);
    }
}
