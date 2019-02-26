using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
/*
 agent 是一种观测并与_环境_交互的 自主参与者 (actor)。
 在 ML-Agent的语境下， 环境是
 一个包含一个 Academy，
 一个或多个 Brain， 
 一个或多个Agent， 
Agent 与其他实体交互的场景

***
场景中的每个平台都是 独立的 agent，但它们全部共享同一个 brain。
3D Balance Ball 通过 这种方式可以加快训练速度，因为所有 12 个 agent 可以并行参与训练任务。

*/
public class Ball3DAgent : Agent
{
    //[Header("Specific to Ball3D")]
    //public GameObject ball; //改成car
    public GameObject car;

    //new
    public GameObject goal;
    public GameObject block;
    public GameObject plane;

    //new-車子前後射線相關
    GameObject hitObject, re_hitObject;
    float rayDistance = 100.0f;
    float goal_dis;
    float wall_dis;
    float re_goal_dis;
    float re_wall_dis;

    //private Rigidbody ballRb;//用不到
    /*  //用不到
    public override void InitializeAgent()
    {
        ballRb = car.GetComponent<Rigidbody>();
    }
    */
    /*
     3D Balance Ball 示例中所用的 Brain 实例使用 State Size 为 8 的 Continuous 向量观测空间。
     这意味着 包含 agent 观测结果的特征向量包含八个元素： 
     平台旋转的 x 和 z 分量以及球相对位置和速度的 x、y 和 z 分量。
     （观测结果值 在 agent 的 CollectObservations() 函数中进行定义。）
     https://blog.csdn.net/u010019717/article/details/80472011

    ***
    * 在决策之前，agent 会收集有关自己在环境中所处的状态的 观测结果。
    * ML-Agents 将观测分为两类： Continuous 和 Discrete。
    * Continuous 向量观测空间 会收集浮点数向量中的观测结果。
    * Discrete 向量观测空间是一个状态表的索引。
    * 大多数示例环境 都使用连续的向量观测空间。
     */
    public override void CollectObservations()
    {
        //AddVectorObs(gameObject.transform.rotation.z);
        //AddVectorObs(gameObject.transform.rotation.x);
        //AddVectorObs(ball.transform.position - gameObject.transform.position);
        //AddVectorObs(ballRb.velocity);

        //改變的物體位置
        //AddVectorObs(car.transform.position);

        //車子的方向
        AddVectorObs(car.transform.rotation.z);
        AddVectorObs(car.transform.rotation.x);

        //觀察車和goal的距離
        AddVectorObs(car.transform.position - goal.transform.position);

        //觀察車和障礙物的距離
        AddVectorObs(wall_dis);
        AddVectorObs(goal_dis);
        AddVectorObs(re_wall_dis);
        AddVectorObs(re_goal_dis);

    }
    private void FixedUpdate()
    {
        //用本物件的位置及方向產生射線
        Ray ray = new Ray(transform.position, transform.forward);
        //反方向射線
        Ray ray_reverse = new Ray(transform.position, -transform.forward);
        RaycastHit hit,re_hit;  //re = reverse

        goal_dis = rayDistance;
        wall_dis = rayDistance;
        re_goal_dis = rayDistance;
        re_wall_dis = rayDistance;
        //前方射線
        if (Physics.Raycast(ray, out hit, rayDistance)) 
        {
            hitObject = hit.transform.gameObject;
            
            if (hitObject.tag == "goal")
                goal_dis = Vector3.Distance(hit.point, transform.position);
            else if(hitObject.tag == "wall")
                wall_dis = Vector3.Distance(hit.point, transform.position);
        }
        //後方射線
        if (Physics.Raycast(ray_reverse, out re_hit, rayDistance)) 
        {
            re_hitObject = re_hit.transform.gameObject;

            if (hitObject.tag == "goal")
                re_goal_dis = Vector3.Distance(re_hit.point, transform.position);
            else if(hitObject.tag == "wall")
                re_wall_dis = Vector3.Distance(re_hit.point, transform.position);
        }
    }

    /*
     在每个模拟步骤调用。接收 brain 选择的 动作。
     Ball3DAgent 示例可以处理连续和离散 运动空间类型。
     在此环境中，两种状态类型之间实际上 没有太大的差别：这两种向量运动空间在每一步都会 导致平台旋转发生小变化。
     AgentAction() 函数 为 agent 分配奖励；
     在此示例中，
     agent 在每一步 将球保持在平台上时收到较小的正奖励， 而在掉球时收到更大的负奖励。
     agent 在掉球时还会被标记为 完成状态，因此会重置一个用于下一模拟步骤的 新球。
     */
    //public override void AgentAction(float[] vectorAction, string textAction)
    public override void AgentAction(float[] moveAction, string textAction)
    {

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            //moveAction[0]---左轉
            //moveAction[1]---右轉
            moveAction[0] = Mathf.Clamp(moveAction[0], -1f, 1f);
            moveAction[1] = Mathf.Clamp(moveAction[1], -1f, 1f);

            //Debug.Log(moveAction[0]+ "***********" + moveAction[1]);

            car.transform.Rotate(new Vector3(0, 10*moveAction[0], 0));
            car.transform.Translate(0, 0, moveAction[1]);

            /*
            if (moveAction[0] > 0)       //如果左轉，ad
            {
                car.transform.Rotate(new Vector3(0, -2, 0));
            }
            else if (moveAction[1] > 0)     //如果右轉，ws
            {
                car.transform.Rotate(new Vector3(0, 2, 0));
            }
            */
        }
        /*
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            var actionZ = 2f * Mathf.Clamp(vectorAction[0], -1f, 1f);
            var actionX = 2f * Mathf.Clamp(vectorAction[1], -1f, 1f);

            if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
                (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
            {
                gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
            }

            if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
                (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
            {
                gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
            }
        }
        //觀察球是否掉落
        if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
            Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
            Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
        {
            Done();
            SetReward(-1f);
        }
        else
        {
            SetReward(0.1f);
        }
        */
        //SetReward(0.1f);
    }

    //若車撞到wall則扣分
    //若撞到goal則加分
    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "wall")
        {
            SetReward(-1f);
            //Debug.Log("fail---xxx");
            Done();
        }
        else if (col.collider.tag == "goal")
        {
            SetReward(1f);
            //Debug.Log("goal---!!!");
            Done();
        }
    }

    /*
     Agent 重置时（包括会话开始时） 调用。
     Ball3DAgent 类使用重置函数来重置 平台和球。
     该函数会将重置值随机化，从而使 训练不局限于特定的开始位置和平台姿态。
     */
    public override void AgentReset()
    {
        //gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        //gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        //ballRb.velocity = new Vector3(0f, 0f, 0f);
        //ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
        //                              + gameObject.transform.position;
        car.transform.position = new Vector3(Random.Range(-3f, 3f), 0.5f, -5f) + plane.transform.position;
        block.transform.position = new Vector3(Random.Range(-3f, 3f), 0.5f, 1.5f) + plane.transform.position;
        goal.transform.position = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 6f)) + plane.transform.position;

        car.transform.Rotate(0,0,0);

        //Random.Range(-3f, 3f)
    }

}
