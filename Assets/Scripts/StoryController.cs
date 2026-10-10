using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 赵州桥修复游戏总控制器脚本
/// 核心功能：统筹全流程任务逻辑（任务1收集构件/任务2知识答题/任务3桥体修复）、UI面板控制、视角/光标管理、音效/动画反馈
/// 作为游戏的核心中枢，串联所有交互模块，实现剧情推进与玩家操作的闭环
/// </summary>
public class StoryController : MonoBehaviour
{
    #region 1. 核心UI与模块引用（按任务分类）
    [Header("任务1 - 构件收集引用")]
    [Tooltip("主角内心独白面板")]
    public GameObject thoughtPanel;
    [Tooltip("内心独白文本")]
    public TextMeshProUGUI thoughtText;
    [Tooltip("NPC对话面板")]
    public GameObject dialogPanel;
    [Tooltip("NPC名称文本")]
    public TextMeshProUGUI npcNameText;
    [Tooltip("NPC对话内容文本")]
    public TextMeshProUGUI npcContentText;
    [Tooltip("任务1 UI面板（收集进度展示）")]
    public GameObject task1UI;
    [Tooltip("构件收集核心脚本引用")]
    public BridgePartCollection bridgePartCollection;

    [Header("任务2 - 知识答题引用")]
    [Tooltip("任务2 UI面板（答题引导）")]
    public GameObject task2UI;
    [Tooltip("答题弹窗面板")]
    public GameObject quizPanel;
    [Tooltip("答题标题文本（题目）")]
    public TextMeshProUGUI quizTitleText;
    [Tooltip("答题选项按钮1")]
    public Button option1Btn;
    [Tooltip("答题选项按钮2")]
    public Button option2Btn;
    [Tooltip("答题选项按钮3")]
    public Button option3Btn;
    [Tooltip("选项1文本")]
    public TextMeshProUGUI option1Text;
    [Tooltip("选项2文本")]
    public TextMeshProUGUI option2Text;
    [Tooltip("选项3文本")]
    public TextMeshProUGUI option3Text;

    [Header("任务3 - 桥体修复引用")]
    [Tooltip("任务3 UI面板（修复引导）")]
    public GameObject task3UI;
    [Tooltip("修复模式面板")]
    public GameObject repairPanel;
    [Tooltip("修复模式专用相机（BridgeFixCamera）")]
    public Transform bridgeFixCamera;
    [Tooltip("主相机（自由探索视角）")]
    private Camera mainCamera;
    [Tooltip("修复部位数组（FixPart_1/2/3）")]
    public Transform[] fixParts;
    [Tooltip("修复部位高亮灯光数组（与fixParts一一对应）")]
    public Light[] fixPartLights;
    [Tooltip("点击修复检测脚本引用")]
    public ClickRepairDetector clickRepairDetector;

    [Header("音效配置（核心）")]
    [Tooltip("单个部位修复音效（敲击/修复声）")]
    public AudioClip repairClip;
    [Tooltip("全局背景音乐（BGM）")]
    public AudioClip bgmClip;
    [Tooltip("修复音效专用音源（独立播放，不影响BGM）")]
    public AudioSource repairAudioSource;
    [Tooltip("BGM专用音源（全局循环，不受场景切换影响）")]
    public AudioSource bgmAudioSource;
    #endregion

    #region 2. 状态控制变量（按任务分类）
    // 基础交互状态
    private bool isNearNPC = false;          // 是否靠近NPC
    private int dialogIndex = 0;             // 通用对话索引

    // 任务1状态
    private bool isTask1Accepted = false;    // 是否接取任务1
    private bool isTask1Completed = false;   // 是否完成任务1

    // 任务2状态
    private bool isTask2Started = false;     // 是否开始任务2
    private int quizIndex = 0;               // 答题索引
    private bool isAnswering = false;        // 是否正在答题
    private int currentCorrectIndex;         // 当前题目正确答案索引
    private bool isTask2Completed = false;   // 是否完成任务2

    // 任务3状态
    private bool isTask3Started = false;     // 是否接取任务3
    private bool isTask3Fixing = false;      // 是否处于修复模式
    private int fixedPartCount = 0;          // 已修复部位数量
    private int totalFixParts = 3;           // 总修复部位数量
    private int task3DialogIndex = 0;        // 任务3对话索引

    // 任务3完成后教育对话状态
    private bool isTask3CompleteDialogShowing = false; // 是否显示修复完成教育对话
    private int task3CompleteDialogIndex = 0;          // 教育对话索引
    #endregion

    #region 3. 剧情文本配置（对话/题库）
    /// <summary>
    /// 任务1接取对话（主角与老者初次交互）
    /// </summary>
    private string[][] task1AcceptDialog = new string[][]
    {
        new string[] {"老者：", "你的衣着好生奇怪。"},
        new string[] {"主角：", "老人家，这里是哪？头好晕..."},
        new string[] {"老者：", "这是赵州桥，我是守桥人。"},
        new string[] {"主角：", "难道我穿越了..."},
        new string[] {"老者：", "看到地图上的光柱了吗？去地图上寻找赵州桥的构件吧！"},
        new string[] {"主角：", "我这就去找！"},
        new string[] {"老者：", "哈哈哈好！孺子可教啊。"}
    };

    /// <summary>
    /// 任务1提交对话（收集完构件后与老者交互）
    /// </summary>
    private string[][] task1SubmitDialog = new string[][]
    {
        new string[] {"主角：", "老人家，我把赵州桥的构件都找回来了！"},
        new string[] {"老者：", "太好了！果然是个靠谱的年轻人～"},
        new string[] {"老者：", "不过要确认你真的了解这些构件，我得考考你！"},
        new string[] {"主角：", "尽管问！我收集的时候都了解过了～"},
        new string[] {"老者：", "那我们开始吧！"}
    };

    /// <summary>
    /// 任务2知识测验题库（题目/选项1/选项2/选项3/正确答案索引）
    /// </summary>
    private string[][] task2Quiz = new string[][]
    {
        new string[] {"赵州桥的拱券采用的是什么设计？", "敞肩拱", "实肩拱", "半圆拱", "0"},
        new string[] {"仰天石主要安装在赵州桥的哪个位置？", "桥面两侧", "桥拱顶部", "桥基处", "0"},
        new string[] {"栏板上最具特色的雕刻图案是？", "龙纹", "花卉纹", "人物纹", "0"}
    };

    /// <summary>
    /// 任务3接取对话（答题完成后开启修复任务）
    /// </summary>
    private string[][] task3Dialog = new string[][]
    {
        new string[] {"老者：", "年轻人，你不仅找回来了构件，还对赵州桥了如指掌！"},
        new string[] {"主角：", "都是您的指点，多谢老人家～"},
        new string[] {"老者：", "哈哈哈！赵州桥年久失修，麻烦你帮忙修复几处破损吧！"},
        new string[] {"主角：", "好！我这就动手修复～"},
        new string[] {"系统：", "你接取了任务三【修复赵州桥】：修复赵州桥的3处破损部位！"}
    };

    /// <summary>
    /// 任务3完成后教育对话（传递古建筑保护理念）
    /// </summary>
    private string[][] task3CompleteDialog = new string[][]
    {
        new string[] {"老者：", "赵州桥终于恢复原貌了，多谢你啊年轻人！"},
        new string[] {"主角：", "这是我应该做的，赵州桥真的太壮观了！"},
        new string[] {"老者：", "只是如今像这样的古建筑，大多年久失修，懂修缮技艺的人也越来越少咯..."},
        new string[] {"主角：", "这可太可惜了...那该怎么办呢？"},
        new string[] {"老者：", "得靠你们这些年轻人啊！多了解、多学习古建筑的知识，把修缮的手艺传下去，才能护住这些老祖宗留下的宝贝。"},
        new string[] {"老者：", "这些物质文化遗产，是咱们民族的根，可不能在咱们这代断了啊！"},
        new string[] {"主角：", "我明白了！我一定会把这些故事和技艺告诉更多年轻人的！"},
        new string[] {"老者：", "好！好！有你这句话，我就放心了～以后常来看看赵州桥啊！"}
    };
    #endregion

    #region 4. 视角控制引用
    /// <summary>
    /// 自由视角相机控制脚本（用于锁定/解锁视角）
    /// </summary>
    private NoClipCamera noClipCamera;
    #endregion

    #region 5. 初始化逻辑（Start）
    /// <summary>
    /// 初始化：隐藏所有UI、配置音效、绑定事件、启动出生剧情
    /// </summary>
    void Start()
    {
        // 初始化隐藏所有UI面板（避免初始界面混乱）
        if (thoughtPanel != null) thoughtPanel.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (task1UI != null) task1UI.SetActive(false);
        if (task2UI != null) task2UI.SetActive(false);
        if (task3UI != null) task3UI.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(false);
        if (repairPanel != null) repairPanel.SetActive(false);

        // 获取视角控制脚本（用于答题/修复时锁定视角）
        noClipCamera = GetComponent<NoClipCamera>();
        // 获取主相机（自由探索视角）
        mainCamera = Camera.main;

        // ========== 音效初始化（核心：保证BGM独立循环，修复音效单次播放） ==========
        // 1. BGM音源初始化（挂载在当前对象，不受相机激活状态影响）
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = 0.4f;               // BGM音量（避免盖过修复音效）
        bgmAudioSource.loop = true;                 // 强制循环播放
        bgmAudioSource.playOnAwake = false;         // 手动控制播放，避免自动停止
        bgmAudioSource.mute = false;                // 确保不静音
        bgmAudioSource.ignoreListenerPause = true;  // 忽略全局音频暂停，保证BGM不中断
        // 启动BGM并打印日志（便于调试）
        if (bgmClip != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
            UnityEngine.Debug.Log("BGM已启动播放");
        }

        // 2. 修复音效音源初始化
        if (repairAudioSource == null)
        {
            repairAudioSource = gameObject.AddComponent<AudioSource>();
        }
        repairAudioSource.volume = 1.0f;            // 修复音效音量（确保清晰可闻）
        repairAudioSource.playOnAwake = false;      // 不自动播放
        repairAudioSource.mute = false;             // 确保不静音
        repairAudioSource.loop = false;             // 单次播放（避免循环）

        // 自动获取构件收集脚本（降低手动赋值成本）
        if (bridgePartCollection == null)
        {
            bridgePartCollection = GetComponent<BridgePartCollection>();
        }

        // 绑定答题按钮点击事件（关联答题逻辑）
        if (option1Btn != null) option1Btn.onClick.AddListener(() => AnswerQuiz(0));
        if (option2Btn != null) option2Btn.onClick.AddListener(() => AnswerQuiz(1));
        if (option3Btn != null) option3Btn.onClick.AddListener(() => AnswerQuiz(2));

        // 启动出生剧情（1秒后显示内心独白）
        Invoke("ShowBirthThought", 1f);
    }
    #endregion

    #region 6. 出生剧情逻辑
    /// <summary>
    /// 显示出生内心独白（初始剧情第一步）
    /// </summary>
    void ShowBirthThought()
    {
        thoughtText.text = "头好晕，我这是在哪";
        thoughtPanel.SetActive(true);
        // 3秒后显示下一步提示
        Invoke("ShowNPCTip", 3f);
    }

    /// <summary>
    /// 显示靠近NPC的提示（初始剧情第二步）
    /// </summary>
    void ShowNPCTip()
    {
        thoughtText.text = "前面好像有位老者，过去看看";
        thoughtPanel.SetActive(true);
        // 3秒后隐藏内心独白面板
        Invoke("HideThoughtPanel", 3f);
    }

    /// <summary>
    /// 隐藏内心独白面板
    /// </summary>
    void HideThoughtPanel()
    {
        thoughtPanel.SetActive(false);
    }
    #endregion

    #region 7. NPC交互检测（碰撞触发）
    /// <summary>
    /// 靠近NPC时触发：显示对应交互提示
    /// </summary>
    /// <param name="other">碰撞到的物体（仅响应OldMan_NPC）</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "OldMan_NPC")
        {
            isNearNPC = true;

            // 根据任务进度显示不同提示文本
            if (!isTask1Accepted)
            {
                thoughtText.text = "按左键和老者对话";
            }
            else if (isTask1Accepted && !isTask1Completed)
            {
                thoughtText.text = "先找齐3个构件再来吧～";
            }
            else if (isTask1Completed && !isTask2Started && !isTask2Completed)
            {
                thoughtText.text = "按左键和老者交任务";
            }
            else if (isTask2Started && !isTask2Completed)
            {
                thoughtText.text = "按左键开始答题";
            }
            else if (isTask2Completed && !isTask3Started)
            {
                thoughtText.text = "按左键和老者开启任务三";
            }
            else if (isTask3Started && !isTask3Fixing)
            {
                thoughtText.text = "按左键进入赵州桥修复界面";
            }

            thoughtPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 离开NPC时触发：隐藏面板、解锁视角
    /// </summary>
    /// <param name="other">离开的物体（仅响应OldMan_NPC）</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "OldMan_NPC")
        {
            isNearNPC = false;
            dialogPanel.SetActive(false);
            thoughtPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(false);
            UnlockCamera();          // 解锁视角（恢复自由探索）
            isAnswering = false;     // 终止答题状态
            isTask3CompleteDialogShowing = false; // 终止教育对话状态
        }
    }
    #endregion

    #region 8. 帧更新逻辑（核心交互检测）
    /// <summary>
    /// 帧更新：检测BGM状态、任务进度、玩家点击交互
    /// </summary>
    void Update()
    {
        // 核心保障：帧检测BGM状态，停止则自动重启（避免BGM中断）
        if (bgmAudioSource != null && bgmClip != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
            UnityEngine.Debug.Log("检测到BGM停止，已重新启动");
        }

        // 检测任务1完成状态（收集3个构件则标记完成）
        if (bridgePartCollection != null && bridgePartCollection.collectedCount == 3)
        {
            isTask1Completed = true;
        }

        // 检测玩家左键点击（仅靠近NPC时生效）
        if (Input.GetMouseButtonDown(0) && isNearNPC)
        {
            thoughtPanel.SetActive(false);

            // 优先显示任务3完成后的教育对话
            if (isTask3CompleteDialogShowing)
            {
                ShowTask3CompleteDialog();
                return;
            }

            // 根据任务进度触发对应逻辑
            if (!isTask1Accepted)
            {
                ShowTask1AcceptDialog();          // 接取任务1
            }
            else if (isTask1Completed && !isTask2Started && !isTask2Completed)
            {
                ShowTask1SubmitDialog();          // 提交任务1，开启任务2
            }
            else if (isTask2Started && !isAnswering && !isTask2Completed)
            {
                ShowQuizQuestion();               // 显示答题弹窗
            }
            else if (isTask2Completed && !isTask3Started)
            {
                ShowTask3Dialog();                // 开启任务3
            }
            else if (isTask3Started && !isTask3Fixing)
            {
                EnterRepairMode();                // 进入修复模式
            }
        }
    }
    #endregion

    #region 9. 任务1逻辑（接取/提交）
    /// <summary>
    /// 显示任务1接取对话（初次与NPC交互）
    /// </summary>
    void ShowTask1AcceptDialog()
    {
        dialogPanel.SetActive(true);

        // 逐句显示对话，完成后接取任务1
        if (dialogIndex < task1AcceptDialog.Length)
        {
            npcNameText.text = task1AcceptDialog[dialogIndex][0];
            npcContentText.text = task1AcceptDialog[dialogIndex][1];
            dialogIndex++;
        }
        else
        {
            AcceptTask1();
        }
    }

    /// <summary>
    /// 接取任务1：更新状态、显示任务UI、隐藏对话面板
    /// </summary>
    void AcceptTask1()
    {
        isTask1Accepted = true;
        npcNameText.text = "系统：";
        npcContentText.text = "你接取了任务【找回赵州桥构件】：找到拱券、仰天石、栏板并收集";
        task1UI.SetActive(true);          // 显示任务1 UI（收集进度）
        Invoke("HideDialogPanel", 3f);    // 3秒后隐藏对话面板
        dialogIndex = 0;                  // 重置对话索引
    }

    /// <summary>
    /// 显示任务1提交对话（收集完构件后）
    /// </summary>
    void ShowTask1SubmitDialog()
    {
        dialogPanel.SetActive(true);

        // 逐句显示对话，完成后开启任务2
        if (dialogIndex < task1SubmitDialog.Length)
        {
            npcNameText.text = task1SubmitDialog[dialogIndex][0];
            npcContentText.text = task1SubmitDialog[dialogIndex][1];
            dialogIndex++;
        }
        else
        {
            StartTask2();
        }
    }

    /// <summary>
    /// 开启任务2：更新状态、切换UI、提示答题
    /// </summary>
    void StartTask2()
    {
        isTask2Started = true;
        npcNameText.text = "系统：";
        npcContentText.text = "任务二【赵州桥知识测验】开启！按左键开始答题～";
        if (task1UI != null) task1UI.SetActive(false);  // 隐藏任务1 UI
        if (task2UI != null) task2UI.SetActive(true);    // 显示任务2 UI

        Invoke("HideDialogPanel", 3f);    // 3秒后隐藏对话面板
        dialogIndex = 0;                  // 重置对话索引
    }
    #endregion

    #region 10. 任务2逻辑（知识答题）
    /// <summary>
    /// 显示答题弹窗：锁定视角、加载题目、初始化选项
    /// </summary>
    void ShowQuizQuestion()
    {
        // 所有题目答完：标记任务2完成，显示完成提示
        if (quizIndex >= task2Quiz.Length)
        {
            isTask2Completed = true;
            isTask2Started = false;
            quizPanel.SetActive(false);
            dialogPanel.SetActive(true);
            npcNameText.text = "老者：";
            npcContentText.text = "全部答对了！你果然认真了解了赵州桥～";
            quizIndex = 0;               // 重置答题索引
            UnlockCamera();              // 解锁视角
            return;
        }

        LockCamera();                    // 锁定视角（避免答题时移动）
        isAnswering = true;              // 标记正在答题
        quizPanel.SetActive(true);       // 显示答题弹窗

        // 加载当前题目数据
        string[] currentQuiz = task2Quiz[quizIndex];
        currentCorrectIndex = int.Parse(currentQuiz[4]); // 解析正确答案索引

        // 赋值题目和选项文本
        quizTitleText.text = currentQuiz[0];
        option1Text.text = currentQuiz[1];
        option2Text.text = currentQuiz[2];
        option3Text.text = currentQuiz[3];
    }

    /// <summary>
    /// 答题逻辑：判断答案对错，处理后续流程
    /// </summary>
    /// <param name="selectIndex">玩家选择的选项索引</param>
    void AnswerQuiz(int selectIndex)
    {
        isAnswering = false;             // 终止答题状态
        string[] currentQuiz = task2Quiz[quizIndex];

        // 答案正确：提示正确，进入下一题
        if (selectIndex == currentCorrectIndex)
        {
            quizTitleText.text = "回答正确！";
            option1Btn.interactable = false;
            option2Btn.interactable = false;
            option3Btn.interactable = false;
            Invoke("NextQuiz", 1f);      // 1秒后加载下一题
        }
        // 答案错误：提示正确答案，重新答题
        else
        {
            quizTitleText.text = $"答错啦！正确答案是：{currentQuiz[currentCorrectIndex + 1]}";
            option1Btn.interactable = false;
            option2Btn.interactable = false;
            option3Btn.interactable = false;
            Invoke("RetryQuiz", 1f);     // 1秒后重新加载当前题
        }
    }

    /// <summary>
    /// 加载下一题：更新索引、重置按钮、显示新题目
    /// </summary>
    void NextQuiz()
    {
        quizIndex++;
        ResetQuizBtn();
        ShowQuizQuestion();
    }

    /// <summary>
    /// 重新答题：重置按钮、重新显示当前题
    /// </summary>
    void RetryQuiz()
    {
        ResetQuizBtn();
        ShowQuizQuestion();
    }

    /// <summary>
    /// 重置答题按钮状态（恢复可点击）
    /// </summary>
    void ResetQuizBtn()
    {
        option1Btn.interactable = true;
        option2Btn.interactable = true;
        option3Btn.interactable = true;
    }
    #endregion

    #region 11. 任务3逻辑（桥体修复）
    /// <summary>
    /// 显示任务3接取对话（答题完成后）
    /// </summary>
    void ShowTask3Dialog()
    {
        dialogPanel.SetActive(true);

        // 逐句显示对话，完成后开启任务3
        if (task3DialogIndex < task3Dialog.Length)
        {
            npcNameText.text = task3Dialog[task3DialogIndex][0];
            npcContentText.text = task3Dialog[task3DialogIndex][1];
            task3DialogIndex++;
        }
        else
        {
            StartTask3();
            Invoke("HideDialogPanel", 3f);    // 3秒后隐藏对话面板
            task3DialogIndex = 0;             // 重置对话索引
        }
    }

    /// <summary>
    /// 开启任务3：更新状态、切换UI、显示修复提示
    /// </summary>
    void StartTask3()
    {
        isTask3Started = true;
        if (task2UI != null) task2UI.SetActive(false);  // 隐藏任务2 UI
        if (task3UI != null) task3UI.SetActive(true);    // 显示任务3 UI

        fixedPartCount = 0;                             // 重置修复计数
        thoughtText.text = "按左键进入赵州桥修复界面";
        thoughtPanel.SetActive(true);
    }

    /// <summary>
    /// 进入修复模式：切换相机、开启灯光、显示修复面板、解锁光标
    /// </summary>
    void EnterRepairMode()
    {
        isTask3Fixing = true;
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);         // 关闭主相机
        if (bridgeFixCamera != null) bridgeFixCamera.gameObject.SetActive(true); // 开启修复相机

        // 开启所有修复部位高亮灯光（引导玩家点击）
        if (fixPartLights != null && fixPartLights.Length > 0)
        {
            for (int i = 0; i < fixPartLights.Length; i++)
            {
                if (fixPartLights[i] != null)
                {
                    fixPartLights[i].gameObject.SetActive(true);
                }
            }
        }

        if (repairPanel != null) repairPanel.SetActive(true); // 显示修复面板
        Cursor.lockState = CursorLockMode.None;               // 解锁光标（便于点击）
        Cursor.visible = true;                                // 显示光标

        // 开启点击修复检测（由ClickRepairDetector处理点击逻辑）
        if (clickRepairDetector != null)
        {
            clickRepairDetector.SetRepairDetection(true);
        }
    }

    /// <summary>
    /// 退出修复模式：恢复相机、关闭灯光、隐藏面板、显示教育对话
    /// </summary>
    void ExitRepairMode()
    {
        isTask3Fixing = false;
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);           // 恢复主相机
        if (bridgeFixCamera != null) bridgeFixCamera.gameObject.SetActive(false); // 关闭修复相机

        // 关闭所有修复部位高亮灯光
        if (fixPartLights != null && fixPartLights.Length > 0)
        {
            for (int i = 0; i < fixPartLights.Length; i++)
            {
                if (fixPartLights[i] != null)
                {
                    fixPartLights[i].gameObject.SetActive(false);
                }
            }
        }

        if (repairPanel != null) repairPanel.SetActive(false); // 隐藏修复面板
        Cursor.lockState = CursorLockMode.Locked;              // 锁定光标
        Cursor.visible = false;                                // 隐藏光标

        // 关闭点击修复检测
        if (clickRepairDetector != null)
        {
            clickRepairDetector.SetRepairDetection(false);
        }

        // 靠近NPC则显示教育对话，否则显示提示文本
        if (dialogPanel != null && isNearNPC)
        {
            dialogPanel.SetActive(true);
            isTask3CompleteDialogShowing = true;
            ShowTask3CompleteDialog();
        }
        else
        {
            thoughtText.text = "赵州桥修复完成！请和老者对话～";
            thoughtPanel.SetActive(true);
            Invoke("HideThoughtPanel", 3f);
        }

        if (task3UI != null) task3UI.SetActive(false); // 隐藏任务3 UI
    }

    /// <summary>
    /// 显示任务3完成后的教育对话（传递古建筑保护理念）
    /// </summary>
    void ShowTask3CompleteDialog()
    {
        // 对话显示完毕：隐藏面板、重置状态、显示最终提示
        if (task3CompleteDialogIndex >= task3CompleteDialog.Length)
        {
            dialogPanel.SetActive(false);
            isTask3CompleteDialogShowing = false;
            task3CompleteDialogIndex = 0;

            thoughtText.text = "恭喜你完成了全部任务！一起守护古建筑，让历史与未来对话！";
            thoughtPanel.SetActive(true);
            Invoke("HideThoughtPanel", 3f);
            return;
        }

        // 逐句显示教育对话
        npcNameText.text = task3CompleteDialog[task3CompleteDialogIndex][0];
        npcContentText.text = task3CompleteDialog[task3CompleteDialogIndex][1];
        task3CompleteDialogIndex++;
    }

    /// <summary>
    /// 核心修复逻辑：关闭灯光、播放动画/音效、更新进度、判断是否完成
    /// （由ClickRepairDetector调用，传入修复部位索引）
    /// </summary>
    /// <param name="partIndex">修复部位索引（0/1/2对应FixPart_1/2/3）</param>
    public void FixBridgePart(int partIndex)
    {
        // 安全校验：索引越界或部位为空则终止
        if (partIndex < 0 || partIndex >= fixParts.Length || fixParts[partIndex] == null)
        {
            UnityEngine.Debug.LogWarning($"修复部位{partIndex + 1}不存在！");
            return;
        }

        // 跳过已修复的部位（避免重复修复）
        if (fixPartLights[partIndex] != null && !fixPartLights[partIndex].isActiveAndEnabled)
        {
            UnityEngine.Debug.LogWarning($"部位{partIndex + 1}已修复，无需重复修复");
            return;
        }

        // 关闭当前部位的高亮灯光（反馈修复成功）
        if (fixPartLights[partIndex] != null)
        {
            fixPartLights[partIndex].gameObject.SetActive(false);
        }

        // 播放修复动画和音效
        PlayRepairAnimation(partIndex);

        // 修复计数+1
        fixedPartCount++;

        // 显示修复进度提示
        if (thoughtPanel != null && thoughtText != null)
        {
            if (fixedPartCount >= totalFixParts)
            {
                thoughtText.text = $"已修复{fixedPartCount}/{totalFixParts}处，修复完成！";
            }
            else
            {
                thoughtText.text = $"已修复{fixedPartCount}/{totalFixParts}处，继续修复！";
            }
            thoughtPanel.SetActive(true);
            Invoke("HideThoughtPanel", 2f); // 2秒后隐藏提示
        }

        // 所有部位修复完成：1秒后退出修复模式
        if (fixedPartCount >= totalFixParts)
        {
            Invoke("ExitRepairMode", 1f);
            isTask3Started = false;
            fixedPartCount = 0; // 重置修复计数
        }
    }

    /// <summary>
    /// 播放修复动画：粒子特效+缩放动画+修复音效
    /// </summary>
    /// <param name="partIndex">修复部位索引</param>
    void PlayRepairAnimation(int partIndex)
    {
        Transform targetPart = fixParts[partIndex];
        if (targetPart == null)
        {
            UnityEngine.Debug.LogError($"部位{partIndex + 1}为空，无法播放动画");
            return;
        }

        // ========== 修复音效播放（核心：强制播放，打印日志） ==========
        if (repairAudioSource == null)
        {
            UnityEngine.Debug.LogError("修复音效音源未初始化！");
            // 兜底：重新创建音源
            repairAudioSource = gameObject.AddComponent<AudioSource>();
            repairAudioSource.volume = 1.0f;
        }

        if (repairClip == null)
        {
            UnityEngine.Debug.LogError("修复音效文件未赋值！请在Inspector面板拖入repairClip");
            return;
        }

        // 强制播放修复音效（忽略当前播放状态，保证每次修复都有音效）
        repairAudioSource.PlayOneShot(repairClip);
        UnityEngine.Debug.Log($"已播放修复部位{partIndex + 1}的音效");

        // 创建临时粒子特效（视觉反馈）
        CreateTempRepairEffect(targetPart.position);

        // 播放部位缩放动画（视觉反馈）
        StartCoroutine(ScaleAnimation(targetPart, 0.3f));
    }

    /// <summary>
    /// 创建临时修复粒子特效（黄色光点飞溅，模拟修复效果）
    /// </summary>
    /// <param name="position">特效生成位置（修复部位坐标）</param>
    private void CreateTempRepairEffect(Vector3 position)
    {
        GameObject effectObj = new GameObject("TempRepairEffect");
        effectObj.transform.position = position;

        // 添加粒子系统组件
        ParticleSystem particle = effectObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = effectObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit")); // 无光照材质，保证显示效果

        // 停止粒子后配置参数（避免参数无效）
        particle.Stop();

        // 粒子主参数配置
        var main = particle.main;
        main.duration = 0.6f;         // 特效持续时间
        main.loop = false;            // 不循环
        main.startSize = 0.25f;       // 粒子初始大小
        main.startColor = new Color(1f, 0.8f, 0f, 0.9f); // 黄色（接近石材修复效果）
        main.startSpeed = 3f;         // 粒子速度
        main.startLifetime = 0.4f;    // 粒子生命周期
        main.maxParticles = 60;       // 最大粒子数（保证效果）
        main.playOnAwake = false;     // 不自动播放

        // 粒子发射参数
        var emission = particle.emission;
        emission.rateOverTime = 80;   // 每秒发射数量

        // 粒子形状参数
        var shape = particle.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere; // 球形发射
        shape.radius = 0.15f;                             // 发射范围

        // 播放粒子特效
        particle.Play();

        // 特效结束后销毁对象（避免内存占用）
        Destroy(effectObj, main.duration + 0.1f);
        UnityEngine.Debug.Log("优化后粒子特效创建成功（更快+更多）");
    }

    /// <summary>
    /// 部位缩放动画：先放大1.5倍，再缩小回原尺寸（模拟修复归位）
    /// </summary>
    /// <param name="target">修复部位Transform</param>
    /// <param name="duration">动画总时长</param>
    /// <returns>协程迭代器</returns>
    IEnumerator ScaleAnimation(Transform target, float duration)
    {
        bool originalActive = target.gameObject.activeSelf; // 记录原始激活状态
        Vector3 originalScale = target.localScale;          // 记录原始尺寸
        Vector3 targetScale = originalScale * 1.5f;         // 目标放大尺寸

        // 放大阶段（占总时长的一半）
        float elapsed = 0f;
        while (elapsed < duration / 2)
        {
            target.gameObject.SetActive(true);
            target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 缩小阶段（占总时长的一半）
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复原始尺寸和激活状态
        target.localScale = originalScale;
        target.gameObject.SetActive(originalActive);
    }
    #endregion

    #region 12. 视角控制（锁定/解锁）
    /// <summary>
    /// 锁定视角：禁用移动/旋转，解锁并显示光标（答题/修复时使用）
    /// </summary>
    void LockCamera()
    {
        if (noClipCamera != null)
        {
            noClipCamera.canMoveCamera = false;
            noClipCamera.canRotateCamera = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 解锁视角：恢复移动/旋转，锁定并隐藏光标（回归自由探索）
    /// </summary>
    void UnlockCamera()
    {
        if (noClipCamera != null)
        {
            noClipCamera.canMoveCamera = true;
            noClipCamera.canRotateCamera = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    #endregion

    #region 13. 辅助方法
    /// <summary>
    /// 隐藏对话面板
    /// </summary>
    void HideDialogPanel()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }
    #endregion
}
