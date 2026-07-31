using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;       // 移动速度
    public float turnDelay = 0.2f;     // 转身延迟时间

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    // --- 【新增】动画组件引用 ---
    private Animator anim;

    // 状态追踪
    private float pressTimeA = 0f;
    private float pressTimeD = 0f;
    private bool isPressingA = false;
    private bool isPressingD = false;
    private bool facingRight = true;   // 默认面朝右侧

    void Start()
    {
        // 获取当前物体上的组件引用
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        // --- 【新增】获取 Animator 组件 ---
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
    }

    // 处理玩家输入
    void HandleInput()
    {
        // 如果不是行动模式，直接退出，不处理 A/D 按键
        if (GameManager.Instance.CurrentState != GameState.Action)
        {
            // 【修复 Bug 的核心代码】
            // 在被拦截时，强制清空之前残留的按键状态
            // 这样切回行动模式时，玩家必须重新按下按键才会移动
            isPressingA = false;
            isPressingD = false;
            return;
        }

        // ... 下面保留你原本处理 A 和 D 键的代码 ...
        // ---------- 处理向右移动 (D键) ----------
        if (Input.GetKeyDown(KeyCode.D))
        {
            isPressingD = true;
            pressTimeD = Time.time; // 记录按下 D 键的时刻

            if (!facingRight)
            {
                // 如果当前朝左，需要转身
                facingRight = true;
                sr.flipX = false; // 取消图片的 X 轴翻转，使其朝右
            }
            else
            {
                // 如果已经朝右，不需要改变方向，直接跳过 0.2 秒的等待
                pressTimeD = Time.time - turnDelay;
            }
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            isPressingD = false;
        }

        // ---------- 处理向左移动 (A键) ----------
        if (Input.GetKeyDown(KeyCode.A))
        {
            isPressingA = true;
            pressTimeA = Time.time; // 记录按下 A 键的时刻

            if (facingRight)
            {
                // 如果当前朝右，需要转身
                facingRight = false;
                sr.flipX = true; // 将图片在 X 轴镜像翻转，使其朝左
            }
            else
            {
                // 如果已经朝左，跳过等待直接移动
                pressTimeA = Time.time - turnDelay;
            }
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            isPressingA = false;
        }
    }

    // 物理相关的移动建议放在 FixedUpdate 中
    void FixedUpdate()
    {
        // 如果不是行动模式，直接退出，不赋予速度
        if (GameManager.Instance.CurrentState != GameState.Action)
        {
            // 确保物理速度归零，并且动画停止
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        // ... 下面保留你原本计算 velocityX 的代码 ...
        float velocityX = 0f;
        // --- 【新增】用于标记当前是否在移动的变量 ---
        bool currentMoving = false;

        // 判断 D 键是否一直按着，并且按下的时间已经超过了 turnDelay (0.2秒)
        if (isPressingD && (Time.time - pressTimeD >= turnDelay))
        {
            velocityX = moveSpeed;
            currentMoving = true; // 正在向右移动
        }
        // 判断 A 键是否一直按着，并且按下的时间已经超过了 turnDelay (0.2秒)
        else if (isPressingA && (Time.time - pressTimeA >= turnDelay))
        {
            velocityX = -moveSpeed;
            currentMoving = true; // 正在向左移动
        }

        // 保持原有的 Y 轴速度（保留重力下落），仅修改 X 轴速度
        rb.velocity = new Vector2(velocityX, rb.velocity.y);

        // --- 【新增】将移动状态通知给 Animator ---
        // 类似于 C++ 中的 anim->SetBool("isWalking", currentMoving);
        if (anim != null) // 增强健壮性，防止忘记挂载组件报错
        {
            anim.SetBool("isWalking", currentMoving);
        }
    }
}