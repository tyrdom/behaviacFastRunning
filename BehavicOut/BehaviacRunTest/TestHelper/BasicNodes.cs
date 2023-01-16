using behaviac;
using SGame.InGame.GameLogic;

public class BasicNodes:BehaviorTree
{
TestAgent TestAgent = new TestAgent();
// Local Parameter
//localValueTest
System.Int32 localValueTest {get; set;} = 0;
//TestValueInt
System.Int32 TestValueInt {get; set;} = 0;
//testInt
System.Int32 testInt {get; set;} = 0;

// BehaviorTreeStatus

 int nowRunningNode{get;set;} = -1;
private EBTStatus Node2Result { get; set; }
private EBTStatus Node4Result { get; set; }
private EBTStatus Node8Result { get; set; }
private EBTStatus Node5Result { get; set; }
private EBTStatus Node7Result { get; set; }
private EBTStatus Node9Result { get; set; }
private EBTStatus Node1Result { get; set; }
private EBTStatus Node0Result { get; set; }
public EBTStatus Tick()
{
switch (nowRunningNode)
{
case 8:
goto Node8Enter;
case 9:
goto Node9Enter;

}
//
//PluginBehaviac.Nodes.Sequence
//Node0
Node0Result = EBTStatus.BT_SUCCESS;
Node0Enter:

//
//PluginBehaviac.Nodes.Condition
//Node2
Node2Result = EBTStatus.BT_SUCCESS;
Node2Enter:


Node2Out:
Node2Result = (testInt >= 0) ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;
if(Node2Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Assignment
//Node3
Node3Enter:
testInt = 0;


Node3Out:

//
//PluginBehaviac.Nodes.Action
//Node4
Node4Result = EBTStatus.BT_SUCCESS;
Node4Enter:
TestAgent.LogMessage("Seq");
Node4Result=EBTStatus.BT_SUCCESS;


Node4Out:
if(Node4Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Action
//Node8
Node8Result = EBTStatus.BT_SUCCESS;
Node8Enter:
Node8Result = TestAgent.TestRunning(5);
 if (Node8Result == EBTStatus.BT_RUNNING )
{
nowRunningNode = 8;
 return EBTStatus.BT_RUNNING;
 }
nowRunningNode = -1;

Node8Out:
if(Node8Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Assignment
//Node6
Node6Enter:
testInt = TestAgent.TestGetValue();


Node6Out:

//
//PluginBehaviac.Nodes.Compute
//Node10
Node10Enter:
testInt=testInt + 1;

Node10Out:

//
//PluginBehaviac.Nodes.Selector
//Node1
Node1Result = EBTStatus.BT_FAILURE;
Node1Enter:

//
//PluginBehaviac.Nodes.Condition
//Node5
Node5Result = EBTStatus.BT_SUCCESS;
Node5Enter:


Node5Out:
Node5Result = (testInt > 4) ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;
if(Node5Result == EBTStatus.BT_SUCCESS)
{
Node1Result = EBTStatus.BT_SUCCESS;
goto Node1Out;
}
//
//PluginBehaviac.Nodes.Action
//Node7
Node7Result = EBTStatus.BT_SUCCESS;
Node7Enter:
TestAgent.LogMessage("Select");
Node7Result=EBTStatus.BT_FAILURE;


Node7Out:
if(Node7Result == EBTStatus.BT_SUCCESS)
{
Node1Result = EBTStatus.BT_SUCCESS;
goto Node1Out;
}
//
//PluginBehaviac.Nodes.Action
//Node9
Node9Result = EBTStatus.BT_SUCCESS;
Node9Enter:
Node9Result = TestAgent.TestRunning(testInt);
 if (Node9Result == EBTStatus.BT_RUNNING )
{
nowRunningNode = 9;
 return EBTStatus.BT_RUNNING;
 }
nowRunningNode = -1;

Node9Out:
if(Node9Result == EBTStatus.BT_SUCCESS)
{
Node1Result = EBTStatus.BT_SUCCESS;
goto Node1Out;
}

Node1Out:
if(Node1Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}

Node0Out:
return Node0Result;

}
}