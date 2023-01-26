using behaviac;
using SGame.InGame.GameLogic;

public class WrapperAI_NewTest_TestNode:IBTree
{
private ObjAgent ObjAgent  {get;}
public WrapperAI_NewTest_TestNode(ObjAgent a)
{
ObjAgent = a; 
}
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

 int Node0RunningNode{get;set;} = -1;
private EBTStatus Node3Result { get; set; }
private EBTStatus Node2Result { get; set; }
private EBTStatus Node8Result { get; set; }
private EBTStatus Node7Result { get; set; }
private EBTStatus Node4Result { get; set; }
private EBTStatus Node14Result { get; set; }
private EBTStatus Node23Result { get; set; }
private EBTStatus Node24Result { get; set; }
private EBTStatus Node26Result { get; set; }
private EBTStatus Node27Result { get; set; }
private EBTStatus Node25Result { get; set; }
private EBTStatus Node31Result { get; set; }
private EBTStatus Node40Result { get; set; }
private EBTStatus Node37Result { get; set; }
private EBTStatus Node18Result { get; set; }
private EBTStatus Node30Result { get; set; }
private int Node30RunningNode { get; set; } = -1;
private EBTStatus Node33Result { get; set; }
private EBTStatus Node35Result { get; set; }
private EBTStatus Node36Result { get; set; }
private EBTStatus Node39Result { get; set; }
private EBTStatus Node34Result { get; set; }
private EBTStatus Node32Result { get; set; }
private int Node32RunningNode { get; set; } = -1;
private int Node29WhichBranchRunning { get; set; } = -1;
private EBTStatus Node19Result { get; set; }
private EBTStatus Node12Result { get; set; }
private int Node12RunningNode { get; set; } = -1;
private EBTStatus Node10Result { get; set; }
private EBTStatus Node16Result { get; set; }
private EBTStatus Node38Result { get; set; }
private EBTStatus Node17Result { get; set; }
private EBTStatus Node15Result { get; set; }
private EBTStatus Node13Result { get; set; }
private int Node13RunningNode { get; set; } = -1;
private int Node11WhichBranchRunning { get; set; } = -1;
private EBTStatus Node0Result { get; set; }
public EBTStatus Tick()
{
switch (Node0RunningNode)
{
case 10:
goto Node10Enter;

}
//
//PluginBehaviac.Nodes.Sequence
//Node0
Node0Result = EBTStatus.BT_INVALID;
Node0Enter:

//
//PluginBehaviac.Nodes.Assignment
//Node1
Node1Enter:
testVar = new VInt3(){x=0,y=0,z=0};


Node1Out:

//
//PluginBehaviac.Nodes.Assignment
//Node5
Node5Enter:
testEnum = SkillSlotType.SLOT_SKILL_2;


Node5Out:

//
//PluginBehaviac.Nodes.Assignment
//Node9
Node9Enter:
TestInt = 0;


Node9Out:

//
//PluginBehaviac.Nodes.Condition
//Node3
Node3Result = EBTStatus.BT_SUCCESS;
Node3Enter:
Node3Result = ObjAgent.IsInValidVint3(testVar) == false ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node3Out:
if(Node3Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Action
//Node2
Node2Enter:
ObjAgent.LogMessage("Seq：合法点");
Node2Result = EBTStatus.BT_SUCCESS;


Node2Out:
if(Node2Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.Selector
//Node4
Node4Result = EBTStatus.BT_INVALID;
Node4Enter:

//
//PluginBehaviac.Nodes.Condition
//Node8
Node8Result = EBTStatus.BT_SUCCESS;
Node8Enter:
Node8Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node8Out:
if(Node8Result == EBTStatus.BT_SUCCESS)
{
Node4Result = EBTStatus.BT_SUCCESS;
goto Node4Out;
}
//
//PluginBehaviac.Nodes.Action
//Node7
Node7Enter:
ObjAgent.LogMessage("Select：害是合法点");
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
Node6Enter:
testVar = new VInt3(){x=1,y=0,z=1};


Node6Out:


Node4Out:
if(Node4Result == EBTStatus.BT_FAILURE)
{
Node0Result = EBTStatus.BT_FAILURE;
goto Node0Out;
}
//
//PluginBehaviac.Nodes.SelectorLoop
//Node11

Node11Enter:

//
//PluginBehaviac.Nodes.WithPrecondition
//Node12
//选择监测
Node12Result = EBTStatus.BT_INVALID;
Node12Enter:

//
//PluginBehaviac.Nodes.Condition
//Node14
//选择监测条件
Node14Result = EBTStatus.BT_SUCCESS;
Node14Enter:
Node14Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node14Out:
if(Node14Result == EBTStatus.BT_FAILURE)
{
goto Node12Out;
}
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
case 33:
goto Node33Enter;

}
//选择监测动作
Node19Result = EBTStatus.BT_INVALID;
Node19Enter:

//
//PluginBehaviac.Nodes.Assignment
//Node20
Node20Enter:
testVar = new VInt3(){x=0,y=0,z=0};


Node20Out:

//
//PluginBehaviac.Nodes.Assignment
//Node21
Node21Enter:
testEnum = SkillSlotType.SLOT_SKILL_2;


Node21Out:

//
//PluginBehaviac.Nodes.Assignment
//Node22
Node22Enter:
TestInt = 0;


Node22Out:

//
//PluginBehaviac.Nodes.Condition
//Node23
Node23Result = EBTStatus.BT_SUCCESS;
Node23Enter:
Node23Result = ObjAgent.IsInValidVint3(testVar) == false ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node23Out:
if(Node23Result == EBTStatus.BT_FAILURE)
{
Node19Result = EBTStatus.BT_FAILURE;
goto Node19Out;
}
//
//PluginBehaviac.Nodes.Action
//Node24
Node24Enter:
ObjAgent.LogMessage("Seq：合法点");
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
Node25Enter:

//
//PluginBehaviac.Nodes.Condition
//Node26
Node26Result = EBTStatus.BT_SUCCESS;
Node26Enter:
Node26Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node26Out:
if(Node26Result == EBTStatus.BT_SUCCESS)
{
Node25Result = EBTStatus.BT_SUCCESS;
goto Node25Out;
}
//
//PluginBehaviac.Nodes.Action
//Node27
Node27Enter:
ObjAgent.LogMessage("Select：害是合法点");
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
Node28Enter:
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

Node29Enter:

//
//PluginBehaviac.Nodes.WithPrecondition
//Node30
//选择监测
Node30Result = EBTStatus.BT_INVALID;
Node30Enter:

//
//PluginBehaviac.Nodes.Condition
//Node31
//选择监测条件
Node31Result = EBTStatus.BT_SUCCESS;
Node31Enter:
Node31Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node31Out:
if(Node31Result == EBTStatus.BT_FAILURE)
{
goto Node30Out;
}
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
goto Node37Enter;

}
//选择监测动作
Node18Result = EBTStatus.BT_INVALID;
Node18Enter:

//
//PluginBehaviac.Nodes.Action
//Node40
Node40Enter:
ObjAgent.LogMessage("分支1-1 ：准备润");
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
Node37Enter:
Node37Result = ObjAgent.PlayAgeAction("balabala");
 if (Node37Result == EBTStatus.BT_RUNNING )
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
//选择监测
Node32Result = EBTStatus.BT_INVALID;
Node32Enter:

//
//PluginBehaviac.Nodes.Action
//Node33
//选择监测条件
Node33Enter:
Node33Result = ObjAgent.IsOffline();
 if (Node33Result == EBTStatus.BT_RUNNING )
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
goto Node39Enter;

}
//选择监测动作
Node34Result = EBTStatus.BT_INVALID;
Node34Enter:

//
//PluginBehaviac.Nodes.Condition
//Node35
Node35Result = EBTStatus.BT_SUCCESS;
Node35Enter:
Node35Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node35Out:
if(Node35Result == EBTStatus.BT_SUCCESS)
{
Node34Result = EBTStatus.BT_SUCCESS;
goto Node34Out;
}
//
//PluginBehaviac.Nodes.Action
//Node36
Node36Enter:
ObjAgent.LogMessage("分支1-2 ：准备润");
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
Node39Enter:
Node39Result = ObjAgent.PlayAgeAction("balabala");
 if (Node39Result == EBTStatus.BT_RUNNING )
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


Node19Out:
Node12Result = Node19Result;


Node12Out:
if(Node12Result != EBTStatus.BT_INVALID)
{
Node11WhichBranchRunning = Node12Result == EBTStatus.BT_RUNNING ? 12 : -1;
goto Node11Out;
}

//
//PluginBehaviac.Nodes.WithPrecondition
//Node13
//选择监测
Node13Result = EBTStatus.BT_INVALID;
Node13Enter:

//
//PluginBehaviac.Nodes.Action
//Node10
//选择监测条件
Node10Enter:
Node10Result = ObjAgent.IsOffline();
 if (Node10Result == EBTStatus.BT_RUNNING )
{
Node0RunningNode = 10;
Node0Result = EBTStatus.BT_RUNNING;
goto Node0Out;
}
Node0RunningNode = -1;

Node10Out:
if(Node10Result == EBTStatus.BT_FAILURE)
{
goto Node13Out;
}
//如果切换了分支后再通过，那么会重置running下面running的节点到-1
if (Node11WhichBranchRunning != 13)
{
Node13RunningNode = -1;
}
//
//PluginBehaviac.Nodes.Selector
//Node15
switch (Node13RunningNode)
{
case 17:
goto Node17Enter;

}
//选择监测动作
Node15Result = EBTStatus.BT_INVALID;
Node15Enter:

//
//PluginBehaviac.Nodes.Condition
//Node16
Node16Result = EBTStatus.BT_SUCCESS;
Node16Enter:
Node16Result = ObjAgent.IsInValidVint3(testVar) == true ? EBTStatus.BT_SUCCESS : EBTStatus.BT_FAILURE;
;


Node16Out:
if(Node16Result == EBTStatus.BT_SUCCESS)
{
Node15Result = EBTStatus.BT_SUCCESS;
goto Node15Out;
}
//
//PluginBehaviac.Nodes.Action
//Node38
Node38Enter:
ObjAgent.LogMessage("分支2 ：准备润");
Node38Result = EBTStatus.BT_FAILURE;


Node38Out:
if(Node38Result == EBTStatus.BT_SUCCESS)
{
Node15Result = EBTStatus.BT_SUCCESS;
goto Node15Out;
}
//
//PluginBehaviac.Nodes.Action
//Node17
Node17Enter:
Node17Result = ObjAgent.PlayAgeAction("balabala");
 if (Node17Result == EBTStatus.BT_RUNNING )
{
Node13RunningNode = 17;
Node13Result = EBTStatus.BT_RUNNING;
goto Node13Out;
}
Node13RunningNode = -1;

Node17Out:
if(Node17Result == EBTStatus.BT_SUCCESS)
{
Node15Result = EBTStatus.BT_SUCCESS;
goto Node15Out;
}

Node15Out:
Node13Result = Node15Result;


Node13Out:
if(Node13Result != EBTStatus.BT_INVALID)
{
Node11WhichBranchRunning = Node13Result == EBTStatus.BT_RUNNING ? 13 : -1;
goto Node11Out;
}


Node11Out:


Node0Out:
return Node0Result;

}
}