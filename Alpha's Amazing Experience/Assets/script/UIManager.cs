using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI 引用")]
    public Transform inputBox;
    public TextMeshProUGUI inputText;

    [Header("跟随目标与参数")]
    public Transform player;
    public Vector3 offsetLeft = new Vector3(-1.5f, 2f, 0f);
    public Vector3 offsetRight = new Vector3(1.5f, 2f, 0f);
    public float worldLeftEdgeThreshold = -8f;

    void Start()
    {
        inputBox.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
        InputManager.OnInputTextChanged += UpdateInputText;
    }

    void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
        InputManager.OnInputTextChanged -= UpdateInputText;
    }

    void HandleStateChanged(GameState newState)
    {
        inputBox.gameObject.SetActive(newState == GameState.Thinking);

        // 【新增保护】每次刚进入思考模式时，强制刷新一次文字
        if (newState == GameState.Thinking && inputText != null)
        {
            inputText.text = "";
        }
    }

    void UpdateInputText(string newText)
    {
        if (inputText != null)
        {
            inputText.text = newText;

            // 【核心逻辑】强制 TMP 立即计算一次最新文字的排版数据
            inputText.ForceMeshUpdate();

            // 判断：如果文字的实际显示宽度，超过了文本框的 RectTransform 宽度
            if (inputText.preferredWidth > inputText.rectTransform.rect.width)
            {
                // 文字太长：水平靠右对齐。多余的左侧文字会被 Masking 隐藏
                inputText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            }
            else
            {
                // 文字较短：水平靠左对齐，正常显示
                inputText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            }

            Debug.Log("UI 接收到文字更新: " + newText);
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance.CurrentState != GameState.Thinking || !inputBox.gameObject.activeSelf) return;
        if (player == null) return;

        if (player.position.x < worldLeftEdgeThreshold)
        {
            // --- 靠近左侧边缘：UI 挂在右上角 ---
            inputBox.position = player.position + offsetRight;

            // 1. 将背景框绕 Y 轴旋转 180 度（实现气泡尾巴翻转）
            inputBox.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // 2. 将文字再绕 Y 轴旋转 180 度（负负得正，保证文字依然从左到右正常阅读）
            if (inputText != null)
            {
                inputText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else
        {
            // --- 正常情况：UI 挂在左上角 ---
            inputBox.position = player.position + offsetLeft;

            // 恢复正常旋转 (0度)
            inputBox.localRotation = Quaternion.Euler(0f, 0f, 0f);
            if (inputText != null)
            {
                inputText.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
    }
}