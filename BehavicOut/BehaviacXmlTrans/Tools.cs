using System.Xml.Linq;

namespace BehaviacXmlTrans;

public static class Tools
{
    public static string ConvertAXmlFile(string path, string editPath, out string csName,
        out string objClassName)
    {
        var root = XElement.Load(path);


        var fileName1 = path.Replace(editPath, "").Replace(Path.DirectorySeparatorChar, '_');
        var lastIndexOf = fileName1[1..fileName1.LastIndexOf(".", StringComparison.Ordinal)];
        csName = lastIndexOf + ".cs";
        //pudge 记录文件名 debug用
        Configs.CurrentFileName = csName;
        var debugString = Configs.DebugMode ? $"#define {Configs.DebugModeString}\n" : "";
        var s = debugString +
                "using System;\nusing behaviac;\nusing SGame.InGame.GameLogic;\nusing PBConfig;\nusing PB;\nusing System.Collections.Generic;\n\npublic class " +
                lastIndexOf +
                CSharpStrings.RunTimeInterface + "\n{\n";

        // foreach (var xAttribute in rootFirstAttribute)
        // {
        //     Console.Out.WriteLine(xAttribute);
        // }

        var element = root.Elements("Node").FirstOrDefault() ?? throw new NullReferenceException();


        // Console.Out.WriteLine(xAttribute.Name);

        var value = element.Attribute("AgentType")?.Value ?? throw new NullReferenceException();
        objClassName = CSharpStrings.SimpleRemoveParameterAndActionHead(value);
        string objName = "agent";
        var name = $"private {objClassName} {objName}  " + ";\n";
        s += name;

        var enumerable = element.Elements();
        var subtreeConstruct = "";
        var initAges = "";
        foreach (var xElement in enumerable)
        {
            // Console.Out.WriteLine(xElement.Name);
            switch (xElement.Name.ToString())
            {
                case "Parameters":
                    var parameters = TransParams(xElement);
                    var localParameter = "// Local Parameter";
                    s += localParameter + parameters;
                    break;
                case "Connector":

                    var result = ConnectorTransFromRoot(xElement, objName, out var countParams,
                        out subtreeConstruct, out var agesToInit);
                    s += countParams;
                    s += result;
                    if (agesToInit.Any())
                    {
                        initAges = agesToInit.Aggregate("",
                            (seed, x) => seed + $"Workspace.Instance.PreLoadAge({x});\n");
                    }

                    break;
            }
        }

        var constructor =
            $"public {lastIndexOf}({objClassName} a)\n{{\n{objName} = a;\n{initAges}{subtreeConstruct}}}\n";
        s += constructor;

        s += "\n}";

        return s;
    }

    public static string ConnectorTransFromRoot(XElement xElement, string agentObjName,
        out string treeStatusParamsAndAgentObj, out string subTreeConstruct, out HashSet<string> agesToInit)
    {
        agesToInit = new HashSet<string>();
        treeStatusParamsAndAgentObj = "\n";
        var head =
            $"public {CSharpStrings.BtStatusEnumName} Tick()\n{{\n NowLocalTick = {Configs.GetTickFuncString};\n";
        var res = "";
        var node = xElement.Elements().FirstOrDefault(x => x.Name == "Node") ?? throw new NullReferenceException();

        var value = node.Attribute("Id")?.Value ?? throw new NullReferenceException();
        var rootNodeId = int.TryParse(value, out var ii) ? ii : throw new ArgumentOutOfRangeException();
        var rootRunningNodeString = " Node" + rootNodeId + "RunningNode";
        var rootRunningEnum = " Node" + rootNodeId + "Running";
        const string tickCount = "\nprivate int NowLocalTick  = -1;\n";
        var nodePath = Array.Empty<NodeMsg>();

        var s = TransNode(node, rootNodeId, -1, "Root", agentObjName, rootNodeId, null, agesToInit, nodePath,
            out var intsMap,
            out var tsp, out var nodeResultInitString, out subTreeConstruct) + "\n";
        var rootStatus = $"{tickCount}\nprivate {rootRunningEnum} {rootRunningNodeString} = {rootRunningEnum}.None;\n";
        var s1 = intsMap.Aggregate("None,", (sd, iii) => sd + $"\nNode{iii.Id},")[..^1];
        rootStatus += $"private enum {rootRunningEnum} \n{{\n{s1}\n}}\n";
        const string behaviorTreeStatus = "// BehaviorTreeStatus\n";
        treeStatusParamsAndAgentObj += behaviorTreeStatus + rootStatus + tsp;

        res += s;


        if (!intsMap.Any()) return head + res + "\n}";
        var aggregate = intsMap.Aggregate("",
            (seed, x) =>
            {
                var aggregate1 = Configs.DebugMode
                    ? x.PathMsg.Aggregate("", ((ss, record) => ss + record.GenDebugLog(agentObjName)))
                    : "";
                return seed + $"case {rootRunningEnum}.Node{x.Id}:\n{aggregate1}goto Node{x.Id}Run;\n";
            });
        var switchString = nodeResultInitString + $"switch ({rootRunningNodeString})\n{{\n"
                                                + aggregate + "\n}\n";
        return head + switchString + res + "\n}\n";
    }

    private static string TransNode(XElement node, int parentId, int extraId, string parentTypeString,
        string agentObjName, int runningGoNode,
        INodeConfig? nodeConfig,
        HashSet<string> agesToInit,
        NodeMsg[] nodePathMsg,
        out List<RunningNodePathTrace> headRunningSwitchIdSToNodePath,
        out string classProperties,
        out string nodeResultInitString, out string subTreeConstruct)
    {
        classProperties = "";
        headRunningSwitchIdSToNodePath = new List<RunningNodePathTrace>();
        nodeResultInitString = "";
        subTreeConstruct = "";
        var firstOrDefault = node.Elements("Comment").FirstOrDefault();
        var value = firstOrDefault?.Attribute("Text")?.Value;
        var res = "";
        var enable = node.Attribute("Enable")?.Value;
        if (enable == "false")
        {
            res += "//禁用的节点\n";
            if (parentTypeString == "Root")
            {
                res += $"return {CSharpStrings.Invalid};//完全被禁用了";
            }

            return res;
        }

        res += "//备注：" + value + "\n";
        var id = node.Attribute("Id")?.Value ?? throw new NullReferenceException();
        var intId = int.TryParse(id, out var iii) ? iii : throw new ArgumentException(id);
        if (intId == 80)
        {
        }

        var idString = "Node" + id;
        var parentIdString = "Node" + parentId;
        var nodeType = node.Attribute("Class")?.Value;

        if (nodeType == null) throw new NullReferenceException($"{idString} type is Null");


        res += "//" + nodeType + "\n";

        // if (id == "5")
        // {
        //     Console.Out.WriteLine($"{id}");
        // }

        var runLabel = idString + "Run:\n";
        var debugLogString = Configs.DebugMode ? Configs.DebugLogString(agentObjName, intId, nodeType) : "";
        var outLabel = idString + "Out:\n";
        var skipLabel = idString + "Skip:\n"; //部分情况使用
        res = res + "//" + idString + "\n";
        var xElements = node.Elements("Connector").ToArray();


        //与父节点关系
        var needResult = true;
        var localSwitch = false;
        var parentEndStringGoto = parentIdString + "Out;\n";
        var parentVarString = parentIdString + "Result";
        var resultVarString = idString + "Result";
        var parentVarNeedString = "";
        var outOpNeed = "";

        var onEnter = "";
        var optEnterLabel = "";
        var runInit = "";
        switch (parentTypeString)
        {
            case "Root":

                parentVarNeedString = $"return {resultVarString};";
                break;
            case "PluginBehaviac.Nodes.DecoratorNot":
                // Node44Result = Node43Result switch
                // {
                //     EBTStatus.BT_RUNNING => EBTStatus.BT_RUNNING,
                //     EBTStatus.BT_SUCCESS => EBTStatus.BT_FAILURE,
                //     EBTStatus.BT_FAILURE => EBTStatus.BT_SUCCESS,
                //     EBTStatus.BT_INVALID => throw new ArgumentOutOfRangeException(),
                //     _ => throw new ArgumentOutOfRangeException()
                // };
                parentVarNeedString =
                    $"{parentVarString} ={resultVarString} switch\n{{\n" +
                    $"{CSharpStrings.Running} => {CSharpStrings.Running},\n" +
                    $"{CSharpStrings.Success} => {CSharpStrings.Fail},\n" +
                    $"{CSharpStrings.Fail} => {CSharpStrings.Success},\n" +
                    $"{CSharpStrings.Invalid} => throw new ArgumentOutOfRangeException(),\n" +
                    $"_ => throw new ArgumentOutOfRangeException()," +
                    "\n};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess" or "PluginBehaviac.Nodes.DecoratorAlwaysFailure":
                onEnter = "//总是成功失败之下，子动作节点不需要结果，结果由这个父节点定，下面的运行状态不可直接跳入\n";
                needResult = false;
                localSwitch = true;
                runningGoNode = intId;
                break;
            case "PluginBehaviac.Nodes.DecoratorFailureUntil":
                onEnter = "//返回失败直到次数之下，子动作节点不需要结果，结果由这个父节点定，下面的运行状态不可直接跳入\n";
                needResult = false;
                localSwitch = true;
                runningGoNode = intId;
                break;
            case "PluginBehaviac.Nodes.DecoratorSuccessUntil":
                onEnter = "//返回成功直到次数之下，子动作节点不需要结果，结果由这个父节点定，下面的运行状态不可直接跳入\n";
                needResult = false;
                localSwitch = true;
                runningGoNode = intId;
                break;
            case "PluginBehaviac.Nodes.Sequence" or "PluginBehaviac.Nodes.And":

                parentVarNeedString = $"if({resultVarString} == {CSharpStrings.Fail})\n"
                                      + "{\n"
                                      + $"{parentVarString} = {CSharpStrings.Fail};\n"
                                      + $"goto {parentEndStringGoto}"
                                      + "}";
                break;
            case "PluginBehaviac.Nodes.Selector" or "PluginBehaviac.Nodes.Or":

                parentVarNeedString = $"if({resultVarString} == {CSharpStrings.Success})\n"
                                      + "{\n"
                                      + $"{parentVarString} = {CSharpStrings.Success};\n"
                                      + $"goto {parentEndStringGoto}"
                                      + "}";
                break;
            case "PluginBehaviac.Nodes.IfElse_condition":

                outOpNeed =
                    $"if({resultVarString} == {CSharpStrings.Fail})\n{{\ngoto Node{extraId}Enter;\n}}"; // 这个下面如果放计算或者赋值会自己报错
                break;
            case "PluginBehaviac.Nodes.IfElse_if":
                onEnter = "// IfElse_if分支\n";
                parentVarNeedString = parentVarString + " = " + resultVarString + ";\n";
                outOpNeed = $"goto Node{parentId}Out;\n";
                break;
            case "PluginBehaviac.Nodes.IfElse_else":
                optEnterLabel = $"{idString}Enter:\n";
                parentVarNeedString = parentVarString + " = " + resultVarString + ";\n";
                break;
            case "PluginBehaviac.Nodes.WithPreconditionPrecondition":

                onEnter = "//选择监测条件之下\n";
                parentVarNeedString = $"if({resultVarString} == {CSharpStrings.Fail})\n" +
                                      "{\n"
                                      + $"goto {parentEndStringGoto}"
                                      + "}\n"
                                      // + $"Node{extraId}Result = {CSharpStrings.Success};\n"
                                      + "//如果切换了分支后再通过，那么会重置running下面running的节点到-1\n"
                                      + $"if (Node{extraId}WhichBranchRunning != {parentId})\n{{\nNode{parentId}RunningNode = Node{parentId}Running.None;\n}}";
                // {$"Node{runningGoNode}Running.None"
                //     Node13RunningNode = -1;
                // }
                break;
            case "PluginBehaviac.Nodes.WithPreconditionAction":
                onEnter = "//选择监测动作之下\n";
                runningGoNode = parentId;
                localSwitch = true;

                // {
                //     case 17:
                //         goto Node17Enter;
                // }
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                onEnter = "";
                parentVarNeedString = $"if({resultVarString} != {CSharpStrings.Fail})\n" +
                                      "{\n"
                                      // + $"{parentVarString} = {resultVarString};\n"
                                      //Node11WhichBranchRunning =(Node13Result == EBTStatus.BT_RUNNING) ? 13 : -1;
                                      + $"Node{parentId}WhichBranchRunning = {resultVarString} == EBTStatus.BT_RUNNING ? {intId} : -1;\n"
                                      // if (Node13Result == EBTStatus.BT_RUNNING)
                                      // {
                                      //     Node11WhichBranchRunning = 13;
                                      // }
                                      + $"{parentVarString} = {resultVarString};\n"
                                      + $"goto {parentEndStringGoto}"
                                      + "}\n";
                break;
            case "PluginBehaviac.Nodes.SelectorProbability":
                onEnter = "//概率选择之下，应该是权重节点\n";
                var selectorProbabilityConfig = (SelectorProbabilityConfig) nodeConfig!;
                var aExp = selectorProbabilityConfig.IdToExpression.TryGetValue(intId, out var exp)
                    ? exp
                    : throw new KeyNotFoundException();
                // if (Node45RandomNowNum >=3)
                // {
                //     Node48RunningNode = -1;
                //     goto Node48Out;
                // }
                onEnter +=
                    $"if(Node{parentId}RandomNowNum >= {aExp})\n{{\nNode{intId}RunningNode = Node{intId}Running.None;\ngoto Node{intId}Out;\n\n}}\n";
                localSwitch = true;
                runningGoNode = intId;
                break;
            case "PluginBehaviac.Nodes.SelectorStochastic":
                onEnter = "//随机选择之下\n";
                onEnter += $"{idString}Enter:\n";

                parentVarNeedString = $"if({resultVarString} == {CSharpStrings.Success})\n"
                                      + "{\n"
                                      + $"{parentVarString} = {CSharpStrings.Success};\n"
                                      + $"goto {parentEndStringGoto}"
                                      + $"}}\ngoto Node{parentId}Run;\n";
                // skipString = skipLabel;
                break;
            case "PluginBehaviac.Nodes.DecoratorWeight":
                onEnter = $"//概率权重节点之下{id}\n";
                parentVarNeedString = $"Node{extraId}Result = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                onEnter = "//循环节点下\n";
                var decoratorLoopConfig = (DecoratorLoopConfig) nodeConfig!;
                if (decoratorLoopConfig.LoopInOneTick)
                {
                    localSwitch = true;
                    runningGoNode = intId;
                }

                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                onEnter = "//循环直到之下，如果下面有running状态的节点，父节点也会running，会合并，会直接跳到running的情况\n";
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                onEnter = "//循环直到成功或运行中之下\n";
                localSwitch = true;
                runningGoNode = intId;
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.Parallel":
                onEnter = "//并行节点之下\n";
                localSwitch = true;
                runningGoNode = intId;
                var parentParallelNodeConfigs = (ParallelNodeConfig) nodeConfig!;
                var childFinishPolicyCheck
                    = parentParallelNodeConfigs?.ChildFinishPolicy switch
                    {
                        "CHILDFINISH_LOOP" => $"//CHILDFINISH_LOOP循环则任何情况都会重新执行\n",
                        "CHILDFINISH_ONCE" =>
                            $"//CHILDFINISH_ONCE则看状态\nif({resultVarString} != {CSharpStrings.Running} && {resultVarString} " +
                            $"!= {CSharpStrings.Invalid})\n{{\ngoto {idString}Out;\n}}\n//CHILDFINISH_ONCE情况下，不是第一次执行或者running，则跳过执行\n",
                        _ => throw new NullReferenceException()
                    };
                onEnter += childFinishPolicyCheck;
                var successAll = parentParallelNodeConfigs.SuccessPolicy switch
                {
                    "SUCCEED_ON_ALL" => true,
                    "SUCCEED_ON_ONE" => false,
                    _ => throw new NullReferenceException()
                };
                var failAll = parentParallelNodeConfigs.FailPolicy switch
                {
                    "FAIL_ON_ALL" => true,
                    "FAIL_ON_ONE" => false,
                    _ => throw new NullReferenceException()
                };
                var onAllFail = failAll
                    ? $"{parentIdString}ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false\n"
                    : "//需要failOne，初始为false遇到失败才会置为true\n";
                var successProcess =
                    (successAll
                        ? "//需要successAll，初始值为true遇到失败情况置为false\n"
                        : $"{parentIdString}ParallelSuccess = true;//需要successOne，初始为false遇到成功则置为true\n") +
                    onAllFail;
                var onAllSuccess = successAll
                    ? $"{parentIdString}ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false\n"
                    : "//需要successOne，初始为false遇到成功则置为true\n";
                var failProcess =
                    (failAll
                        ? "//需要failAll，初始值为true遇到成功情况置为false\n"
                        : $"{parentIdString}ParallelFail = true;//需要failOne，初始为false遇到失败才会置为true\n") +
                    onAllSuccess;
                var runningProcess = onAllFail +
                                     onAllSuccess +
                                     $"{parentIdString}ParallelRunning = true;//任何running都会使得并行节点running\n";
                parentVarNeedString = $"switch({resultVarString})\n{{\n" +
                                      $"case {CSharpStrings.Success}:\n{successProcess}\nbreak;\n" +
                                      $"case {CSharpStrings.Fail}:\n{failProcess}\nbreak;\n" +
                                      $"case {CSharpStrings.Running}:\n{runningProcess}\nbreak;\n" +
                                      "default:\nthrow new ArgumentOutOfRangeException();" +
                                      "}\n";
                // switch (Node46Result)
                // {
                //
                //     case EBTStatus.BT_SUCCESS:
                //         Node47ParallelSuccess = true;
                //         Node47ParallelFail = false;
                //         break;
                //     case EBTStatus.BT_FAILURE:
                //         break;
                //     case EBTStatus.BT_RUNNING:
                //         break;
                //     default:
                //         throw new ArgumentOutOfRangeException();
                // }

                break;
            default:
                throw new ArgumentException($"no fit parent {parentTypeString} id {parentId}");
        }

        var append = localSwitch
            ? new[] {new NodeMsg(intId, nodeType)}
            : nodePathMsg.Append(new NodeMsg(intId, nodeType)).ToArray();

        // 解析节点
        var beforeOut = "";
        var acp2 = "";
        var outPutString = "";
        var enterDo = "";
        var goNode = "Node" + runningGoNode;
        var runningNodeString =
            goNode + "RunningNode";
        var runString = needResult ? resultVarString + $" = {CSharpStrings.Running};\n" : "";
        var statusVar = needResult
            ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString};\n"
            : "";
        var statusVarInitSuccess = needResult
            ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} = {CSharpStrings.Success};\n"
            : "";
        var mustStatusVar =
            $"private {CSharpStrings.BtStatusEnumName} {resultVarString};\n"; //有子节点的情况，虽然父节点不要求状态，但是字节点需要

        var successString = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "";
        var failString = needResult ? resultVarString + $" = {CSharpStrings.Fail};\n" : "";
        var continueRunningString = $"{runningNodeString} = {goNode}Running.Node{id};\n"
                                    + $"{goNode}Result = {CSharpStrings.Running};\n"
                                    + $"goto Node{runningGoNode}Out;\n";

        var inOneTickLoopLabel = "";
        var stopRunningEnum = $"Node{runningGoNode}Running.None";
        var outRunningString = $"{runningNodeString} = {stopRunningEnum};\n"; //这里runningGoNode要保证不会变了
        string countString;
        // $"case {id}:\n" + $"goto {idString}Run;\n";
        INodeConfig? extraConfig = null;
        var loopInOneTick = false; // 循环类 一个tick内完成
        var needCheckRunningWhenOut = intId != runningGoNode; //避免死循环
        var runningCheckAndGo = needCheckRunningWhenOut
            ? $"if({resultVarString} == {CSharpStrings.Running})\n{{\n{continueRunningString}\n}}\n{outRunningString}"
            : "";
        switch (nodeType)
        {
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                acp2 = mustStatusVar;
                runInit = successString;
                outPutString = successString;
                break;
            case "PluginBehaviac.Nodes.DecoratorAlwaysFailure":
                acp2 = mustStatusVar;
                runInit = failString;
                outPutString = failString;
                break;
            case "PluginBehaviac.Nodes.DecoratorFailureUntil" or "PluginBehaviac.Nodes.DecoratorSuccessUntil":
                acp2 = mustStatusVar;
                var okk = nodeType switch
                {
                    "PluginBehaviac.Nodes.DecoratorFailureUntil" => new[]
                        {CSharpStrings.Fail, CSharpStrings.Success},
                    "PluginBehaviac.Nodes.DecoratorSuccessUntil" => new[]
                        {CSharpStrings.Success, CSharpStrings.Fail},
                    _ => throw new ArgumentOutOfRangeException()
                };
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();
                var s1 = $"{resultVarString} = {okk[0]};" +
                         $"{idString}NowRunTime++;\n}}\n" +
                         $"{resultVarString} = {okk[1]};";
                if (countString == "const int -1") //不计次数，相当于总是失败、成功
                {
                    beforeOut = needResult ? resultVarString + $" = {okk[0]};\n" : "";
                    ;
                }
                else if (countString.StartsWith("const")) //常量次数，直接写死最大次数
                {
                    var removeParameterAndActionHead = CSharpStrings.SimpleRemoveParameterAndActionHead(countString);
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    beforeOut = $"if({idString}NowRunTime < {removeParameterAndActionHead})\n{{\n" +
                                s1;
                }
                else //变量次数，需要在进入时设置一次次数
                {
                    acp2 =
                        $"private {CSharpStrings.BtStatusEnumName} {resultVarString} = {CSharpStrings.Invalid}//第一次进入会初始化Count\n";
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime = -1;\n";
                    var removeParameterAndActionHead = CSharpStrings.SimpleRemoveParameterAndActionHead(countString);
                    onEnter = $"if({resultVarString} == {CSharpStrings.Invalid})\n{{\n\n" +
                              $"{idString}MaxRunTime = {removeParameterAndActionHead}}}\n";
                    beforeOut = $"if({idString}NowRunTime < {idString}MaxRunTime)\n{{\n" +
                                s1;
                }

                break;
            case "PluginBehaviac.Nodes.DecoratorNot":
                acp2 = mustStatusVar;
                break;
            case "PluginBehaviac.Nodes.Sequence" or "PluginBehaviac.Nodes.And":
                acp2 = mustStatusVar;
                runInit = successString;
                break;
            case "PluginBehaviac.Nodes.Selector" or "PluginBehaviac.Nodes.Or":
                acp2 = mustStatusVar;
                runInit = failString;
                break;
            case "PluginBehaviac.Nodes.False":
                acp2 = statusVar;
                beforeOut = failString;
                break;
            case "PluginBehaviac.Nodes.True":
                acp2 = statusVar;
                beforeOut = successString;
                break;
            case "PluginBehaviac.Nodes.IfElse":
                acp2 = mustStatusVar;
                runInit = successString;
                var orDefault =
                    xElements.FirstOrDefault(x =>
                        (x.Attribute("Identifier")?.Value ?? throw new NullReferenceException()) == "_else") ??
                    throw new ArgumentOutOfRangeException();
                var value1 = orDefault.Element("Node")?.Attribute("Id")?.Value ?? throw new NullReferenceException();
                extraId = int.Parse(value1);
                break;
            case "PluginBehaviac.Nodes.Condition":
                ConditionConvert(node, needResult, resultVarString, idString,
                    agentObjName, agesToInit, out beforeOut, out acp2);
                break;
            case "PluginBehaviac.Nodes.Noop":
                acp2 = statusVar;
                enterDo = successString;
                break;

            case "PluginBehaviac.Nodes.Action":

                acp2 = statusVar;

                var method = node.Attribute("Method")?.Value ??
                             throw new NullReferenceException($"no method in action node {id}");

                var findReturnType =
                    CSharpStrings.FindReturnTypeAndEtc(method, agentObjName, idString, out var methodName,
                        out var constListP, out var ages);
                agesToInit.UnionWith(ages);
                acp2 += constListP;

                if (findReturnType == "behaviac::EBTStatus")
                {
                    var varString = (needResult ? resultVarString + " = " : "") + methodName + ";\n";
                    var runningNow = needResult
                        ? "if (" + resultVarString + $" == {CSharpStrings.Running})\n" +
                          "{\n" + continueRunningString
                          + "}\n"
                          + outRunningString
                        : "";

                    headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                    beforeOut = varString + runningNow;
                }
                else
                {
                    var a = methodName + ";\n";
                    var resOp = node.Attribute("ResultOption")?.Value ??
                                throw new NullReferenceException($"not Res @ {id}");
                    var stringToEnum = CSharpStrings.StringToEnum(resOp);
                    var b = resultVarString + " = " + stringToEnum + ";\n";
                    beforeOut = a + b;
                }

                break;
            case "PluginBehaviac.Nodes.Assignment":
                acp2 += statusVarInitSuccess;

                var l = node.Attribute("Opl")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var r = node.Attribute("Opr")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var rr = ConvertArmToFuncOrParam(agentObjName, r, idString, agesToInit, out var constListParam);
                acp2 += constListParam;
                beforeOut = CSharpStrings.SimpleRemoveParameterAndActionHead(l) + " = " +
                            rr + ";\n";
                // parentVarNeedString = "";
                break;
            case "PluginBehaviac.Nodes.Compute":
                parentVarNeedString = "";
                var o = node.Attribute("Opl")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var p1 = node.Attribute("Opr1")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                var p2 = node.Attribute("Opr2")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                var op = node.Attribute("Operator")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                beforeOut = CSharpStrings.SimpleRemoveParameterAndActionHead(o)
                            + " = "
                            + ConvertArmToFuncOrParam(agentObjName, p1, idString + "Op1", agesToInit, out var cLsp1)
                            + CSharpStrings.GenOperator(op)
                            + ConvertArmToFuncOrParam(agentObjName, p2, idString + "Op2", agesToInit, out var cLsp2) +
                            ";\n";
                acp2 += cLsp1;
                acp2 += cLsp2;
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                // acp2 = needResult
                //     ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                //     : "";
                // private int Node11WhichBranchRunning { get; set; } = -1;
                acp2 = mustStatusVar;
                acp2 += $"private int {idString}WhichBranchRunning = -1;\n";
                runInit = failString;

                outPutString +=
                    runningCheckAndGo;
                if (needCheckRunningWhenOut)
                {
                    headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                }

                // tail = "";
                extraId = intId;
                break;
            case "PluginBehaviac.Nodes.SelectorStochastic":
                acp2 = mustStatusVar;
                acp2 += $"private int {idString}BranchOrderRunMaxTime;\n";


                var allChildrenIds = GetAllChildrenAttr(node, "Id");
                // private Node54SelectorStochastic[] Node54BranchOrder = new[] {Node54SelectorStochastic.Node67, Node54SelectorStochastic.Node56, Node54SelectorStochastic.Node57};
                //
                // private enum Node54SelectorStochastic
                // {
                //     Node67,Node56,Node57
                // }        
                var s5 = allChildrenIds.Aggregate("", (s3, s4) => s3 + $"Node{s4},")[..^1];

                acp2 += $"private enum {idString}SelectorStochastic\n{{\n{s5}\n}}\n";

                var s6 = allChildrenIds.Aggregate("", (s3, s4) => s3 + $"{idString}SelectorStochastic.Node{s4},")[..^1];
                acp2 += $"private readonly {idString}SelectorStochastic[] {idString}BranchOrder = new[] {{{s6}}};";
                var length = allChildrenIds.Length;
                enterDo += $@"for (uint i = {length}; i > 1; i--)
{{
var random = FrameRandom.Random(i);
({idString}BranchOrder[random], {idString}BranchOrder[i - 1]) =
({idString}BranchOrder[i - 1], {idString}BranchOrder[random]);
}}
{idString}BranchOrderRunMaxTime = {length};
";
                var s7 = allChildrenIds.Aggregate("",
                    (s3, s4) => s3 + $"case {idString}SelectorStochastic.Node{s4}:\ngoto Node{s4}Enter;\n");

                runInit += $@"{resultVarString} = {CSharpStrings.Fail};
{idString}BranchOrderRunMaxTime--;
if ({idString}BranchOrderRunMaxTime>=0)
{{
switch ({idString}BranchOrder[{idString}BranchOrderRunMaxTime])
{{
{s7}
}}
}}
else
{{
goto {idString}Out;
}}";


                break;
            case "PluginBehaviac.Nodes.SelectorProbability":
                acp2 = mustStatusVar;
                acp2 += $"private uint {idString}RandomMaxNum = 0;\n";
                acp2 += $"private ushort {idString}RandomNowNum = 0;\n";
                var allChildrenAttr = GetAllChildrenAttr(node, "Weight");
                var allChildrenId = GetAllChildrenAttr(node, "Id");
                var stringsEnumerable = Enumerable.Range(1, allChildrenAttr.Length).Select(x => allChildrenAttr[..x]);
                var enumerable = stringsEnumerable.Select(aChildrenAttr =>
                {
                    var cSum = aChildrenAttr.Where(x => x.StartsWith("const"))
                        .Select(x => int.Parse(CSharpStrings.SimpleRemoveParameterAndActionHead(x)))
                        .Sum();
                    var pStr = aChildrenAttr.Where(x => !x.StartsWith("const"))
                        .Select(CSharpStrings.SimpleRemoveParameterAndActionHead)
                        .Aggregate("", (s, x) => s + x + " + ");
                    var armRight = pStr + cSum;
                    return armRight;
                }).ToArray();
                var dictionary = allChildrenId.Zip(enumerable)
                    .ToDictionary(pair => int.Parse(pair.First), pair => pair.Second);
                extraConfig = new SelectorProbabilityConfig(dictionary);
                var foo = enumerable[allChildrenAttr.Length - 1];

                runInit = $"{idString}RandomMaxNum = (uint)({foo});//获得权重总数\n";
                runInit += $"{idString}RandomNowNum = FrameRandom.Random({idString}RandomMaxNum);\n";
                extraId = intId;
                outPutString +=
                    runningCheckAndGo;
                if (needCheckRunningWhenOut)
                {
                    headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                }


                break;
            case "PluginBehaviac.Nodes.DecoratorWeight":
                acp2 = mustStatusVar;
                enterDo = $"//这个必然会在概率选择之下{id}\n";
                outPutString = $"//output这个必然会在概率选择之下{id}\n";
                break;

            case "PluginBehaviac.Nodes.DecoratorLoopUntil" or "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                acp2 = mustStatusVar;
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();

                string? s2;
                switch (nodeType)
                {
                    case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                        beforeOut += "//循环直到默认不能是一帧内完成循环\n";
                        var untilString = nodeType == "PluginBehaviac.Nodes.DecoratorLoopUntil"
                            ? node.Attribute("Until")?.Value ?? throw new NullReferenceException()
                            : "false";
                        var boolGenStatus = CSharpStrings.BoolGenStatus(untilString);
                        s2 = $"{resultVarString} != {boolGenStatus}";
                        break;
                    case "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                        loopInOneTick = true;
                        beforeOut += "//循环直到返回成功或者运行中只能是一帧内完成循环\n";
                        s2 =
                            $"({resultVarString} != {CSharpStrings.Success} && {resultVarString} != {CSharpStrings.Running})";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));


                if (countString == "const int -1" &&
                    nodeType != "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning")
                {
                    // if (Node51Result != EBTStatus.BT_SUCCESS)
                    // {
                    //     Node51Result = EBTStatus.BT_RUNNING;
                    //     Node0RunningNode = 51;
                    //     Node0Result = EBTStatus.BT_RUNNING;
                    //     goto Node0Out;
                    // }
                    beforeOut += loopInOneTick
                        ? "if(" + s2 + ")\n{\n" + $"goto {idString}Run;\n" + "}\n" +
                          $"if({resultVarString} == {CSharpStrings.Running})"
                        : "if(" + s2 + ")";
                    beforeOut += "\n{\n" + runString +
                                 continueRunningString +
                                 "}\n" + outRunningString;
                }
                else
                {
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime = -1;\n";
                    var timeInit =
                        $"{idString}NowRunTime = 0;\n";
                    var acpAdd = "";
                    var b = nodeType == "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning";
                    timeInit
                        += b
                            ? $"{idString}MaxRunTime = 1;\n//循环直到返回成功或者运行中只能固定是循环1次，和配置没有关系,特殊做\n"
                            : $"{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName, countString, idString, agesToInit, out acpAdd)};\n";
                    if (loopInOneTick)
                    {
                        runInit += timeInit;
                        inOneTickLoopLabel = idString + "LoopInOneTick:\n";
                    }
                    else
                    {
                        onEnter += timeInit;
                    }

                    acp2 += acpAdd;
                    beforeOut += loopInOneTick
                        ? "if(" + idString + "NowRunTime < " + idString +
                          $"MaxRunTime && {s2})" + "\n{\n"
                          + $"{idString}NowRunTime++;\n" + $"goto {idString}LoopInOneTick;\n}}\n" +
                          $"if({resultVarString} == {CSharpStrings.Running})\n{{\n"
                        : "if(" + idString + "NowRunTime < " + idString +
                          $"MaxRunTime && {s2})\n{{\n" + $"{idString}NowRunTime++;\n";
                    beforeOut +=
                        $"{runString}"
                        + continueRunningString
                        + "}\n"
                        + outRunningString;
                }

                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                acp2 = statusVar;
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();
                // headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                loopInOneTick = (node.Attribute("DoneWithinFrame")?.Value ?? throw new NullReferenceException()) ==
                                "true";
                extraConfig = new DecoratorLoopConfig(loopInOneTick);
                //Node13Result= EBTStatus.BT_RUNNING;
                // goto Node13Out;

                if (countString == "const int -1")
                {
                    beforeOut = runString + continueRunningString;
                    if (loopInOneTick)
                    {
                        throw new ArgumentException($"{idString} -1 Cant Loop in 1 tick");
                    }

                    headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                }
                else
                {
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime = -1;\n";
                    var initTimes =
                        $"{idString}NowRunTime = 0;\n{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName, countString, idString, agesToInit, out var acpA)};\n";
                    acp2 += acpA;
                    if (loopInOneTick)
                    {
                        runInit = initTimes;
                        inOneTickLoopLabel = idString + "LoopInOneTick:\n";
                    }
                    else
                    {
                        enterDo = initTimes;
                        headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                    }

                    var decorateWhenChildEnds =
                        (node.Attribute("DecorateWhenChildEnds")?.Value ?? throw new NullReferenceException()) ==
                        "true";

                    //子节点结束时作用
                    if (decorateWhenChildEnds && loopInOneTick)
                    {
                        beforeOut += "//在一帧内结束的情况下，子节点结束时作用勾上，如果running则不会增加次数，会一直running\n";
                        beforeOut +=
                            $"if({resultVarString} == {CSharpStrings.Running})\n{{\ngoto {idString}LoopInOneTick;\n}}\n";
                    }

                    var additionalCond = loopInOneTick ? $" && {resultVarString} != {CSharpStrings.Fail}" : "";
                    var endAdd = loopInOneTick && !decorateWhenChildEnds
                        ? $"//选择了一帧内执行，但是不阻塞running的情况下，running会被当成success\nif({resultVarString} == {CSharpStrings.Running}){resultVarString} = {CSharpStrings.Success};\n"
                        : "";
                    var thisContinueRunningString = loopInOneTick
                        ? $"goto {idString}LoopInOneTick;\n"
                        : $"{runString}" + continueRunningString;
                    beforeOut += "if(" + idString + "NowRunTime < " + idString + "MaxRunTime" + additionalCond +
                                 ")\n{\n"
                                 + $"{idString}NowRunTime++;\n"
                                 + thisContinueRunningString
                                 + "}\n"
                                 + outRunningString + endAdd;
                }

                break;
            case "PluginBehaviac.Nodes.WithPrecondition":
                acp2 = statusVar;
                //Node13BranchRunningNode
                enterDo = resultVarString + $" = {CSharpStrings.Fail};\n";
                break;
            case "PluginBehaviac.Nodes.WaitFrames":
                acp2 =
                    $"private {CSharpStrings.BtStatusEnumName} {resultVarString} ;\n";
                acp2 += $"private int {idString}StartFrame  = -1;\n";
                var waitTickString = node.Attribute("Frames")?.Value ?? throw new NullReferenceException();
                var waitTick =
                    ConvertArmToFuncOrParam(agentObjName, waitTickString, idString, agesToInit, out var acp2A);
                acp2 += acp2A;
                headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                enterDo = $"{idString}StartFrame = NowLocalTick;\n";
                beforeOut = $"if (NowLocalTick - {idString}StartFrame < {waitTick})\n"
                            + "{\n" + continueRunningString + "}\n"
                            + outRunningString
                            + $"{resultVarString} = {CSharpStrings.Success};\n";
                // if (NowLocalTick - Node49StartFrame + 1 >= 100)
                // {
                //     Node0RunningNode = 44;
                //     Node0Result = EBTStatus.BT_RUNNING;
                //     goto Node0Out;
                // }
                //
                // Node49Result = EBTStatus.BT_SUCCESS;
                break;
            case "PluginBehaviac.Nodes.Parallel":
                var childFinishPolicy =
                    node.Attribute("ChildFinishPolicy")?.Value ?? throw new NullReferenceException();
                var exitPolicy = node.Attribute("ExitPolicy")?.Value ?? throw new NullReferenceException();
                var failurePolicy = node.Attribute("FailurePolicy")?.Value ?? throw new NullReferenceException();
                var failurePolicyInit = failurePolicy switch
                {
                    "FAIL_ON_ALL" => "true",
                    "FAIL_ON_ONE" => "false",
                    _ => throw new ArgumentOutOfRangeException()
                };
                var successPolicy = node.Attribute("SuccessPolicy")?.Value ?? throw new NullReferenceException();
                var successPolicyInit = successPolicy switch
                {
                    "SUCCEED_ON_ALL" => "true",
                    "SUCCEED_ON_ONE" => "false",
                    _ => throw new ArgumentOutOfRangeException()
                };
                var childFinishLoop = childFinishPolicy switch
                {
                    "CHILDFINISH_LOOP" => true,
                    "CHILDFINISH_ONCE" => false,
                    _ => throw new ArgumentOutOfRangeException()
                };
                acp2 = mustStatusVar;
                acp2 += $"private bool {idString}ParallelSuccess = {successPolicyInit};\n";
                acp2 += $"private bool {idString}ParallelFail = {failurePolicyInit};\n";
                acp2 += $"private bool {idString}ParallelRunning = false;\n";
                var allChildId = GetAllChildrenAttr(node, "Id");
                var aggregate = childFinishLoop
                    ? "//使用了childFinishLoop模式，所以不需要重置子节点状态到invalid\n"
                    : allChildId
                        .Aggregate("",
                            (seed, x) =>
                                seed +
                                $"Node{x}Result = {CSharpStrings.Invalid};\n"); //如果CHILDFINISH_ONCE的话，需要先把所有子节点状态变为Invalid记录没跑过
                runInit =
                    $"{idString}ParallelSuccess = {successPolicyInit};\n{idString}ParallelFail = {failurePolicyInit};\n{idString}ParallelRunning = false;\n" +
                    aggregate;
                extraConfig = new ParallelNodeConfig
                    (childFinishPolicy, successPolicy, failurePolicy); //子节点一些运行策略依赖父节点配置，需要把设置传下去
                // Node47Result = Node47ParallelFail ? EBTStatus.BT_FAILURE :
                // Node47ParallelSuccess ? EBTStatus.BT_SUCCESS :
                //     Node47ParallelRunning ? EBTStatus.BT_RUNNING : EBTStatus.BT_FAILURE;
                beforeOut +=
                    $"{resultVarString} = {idString}ParallelFail ? {CSharpStrings.Fail} :" +
                    $" {idString}ParallelSuccess ? {CSharpStrings.Success} : {idString}ParallelRunning ? {CSharpStrings.Running} : {CSharpStrings.Fail};\n";
                var allChildrenCanRun = GetAllChildrenAttr(node, "Id", true);
                var exitProcess = exitPolicy switch
                {
                    "EXIT_ABORT_RUNNINGSIBLINGS" => "//EXIT_ABORT_RUNNINGSIBLINGS 在成功或失败的时候要退出其他running状态的子节点\n" +
                                                    $"if ({resultVarString} == {CSharpStrings.Fail} || {resultVarString} == {CSharpStrings.Success})\n{{\n" +
                                                    //重置子节点的状态，running节点到-1，状态变为invalid
                                                    allChildrenCanRun.Aggregate("",
                                                        (seed, x) =>
                                                            seed +
                                                            $"Node{x}RunningNode = Node{x}Running.None;\nNode{x}Result = {CSharpStrings.Invalid};\n") +
                                                    "}\n",
                    "EXIT_NONE" => "//EXIT_NONE 退出不做任何操作，可保持原有节点的状态\n",
                    _ => throw new ArgumentOutOfRangeException()
                };
                beforeOut += exitProcess;
                outPutString += "if (" + resultVarString + $" == {CSharpStrings.Running})\n" +
                                "{\n" + continueRunningString
                                + "}\n"
                                + outRunningString;
                headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                break;
            case "Behaviac.Design.Nodes.ReferencedBehavior":
                acp2 = statusVar;
                var subTreeValue = node.Attribute("ReferenceBehavior")?.Value ?? throw new NullReferenceException();
                var subTreeName = CSharpStrings.SimpleRemoveParameterAndActionHead(subTreeValue);
                var subTreeType = subTreeName[1..^1].Replace('/', '_');
                // private IBTree Node9SubTree { get; }
                acp2 += $"private {subTreeType} {idString}SubTree;\n";
                //pudge add debug:树切换时打印log
                var refTreeDebugLogStr = Configs.DebugMode ? Configs.DebugTreeLog(agentObjName, subTreeType) : "";
                beforeOut += $"{refTreeDebugLogStr}\n";

                beforeOut += (needResult ? $"{resultVarString} = " : "") + $"{idString}SubTree.Tick();\n";
                // Node9SubTree = new WrapperAI_NewTest_TestNode(a);
                subTreeConstruct += $"{idString}SubTree = new {subTreeType}(a);\n";
                if (needResult)
                {
                    var runningNow =
                        " if (" + resultVarString + $" == {CSharpStrings.Running})\n" +
                        "{\n" + continueRunningString
                        + "}\n"
                        + outRunningString;

                    headRunningSwitchIdSToNodePath.Add(new RunningNodePathTrace(intId, nodePathMsg));
                    beforeOut += runningNow;
                }

                break;
            default:
                throw new ArgumentException($"Cant Read  {id} :Type {nodeType}");
        }

        //节点附件处理 先不做
        var attachments = node.Elements("Attachment");
        if (attachments.Any()) throw new NotSupportedException($"not support attachment in{id}");
        // var attachmentOnlyEnterLable = "";
        // foreach (var xElement in attachments)
        // {
        //     var attachmentType = xElement.Attribute("Class")?.Value ?? throw new NullReferenceException();
        //     var attachmentId = xElement.Attribute("Id")?.Value ?? throw new NullReferenceException();
        //     var attachIdString = idString + "Attachment" + attachmentId;
        //     switch (attachmentType)
        //     {
        //         case "PluginBehaviac.Events.Precondition":
        //             
        //             break;
        //         case "PluginBehaviac.Events.Effector":
        //
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException($"not fit attachmentType {attachmentType}");
        //     }
        // }


        // 向子树递归收集
        var s = "";
        foreach (var connector in xElements)
        {
            var s1 = TransConnector(connector, intId, extraId, nodeType, agentObjName, runningGoNode,
                extraConfig,
                agesToInit,
                append, out var newHeadRunningSwitchIdSToNodePath,
                out var acp,
                out var irs, out var subTreeConstructChildren);
            s += s1;
            classProperties += acp;
            headRunningSwitchIdSToNodePath.AddRange(newHeadRunningSwitchIdSToNodePath);
            nodeResultInitString += irs;
            subTreeConstruct += subTreeConstructChildren;
        }

        s = s == ""
            ? ""
            : "\n" + s +
              "\n";

        var localS = "";
        if (localSwitch) //并行，选择监测的分支会有局部的running情况，下属节点的running归于对应分支 这个地方空字符串也会生成switch 待优化
        {
            acp2 +=
                $"private Node{runningGoNode}Running Node{runningGoNode}RunningNode = {stopRunningEnum};\n";
            var s2 = headRunningSwitchIdSToNodePath.Aggregate("None,", ((s1, i) => s1 + $"\nNode{i.Id},"))[..^1];
            acp2 += $"private enum Node{runningGoNode}Running {{" + s2 +
                    "}\n";
            var aggregate = headRunningSwitchIdSToNodePath
                .Select(x =>
                {
                    var aggregate1 = Configs.DebugMode
                        ? x.PathMsg.Aggregate("", ((s1, record) => s1 + record.GenDebugLog(agentObjName)))
                        : "";
                    return $"case Node{runningGoNode}Running.Node{x.Id}:\n{aggregate1}goto Node{x.Id}Run;\n";
                }).Aggregate(
                    "", (seed, x) =>
                        seed + x);
            localS = $"switch (Node{runningGoNode}RunningNode)\n{{\n" + aggregate + "\n}\n";
            headRunningSwitchIdSToNodePath.Clear();
        }

        classProperties += acp2;

        res += onEnter + optEnterLabel + localS + enterDo + runLabel + runInit + inOneTickLoopLabel + debugLogString +
               s +
               beforeOut + outLabel +
               outPutString +
               parentVarNeedString +
               outOpNeed;
        return res;
    }

    private static string[] GetAllChildrenAttr(XContainer node, string id, bool onlyMaybeRunning = false)
    {
        return node.Element("Connector")?.Elements("Node").Where(x => x.Attribute("Enable")?.Value == "true" &&
                                                                      (!onlyMaybeRunning ||
                                                                       !NodeMustCantRunning.Contains(
                                                                           x.Attribute("Class")?.Value)
                                                                      ))
                   .Select(x => x.Attribute(id)?.Value ?? throw new NullReferenceException()).ToArray() ??
               throw new NullReferenceException();
    }

    private static string[] NodeMustCantRunning = {"PluginBehaviac.Nodes.Assignment"};

    private static int GetAllChildrenCount(XContainer node)
    {
        var xElements = node.Element("Connector")?.Elements("Node");
        if (xElements == null)
        {
            throw new NullReferenceException();
        }

        return xElements.Count();
    }

    private static string ConvertArmToFuncOrParam(string agentObjName, string r, string nodeId,
        HashSet<string> agesToInit,
        out string constListParam)
    {
        string rr;
        constListParam = "";
        if (r.Contains('(')) //是函数
        {
            CSharpStrings.FindReturnTypeAndEtc(r, agentObjName, nodeId, out var meName, out constListParam,
                out var ages);
            agesToInit.UnionWith(ages);
            rr = meName;
        }
        else //某个量
        {
            // "const vector&lt;int&gt; 4:0|3|5|0"
            if (r.Contains("vector")) //数组赋值
            {
                var indexOf = r.IndexOf('<') + 1;
                var of = r.IndexOf('>');
                var s = r[indexOf..of];
                var i = r.IndexOf(':') + 1;
                var s1 = r[i..].Replace('|', ',');
                constListParam = $"private List<{s}> {nodeId}ConstVal = new List<{s}>(){{{s1}}};//应该不要反复赋值一个常量数组\n";
                rr = $"{nodeId}ConstVal";
            }

            else
            {
                rr = CSharpStrings.SimpleRemoveParameterAndActionHead(r);
            }
        }

        return rr;
    }

    private static void ConditionConvert(XElement node, bool needResult, string resultVarString, string idString,
        string agentObjName,
        HashSet<string> agesToInit,
        out string body, out string acp2)
    {
        var btStatusEnumName = CSharpStrings.BtStatusEnumName;
        acp2 = needResult ? $"private {btStatusEnumName} {resultVarString};\n" : "";
        // var headResult = needResult ? "" + resultVarString + $" = {success};\n" : "\n";
        var op = node.Attribute("Operator")?.Value ?? throw new Exception("parameter null");
        var left = node.Attribute("Opl")?.Value ?? throw new Exception("parameter null");
        var right = node.Attribute("Opr")?.Value ?? throw new Exception("parameter null");
        var link = CSharpStrings.GenOperator(op);
        var bb = ConvertArmToFuncOrParam(agentObjName, left, idString + "Opl", agesToInit, out var acp2P1) + link +
                 ConvertArmToFuncOrParam(agentObjName, right, idString + "Opr", agesToInit, out var acp2P2);
        acp2 += acp2P1;
        acp2 += acp2P2;
        bb = $"{bb} ? {CSharpStrings.Success} : {CSharpStrings.Fail};\n";
        body = needResult ? resultVarString + $" = {bb}\n" : "";
        // return headResult;
    }


    private static string TransConnector(XElement connector, int parentId, int extraId, string parentString,
        string agentObjName,
        int nowCheckRunningNode,
        INodeConfig? nodeConfig, HashSet<string> agesToInit, NodeMsg[] nodePath,
        out List<RunningNodePathTrace> runningSwitchToNodePathCollector,
        out string acp, out string irs, out string subTreeConstruct)
    {
        var id = connector.Attribute("Identifier")?.Value ?? throw new NullReferenceException();
        runningSwitchToNodePathCollector = new List<RunningNodePathTrace>();
        var fixParentString = parentString;
        switch (id)
        {
            case "GenericChildren":
                break;
            case "_condition":
                fixParentString += id;
                break;
            case "_if":
                fixParentString += id;
                break;
            case "_else":
                fixParentString += id;
                break;
            case "Precondition":
                fixParentString += id;
                break;
            case "Action":
                fixParentString += id;
                break;
        }

        var xElements = connector.Elements("Node");
        var res = "";
        acp = "";
        irs = "";
        subTreeConstruct = "";
        foreach (var xElement in xElements)
        {
            var transNode = TransNode(xElement, parentId, extraId, fixParentString, agentObjName,
                                nowCheckRunningNode,
                                nodeConfig, agesToInit, nodePath, out var aRunningSwitchToNodePath,
                                out var aStr, out var airs, out var subTreeConstruct2) +
                            "\n";
            res += transNode;
            acp += aStr;
            irs += airs;
            runningSwitchToNodePathCollector.AddRange(aRunningSwitchToNodePath);
            subTreeConstruct += subTreeConstruct2;
        }

        return res;
    }


    private static string TransParams(XElement xElement1)
    {
        var usedParams = new List<string>();
        var result = "\n";
        foreach (var xElement in xElement1.Elements())
        {
            var name = xElement.Attribute("Name")?.Value ?? throw new NullReferenceException();
            if (usedParams.Contains(name))
            {
                result += $"// 重复变量名 {name}";
                continue;
            }

            if (name.Contains('$'))
            {
                result += $"// 非法变量名，含有$ {name}";
                continue;
            }

            var type = xElement.Attribute("Type")?.Value ?? throw new NullReferenceException();
            var value = xElement.Attribute("DefaultValue")?.Value ?? throw new NullReferenceException();
            var des = "//" + xElement.Attribute("Desc")?.Value;
            var s = des + "\n" + FixLocalParam(type, name, value) + ";\n";
            result += s;
            usedParams.Add(name);
        }

        return result;
    }

    private static string FixLocalParam(string type, string name, string value)
    {
        var pt = FindParamType(type, out var t);

        switch (pt)
        {
            case PType.Enum:
                return t + " " + name + " = " + t + "." + value;
            case PType.Struct:
                return t + " " + name + "  = " + "new" + "()" +
                       (CSharpStrings.CheckStructParamsAndFix(value, out var value2) ? value2 : "");
            case PType.System:
                var vv = t switch
                {
                    "float" => value + "f",
                    "string" => $"\"{value}\"",
                    _ => value
                };
                return t + " " + name + " = " + vv;
            case PType.Array:
                FindParamType(t, out var t2);
                var indexOf = value.IndexOf(':') + 1;
                var s = value[indexOf..];
                var replace = s.Replace('|', ',');
                //List<AISearcherType>
                return $"List<{t2}> " + name + " = " + $"new List<{t2}>{{{replace}}}";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    //参数，变量等解析需要统一一下


    public static PType FindParamType(string fullTypeName, out string fixString)
    {
        if (!fullTypeName.StartsWith("XMLPluginBehaviac.") && fullTypeName.StartsWith("System"))
        {
            if (fullTypeName.StartsWith("System.Collections.Generic.List`1"))
            {
                // System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                var replace1 = fullTypeName.Replace("System.Collections.Generic.List`1[[", "");
                var indexOf = replace1.IndexOf(',');
                fixString = replace1[..indexOf];
                return PType.Array;
            }

            fixString = CSharpStrings.CSTypeShort[fullTypeName];
            return PType.System;
        } //说明是系统类型或者集合类型

        if (!fullTypeName.StartsWith("XMLPluginBehaviac."))
            throw new NotSupportedException($"cant find {fullTypeName}"); //自定义类型
        var s = fullTypeName.Replace("XMLPluginBehaviac.", "");
        var replace = s.Replace("_", "::");
        var strings = s.Split('_');
        var enumerable = Enumerable.Range(1, strings.Length).Select(i =>

        {
            var aggregate = strings[..i].Aggregate("", (ss, x) => ss + x + "::")[..^2];
            var a2 = strings[i..].Aggregate("", (s1, s2) => s1 + '_' + s2);
            var aggregate1 = aggregate + a2;
            return aggregate1;
        }).ToArray();
        var pt = FindCustomParamType(replace, out replace, enumerable);
        var lastIndexOf = replace.Replace("::", ".").Replace("SGame.InGame.GameLogic.", "");
        fixString = lastIndexOf;
        return pt;
    }

    public static PType FindCustomParamType(string replace, out string fixString,
        string[]? optNames = default)
    {
        var xElement = Configs.MetaXml.Element("types") ?? throw new NullReferenceException();
        XElement? firstOrDefault = null;
        fixString = replace;
        foreach (var x in xElement.Elements())
        {
            var value = x.Attribute("Type")?.Value;
            if (value == replace)
            {
                firstOrDefault = x;
                break;
            }

            var orDefault = optNames?.FirstOrDefault(s => s == value);
            if (orDefault == null) continue;
            fixString = orDefault;
            firstOrDefault = x;
        }

        if (firstOrDefault == null)
        {
            throw new NullReferenceException(
                $"cant find type {replace} {optNames?.Aggregate("", ((s, s1) => s + ',' + s1))} ");
        }

        var xName = firstOrDefault.Name.LocalName;
        return xName switch
        {
            "enumtype" => PType.Enum,
            "struct" => PType.Struct,
            _ => throw new NullReferenceException()
        };
    }

    public static string GenConstructor(IEnumerable<(string, string objTypeName)> list)
    {
        //     using behaviac;
        //
        // public interface IBTree
        // {
        //     public EBTStatus Tick();
        // }
        var interfaceStr =
            "using System;\nusing behaviac;\nusing SGame.InGame.GameLogic;\n\npublic interface IBTree\n{\npublic EBTStatus Tick();\n}\n\n\n";
        // public static class BTreeStandard
        // {
        //     public static IBTree GenByConfig(string s, ObjAgent a)
        //     {
        //         return s switch
        //         {
        //             "aaa" => new WrapperAI_Hero_Jup_Strategy(a),
        //             _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        //         };
        //     }
        // }

        var enumerable = list.Select(x =>
                $" \"{x.Item1.Replace(Path.DirectorySeparatorChar, '/')[1..]}\" => new {x.Item1.Replace(Path.DirectorySeparatorChar, '_')[1..]}(({x.objTypeName})a),\n")
            .Aggregate("", ((s, s1) => s + s1));
        var staticFunc = "public static class BTreeStandard\n" +
                         "{\n" +
                         $"public static IBTree GenByConfig(string s, {Configs.AgentBaseOrInterface} a)\n" +
                         "{\n" +
                         "return s switch\n{\n" +
                         enumerable +
                         "_ => throw new ArgumentOutOfRangeException(nameof(s), s, null)\n};\n}\n}\n";
        return interfaceStr + staticFunc;
    }
}

public enum PType
{
    Enum,
    Struct,
    System,
    Array
}

internal interface INodeConfig
{
}

internal record ParallelNodeConfig(string ChildFinishPolicy, string SuccessPolicy, string FailPolicy) : INodeConfig
{
}

internal record SelectorProbabilityConfig(IReadOnlyDictionary<int, string> IdToExpression) : INodeConfig;

internal record DecoratorLoopConfig(bool LoopInOneTick) : INodeConfig;

internal record NodeMsg(int Id, string Type)
{
    public string GenDebugLog(string agentName)
    {
        return Configs.DebugLogString(agentName, Id, Type);
    }
}

internal record RunningNodePathTrace(int Id, NodeMsg[] PathMsg);