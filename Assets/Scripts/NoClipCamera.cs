using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无碰撞自由视角相机控制脚本
/// 核心功能：实现第一人称自由移动、鼠标视角旋转、碰撞检测防穿模、自动贴地行走
/// 支持移动/旋转锁定，适配答题/修复等场景的视角控制需求
/// </summary>
public class NoClipCamera : MonoBehaviour
{
    [Header("基础控制参数")]
    [Tooltip("相机移动速度（单位：米/秒）")]
    public float moveSpeed = 15f;
    [Tooltip("鼠标视角旋转速度")]
    public float rotateSpeed = 8f;
    [Tooltip("相机距离地面的高度（避免贴地）")]
    public float cameraHeight = 5f;
    [Tooltip("用于检测地面的图层（地形/桥面）")]
    public LayerMask detectLayer;
    [Tooltip("碰撞阻挡图层（树/桥体/石头等障碍物）")]
    public LayerMask obstacleLayer;

    [Header("视角初始配置")]
    [Tooltip("初始视角后移距离（避免出生点穿模）")]
    public float backDistance = 40f;

    // 视角垂直旋转角度（限制上下视角范围）
    private float xRotation = 0f;
    // 地形引用（兼容旧版贴地逻辑）
    private Transform terrainTransform;
    // 胶囊碰撞体（用于碰撞检测，模拟相机体积）
    private CapsuleCollider capsuleCol;

    [Header("视角锁定开关")]
    [Tooltip("是否允许鼠标旋转视角（答题时禁用）")]
    public bool canRotateCamera = true;
    [Tooltip("是否允许键盘移动相机（答题/修复时禁用）")]
    public bool canMoveCamera = true;

    /// <summary>
    /// 初始化：设置鼠标状态、碰撞体、初始出生点
    /// </summary>
    void Start()
    {
        // 隐藏并锁定鼠标（沉浸式视角控制）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化胶囊碰撞体（用于碰撞检测，防止相机穿模）
        capsuleCol = GetComponent<CapsuleCollider>();
        if (capsuleCol == null)
        {
            capsuleCol = gameObject.AddComponent<CapsuleCollider>();
            capsuleCol.radius = 1f;       // 碰撞体半径（模拟相机体积）
            capsuleCol.height = 2f;       // 碰撞体高度
            capsuleCol.center = new Vector3(0, 1f, 0); // 碰撞体中心偏移
        }

        // 自定义相机出生点（可修改X/Z适配场景）
        Vector3 customBornPos = new Vector3(70, 0, 90);
        // 射线检测地面高度（适配地形/桥面，避免悬空）
        Ray ray = new Ray(customBornPos + Vector3.up * 10f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, detectLayer))
        {
            transform.position = new Vector3(customBornPos.x, hit.point.y + cameraHeight, customBornPos.z);
        }
        else
        {
            // 射线检测失败时的兜底位置
            transform.position = new Vector3(customBornPos.x, 5, customBornPos.z);
        }
        // 初始后移视角，避免出生点穿模
        transform.Translate(Vector3.back * backDistance);
    }

    /// <summary>
    /// 帧更新：处理移动、旋转、碰撞检测、贴地逻辑
    /// </summary>
    void Update()
    {
        // ESC键恢复鼠标（退出沉浸式视角）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        float horizontal = 0; // 左右移动轴（A/D）
        float vertical = 0;   // 前后移动轴（W/S）

        // 仅当允许移动时，检测键盘输入
        if (canMoveCamera)
        {
            // 瞬时按键检测（无惯性，按下即移动，松开即停止）
            if (Input.GetKey(KeyCode.A)) horizontal = -1;
            if (Input.GetKey(KeyCode.D)) horizontal = 1;
            if (Input.GetKey(KeyCode.W)) vertical = 1;
            if (Input.GetKey(KeyCode.S)) vertical = -1;
        }

        // 计算移动方向（仅水平方向，避免上下移动）
        Vector3 moveDir = transform.forward * vertical + transform.right * horizontal;
        moveDir.y = 0;
        moveDir.Normalize(); // 标准化方向，防止斜向移动速度过快

        // 仅当允许移动时，执行移动+碰撞检测
        if (canMoveCamera)
        {
            // 预判目标位置，检测是否有障碍物
            Vector3 targetPos = transform.position + moveDir * moveSpeed * Time.deltaTime;
            if (!IsPositionObstructed(targetPos))
            {
                // 无碰撞：正常移动到目标位置
                transform.position = targetPos;
            }
            else
            {
                // 有碰撞：拆分X/Z轴单独检测，绕开障碍物
                Vector3 xDir = new Vector3(moveDir.x, 0, 0);
                Vector3 zDir = new Vector3(0, 0, moveDir.z);
                // 优先沿X轴移动（无碰撞则移动）
                if (!IsPositionObstructed(transform.position + xDir * moveSpeed * Time.deltaTime))
                {
                    transform.Translate(xDir * moveSpeed * Time.deltaTime, Space.World);
                }
                // X轴有碰撞则沿Z轴移动
                else if (!IsPositionObstructed(transform.position + zDir * moveSpeed * Time.deltaTime))
                {
                    transform.Translate(zDir * moveSpeed * Time.deltaTime, Space.World);
                }
            }
        }

        // 仅当允许旋转时，处理鼠标视角控制
        if (canRotateCamera)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed; // 水平旋转（左右移鼠标）
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed; // 垂直旋转（上下移鼠标）
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f); // 限制垂直视角（避免抬头/低头过度）
            transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y + mouseX, 0);
        }

        // 仅当允许移动时，执行贴地逻辑（保持相机高度）
        if (canMoveCamera)
        {
            GroundCamera();
        }
    }

    /// <summary>
    /// 检测目标位置是否被障碍物阻挡（防穿模核心方法）
    /// </summary>
    /// <param name="targetPos">待检测的目标位置</param>
    /// <returns>true=有阻挡，false=无阻挡</returns>
    bool IsPositionObstructed(Vector3 targetPos)
    {
        // 胶囊体碰撞检测：模拟相机体积，检测目标位置是否有障碍物
        return Physics.CheckCapsule(
            new Vector3(targetPos.x, targetPos.y - capsuleCol.height / 2, targetPos.z), // 胶囊体底部
            new Vector3(targetPos.x, targetPos.y + capsuleCol.height / 2, targetPos.z), // 胶囊体顶部
            capsuleCol.radius,                                                          // 胶囊体半径
            obstacleLayer                                                               // 检测的阻挡图层
        );
    }

    /// <summary>
    /// 贴地行走逻辑：保持相机与地面/桥面的固定高度
    /// </summary>
    void GroundCamera()
    {
        // 第一步：射线检测地面（兼容地形+桥面，优先方案）
        Ray ray = new Ray(transform.position + Vector3.up * 10f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, detectLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + cameraHeight, transform.position.z);
        }
        // 第二步：兼容旧逻辑（仅地形），兜底使用
        else if (terrainTransform != null)
        {
            float terrainY = terrainTransform.GetComponent<Terrain>().SampleHeight(transform.position);
            transform.position = new Vector3(transform.position.x, terrainY + cameraHeight, transform.position.z);
        }
    }

    /// <summary>
    /// 应用退出时恢复鼠标状态（避免编辑器鼠标锁定）
    /// </summary>
    void OnApplicationQuit()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
