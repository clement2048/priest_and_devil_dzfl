using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GhostBoatGame;

public class Clickable : MonoBehaviour
{
    IUserAction action;
    RoleModel role = null;

    public void SetRole(RoleModel role)
    {
        this.role = role;
    }
    private void Start()
    {
        action = SSDirector.GetInstance().CurrentScenceController as IUserAction;//获取控制器
    }
    private void OnMouseDown()//点击时调用控制器中的相关函数，用gameObject的name来区分对象。
    {
        if (gameObject.name == "boat")
        {
            action.MoveBoat();
        }
        else
        {
            action.MoveRole(role);
        }
    }
}
public class UserGUI : MonoBehaviour
{
    private IUserAction action;
    public int status = 0;
    bool isShow = false;

    GUIStyle white_style = new GUIStyle();
    GUIStyle black_style = new GUIStyle();
    GUIStyle title_style = new GUIStyle();

    void Start()
    {
        action = SSDirector.GetInstance().CurrentScenceController as IUserAction;

        //字体初始化
        white_style.normal.textColor = Color.white;
        white_style.fontSize = 20;

        black_style.normal.textColor = Color.black;
        black_style.fontSize = 30;

        title_style.normal.textColor = Color.black;
        title_style.fontSize = 45;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 50), "牧师与魔鬼", title_style);
        GUI.Label(new Rect(Screen.width / 2 - 40, 70, 200, 20), "游戏规则：", white_style);
        GUI.Label(new Rect(Screen.width / 2 - 230, 95, 300, 20), "让全部牧师（绿）和魔鬼（红）渡河，船至少由一个角色驾驶。", white_style);
        GUI.Label(new Rect(Screen.width / 2 - 170, 120, 240, 20), "在开船之后，每一边魔鬼数量都不能多于牧师数量", white_style);

        if (status == -1)
        {
            GUI.Label(new Rect(Screen.width / 2 - 60, 180, 100, 30), "You Lose!", black_style);
            if (GUI.Button(new Rect(Screen.width / 2 - 40, 240, 100, 30), "Restart"))
            {
                action.Restart();
                status = 0;
            }
        }
        else if (status == 1)
        {
            GUI.Label(new Rect(Screen.width / 2 - 62, 180, 100, 30), "You Win!", black_style);
            if (GUI.Button(new Rect(Screen.width / 2 - 40, 240, 100, 30), "Restart"))
            {
                action.Restart();
                status = 0;
            }
        }
    }
}
