using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 赵州桥构件收集核心脚本
/// 核心功能：检测玩家与构件的碰撞、收集构件、显示构件知识弹窗、更新收集进度、控制视角/光标状态
/// 适配任务一「找回赵州桥构件」的全流程交互逻辑
/// </summary>
public class BridgePartCollection : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("任务提示文本（显示当前收集目标）")]
    public TextMeshProUGUI taskTipText;
    [Tooltip("收集进度文本（显示已收集/总数量）")]
    public TextMeshProUGUI progressText;
    [Tooltip("构件知识弹窗面板（收集后显示文化科普内容）")]
    public GameObject knowledgePanel;
    [Tooltip("知识弹窗内的科普文本")]
    public TextMeshProUGUI knowledgeText;

    [Header("视角控制引用")]
    [Tooltip("关联的无碰撞相机脚本（用于锁定/解锁视角）")]
    public NoClipCamera noClipCamera;

    [Header("收集进度配置")]
    [Tooltip("已收集的构件数量（初始0，最大3）")]
    public int collectedCount = 0;

    // 临时记录玩家当前靠近的构件（用于点击收集判断）
    private GameObject currentNearPart = null;
    // 构件对应的文化科普内容（与「拱券/仰天石/栏板」一一对应）
    private readonly string[] partKnowledge = new string[]
    {
        "拱券：赵州桥采用敞肩拱设计，是世界桥梁史上的创举，能减少水流冲击，提升桥的稳定性。",
        "仰天石：位于桥面两侧的石条，既起装饰作用，也能防止行人跌落，体现了实用与美学的结合。",
        "栏板：赵州桥的栏板雕刻有龙纹图案，是隋代石雕艺术的代表，兼具艺术价值与防护功能。"
    };

    /// <summary>
    /// 初始化：设置初始提示、隐藏弹窗、配置光标/视角
    /// </summary>
    void Start()
    {
        // 初始化任务提示文本
        if (taskTipText != null)
        {
            taskTipText.text = "任务：找到赵州桥的3个核心构件，了解它的建筑结构";
        }
        // 初始化收集进度显示
        UpdateProgressText();

        // 初始隐藏知识科普弹窗
        if (knowledgePanel != null)
        {
            knowledgePanel.SetActive(false);
        }

        // 初始化光标状态（隐藏+锁定，适配沉浸式探索）
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 自动获取NoClipCamera组件（同物体挂载时无需手动赋值）
        if (noClipCamera == null)
        {
            noClipCamera = GetComponent<NoClipCamera>();
        }
    }

    /// <summary>
    /// 碰撞检测：玩家靠近构件时触发，更新提示文本
    /// </summary>
    /// <param name="other">碰撞到的物体（仅响应拱券/仰天石/栏板）</param>
    private void OnTriggerEnter(Collider other)
    {
        // 仅响应指定的三个核心构件
        if (taskTipText != null && (other.gameObject.name == "拱券" || other.gameObject.name == "仰天石" || other.gameObject.name == "栏板"))
        {
            currentNearPart = other.gameObject; // 记录当前靠近的构件
            taskTipText.text = $"找到了【{other.gameObject.name}】，点击左键收集"; // 提示玩家点击收集
        }
    }

    /// <summary>
    /// 碰撞退出：玩家离开构件时，恢复初始任务提示
    /// </summary>
    /// <param name="other">离开的物体（仅响应拱券/仰天石/栏板）</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "拱券" || other.gameObject.name == "仰天石" || other.gameObject.name == "栏板")
        {
            currentNearPart = null; // 清空当前靠近的构件记录
            taskTipText.text = "任务：找到赵州桥的3个核心构件，了解它的建筑结构"; // 恢复初始提示
        }
    }

    /// <summary>
    /// 帧更新：检测鼠标点击，完成构件收集逻辑
    /// </summary>
    void Update()
    {
        // 条件：左键点击 + 靠近有效构件 + 构件未被收集（处于激活状态）
        if (Input.GetMouseButtonDown(0) && currentNearPart != null && currentNearPart.activeSelf)
        {
            currentNearPart.SetActive(false); // 隐藏构件（标记为已收集）
            collectedCount++; // 收集数量+1
            UpdateProgressText(); // 更新进度显示
            ShowKnowledge(currentNearPart.name); // 显示该构件的科普弹窗
            currentNearPart = null; // 清空当前构件记录

            // 收集完成（3个）：更新提示文本并标绿
            if (taskTipText != null && collectedCount == 3)
            {
                taskTipText.text = "恭喜你！集齐了赵州桥的核心构件！";
                taskTipText.color = Color.green;
            }
        }
    }

    /// <summary>
    /// 显示构件科普弹窗：锁定视角+解锁光标，展示文化知识
    /// </summary>
    /// <param name="partName">构件名称（拱券/仰天石/栏板）</param>
    private void ShowKnowledge(string partName)
    {
        // 空值校验：避免空引用报错
        if (knowledgeText == null || knowledgePanel == null || noClipCamera == null) return;

        // 根据构件名称匹配对应的科普内容
        switch (partName)
        {
            case "拱券":
                knowledgeText.text = partKnowledge[0];
                break;
            case "仰天石":
                knowledgeText.text = partKnowledge[1];
                break;
            case "栏板":
                knowledgeText.text = partKnowledge[2];
                break;
        }

        // 显示科普弹窗
        knowledgePanel.SetActive(true);
        // 锁定视角旋转（防止玩家弹窗期间移动视角，聚焦科普内容）
        noClipCamera.canRotateCamera = false;
        // 显示并解锁光标（便于玩家点击关闭弹窗）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// 关闭科普弹窗：解锁视角+恢复光标，回归探索状态
    /// （需绑定到弹窗的「关闭按钮」点击事件）
    /// </summary>
    public void CloseKnowledgePanel()
    {
        // 隐藏科普弹窗
        if (knowledgePanel != null)
        {
            knowledgePanel.SetActive(false);
        }

        // 解锁视角旋转（恢复自由探索）
        if (noClipCamera != null)
        {
            noClipCamera.canRotateCamera = true;
        }

        // 未收集完成时，恢复初始任务提示
        if (taskTipText != null && collectedCount < 3)
        {
            taskTipText.text = "任务：找到赵州桥的3个核心构件，了解它的建筑结构";
        }

        // 恢复光标状态（隐藏+锁定）
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// 更新收集进度文本：同步显示已收集数量/总数量
    /// </summary>
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"已收集：{collectedCount}/3";
        }
    }
}
