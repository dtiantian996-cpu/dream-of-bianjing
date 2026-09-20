using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject Player;
    public float m_speed = 5f;

    void Update()
    {
        PlayerMove_KeyRighidbody2();
    }

    //通过Rigidbody组件 键盘控制移动 AddForce控制移动 角色身上需要挂载Rigidbody组件
    public void PlayerMove_KeyRighidbody2()
    {
        float horizontal = Input.GetAxis("Horizontal"); //A D 左右
        float vertical = Input.GetAxis("Vertical"); //W S 上 下

        Player.GetComponent<Rigidbody>().AddForce(Vector3.forward * vertical * m_speed);
        Player.GetComponent<Rigidbody>().AddForce(Vector3.right * horizontal * m_speed);
    }
}