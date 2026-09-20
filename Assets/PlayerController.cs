using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walk
}

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;
    public float turnSpeed = 10f;

    [Header("State Info")]
    public PlayerState currentState;

    private Vector3 targetVelocity;
    private Vector3 currentVelocity;
    private Vector3 velocitySmooth; // SmoothDamp 的引用变量
    private Rigidbody rb;
    private Animator animator; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); 
        
        // 强制关闭 Root Motion，防止动画锁死位置
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        SwitchState(PlayerState.Idle);
    }

    // 所有的物理移动计算都应该放在 FixedUpdate
    void FixedUpdate()
    {
        // 1. 计算平滑速度 (注意这里使用 fixedDeltaTime)
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmooth, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        // 2. 应用移动
        if (rb != null)
        {
            // 只有当速度足够大时才移动，避免微小的浮点数漂移
            if (currentVelocity.sqrMagnitude > 0.001f)
            {
                Vector3 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
            }
        }

        // 3. 处理旋转 (放在这里保证和移动同步)
        HandleRotation();
        
        // 4. 处理动画状态
        HandleStateLogic();
    }

    void HandleStateLogic()
    {
        // 修改点：使用 currentVelocity (实际速度) 来判断是否在移动
        // 这样可以避免 "动画停了人还在滑" 或者 "人动了动画还没播" 的违和感
        bool isMoving = currentVelocity.sqrMagnitude > 0.1f; // 阈值稍微调高一点

        if (isMoving)
        {
            if (currentState != PlayerState.Walk) SwitchState(PlayerState.Walk);
        }
        else
        {
            if (currentState != PlayerState.Idle) SwitchState(PlayerState.Idle);
        }
    }

    void SwitchState(PlayerState newState)
    {
        if (currentState == newState) return; // 防止重复设置

        currentState = newState;
        
        if (animator != null)
        {
            switch (newState)
            {
                case PlayerState.Idle:
                    animator.SetBool("IsWalking", false);
                    break;
                case PlayerState.Walk:
                    animator.SetBool("IsWalking", true);
                    break;
            }
        }
    }

    
    public void TryInteract()
    {
        // 1. 查找当前场景中【激活】的 SayDialog
        Fungus.SayDialog activeDialog = null;
        var allDialogs = FindObjectsOfType<Fungus.SayDialog>();
        
        foreach (var d in allDialogs)
        {
            if (d.gameObject.activeInHierarchy)
            {
                activeDialog = d;
                break;
            }
        }

        if (activeDialog == null)
        {
            Debug.Log("未找到激活的对话框，无法交互");
            return;
        }

        // 2. 获取核心组件 Writer
        var writer = activeDialog.GetComponent<Fungus.Writer>();

        if (writer != null)
        {
            writer.OnNextLineEvent(); 
        }
        else
        {
            Debug.LogError("找到对话框，但没有找到 Writer 组件！");
        }
    }
    
    void HandleRotation()
    {
        // 只有当有输入意图(targetVelocity)或者当前有速度时才旋转
        // 这里使用 currentVelocity 可以让转身更自然
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized);
            // 旋转使用 fixedDeltaTime
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetMoveDirection(string direction)
    {
        Vector3 directionVector = Vector3.zero;
        switch (direction.ToLower())
        {
            case "up": directionVector = Vector3.back; break;
            case "down": directionVector = Vector3.forward; break;
            case "left": directionVector = Vector3.right; break;
            case "right": directionVector = Vector3.left; break;
        }
        targetVelocity = directionVector * moveSpeed;
    }

    public void StopMoving()
    {
        targetVelocity = Vector3.zero;
    }

    public void StopImmediately()
    {
        targetVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;
        velocitySmooth = Vector3.zero;
        SwitchState(PlayerState.Idle);
    }
}