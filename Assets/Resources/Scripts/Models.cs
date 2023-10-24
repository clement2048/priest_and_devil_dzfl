using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GhostBoatGame
{
    //加载场景接口
    public interface ISceneController
    {
        void LoadResources();
    }

    //用户操作接口
    public interface IUserAction
    {
        void MoveBoat(); //移动船
        void Restart(); //重新开始
        void MoveRole(RoleModel role); //移动角色
    }

    //导演类
    public class SSDirector : System.Object
    {
        private static SSDirector _instance;
        public ISceneController CurrentScenceController { get; set; }

        public static SSDirector GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SSDirector();
            }
            return _instance;
        }
    }

    public class RoleModel
    {
        GameObject role;
        int role_sign;  //0为牧师，1为魔鬼
        bool on_boat;   //是否在船上
        LandModel land = (SSDirector.GetInstance().CurrentScenceController as Controller).src_land;//所在的陆地

        GhostBoatActionManager moveController;
        Clickable click;//给model赋予属性，可点击、可移动，相当于添加了脚本。
        public float speed = 15;

        public RoleModel(int id, Vector3 pos)
        {
            if (id == 0)
            {
                role_sign = 0;
                role = Object.Instantiate(Resources.Load("Prefabs/Priest", typeof(GameObject)), pos, Quaternion.identity) as GameObject;
            }
            else
            {
                role_sign = 1;
                role = Object.Instantiate(Resources.Load("Prefabs/Ghost", typeof(GameObject)), pos, Quaternion.identity) as GameObject;
            }
            click = role.AddComponent(typeof(Clickable)) as Clickable;
            click.SetRole(this);
            moveController = (SSDirector.GetInstance().CurrentScenceController as Controller).action_manager;
        }

        public int GetSign()
        {
            return role_sign;
        }

        public string GetName()
        {
            return role.name;
        }

        public LandModel GetLandModel()
        {
            return land;
        }

        public bool IsOnBoat()
        {
            return on_boat;
        }

        public void SetName(string name)
        {
            role.name = name;
        }

        public void Move(Vector3 end)//移动操作的对外接口
        {
            Vector3 middle = new Vector3(role.transform.position.x, end.y, end.z);
            moveController.moveRole(role, middle, end, speed);
        }

        public void ToLand(LandModel land)//上岸的移动
        {
            Vector3 pos = land.GetEmptyPosition();
            Vector3 middle = new Vector3(role.transform.position.x, pos.y, pos.z);
            moveController.moveRole(role, middle, pos, speed);
            this.land = land;
            on_boat = false;
        }

        public void ToBoat(BoatModel boat)//上船的移动
        {
            Vector3 pos = boat.GetEmptyPosition();
            Vector3 middle = new Vector3(pos.x, role.transform.position.y, pos.z);
            moveController.moveRole(role, middle, pos, speed);
            this.land = null;
            on_boat = true;
        }

        public void Reset()
        {
            LandModel land = (SSDirector.GetInstance().CurrentScenceController as Controller).src_land;
            ToLand(land);
            land.AddRole(this);
        }
    }

    public class LandModel
    {
        GameObject land;   //陆地对象
        public int land_mark;//src为1，des为-1。
        RoleModel[] roles = new RoleModel[6];//陆地上的角色对象
        Vector3[] role_positions;//每个角色的位置

        public LandModel(int sign)
        {//根据对象标识初始化
            land_mark = sign;
            land = Object.Instantiate(Resources.Load("Prefabs/Land", typeof(GameObject)), new Vector3(10.5F * land_mark, 0.5F, 0), Quaternion.identity) as GameObject;
            role_positions = new Vector3[] { new Vector3(6.5F * land_mark, 1.8F, 0), new Vector3(8.0F * land_mark, 1.8F, 0), new Vector3(9.5F * land_mark, 1.8F, 0), new Vector3(11.0F * land_mark, 1.8F, 0), new Vector3(12.5F * land_mark, 1.8F, 0), new Vector3(14.0F * land_mark, 1.8F, 0) };
        }

        public int GetLandMark()
        {
            return land_mark;
        }

        public Vector3 GetEmptyPosition()
        {//找到当前空位置
            int pos = -1;
            for (int i = 0; i < 6; i++)
            {
                if (roles[i] == null)
                {
                    pos = i;
                    break;
                }
            }

            return role_positions[pos];
        }

        public void AddRole(RoleModel role)
        {//添加role
            for (int i = 0; i < 6; i++)
            {
                if (roles[i] == null)
                {
                    roles[i] = role;
                    break;
                }
            }
        }

        public RoleModel RemoveRole(string name)
        {//删除role
            for (int i = 0; i < 6; i++)
            {
                if (roles[i] != null && roles[i].GetName() == name)
                {
                    roles[i] = null;
                    return roles[i];
                }
            }
            return null;
        }

        public int GetTotal(int id)
        {
            int sum = 0;
            if (id == 0)
            {//牧师
                for (int i = 0; i < 6; i++)
                {
                    if (roles[i] != null && roles[i].GetSign() == 0)
                    {
                        sum++;
                    }
                }
            }
            else if (id == 1)
            {//魔鬼
                for (int i = 0; i < 6; i++)
                {
                    if (roles[i] != null && roles[i].GetSign() == 1)
                    {
                        sum++;
                    }
                }
            }
            return sum;
        }

        public void Reset()
        {
            roles = new RoleModel[6];
        }
    }

    public class BoatModel
    {
        GameObject boat;//船对象
        Vector3[] src_empty_pos;//船在src陆地的两个空位位置
        Vector3[] des_empty_pos;//船在des陆地的两个空位位置
        public float speed = 15;

        GhostBoatActionManager moveController;
        Clickable click;

        int boat_mark = 1;//船在src为1，在des为-1。
        RoleModel[] roles = new RoleModel[2];//船上的两个角色。

        public BoatModel()
        {//初始化对象
            boat = Object.Instantiate(Resources.Load("Prefabs/Boat", typeof(GameObject)), new Vector3(4.5F, 0.5F, 0), Quaternion.identity) as GameObject;
            boat.name = "boat";
            moveController = (SSDirector.GetInstance().CurrentScenceController as Controller).action_manager;
            click = boat.AddComponent(typeof(Clickable)) as Clickable;
            src_empty_pos = new Vector3[] { new Vector3(3.8F, 1.1F, 0), new Vector3(5.2F, 1.1F, 0) };
            des_empty_pos = new Vector3[] { new Vector3(-5.2F, 1.1F, 0), new Vector3(-3.8F, 1.1F, 0) };
        }

        public int Total()
        {
            int sum = 0;
            for (int i = 0; i < 2; i++)
            {
                if (roles[i] != null)
                {
                    sum++;
                }
            }
            return sum;
        }

        public void Move()//移动船只的同时，调用角色的函数Move，同时将角色移到指定位置。
        {
            if (boat_mark == -1)
            {
                moveController.moveBoat(boat, new Vector3(4.5F, 0.5F, 0), speed);
                for (int i = 0; i < 2; i++)
                {
                    if (roles[i] != null)
                    {
                        roles[i].Move(src_empty_pos[i]);
                    }
                }
                boat_mark = 1;
            }
            else
            {
                moveController.moveBoat(boat, new Vector3(-4.5F, 0.5F, 0), speed);
                for (int i = 0; i < 2; i++)
                {
                    if (roles[i] != null)
                    {
                        roles[i].Move(des_empty_pos[i]);
                    }
                }
                boat_mark = -1;
            }
        }

        public int GetBoatMark()
        {
            return boat_mark;
        }

        public Vector3 GetEmptyPosition()
        {//找到当前空位置
            if (boat_mark == 1)
            {
                int pos = -1;
                for (int i = 0; i < 2; i++)
                {
                    if (roles[i] == null)
                    {
                        pos = i;
                        break;
                    }
                }
                return src_empty_pos[pos];
            }
            else
            {
                int pos = -1;
                for (int i = 0; i < 2; i++)
                {
                    if (roles[i] == null)
                    {
                        pos = i;
                        break;
                    }
                }
                return des_empty_pos[pos];
            }
        }

        public void AddRole(RoleModel role)
        {//添加role
            for (int i = 0; i < 2; i++)
            {
                if (roles[i] == null)
                {
                    roles[i] = role;
                    break;
                }
            }
        }

        public RoleModel RemoveRole(string name)
        {//删除role
            for (int i = 0; i < 2; i++)
            {
                if (roles[i] != null && roles[i].GetName() == name)
                {
                    roles[i] = null;
                    return roles[i];
                }
            }
            return null;
        }

        public void Reset()
        {
            if (boat_mark == -1)
            {
                moveController.moveBoat(boat, new Vector3(4.5F, 0.5F, 0), speed);
                boat_mark = 1;
            }
            roles = new RoleModel[2];
        }
    }



}