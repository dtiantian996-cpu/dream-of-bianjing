using UnityEngine;
using UnityEngine.UI;

// 脚本名要和文件名一致（FollowTarget）
public class FollowTarget : MonoBehaviour
{
    // 【需要手动拖入】人物对象的Transform组件
    public Transform target;
    // 指示相对于人物头顶的偏移量（可在Inspector中调整）
    public Vector3 offset = new Vector3(0, 2, 0);
    // 缓存Canvas组件
    private Canvas canvas;


    void Start()
    {
        // 获取父物体的Canvas组件（确保指示在UI层级）
        canvas = GetComponentInParent<Canvas>();
    }


    void Update()
    {
        if (target == null) return; // 若没拖入目标，避免报错

        // 1. 将人物世界坐标（+偏移）转为屏幕坐标
        Vector2 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        // 2. 将屏幕坐标设为指示的UI位置
        transform.position = screenPos;
    }
}