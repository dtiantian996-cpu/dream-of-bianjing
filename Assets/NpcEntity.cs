using Fungus;
using UnityEngine;

public class NpcEntity : MonoBehaviour
{
    [Header("npc名字，需与Block名字一致")]
    public string npcName;

    private Flowchart flowchart;
    private bool canSay;

    // 【新增变量】：追踪是否已经自动触发过第一次对话
    private bool hasSaidFirstTime = false;


    void Start()
    {
        flowchart = GameObject.Find("Flowchart").GetComponent<Flowchart>();
    }

    private void Update()
    {
        // 鼠标按下左键触发对话方法 (保留，用于后续的点击或分支选择触发)
        if (Input.GetMouseButtonDown(0))
        {
            Say();
        }
    }

    void Say()
    {
        if (canSay)
        {
            if (flowchart.HasBlock(npcName))
            {
                flowchart.ExecuteBlock(npcName);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果检测到玩家进入触发范围
        if (other.tag.Equals("Player"))
        {
            canSay = true;

            // 【核心改动】：判断是否是第一次自动触发
            if (!hasSaidFirstTime)
            {
                if (flowchart.HasBlock(npcName))
                {
                    // 1. 立即触发对话
                    flowchart.ExecuteBlock(npcName);

                    // 2. 标记为已触发，防止下次靠近再次自动触发
                    hasSaidFirstTime = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 如果检测到玩家离开触发范围
        if (other.tag.Equals("Player"))
        {
            canSay = false;
        }
    }
}