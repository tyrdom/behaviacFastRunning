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
private EBTStatus Node59Result { get; set; }
private EBTStatus Node58Result { get; set; }
private EBTStatus Node48Result { get; set; }
private int Node48RunningNode { get; set; } = -1;
private EBTStatus Node61Result { get; set; }
private EBTStatus Node60Result { get; set; }
private EBTStatus Node50Result { get; set; }
private int Node50RunningNode { get; set; } = -1;
private EBTStatus Node63Result { get; set; }
private EBTStatus Node62Result { get; set; }
private EBTStatus Node52Result { get; set; }
private int Node52RunningNode { get; set; } = -1;
private EBTStatus Node45Result { get; set; }
private uint Node45RandomMaxNum { get; set; } = 0;
private ushort Node45RandomNowNum { get; set; } = 0;
private EBTStatus Node0Result { get; set; }
public EBTStatus Tick()
{
 NowLocalTick++;
//
//PluginBehaviac.Nodes.Sequence
//Node0
Node0Result = EBTStatus.BT_SUCCESS;
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

//

//
//PluginBehaviac.Nodes.SelectorProbability
//Node45
Node45RandomMaxNum = (uint)(TestInt + 4);//获得权重总数
Node45RandomNowNum = FrameRandom.Random(Node45RandomMaxNum);
Node45Run:

//
//PluginBehaviac.Nodes.DecoratorWeight
//Node48
//概率选择之下，应该是权重节点
if(Node45RandomNowNum >= 3)
{
Node48RunningNode = -1;
goto Node48Out;

}
switch (Node48RunningNode)
{
case 59:
goto Node59Run;

}
//这个必然会在概率选择之下
Node48Run:

//
//PluginBehaviac.Nodes.Sequence
//Node58
//概率权重节点之下
Node58Result = EBTStatus.BT_SUCCESS;
Node58Run:

//
//PluginBehaviac.Nodes.Assignment
//Node49
Node49Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node49Out:

//
//PluginBehaviac.Nodes.Action
//Node59
Node59Run:
Node59Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node59Result == EBTStatus.BT_RUNNING)
{
Node48RunningNode = 59;
Node48Result = EBTStatus.BT_RUNNING;
goto Node48Out;
}
Node48RunningNode = -1;
Node59Out:
if(Node59Result == EBTStatus.BT_FAILURE)
{
Node58Result = EBTStatus.BT_FAILURE;
goto Node58Out;
}

Node58Out:
Node45Result = Node58Result;


Node48Out:

//
//PluginBehaviac.Nodes.DecoratorWeight
//Node50
//概率选择之下，应该是权重节点
if(Node45RandomNowNum >= TestInt + 3)
{
Node50RunningNode = -1;
goto Node50Out;

}
switch (Node50RunningNode)
{
case 61:
goto Node61Run;

}
//这个必然会在概率选择之下
Node50Run:

//
//PluginBehaviac.Nodes.Sequence
//Node60
//概率权重节点之下
Node60Result = EBTStatus.BT_SUCCESS;
Node60Run:

//
//PluginBehaviac.Nodes.Assignment
//Node51
Node51Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node51Out:

//
//PluginBehaviac.Nodes.Action
//Node61
Node61Run:
Node61Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node61Result == EBTStatus.BT_RUNNING)
{
Node50RunningNode = 61;
Node50Result = EBTStatus.BT_RUNNING;
goto Node50Out;
}
Node50RunningNode = -1;
Node61Out:
if(Node61Result == EBTStatus.BT_FAILURE)
{
Node60Result = EBTStatus.BT_FAILURE;
goto Node60Out;
}

Node60Out:
Node45Result = Node60Result;


Node50Out:

//
//PluginBehaviac.Nodes.DecoratorWeight
//Node52
//概率选择之下，应该是权重节点
if(Node45RandomNowNum >= TestInt + 4)
{
Node52RunningNode = -1;
goto Node52Out;

}
switch (Node52RunningNode)
{
case 63:
goto Node63Run;

}
//这个必然会在概率选择之下
Node52Run:

//
//PluginBehaviac.Nodes.Sequence
//Node62
//概率权重节点之下
Node62Result = EBTStatus.BT_SUCCESS;
Node62Run:

//
//PluginBehaviac.Nodes.Assignment
//Node53
Node53Run:
testVar = new VInt3(){x=0,y=0,z=0};
Node53Out:

//
//PluginBehaviac.Nodes.Action
//Node63
Node63Run:
Node63Result = ObjAgent.PlayAgeAction( "balabala");
 if (Node63Result == EBTStatus.BT_RUNNING)
{
Node52RunningNode = 63;
Node52Result = EBTStatus.BT_RUNNING;
goto Node52Out;
}
Node52RunningNode = -1;
Node63Out:
if(Node63Result == EBTStatus.BT_FAILURE)
{
Node62Result = EBTStatus.BT_FAILURE;
goto Node62Out;
}

Node62Out:
Node45Result = Node62Result;


Node52Out:


if(Node45Result == EBTStatus.BT_RUNNING)
{
Node0RunningNode = 45;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;

}
Node0RunningNode = -1;
Node45Out:
if(Node45Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//


Node0Out:
return Node0Result;

}public WrapperAI_NewTest_TestNode3(ObjAgent a)
{
ObjAgent = a;
}

}