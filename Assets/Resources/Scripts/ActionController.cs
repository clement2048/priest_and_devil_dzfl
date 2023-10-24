using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GhostBoatGame;

public enum SSActionEventType : int { Started, Competeted }

//接口
public interface ISSActionCallback {
    void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, Object objectParam = null);
}

//动作基类
public class SSAction : ScriptableObject {
    public bool enable = true;                      //是否可进行
    public bool destroy = false;                    //是否已完成

    public GameObject gameobject;                   //动作对象
    public Transform transform;                     //动作对象的transform
    public ISSActionCallback callback;              //回调函数

    /*防止用户自己new对象*/
    protected SSAction() { }                      

    public virtual void Start() {
        throw new System.NotImplementedException();
    }

    public virtual void Update() {
        throw new System.NotImplementedException();
    }
}

//子类，单个动作
public class SSMoveToAction : SSAction {
    public Vector3 target;  //目的地
    public float speed;     //移动速率

    private SSMoveToAction() { }
    public static SSMoveToAction GetSSAction(Vector3 _target, float _speed) {
        SSMoveToAction action = ScriptableObject.CreateInstance<SSMoveToAction>();
        action.target = _target;
        action.speed = _speed;
        return action;
    }

    public override void Update() {
        this.transform.position = Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);
        //动作完成，通知动作管理者或动作组合
        if (this.transform.position == target) {
            this.destroy = true;
            this.callback.SSActionEvent(this);      
        }
    }

    public override void Start()
    {
        ;
    }
}

//子类，动作组合
public class SequenceAction : SSAction, ISSActionCallback {
    public List<SSAction> sequence;    //动作的列表
    public int repeat = -1;            //-1就是无限循环
    public int start = 0;              //当前做的动作的标号

    public static SequenceAction GetSSAcition(int repeat, int start, List<SSAction> sequence) {
        SequenceAction action = ScriptableObject.CreateInstance<SequenceAction>();
        action.sequence = sequence;
        action.repeat = repeat;
        action.start = start;
        return action;
    }

    public override void Start() {
        foreach (SSAction action in sequence) {
            action.gameobject = this.gameobject;    //组合动作的游戏对象都是这一个
            action.transform = this.transform;
            action.callback = this;                 //每一个动作的回调函数为该动作组合
            action.Start();
        }
    }

    public override void Update() {
        if (sequence.Count == 0) return;
        if (start < sequence.Count) {
            sequence[start].Update();               //执行之后，通过回调函数让start值递增
        }
    }

    //回调函数
    public void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, Object objectParam = null) {
        source.destroy = false;                     //由于可能还会再次调用，因此先不删除
        this.start++;                               
        if (this.start >= sequence.Count) {
            this.start = 0;
            if (repeat > 0) repeat--;               
            if (repeat == 0) {                      
                this.destroy = true;                //删除
                this.callback.SSActionEvent(this);  //通知管理者
            }
        }
    }
}

//动作管理基类
public class SSActionManager : MonoBehaviour {
    private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction>();    //动作字典
    private List<SSAction> waitingAdd = new List<SSAction>();                       //等待执行的动作列表
    private List<int> waitingDelete = new List<int>();                              //等待删除的key列表                

    protected void Update() {
        //将等待执行的动作加入字典并清空待执行列表
        foreach (SSAction ac in waitingAdd) {
            actions[ac.GetInstanceID()] = ac;                                       
        }
        waitingAdd.Clear();

        //对于字典中每一个动作，看是执行还是删除
        foreach (KeyValuePair<int, SSAction> kv in actions) {
            SSAction ac = kv.Value;
            if (ac.destroy) {
                waitingDelete.Add(ac.GetInstanceID());
            }
            else if (ac.enable) {
                ac.Update();//可能是组合动作的执行，也可能是单个动作的执行
            }
        }

        //删除所有已完成的动作并清空待删除列表
        foreach (int key in waitingDelete) {
            SSAction ac = actions[key];
            actions.Remove(key);
            Object.Destroy(ac);//让Unity帮着删除
        }
        waitingDelete.Clear();
    }

    //外界只需要调用动作管理类的RunAction函数即可完成动作。
    public void RunAction(GameObject gameobject, SSAction action, ISSActionCallback manager) {
        action.gameobject = gameobject;
        action.transform = gameobject.transform;
        action.callback = manager;
        waitingAdd.Add(action);
        action.Start();
    }
}

//本次游戏所需要的动作管理类
public class GhostBoatActionManager : SSActionManager, ISSActionCallback {
    private SSAction boatMove;
    private SequenceAction roleMove;

    protected new void Update() {
        base.Update();
    }

    public void moveBoat(GameObject boat, Vector3 end, float speed) {
        boatMove = SSMoveToAction.GetSSAction(end, speed);
        this.RunAction(boat, boatMove, this);
    }

    public void moveRole(GameObject role, Vector3 middle, Vector3 end, float speed) {
        //两段移动
        SSAction action1 = SSMoveToAction.GetSSAction(middle, speed);
        SSAction action2 = SSMoveToAction.GetSSAction(end, speed);
        //两个动作，都只重复一次
        roleMove = SequenceAction.GetSSAcition(1, 0, new List<SSAction> { action1, action2 });
        this.RunAction(role, roleMove, this);
    }

    public void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, Object objectParam = null){

    }
}