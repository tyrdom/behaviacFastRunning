using System;
using behaviac;
using SGame.InGame.GameLogic;
using PBConfig;

public class WrapperAI_NewTest_TestNode3:IBTree
{
private ObjAgent ObjAgent  {get;}
// Local Parameter
//testVar
VInt3 testVar {get;set;} = new(){x=0,y=0,z=0};
//TestInt
System.Int32 TestInt {get;set;} = 0;
//testEnum
SkillSlotType testEnum {get;set;} = SkillSlotType.SLOT_SKILL_0;
//EnumTest2
AISkillTypeTag EnumTest2 {get;set;} = AISkillTypeTag.None;

// BehaviorTreeStatus

private int NowLocalTick { get; set; } = -1;

private int  Node0RunningNode{get;set;} = -1;
private EBTStatus Node43Result { get; set; }
private EBTStatus Node8Result { get; set; }
private EBTStatus Node7Result { get; set; }
private EBTStatus Node4Result { get; set; }
private EBTStatus Node46Result { get; set; }
private EBTStatus Node3Result { get; set; }
private int Node3RunningNode { get; set; } = -1;
private EBTStatus Node38Result { get; set; }
private EBTStatus Node2Result { get; set; }
private EBTStatus Node10Result { get; set; }
private EBTStatus Node16Result { get; set; }
private EBTStatus Node17Result { get; set; }
private int Node13RunningNode { get; set; } = -1;
private EBTStatus Node13Result { get; set; }
private EBTStatus Node14Result { get; set; }
private EBTStatus Node41Result { get; set; }
private EBTStatus Node23Result { get; set; }
private EBTStatus Node24Result { get; set; }
private EBTStatus Node26Result { get; set; }
private EBTStatus Node15Result { get; set; }
private EBTStatus Node27Result { get; set; }
private EBTStatus Node25Result { get; set; }
private EBTStatus Node31Result { get; set; }
private EBTStatus Node40Result { get; set; }
private EBTStatus Node37Result { get; set; }
private EBTStatus Node18Result { get; set; }
private int Node30RunningNode { get; set; } = -1;
private EBTStatus Node30Result { get; set; }
private EBTStatus Node33Result { get; set; }
private EBTStatus Node35Result { get; set; }
private EBTStatus Node36Result { get; set; }
private EBTStatus Node39Result { get; set; }
private EBTStatus Node34Result { get; set; }
private int Node32RunningNode { get; set; } = -1;
private EBTStatus Node32Result { get; set; }
private EBTStatus Node29Result { get; set; }
private int Node29WhichBranchRunning { get; set; } = -1;
private EBTStatus Node19Result { get; set; }
private int Node12RunningNode { get; set; } = -1;
private EBTStatus Node12Result { get; set; }
private EBTStatus Node11Result { get; set; }
private int Node11WhichBranchRunning { get; set; } = -1;
private int Node11RunningNode { get; set; } = -1;
private EBTStatus Node9Result { get; set; }
private WrapperAI_NewTest_TestNode Node9SubTree { get; }
private int Node9RunningNode { get; set; } = -1;
private EBTStatus Node47Result { get; set; }
private bool Node47ParallelSuccess { get; set; } = true;
private bool Node47ParallelFail { get; set; } = true;
private bool Node47ParallelRunning { get; set; } = false;
private EBTStatus Node0Result { get; set; }
public EBTStatus Tick()
{
 NowLocalTick++;
//
//PluginBehaviac.Nodes.Sequence
//Node0
Node0Result = EBTStatus.BT_INVALID;
Node0Run:

//
//PluginBehaviac.Nodes.Assignment
//Node1
Node1Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node1Out:

//
//PluginBehaviac.Nodes.Compute
//Node42
Node42Run:
TestInt = ObjAgent.GetIntVariable( "") + TestInt;
Node42Out:

//
//PluginBehaviac.Nodes.True
//Node43
Node43Run:
Node43Result = EBTStatus.BT_SUCCESS;
Node43Out:
if(Node43Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Parallel
//Node47
Node47ParallelSuccess = true;
Node47ParallelFail = true;
Node47ParallelRunning = false;
//使用了childFinishLoop模式，所以不需要重置子节点状态到invalid
Node47Run:

//
//PluginBehaviac.Nodes.Sequence
//Node3
switch (Node3RunningNode)
{
case 46:
goto Node46Run;

}
//并行节点之下
//CHILDFINISH_LOOP循环则任何情况都会重新执行
Node3Result = EBTStatus.BT_INVALID;
Node3Run:

//
//PluginBehaviac.Nodes.Assignment
//Node5
Node5Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node5Out:

//
//PluginBehaviac.Nodes.DecoratorLoop
//Node46
Node46Run:

//
//PluginBehaviac.Nodes.Selector
//Node4
//循环节点下
Node4Result = EBTStatus.BT_INVALID;
Node4Run:

//
//PluginBehaviac.Nodes.Condition
//Node8
Node8Result = EBTStatus.BT_SUCCESS;
Node8Run:
Node8Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node8Out:
if(Node8Result == EBTStatus.BT_SUCCESS)
{
Node4Result = EBTStatus.BT_SUCCESS;
goto Node4Out;
}
//
//PluginBehaviac.Nodes.Action
//Node7
Node7Run:
ObjAgent.LogMessage( "Select：害是合法点");
Node7Result = EBTStatus.BT_FAILURE;
Node7Out:
if(Node7Result == EBTStatus.BT_SUCCESS)
{
Node4Result = EBTStatus.BT_SUCCESS;
goto Node4Out;
}
//
//PluginBehaviac.Nodes.Assignment
//Node6
Node6Run:
testVar = new VInt3(){x=1,y=0,z=1};
Node6Out:


Node4Out:
Node46Result = Node4Result;


Node46Result = EBTStatus.BT_RUNNING;
Node3RunningNode = 46;
Node3Result = EBTStatus.BT_RUNNING;
goto Node3Out;
Node46Out:
if(Node46Result == EBTStatus.BT_FAILURE)
{
Node3Result = EBTStatus.BT_FAILURE;
goto Node3Out;
}

Node3Out:
switch(Node3Result)
{
case EBTStatus.BT_SUCCESS:
//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false

break;
case EBTStatus.BT_FAILURE:
//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false

break;
case EBTStatus.BT_RUNNING:
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelRunning = true;//任何running都会使得并行节点running

break;
default:
throw new ArgumentOutOfRangeException();}

//
//PluginBehaviac.Nodes.SelectorLoop
//Node11
switch (Node11RunningNode)
{
case 10:
goto Node10Run;

}
//并行节点之下
//CHILDFINISH_LOOP循环则任何情况都会重新执行

Node11Run:

//
//PluginBehaviac.Nodes.WithPrecondition
//Node13
Node13Result = EBTStatus.BT_INVALID;
Node13Run:

//
//PluginBehaviac.Nodes.Selector
//Node16
//选择监测条件之下
Node16Result = EBTStatus.BT_INVALID;
Node16Run:

//
//PluginBehaviac.Nodes.Condition
//Node38
Node38Result = EBTStatus.BT_SUCCESS;
Node38Run:
Node38Result = ObjAgent.IsInValidVint3( testVar) == false ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node38Out:
if(Node38Result == EBTStatus.BT_SUCCESS)
{
Node16Result = EBTStatus.BT_SUCCESS;
goto Node16Out;
}
//
//PluginBehaviac.Nodes.Condition
//Node2
Node2Result = EBTStatus.BT_SUCCESS;
Node2Run:
Node2Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node2Out:
if(Node2Result == EBTStatus.BT_SUCCESS)
{
Node16Result = EBTStatus.BT_SUCCESS;
goto Node16Out;
}
//
//PluginBehaviac.Nodes.Action
//Node10
Node10Run:
Node10Result = ObjAgent.IsOffline();
 if (Node10Result == EBTStatus.BT_RUNNING)
{
Node11RunningNode = 10;
Node11Result = EBTStatus.BT_RUNNING;
goto Node11Out;
}
Node11RunningNode = -1;
Node10Out:
if(Node10Result == EBTStatus.BT_SUCCESS)
{
Node16Result = EBTStatus.BT_SUCCESS;
goto Node16Out;
}

Node16Out:
if(Node16Result == EBTStatus.BT_FAILURE)
{
goto Node13Out;
}
Node11Result = EBTStatus.BT_SUCCESS;
//如果切换了分支后再通过，那么会重置running下面running的节点到-1
if (Node11WhichBranchRunning != 13)
{
Node13RunningNode = -1;
}
//
//PluginBehaviac.Nodes.Action
//Node17
switch (Node13RunningNode)
{
case 17:
goto Node17Run;

}
//选择监测动作之下
Node17Run:
Node17Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node17Result == EBTStatus.BT_RUNNING)
{
Node13RunningNode = 17;
Node13Result = EBTStatus.BT_RUNNING;
goto Node13Out;
}
Node13RunningNode = -1;
Node17Out:
Node13Result = Node17Result;


Node13Out:
if(Node13Result != EBTStatus.BT_INVALID)
{
Node11WhichBranchRunning = Node13Result == EBTStatus.BT_RUNNING ? 13 : -1;
goto Node11Out;
}

//
//PluginBehaviac.Nodes.WithPrecondition
//Node12
Node12Result = EBTStatus.BT_INVALID;
Node12Run:

//
//PluginBehaviac.Nodes.Condition
//Node14
//选择监测条件之下
Node14Result = EBTStatus.BT_SUCCESS;
Node14Run:
Node14Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node14Out:
if(Node14Result == EBTStatus.BT_FAILURE)
{
goto Node12Out;
}
Node11Result = EBTStatus.BT_SUCCESS;
//如果切换了分支后再通过，那么会重置running下面running的节点到-1
if (Node11WhichBranchRunning != 12)
{
Node12RunningNode = -1;
}
//
//PluginBehaviac.Nodes.Sequence
//Node19
switch (Node12RunningNode)
{
case 41:
goto Node41Run;
case 15:
goto Node15Run;
case 33:
goto Node33Run;

}
//选择监测动作之下
Node19Result = EBTStatus.BT_INVALID;
Node19Run:

//
//PluginBehaviac.Nodes.Assignment
//Node20
Node20Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node20Out:

//
//PluginBehaviac.Nodes.Assignment
//Node21
Node21Run:
testEnum = SkillSlotType.SLOT_SKILL_2;
Node21Out:

//
//PluginBehaviac.Nodes.Assignment
//Node22
Node22Run:
TestInt = 0;
Node22Out:

//
//PluginBehaviac.Nodes.Action
//Node41
Node41Run:
Node41Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node41Result == EBTStatus.BT_RUNNING)
{
Node12RunningNode = 41;
Node12Result = EBTStatus.BT_RUNNING;
goto Node12Out;
}
Node12RunningNode = -1;
Node41Out:
if(Node41Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}
//
//PluginBehaviac.Nodes.Condition
//Node23
Node23Result = EBTStatus.BT_SUCCESS;
Node23Run:
Node23Result = ObjAgent.IsInValidVint3( testVar) == false ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node23Out:
if(Node23Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}
//
//PluginBehaviac.Nodes.Action
//Node24
Node24Run:
ObjAgent.LogMessage( "Seq：合法点");
Node24Result = EBTStatus.BT_SUCCESS;
Node24Out:
if(Node24Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}
//
//PluginBehaviac.Nodes.Selector
//Node25
Node25Result = EBTStatus.BT_INVALID;
Node25Run:

//
//PluginBehaviac.Nodes.Condition
//Node26
Node26Result = EBTStatus.BT_SUCCESS;
Node26Run:
Node26Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node26Out:
if(Node26Result == EBTStatus.BT_SUCCESS)
{
Node25Result = EBTStatus.BT_SUCCESS;
goto Node25Out;
}
//
//PluginBehaviac.Nodes.Action
//Node15
Node15Run:
Node15Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node15Result == EBTStatus.BT_RUNNING)
{
Node12RunningNode = 15;
Node12Result = EBTStatus.BT_RUNNING;
goto Node12Out;
}
Node12RunningNode = -1;
Node15Out:
if(Node15Result == EBTStatus.BT_SUCCESS)
{
Node25Result = EBTStatus.BT_SUCCESS;
goto Node25Out;
}
//
//PluginBehaviac.Nodes.Action
//Node27
Node27Run:
ObjAgent.LogMessage( "Select：害是合法点");
Node27Result = EBTStatus.BT_FAILURE;
Node27Out:
if(Node27Result == EBTStatus.BT_SUCCESS)
{
Node25Result = EBTStatus.BT_SUCCESS;
goto Node25Out;
}
//
//PluginBehaviac.Nodes.Assignment
//Node28
Node28Run:
testVar = new VInt3(){x=1,y=0,z=1};
Node28Out:


Node25Out:
if(Node25Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}
//
//PluginBehaviac.Nodes.SelectorLoop
//Node29

Node29Run:

//
//PluginBehaviac.Nodes.WithPrecondition
//Node30
Node30Result = EBTStatus.BT_INVALID;
Node30Run:

//
//PluginBehaviac.Nodes.Condition
//Node31
//选择监测条件之下
Node31Result = EBTStatus.BT_SUCCESS;
Node31Run:
Node31Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node31Out:
if(Node31Result == EBTStatus.BT_FAILURE)
{
goto Node30Out;
}
Node29Result = EBTStatus.BT_SUCCESS;
//如果切换了分支后再通过，那么会重置running下面running的节点到-1
if (Node29WhichBranchRunning != 30)
{
Node30RunningNode = -1;
}
//
//PluginBehaviac.Nodes.Sequence
//Node18
switch (Node30RunningNode)
{
case 37:
goto Node37Run;

}
//选择监测动作之下
Node18Result = EBTStatus.BT_INVALID;
Node18Run:

//
//PluginBehaviac.Nodes.Action
//Node40
Node40Run:
ObjAgent.LogMessage( "分支1-1 ：准备润");
Node40Result = EBTStatus.BT_FAILURE;
Node40Out:
if(Node40Result == EBTStatus.BT_FAILURE)
{
Node18Result = EBTStatus.BT_FAILURE;
goto Node18Out;
}
//
//PluginBehaviac.Nodes.Action
//Node37
Node37Run:
Node37Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node37Result == EBTStatus.BT_RUNNING)
{
Node30RunningNode = 37;
Node30Result = EBTStatus.BT_RUNNING;
goto Node30Out;
}
Node30RunningNode = -1;
Node37Out:
if(Node37Result == EBTStatus.BT_FAILURE)
{
Node18Result = EBTStatus.BT_FAILURE;
goto Node18Out;
}

Node18Out:
Node30Result = Node18Result;


Node30Out:
if(Node30Result != EBTStatus.BT_INVALID)
{
Node29WhichBranchRunning = Node30Result == EBTStatus.BT_RUNNING ? 30 : -1;
goto Node29Out;
}

//
//PluginBehaviac.Nodes.WithPrecondition
//Node32
Node32Result = EBTStatus.BT_INVALID;
Node32Run:

//
//PluginBehaviac.Nodes.Action
//Node33
//选择监测条件之下
Node33Run:
Node33Result = ObjAgent.IsOffline();
 if (Node33Result == EBTStatus.BT_RUNNING)
{
Node12RunningNode = 33;
Node12Result = EBTStatus.BT_RUNNING;
goto Node12Out;
}
Node12RunningNode = -1;
Node33Out:
if(Node33Result == EBTStatus.BT_FAILURE)
{
goto Node32Out;
}
Node29Result = EBTStatus.BT_SUCCESS;
//如果切换了分支后再通过，那么会重置running下面running的节点到-1
if (Node29WhichBranchRunning != 32)
{
Node32RunningNode = -1;
}
//
//PluginBehaviac.Nodes.Selector
//Node34
switch (Node32RunningNode)
{
case 39:
goto Node39Run;

}
//选择监测动作之下
Node34Result = EBTStatus.BT_INVALID;
Node34Run:

//
//PluginBehaviac.Nodes.Condition
//Node35
Node35Result = EBTStatus.BT_SUCCESS;
Node35Run:
Node35Result = ObjAgent.IsInValidVint3( testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node35Out:
if(Node35Result == EBTStatus.BT_SUCCESS)
{
Node34Result = EBTStatus.BT_SUCCESS;
goto Node34Out;
}
//
//PluginBehaviac.Nodes.Action
//Node36
Node36Run:
ObjAgent.LogMessage( "分支1-2 ：准备润");
Node36Result = EBTStatus.BT_FAILURE;
Node36Out:
if(Node36Result == EBTStatus.BT_SUCCESS)
{
Node34Result = EBTStatus.BT_SUCCESS;
goto Node34Out;
}
//
//PluginBehaviac.Nodes.Action
//Node39
Node39Run:
Node39Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node39Result == EBTStatus.BT_RUNNING)
{
Node32RunningNode = 39;
Node32Result = EBTStatus.BT_RUNNING;
goto Node32Out;
}
Node32RunningNode = -1;
Node39Out:
if(Node39Result == EBTStatus.BT_SUCCESS)
{
Node34Result = EBTStatus.BT_SUCCESS;
goto Node34Out;
}

Node34Out:
Node32Result = Node34Result;


Node32Out:
if(Node32Result != EBTStatus.BT_INVALID)
{
Node29WhichBranchRunning = Node32Result == EBTStatus.BT_RUNNING ? 32 : -1;
goto Node29Out;
}


Node29Out:
if(Node29Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}

Node19Out:
Node12Result = Node19Result;


Node12Out:
if(Node12Result != EBTStatus.BT_INVALID)
{
Node11WhichBranchRunning = Node12Result == EBTStatus.BT_RUNNING ? 12 : -1;
goto Node11Out;
}


Node11Out:
switch(Node11Result)
{
case EBTStatus.BT_SUCCESS:
//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false

break;
case EBTStatus.BT_FAILURE:
//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false

break;
case EBTStatus.BT_RUNNING:
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelRunning = true;//任何running都会使得并行节点running

break;
default:
throw new ArgumentOutOfRangeException();}

//
//Behaviac.Design.Nodes.ReferencedBehavior
//Node9
switch (Node9RunningNode)
{
case 9:
goto Node9Run;

}
//并行节点之下
//CHILDFINISH_LOOP循环则任何情况都会重新执行
Node9Run:
Node9Result = Node9SubTree.Tick();
 if (Node9Result == EBTStatus.BT_RUNNING)
{
Node9RunningNode = 9;
Node9Result = EBTStatus.BT_RUNNING;
goto Node9Out;
}
Node9RunningNode = -1;
Node9Out:
switch(Node9Result)
{
case EBTStatus.BT_SUCCESS:
//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false

break;
case EBTStatus.BT_FAILURE:
//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false

break;
case EBTStatus.BT_RUNNING:
Node47ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false
Node47ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false
Node47ParallelRunning = true;//任何running都会使得并行节点running

break;
default:
throw new ArgumentOutOfRangeException();}


Node47Out:
Node47Result = Node47ParallelFail ? EBTStatus.BT_FAILURE : Node47ParallelSuccess ? EBTStatus.BT_SUCCESS : Node47ParallelRunning ? EBTStatus.BT_RUNNING : EBTStatus.BT_FAILURE;
//EXIT_ABORT_RUNNINGSIBLINGS 在成功或失败的时候要退出其他running状态的子节点
if (Node47Result == EBTStatus.BT_FAILURE || Node47Result == EBTStatus.BT_SUCCESS)
{
Node3RunningNode = -1;
Node3Result = EBTStatus.BT_INVALID;
Node11RunningNode = -1;
Node11Result = EBTStatus.BT_INVALID;
Node9RunningNode = -1;
Node9Result = EBTStatus.BT_INVALID;
}
if(Node47Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}

Node0Out:
return Node0Result;

}public WrapperAI_NewTest_TestNode3(ObjAgent a)
{
ObjAgent = a;
Node9SubTree = new WrapperAI_NewTest_TestNode(a);
}

}