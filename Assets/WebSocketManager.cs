using System;
using System.Collections;
using UnityEngine;
using NativeWebSocket;

// 1. 定义一个简单的类来匹配 JSON 数据结构
[Serializable]
public class MessageData
{
    public string type;      // 对应 "type": "move" / "interact"
    public string direction; // 对应 "direction": "up"
    public string clientType;
}

public class WebSocketManager : MonoBehaviour
{
    [Header("WebSocket设置")]
    public string serverUrl = "ws://127.0.0.1:8080";
    
    [Header("状态")]
    public bool isConnected = false;
    public string connectionStatus = "未连接";
    
    [Header("玩家引用")]
    public PlayerController playerController;
    
    private WebSocket websocket;
    private static WebSocketManager instance;

    public static WebSocketManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<WebSocketManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    async void Start()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerController = player.GetComponent<PlayerController>();
        }
        
        await System.Threading.Tasks.Task.Delay(100);
        await Connect();
    }
    
    public async System.Threading.Tasks.Task Connect()
    {
        try
        {
            websocket = new WebSocket(serverUrl);
            
            websocket.OnOpen += () =>
            {
                Debug.Log("✅ WebSocket连接成功!");
                isConnected = true;
                connectionStatus = "已连接";
                RegisterAsGame();
            };
            
            websocket.OnError += (e) => Debug.LogError($"❌ WebSocket错误: {e}");
            websocket.OnClose += (e) => 
            {
                isConnected = false;
                connectionStatus = "已断开";
                StartCoroutine(ReconnectAfterDelay(3f));
            };
            
            websocket.OnMessage += (bytes) =>
            {
                // 获取原始字符串
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                HandleMessage(message);
            };
            
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"连接异常: {e.Message}");
        }
    }
    
    void RegisterAsGame()
    {
        if (websocket.State == WebSocketState.Open)
        {
            websocket.SendText("{\"type\":\"register\",\"clientType\":\"game\"}");
        }
    }
    
    // ---------------------------------------------------------
    // 核心修改：使用 JsonUtility 解析，而不是 Contains
    // ---------------------------------------------------------
    void HandleMessage(string jsonMessage)
    {
        try
        {
            // 使用 Unity 自带的 JSON 解析器，非常稳健
            // 即使 JSON 里有空格、换行，它也能正确读出字段
            MessageData data = JsonUtility.FromJson<MessageData>(jsonMessage);

            if (data == null) return;

            // 根据解析出来的 type 字段做判断
            switch (data.type)
            {
                case "move":
                    if (playerController != null && !string.IsNullOrEmpty(data.direction))
                    {
                        playerController.SetMoveDirection(data.direction);
                    }
                    break;

                case "stop":
                    if (playerController != null)
                    {
                        playerController.StopMoving();
                    }
                    break;

                case "interact":
                    if (playerController != null)
                    {
                        playerController.TryInteract();
                    }
                    else
                    {
                        Debug.LogError("收到交互指令，但 PlayerController 为空！");
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON解析失败: {e.Message}");
        }
    }
    
    IEnumerator ReconnectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isConnected) _ = Connect();
    }
    
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null) websocket.DispatchMessageQueue();
        #endif
    }
    
    async void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open) await websocket.Close();
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), $"状态: {connectionStatus}");
        if (!isConnected && GUI.Button(new Rect(10, 40, 100, 30), "重新连接")) _ = Connect();
    }
}