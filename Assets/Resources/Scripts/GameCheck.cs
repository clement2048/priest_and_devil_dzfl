using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GhostBoatGame;

public class GameCheck : MonoBehaviour {
    public Controller sceneController;

    protected void Start() {
        sceneController = (Controller)SSDirector.GetInstance().CurrentScenceController;
    }

    public int GameJudge()
    {
        int src_priest = sceneController.src_land.GetTotal(0);
        int src_ghost = sceneController.src_land.GetTotal(1);
        int des_priest = sceneController.des_land.GetTotal(0);
        int des_ghost = sceneController.des_land.GetTotal(1);

        if (des_priest == 3)
        {    //全部到终点，获胜
            return 1;
        }

        if (sceneController.boat.GetBoatMark() == 1)//由于在这一边船还没开，因此只需检测另一边的数量即可。
        {
            if (des_priest < des_ghost && des_priest > 0)
            {//失败
                return -1;
            }
        }
        else
        {
            if (src_priest < src_ghost && src_priest > 0)
            {//失败
                return -1;
            }
        }

        return 0;//游戏继续进行
    }
}

