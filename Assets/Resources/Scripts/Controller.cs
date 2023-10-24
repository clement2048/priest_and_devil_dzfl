using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GhostBoatGame;

public class Controller : MonoBehaviour, ISceneController, IUserAction
{
    public LandModel src_land;//起点陆地
    public LandModel des_land;//终点陆地
    public BoatModel boat;//船
    public RoleModel[] roles;//角色

    public UserGUI user_gui;//GUI界面，用于控制游戏状态
    public GhostBoatActionManager action_manager;//动作控制器
    public GameCheck checker;//裁判

    // Start is called before the first frame update
    void Start()
    {
        SSDirector director = SSDirector.GetInstance();
        director.CurrentScenceController = this;      //脚本在此处运行，故现让Controller指向自己。
        user_gui = gameObject.AddComponent<UserGUI>();//添加GUI属性
        action_manager = gameObject.AddComponent<GhostBoatActionManager>();
        checker = gameObject.AddComponent<GameCheck>();
        LoadResources();
    }

    public void LoadResources()
    {
        GameObject water = Instantiate(Resources.Load("Prefabs/Water", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
        src_land = new LandModel(1);
        des_land = new LandModel(-1);
        boat = new BoatModel();     //初始化时已指定角色为空，靠起点停
        roles = new RoleModel[6];   //初始化时已指定角色都在大陆上

        for (int i = 0; i < 3; i++)
        {
            RoleModel role = new RoleModel(0, src_land.GetEmptyPosition());
            role.SetName("priest" + i); //命名，便于指定区分
            src_land.AddRole(role);     //角色添加到陆地
            roles[i] = role;
        }

        for (int i = 0; i < 3; i++)
        {
            RoleModel role = new RoleModel(1, src_land.GetEmptyPosition());
            role.SetName("ghost" + i);
            src_land.AddRole(role);
            roles[i + 3] = role;
        }
    }

    public void MoveBoat()
    {
        if (boat.Total() == 0 || user_gui.status != 0)
        {//空船不可以移动，且必须为游戏进行状态
            return;
        }
        boat.Move();
        user_gui.status = checker.GameJudge();//判断游戏结束
    }

    public void MoveRole(RoleModel role)
    {
        if (user_gui.status != 0)
        {
            return;
        }
        if (role.IsOnBoat())//人在船上，上岸
        {
            LandModel land;//确定上哪边的岸
            if (boat.GetBoatMark() == -1)
            {
                land = des_land;
            }
            else
            {
                land = src_land;
            }
            boat.RemoveRole(role.GetName());//船操作
            role.ToLand(land);//角色操作
            land.AddRole(role);//陆地操作
        }
        else
        {
            LandModel land = role.GetLandModel();
            if (boat.Total() == 2 || land.GetLandMark() != boat.GetBoatMark())
            {//船已满或船不在此岸边
                return;
            }
            land.RemoveRole(role.GetName());
            role.ToBoat(boat);
            boat.AddRole(role);
        }

        user_gui.status = checker.GameJudge();
    }

    public void Restart()
    {
        src_land.Reset();
        des_land.Reset();
        boat.Reset();
        for (int i = 0; i < 6; i++)
        {
            roles[i].Reset();
        }
    }

}
