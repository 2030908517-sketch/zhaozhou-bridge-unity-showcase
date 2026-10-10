using System.Linq;
using UnityEngine;

/// <summary>
/// 赵州桥修复模式点击检测核心脚本
/// 核心功能：在修复模式下通过鼠标点击3D高亮部位触发修复，替代传统拖动交互
/// 适配任务三「桥体修复」的点击交互逻辑，关联StoryController完成修复流程
/// </summary>
public class ClickRepairDetector : MonoBehaviour
{
    [Header("核心配置（必绑）")]
    [Tooltip("修复模式专用相机（BridgeFixCamera，用于射线检测的视角基准）")]
    public Camera fixCamera;
    [Tooltip("需要修复的部位数组（FixPart_1/FixPart_2/FixPart_3，与StoryController的fixParts对应）")]
    public Transform[] fixParts;
    [Tooltip("剧情总控制器（用于调用FixBridgePart方法完成修复逻辑）")]
    public StoryController storyController;
    [Tooltip("射线检测最大距离（根据场景尺寸调整，默认100米）")]
    public float clickCheckDistance = 100f;

    [Header("调试配置（可选）")]
    [Tooltip("是否在Scene视图显示检测射线（调试用，发布时建议关闭）")]
    public bool showDebugRay = true;
    [Tooltip("调试射线的显示颜色（默认红色）")]
    public Color rayColor = Color.red;

    [Header("运行状态（只读）")]
    [Tooltip("是否处于修复模式（由StoryController控制开关）")]
    public bool isInRepairMode = false;

    /// <summary>
    /// 帧更新：仅在修复模式下检测鼠标左键点击，触发修复检测
    /// </summary>
    void Update()
    {
        // 非修复模式下直接返回，不执行任何检测逻辑
        if (!isInRepairMode) return;

        // 检测玩家鼠标左键点击操作
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickRepair();
        }
    }

    /// <summary>
    /// 核心检测逻辑：判断点击是否命中修复部位，命中则触发修复
    /// </summary>
    private void CheckClickRepair()
    {
        // 空值校验：修复相机未绑定则报错并终止
        if (fixCamera == null)
        {
            UnityEngine.Debug.LogError("[ClickRepairDetector] fixCamera未绑定！请在Inspector面板绑定BridgeFixCamera");
            return;
        }

        // 从修复相机向鼠标点击位置发射射线
        Ray ray = fixCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 调试功能：在Scene视图绘制检测射线，持续2秒便于观察
        if (showDebugRay)
        {
            UnityEngine.Debug.DrawRay(ray.origin, ray.direction * clickCheckDistance, rayColor, 2f);
        }

        // 执行射线检测：判断是否命中有效物体
        if (Physics.Raycast(ray, out hit, clickCheckDistance))
        {
            // 调试日志：打印命中物体名称和所在图层，便于排查未命中问题
            UnityEngine.Debug.Log($"[ClickRepairDetector] 射线命中物体：{hit.transform.name}，所在图层：{LayerMask.LayerToName(hit.transform.gameObject.layer)}");

            // 空值校验：修复部位数组未配置则报错并终止
            if (fixParts == null || fixParts.Length == 0)
            {
                UnityEngine.Debug.LogError("[ClickRepairDetector] 修复部位数组fixParts未绑定！请添加FixPart_1/2/3");
                return;
            }

            // 遍历修复部位数组，判断是否命中目标部位
            for (int i = 0; i < fixParts.Length; i++)
            {
                // 跳过空的修复部位（避免空引用）
                if (fixParts[i] == null) continue;

                // 命中判断：直接命中部位本体 或 命中部位的子物体（兼容复杂模型）
                if (hit.transform == fixParts[i] || hit.transform.IsChildOf(fixParts[i]))
                {
                    // 空值校验：剧情控制器未绑定则报错并终止
                    if (storyController == null)
                    {
                        UnityEngine.Debug.LogError("[ClickRepairDetector] storyController未绑定！请绑定StoryController脚本");
                        return;
                    }

                    // 调用剧情控制器的修复方法，传入部位索引完成修复
                    storyController.FixBridgePart(i);
                    UnityEngine.Debug.Log($"[ClickRepairDetector] 成功触发部位{i + 1}修复！");
                    return; // 命中后直接返回，避免重复检测
                }
            }

            // 命中物体但非修复部位：打印警告日志，提示当前配置的修复部位
            string partNames = string.Join(",", fixParts.Select(t => t?.name ?? "空部位"));
            UnityEngine.Debug.LogWarning($"[ClickRepairDetector] 命中的不是修复部位！当前配置的修复部位：{partNames}");
        }
        else
        {
            // 射线未命中任何物体：打印警告日志，提示排查方向
            UnityEngine.Debug.LogWarning("[ClickRepairDetector] 射线未命中任何物体！请检查：1.修复部位是否有碰撞体 2.检测距离是否足够 3.相机图层是否正确");
        }
    }

    /// <summary>
    /// 修复模式开关（由StoryController调用）
    /// 控制是否开启点击检测逻辑，适配修复模式的进入/退出
    /// </summary>
    /// <param name="isOn">true=开启修复检测，false=关闭修复检测</param>
    public void SetRepairDetection(bool isOn)
    {
        isInRepairMode = isOn;
        // 打印状态日志，便于调试模式切换问题
        UnityEngine.Debug.Log($"[ClickRepairDetector] 修复检测状态：{(isOn ? "开启" : "关闭")}（点击高亮部位可修复）");
    }
}
