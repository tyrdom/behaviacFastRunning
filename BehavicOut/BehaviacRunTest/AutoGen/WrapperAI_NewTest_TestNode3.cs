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
private EBTStatus Node55Result { get; set; }
private EBTStatus Node68Result { get; set; }
private int Node68RunningNode { get; set; } = -1;
private EBTStatus Node66Result { get; set; }
private EBTStatus Node73Result { get; set; }
private EBTStatus Node72Result { get; set; }
private EBTStatus Node71Result { get; set; }
private EBTStatus Node70Result { get; set; }
private int Node70NowRunTime { get; set; } = 0;
private int Node70MaxRunTime { get; set; } = -1;
private EBTStatus Node76Result { get; set; }
private EBTStatus Node75Result { get; set; }
private EBTStatus Node74Result { get; set; }
private int Node74NowRunTime { get; set; } = 0;
private int Node74MaxRunTime { get; set; } = -1;
private EBTStatus Node0Result { get; set; }
public EBTStatus Tick()
{
 NowLocalTick++;
switch ( Node0RunningNode)
{
case 72:
goto Node72Run;
case 74:
goto Node74Run;
case 76:
goto Node76Run;

}
//备注：
//PluginBehaviac.Nodes.Sequence
//Node0
Node0Result = EBTStatus.BT_SUCCESS;
Node0Run:

//备注：
//PluginBehaviac.Nodes.Assignment
//Node1
Node1Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node1Out:

//备注：
//PluginBehaviac.Nodes.DecoratorAlwaysSuccess
//Node66
Node66Result = EBTStatus.BT_SUCCESS;
Node66Run:

//备注：
//PluginBehaviac.Nodes.Sequence
//Node68
//总是成功失败之下，子动作节点不需要结果，结果由这个父节点定，下面的运行状态不可直接跳入
switch (Node68RunningNode)
{
case 55:
goto Node55Run;

}
Node68Run:

//备注：
//PluginBehaviac.Nodes.Action
//Node55
Node55Run:
Node55Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node55Result == EBTStatus.BT_RUNNING)
{
Node68RunningNode = 55;
Node68Result = EBTStatus.BT_RUNNING;
goto Node68Out;
}
Node68RunningNode = -1;
Node55Out:
if(Node55Result == EBTStatus.BT_FAILURE)
{
Node68Result = EBTStatus.BT_FAILURE;
goto Node68Out;
}

Node68Out:


Node66Out:
Node66Result = EBTStatus.BT_SUCCESS;
if(Node66Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//备注：
//PluginBehaviac.Nodes.Compute
//Node42
Node42Run:
TestInt = ObjAgent.GetIntVariable( "") / TestInt;
Node42Out:

//禁用的节点

//禁用的节点

//禁用的节点

//禁用的节点

//禁用的节点

//禁用的节点

//禁用的节点

//备注：
//PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning
//Node70
Node70NowRunTime = 0;
Node70MaxRunTime = TestInt;
Node70Run:

//备注：
//PluginBehaviac.Nodes.Sequence
//Node71
//循环直到成功或运行中之下
Node71Result = EBTStatus.BT_SUCCESS;
Node71Run:

//备注：
//PluginBehaviac.Nodes.Condition
//Node73
Node73Run:
Node73Result = TestInt == 3 ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;

Node73Out:
if(Node73Result == EBTStatus.BT_FAILURE)
{
Node71Result = EBTStatus.BT_FAILURE;
goto Node71Out;
}
//备注：
//PluginBehaviac.Nodes.Action
//Node72
Node72Run:
Node72Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node72Result == EBTStatus.BT_RUNNING)
{
Node0RunningNode = 72;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;
}
Node0RunningNode = -1;
Node72Out:
if(Node72Result == EBTStatus.BT_FAILURE)
{
Node71Result = EBTStatus.BT_FAILURE;
goto Node71Out;
}

Node71Out:
Node70Result = Node71Result;


//循环直到返回成功或者运行中只能是一帧内完成循环
if(Node70NowRunTime < Node70MaxRunTime && (Node70Result != EBTStatus.BT_SUCCESS || Node70Result != EBTStatus.BT_RUNNING))
{
Node70NowRunTime++;
goto Node70Run;
}
if(Node70Result == EBTStatus.BT_RUNNING)
{
Node70Result = EBTStatus.BT_RUNNING;
Node0RunningNode = 70;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;
}
Node0RunningNode = -1;
Node70Out:
if(Node70Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//备注：
//PluginBehaviac.Nodes.DecoratorLoopUntil
//Node74
Node74NowRunTime = 0;
Node74MaxRunTime = TestInt;
Node74Run:

//备注：
//PluginBehaviac.Nodes.Sequence
//Node75
//循环直到之下，如果下面有running状态的节点，父节点也会running，会合并，会直接跳到running的情况
Node75Result = EBTStatus.BT_SUCCESS;
Node75Run:

//备注：
//PluginBehaviac.Nodes.Action
//Node76
Node76Run:
Node76Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node76Result == EBTStatus.BT_RUNNING)
{
Node0RunningNode = 76;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;
}
Node0RunningNode = -1;
Node76Out:
if(Node76Result == EBTStatus.BT_FAILURE)
{
Node75Result = EBTStatus.BT_FAILURE;
goto Node75Out;
}

Node75Out:
Node74Result = Node75Result;


//循环直到默认不能是一帧内完成循环
if(Node74NowRunTime < Node74MaxRunTime && Node74Result != EBTStatus.BT_SUCCESS)
{
Node74NowRunTime++;
Node74Result = EBTStatus.BT_RUNNING;
Node0RunningNode = 74;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;
}
Node0RunningNode = -1;
Node74Out:
if(Node74Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}

Node0Out:
return Node0Result;

}
public WrapperAI_NewTest_TestNode3(ObjAgent a)
{
ObjAgent = a;
}

}