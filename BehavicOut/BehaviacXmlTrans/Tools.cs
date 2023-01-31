using System.Xml.Linq;

namespace BehaviacXmlTrans;

public static class Tools
{
    public static string ConvertAXmlFile(string path, out string csName)
    {
        var root = XElement.Load(path);
        var strings = path.Split(Path.DirectorySeparatorChar);
        var valueTuple = strings.Aggregate(("", false), (tuple, s1) =>
        {
            var (item1, item2) = tuple;
            var aa = item2 ? item1 + "_" + s1 : item1;
            var bb = s1 == Configs.EditDir || item2;
            return (aa, bb);
        });
        var fileName1 = valueTuple.Item1;
        var lastIndexOf = fileName1[1..fileName1.LastIndexOf(".", StringComparison.Ordinal)];
        csName = lastIndexOf + ".cs";
        var s = "using System;\nusing behaviac;\nusing SGame.InGame.GameLogic;\nusing PBConfig;\n\npublic class " +
                lastIndexOf +
                CSharpStrings.RunTimeInterface + "\n{\n";

        // foreach (var xAttribute in rootFirstAttribute)
        // {
        //     Console.Out.WriteLine(xAttribute);
        // }

        var element = root.Elements("Node").FirstOrDefault() ?? throw new NullReferenceException();


        // Console.Out.WriteLine(xAttribute.Name);

        var value = element.Attribute("AgentType")?.Value ?? throw new NullReferenceException();
        var className = CSharpStrings.RemoveParameterAndActionHead(value);
        var name = $"private {className} {className}  " + "{get;}\n";
        s += name;

        var enumerable = element.Elements();
        var subtreeConstruct = "";
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

                    var result = ConnectorTransFromRoot(xElement, className, out var countParams, out subtreeConstruct);
                    s += countParams;
                    s += result;
                    break;
            }
        }

        var constructor = $"public {lastIndexOf}({className} a)\n{{\n{className} = a;\n{subtreeConstruct}}}\n";
        s += constructor;

        s += "\n}";

        return s;
    }

    public static string ConnectorTransFromRoot(XElement xElement, string agentObjName,
        out string treeStatusParamsAndAgentObj, out string subTreeConstruct)
    {
        treeStatusParamsAndAgentObj = "\n";
        var head = $"public {CSharpStrings.BtStatusEnumName} Tick()\n{{\n NowLocalTick++;\n";
        var res = "";
        var node = xElement.Elements().FirstOrDefault(x => x.Name == "Node") ?? throw new NullReferenceException();
        var runningSwitch = "";
        var value = node.Attribute("Id")?.Value ?? throw new NullReferenceException();
        var result = int.TryParse(value, out var ii) ? ii : throw new ArgumentOutOfRangeException();
        var rootRunningNodeString = " Node" + result + "RunningNode";
        const string tickCount = "\nprivate int NowLocalTick { get; set; } = -1;\n";
        var rootStatus = $"{tickCount}\nprivate int {rootRunningNodeString}{{get;set;}} = -1;\n";
        var s = TransNode(node, result, -1, "Root", agentObjName, result, null,
            out var tsp,
            out var aRunningSwitch, out var nodeResultInitString, out subTreeConstruct) + "\n";
        var behaviortreestatus = "// BehaviorTreeStatus\n";
        treeStatusParamsAndAgentObj += behaviortreestatus + rootStatus + tsp;
        runningSwitch += aRunningSwitch;
        res += s;


        if (runningSwitch == "") return head + res + "\n}";

        var switchString = nodeResultInitString + $"switch ({rootRunningNodeString})\n{{\n"
                                                + runningSwitch + "\n}\n";
        return head + switchString + res + "\n}";
    }

    private static string TransNode(XElement node, int parentId, int extraId, string parentTypeString,
        string agentObjName, int runningGoNode,
        ParallelNodeConfig? parentParallelNodeConfig,
        out string treeStatusValues,
        out string headRunningSwitch, out string nodeResultInitString, out string subTreeConstruct)
    {
        treeStatusValues = "";
        headRunningSwitch = "";
        nodeResultInitString = "";
        subTreeConstruct = "";
        var firstOrDefault = node.Elements("Comment").FirstOrDefault();
        var value = firstOrDefault?.Attribute("Text")?.Value;
        var res = "//" + value + "\n";
        var enable = node.Attribute("Enable")?.Value;
        if (enable == "false")
        {
            return res;
        }

        var id = node.Attribute("Id")?.Value ?? throw new NullReferenceException();
        var intId = int.TryParse(id, out var iii) ? iii : throw new ArgumentException(id);
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
        var outLabel = idString + "Out:\n";
        res = res + "//" + idString + "\n";
        var xElements = node.Elements("Connector").ToArray();


        //与父节点关系
        var needResult = true;
        var localSwitch = false;
        var parentEndStringGoto = parentIdString + "Out;\n";
        var parentVarString = parentIdString + "Result";
        var resultVarString = idString + "Result";
        var tail = "";
        var onEnter = "";

        switch (parentTypeString)
        {
            case "Root":

                tail = $"return {resultVarString};";
                break;
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                needResult = false;
                break;
            case "PluginBehaviac.Nodes.Sequence" or "PluginBehaviac.Nodes.And":

                tail = $"if({resultVarString} == {CSharpStrings.Fail})\n"
                       + "{\n"
                       + $"{parentVarString} = {CSharpStrings.Fail};\n"
                       + $"goto {parentEndStringGoto}"
                       + "}";
                break;
            case "PluginBehaviac.Nodes.Selector":

                tail = $"if({resultVarString} == {CSharpStrings.Success})\n"
                       + "{\n"
                       + $"{parentVarString} = {CSharpStrings.Success};\n"
                       + $"goto {parentEndStringGoto}"
                       + "}";
                break;
            case "PluginBehaviac.Nodes.IfElse_condition":

                tail = $"if({resultVarString} == {CSharpStrings.Fail})\n{{\ngoto Node{extraId}Run;\n}}";
                break;
            case "PluginBehaviac.Nodes.IfElse_if":

                tail = parentVarString + " = " + resultVarString + $";\ngoto Node{parentId}Out;\n";
                break;
            case "PluginBehaviac.Nodes.IfElse_else":
                tail = parentVarString + " = " + resultVarString + ";\n";
                break;
            case "PluginBehaviac.Nodes.WithPreconditionPrecondition":

                onEnter = "//选择监测条件之下\n";
                tail = $"if({resultVarString} == {CSharpStrings.Fail})\n" +
                       "{\n"
                       + $"goto {parentEndStringGoto}"
                       + "}\n"
                       + $"Node{extraId}Result = {CSharpStrings.Success};\n"
                       + "//如果切换了分支后再通过，那么会重置running下面running的节点到-1\n"
                       + $"if (Node{extraId}WhichBranchRunning != {parentId})\n{{\nNode{parentId}RunningNode = -1;\n}}";
                // {
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
                tail = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                onEnter = "";
                tail = $"if({resultVarString} != {CSharpStrings.Invalid})\n" +
                       "{\n"
                       // + $"{parentVarString} = {resultVarString};\n"
                       //Node11WhichBranchRunning =(Node13Result == EBTStatus.BT_RUNNING) ? 13 : -1;
                       + $"Node{parentId}WhichBranchRunning = {resultVarString} == EBTStatus.BT_RUNNING ? {intId} : -1;\n"
                       // if (Node13Result == EBTStatus.BT_RUNNING)
                       // {
                       //     Node11WhichBranchRunning = 13;
                       // }
                       + $"goto {parentEndStringGoto}"
                       + "}\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                onEnter = "//循环节点下\n";
                tail = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                onEnter = "//循环直到之下\n";
                tail = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.Parallel":
                onEnter = "//并行节点之下\n";
                localSwitch = true;
                runningGoNode = intId;
                var childFinishPolicyCheck
                    = parentParallelNodeConfig?.ChildFinishPolicy switch
                    {
                        "CHILDFINISH_LOOP" => $"//CHILDFINISH_LOOP循环则任何情况都会重新执行\n",
                        "CHILDFINISH_ONCE" =>
                            $"//CHILDFINISH_ONCE则看状态\nif({resultVarString} != {CSharpStrings.Running} && {resultVarString} " +
                            $"!= {CSharpStrings.Invalid})\n{{\ngoto {outLabel}\n}}\n//CHILDFINISH_ONCE情况下，不是第一次执行或者running，则跳过执行\n",
                        _ => throw new NullReferenceException()
                    };
                onEnter += childFinishPolicyCheck;
                var successAll = parentParallelNodeConfig.SuccessPolicy switch
                {
                    "SUCCEED_ON_ALL" => true,
                    "SUCCEED_ON_ONE" => false,
                    _ => throw new NullReferenceException()
                };
                var failAll = parentParallelNodeConfig.FailPolicy switch
                {
                    "FAIL_ON_ALL" => true,
                    "FAIL_ON_ONE" => false,
                    _ => throw new NullReferenceException()
                };
                var onAllFail = (failAll
                    ? $"{parentIdString}ParallelFail = false;//需要failAll，初始值为true遇到成功情况置为false\n"
                    : "//需要failOne，初始为false遇到失败才会置为true\n");
                var successProcess =
                    (successAll
                        ? "//需要successAll，初始值为true遇到失败情况置为false\n"
                        : $"{parentIdString}ParallelSuccess = true;//需要successOne，初始为false遇到成功则置为true\n") +
                    onAllFail;
                var onAllSuccess = (successAll
                    ? $"{parentIdString}ParallelSuccess = false;//需要successAll，初始值为true遇到失败情况置为false\n"
                    : "//需要successOne，初始为false遇到成功则置为true\n");
                var failProcess =
                    (failAll
                        ? "//需要failAll，初始值为true遇到成功情况置为false\n"
                        : $"{parentIdString}ParallelFail = true;//需要failOne，初始为false遇到失败才会置为true\n") +
                    onAllSuccess;
                var runningProcess = onAllFail +
                                     onAllSuccess +
                                     $"{parentIdString}ParallelRunning = true;//任何running都会使得并行节点running\n";
                tail = $"switch({resultVarString})\n{{\n" +
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

// 解析节点

        var body = "";
        var acp2 = "";
        var outPutString = "";
        var enterDo = "";
        var goNode = "Node" + runningGoNode;
        var runningNodeString =
            goNode + "RunningNode";
        var runString = needResult ? resultVarString + $" = {CSharpStrings.Running};\n" : "";
        var statusVar = needResult
            ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
            : "";
        var mustStatusVar = $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n";

        var successString = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "";
        var invalidString = needResult ? resultVarString + $" = {CSharpStrings.Invalid};\n" : "";
        var failString = needResult ? resultVarString + $" = {CSharpStrings.Fail};\n" : "";
        var continueRunningString = $"{runningNodeString} = {id};\n"
                                    + $"{goNode}Result = {CSharpStrings.Running};\n"
                                    + $"goto Node{runningGoNode}Out;\n";
        var outRunningString = $"{runningNodeString} = -1;\n";
        string countString;
        var runningSwitch = $"case {id}:\n" + $"goto {idString}Run;\n";
        ParallelNodeConfig? extraConfig = null;
        switch (nodeType)
        {
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                acp2 = statusVar;

                enterDo = successString;
                outPutString = successString;

                break;
            case "PluginBehaviac.Nodes.Sequence" or "PluginBehaviac.Nodes.And":
                acp2 = mustStatusVar;
                enterDo = invalidString;
                break;
            case "PluginBehaviac.Nodes.Selector":
                acp2 = mustStatusVar;
                enterDo = invalidString;
                break;
            case "PluginBehaviac.Nodes.False":
                acp2 = statusVar;
                body = failString;
                break;
            case "PluginBehaviac.Nodes.True":
                acp2 = statusVar;
                body = successString;
                break;
            case "PluginBehaviac.Nodes.IfElse":
                acp2 = mustStatusVar;
                enterDo = successString;
                var orDefault =
                    xElements.FirstOrDefault(x =>
                        (x.Attribute("Identifier")?.Value ?? throw new NullReferenceException()) == "_else") ??
                    throw new ArgumentOutOfRangeException();
                var value1 = orDefault.Element("Node")?.Attribute("Id")?.Value ?? throw new NullReferenceException();
                extraId = int.Parse(value1);
                break;
            case "PluginBehaviac.Nodes.Condition":
                enterDo = ConditionConvert(node, needResult, resultVarString,
                    agentObjName, out body, out acp2);
                break;
            case "PluginBehaviac.Nodes.Noop":
                acp2 = statusVar;
                enterDo = successString;
                break;

            case "PluginBehaviac.Nodes.Action":

                acp2 = statusVar;

                var method = node.Attribute("Method")?.Value ??
                             throw new NullReferenceException($"no method in action node {id}");

                var findReturnType = CSharpStrings.FindReturnTypeAndEtc(method, agentObjName, out var methodName);


                if (findReturnType == "behaviac::EBTStatus")
                {
                    var varString = resultVarString + " = " + methodName + ";\n";
                    var runningNow =
                        " if (" + resultVarString + $" == {CSharpStrings.Running} )\n" +
                        "{\n" + continueRunningString
                        + "}\n"
                        + outRunningString;

                    headRunningSwitch += runningSwitch;
                    body = varString + runningNow;
                }
                else
                {
                    var a = methodName + ";\n";
                    var resOp = node.Attribute("ResultOption")?.Value ??
                                throw new NullReferenceException($"not Res @ {id}");
                    var stringToEnum = CSharpStrings.StringToEnum(resOp);
                    var b = resultVarString + " = " + stringToEnum + ";\n";
                    body = a + b;
                }

                break;
            case "PluginBehaviac.Nodes.Assignment":
                var l = node.Attribute("Opl")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var r = node.Attribute("Opr")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var rr = ConvertArmToFuncOrParam(agentObjName, r);
                body = CSharpStrings.RemoveParameterAndActionHead(l) + " = " +
                       rr + ";\n";
                tail = "";
                break;
            case "PluginBehaviac.Nodes.Compute":
                tail = "";
                var o = node.Attribute("Opl")?.Value ??
                        throw new NullReferenceException($"not Res @ {id}");
                var p1 = node.Attribute("Opr1")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                var p2 = node.Attribute("Opr2")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                var op = node.Attribute("Operator")?.Value ??
                         throw new NullReferenceException($"not Res @ {id}");
                body = CSharpStrings.RemoveParameterAndActionHead(o)
                       + " = "
                       + ConvertArmToFuncOrParam(agentObjName,p1)
                       + CSharpStrings.GenOperator(op)
                       + ConvertArmToFuncOrParam(agentObjName,p2) + ";\n";
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                // acp2 = needResult
                //     ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                //     : "";
                // private int Node11WhichBranchRunning { get; set; } = -1;
                acp2 = mustStatusVar;
                acp2 += $"private int {idString}WhichBranchRunning {{ get; set; }} = -1;\n";
                enterDo = "\n";
                // tail = "";
                extraId = intId;
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                acp2 = mustStatusVar;
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();
                var untilString = node.Attribute("Until")?.Value ?? throw new NullReferenceException();
                var boolGenStatus = CSharpStrings.BoolGenStatus(untilString);
                headRunningSwitch += runningSwitch;
                if (countString == "const int -1")
                {
                    // if (Node51Result != EBTStatus.BT_SUCCESS)
                    // {
                    //     Node51Result = EBTStatus.BT_RUNNING;
                    //     Node0RunningNode = 51;
                    //     Node0Result = EBTStatus.BT_RUNNING;
                    //     goto Node0Out;
                    // }

                    body = $"if({resultVarString} != {boolGenStatus})\n{{\n" + runString + continueRunningString +
                           "}\n" + outRunningString;
                }
                else
                {
                    acp2 += "private int " + idString + "NowRunTime { get; set; } = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime { get; set; } = -1;\n";
                    enterDo =
                        $"{idString}NowRunTime = 0;\n{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName,countString)};\n";
                    body = "if(" + idString + "NowRunTime <" + idString +
                           $"MaxRunTime && {resultVarString} != {boolGenStatus})\n{{\n"
                           + $"{idString}NowRunTime++;\n"
                           + $"{runString}"
                           + continueRunningString
                           + "}\n"
                           + outRunningString;
                }

                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                acp2 = statusVar;
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();
                headRunningSwitch += runningSwitch;
                //Node13Result= EBTStatus.BT_RUNNING;
                // goto Node13Out;

                if (countString == "const int -1")
                {
                    body = runString + continueRunningString;
                }
                else
                {
                    acp2 += "private int " + idString + "NowRunTime { get; set; } = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime { get; set; } = -1;\n";
                    enterDo =
                        $"{idString}NowRunTime = 0;\n{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName,countString)};\n";
                    body = "if(" + idString + "NowRunTime <" + idString + "MaxRunTime)\n{\n"
                           + $"{idString}NowRunTime++;\n"
                           + $"{runString}"
                           + continueRunningString
                           + "}\n"
                           + outRunningString;
                }

                break;
            case "PluginBehaviac.Nodes.WithPrecondition":
                acp2 = statusVar;
                //Node13BranchRunningNode


                enterDo = resultVarString + $" = {CSharpStrings.Invalid};\n";
                break;
            case "PluginBehaviac.Nodes.WaitFrames":
                acp2 =
                    $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n";

                acp2 += $"private int {idString}StartFrame {{ get; set; }} = -1;\n";
                var waitTickString = node.Attribute("Frames")?.Value ?? throw new NullReferenceException();
                var waitTick = ConvertArmToFuncOrParam(agentObjName,waitTickString);
                headRunningSwitch += runningSwitch;
                enterDo = $"{idString}StartFrame = NowLocalTick;\n";
                body = $"if (NowLocalTick - {idString}StartFrame + 1 >= {waitTick})\n"
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
                acp2 += $"private bool {idString}ParallelSuccess {{ get; set; }} = {successPolicyInit};\n";
                acp2 += $"private bool {idString}ParallelFail {{ get; set; }} = {failurePolicyInit};\n";
                acp2 += $"private bool {idString}ParallelRunning {{ get; set; }} = false;\n";
                var allChildId = GetAllChildId(node);
                var aggregate = childFinishLoop
                    ? "//使用了childFinishLoop模式，所以不需要重置子节点状态到invalid\n"
                    : allChildId
                        .Aggregate("",
                            (seed, x) =>
                                seed +
                                $"Node{x}Result = {CSharpStrings.Invalid};\n"); //如果CHILDFINISH_ONCE的话，需要先把所有子节点状态变为Invalid记录没跑过
                enterDo =
                    $"{idString}ParallelSuccess = {successPolicyInit};\n{idString}ParallelFail = {failurePolicyInit};\n{idString}ParallelRunning = false;\n" +
                    aggregate;
                extraConfig = new ParallelNodeConfig
                {
                    ChildFinishPolicy = childFinishPolicy, FailPolicy = failurePolicy, SuccessPolicy = successPolicy
                }; //子节点一些运行策略依赖父节点配置，需要把设置传下去
                // Node47Result = Node47ParallelFail ? EBTStatus.BT_FAILURE :
                // Node47ParallelSuccess ? EBTStatus.BT_SUCCESS :
                //     Node47ParallelRunning ? EBTStatus.BT_RUNNING : EBTStatus.BT_FAILURE;
                outPutString =
                    $"{resultVarString} = {idString}ParallelFail ? {CSharpStrings.Fail} :" +
                    $" {idString}ParallelSuccess ? {CSharpStrings.Success} : {idString}ParallelRunning ? {CSharpStrings.Running} : {CSharpStrings.Fail};\n";

                var exitProcess = exitPolicy switch
                {
                    "EXIT_ABORT_RUNNINGSIBLINGS" => "//EXIT_ABORT_RUNNINGSIBLINGS 在成功或失败的时候要退出其他running状态的子节点\n" +
                                                    $"if ({resultVarString} == {CSharpStrings.Fail} || {resultVarString} == {CSharpStrings.Success})\n{{\n" +
                                                    //重置子节点的状态，running节点到-1，状态变为invalid
                                                    allChildId.Aggregate("",
                                                        (seed, x) =>
                                                            seed +
                                                            $"Node{x}RunningNode = -1;\nNode{x}Result = {CSharpStrings.Invalid};\n") +
                                                    "}\n",
                    "EXIT_NONE" => "//EXIT_NONE 退出不做任何操作，可保持原有节点的状态\n",
                    _ => throw new ArgumentOutOfRangeException()
                };
                outPutString += exitProcess;
                break;
            case "Behaviac.Design.Nodes.ReferencedBehavior":
                acp2 = statusVar;
                var subTreeValue = node.Attribute("ReferenceBehavior")?.Value ?? throw new NullReferenceException();
                var subTreeName = CSharpStrings.RemoveParameterAndActionHead(subTreeValue);
                var subTreeType = subTreeName[1..^1].Replace('/', '_');
                // private IBTree Node9SubTree { get; }
                acp2 += $"private {subTreeType} {idString}SubTree {{ get; }}\n";
                body = $"{idString}SubTree.Tick();\n";
                // Node9SubTree = new WrapperAI_NewTest_TestNode(a);
                subTreeConstruct += $"{idString}SubTree = new {subTreeType}(a);\n";
                break;
            default:
                throw new ArgumentException($"Cant Read  {id} :Type {nodeType}");
        }

        // 节点附件处理 先不做
        // var attachments = node.Elements("Attachment");
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
            var s1 = TransConnector(connector, intId, extraId, nodeType, agentObjName, runningGoNode, extraConfig,
                out var acp,
                out var aRunningSwitch,
                out var irs, out var subTreeConstructChildren);
            s += s1;
            treeStatusValues += acp;
            headRunningSwitch += aRunningSwitch;
            nodeResultInitString += irs;
            subTreeConstruct += subTreeConstructChildren;
        }

        s = s == ""
            ? ""
            : "\n" + s +
              "\n";

        var localS = "";
        if (localSwitch) //并行，选择监测的分支会有局部的running情况，下属节点的running归于对应分支
        {
            acp2 += $"private int Node{runningGoNode}RunningNode {{ get; set; }} = -1;\n";
            localS = $"switch (Node{runningGoNode}RunningNode)\n{{\n" + headRunningSwitch + "\n}\n";
            headRunningSwitch = "";
        }

        treeStatusValues += acp2;
        res += localS + onEnter + enterDo + runLabel + s + body + outLabel + outPutString + tail;
        return res;
    }

    private static string[] GetAllChildId(XElement node)
    {
        return node.Element("Connector")?.Elements("Node")
                   .Select(x => x.Attribute("Id")?.Value ?? throw new NullReferenceException()).ToArray() ??
               throw new NullReferenceException();
    }

    private static string ConvertArmToFuncOrParam(string agentObjName, string r)
    {
        string rr;
        if (r.Contains("(")) //是函数
        {
            CSharpStrings.FindReturnTypeAndEtc(r, agentObjName, out var meName);
            rr = meName;
        }
        else //某个量
        {
            rr = CSharpStrings.RemoveParameterAndActionHead(r);
        }

        return rr;
    }

    private static string ConditionConvert(XElement node, bool needResult, string resultVarString,
        string agentObjName,
        out string body, out string acp2)
    {
        var btStatusEnumName = CSharpStrings.BtStatusEnumName;
        acp2 = needResult ? $"private {btStatusEnumName} {resultVarString} {{ get; set; }}\n" : "";
        var success = CSharpStrings.Success;
        var headResult = needResult ? "" + resultVarString + $" = {success};\n" : "\n";
        var op = node.Attribute("Operator")?.Value ?? throw new Exception("parameter null");
        var left = node.Attribute("Opl")?.Value ?? throw new Exception("parameter null");
        var right = node.Attribute("Opr")?.Value ?? throw new Exception("parameter null");
        var link = CSharpStrings.GenOperator(op);
        var bb = ConvertArmToFuncOrParam(agentObjName, left) + link +
                 ConvertArmToFuncOrParam(agentObjName, right);
        bb = $"{bb} ? {CSharpStrings.Success} : {CSharpStrings.Fail};\n";
        body = needResult ? resultVarString + $" = {bb}\n" : "";
        return headResult;
    }


    private static string TransConnector(XElement connector, int parentId, int extraId, string parentString,
        string agentObjName,
        int nowCheckRunningNode,
        ParallelNodeConfig? parallelNodeConfig,
        out string acp,
        out string aRunningSwitch, out string irs, out string subTreeConstruct)
    {
        var id = connector.Attribute("Identifier")?.Value ?? throw new NullReferenceException();

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
        aRunningSwitch = "";
        subTreeConstruct = "";
        foreach (var xElement in xElements)
        {
            var transNode = TransNode(xElement, parentId, extraId, fixParentString, agentObjName,
                                nowCheckRunningNode, parallelNodeConfig,
                                out var aStr, out var ars,
                                out var airs, out var subTreeConstruct2) +
                            "\n";
            res += transNode;
            acp += aStr;
            aRunningSwitch += ars;
            irs += airs;
            subTreeConstruct += subTreeConstruct2;
        }

        return res;
    }


    public static string TransParams(XElement xElement1)
    {
        return (from xElement in xElement1.Elements()
                let name = xElement.Attribute("Name")?.Value
                let type = xElement.Attribute("Type")?.Value
                let value = xElement.Attribute("DefaultValue")?.Value
                let des = "//" + xElement.Attribute("Desc")?.Value
                select des + "\n" + FixParam(type, name, value) + ";\n")
            .Aggregate("\n", (current, pString) => current + pString);
    }

    private static string FixParam(string type, string name, string value)
    {
        var t = type;
        var pt = PType.System;
        if (type.Contains("XMLPluginBehaviac."))
        {
            var replace = type.Replace("XMLPluginBehaviac.", "").Replace("_", "::");
            pt = FindParamType(replace);
            var lastIndexOf = replace.LastIndexOf(':') + 1;
            t = lastIndexOf > 0 ? replace[lastIndexOf..] : replace;
        }

        return pt switch
        {
            PType.Enum => t + " " + name + " {get;set;} = " + t + "." + value,
            PType.Struct => t + " " + name + " {get;set;} = " + "new" + "()" +
                            (CSharpStrings.CheckStructParamsAndFix(value, out var value2) ? value2 : ""),
            PType.System => t + " " + name + " {get;set;} = " + value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static PType FindParamType(string replace, bool maySys = false)
    {
        var xElement = Configs.MetaXml.Element("types") ?? throw new NullReferenceException();
        var firstOrDefault = xElement.Elements().FirstOrDefault(x => x.Attribute("Type")?.Value == replace);
        if (firstOrDefault == null)
        {
            return maySys ? PType.System : throw new NullReferenceException($"cant find type {replace}");
        }

        var xName = firstOrDefault.Name.LocalName;
        return xName switch
        {
            "enumtype" => PType.Enum,
            "struct" => PType.Struct,
            _ => throw new NullReferenceException()
        };
    }
}

public enum PType
{
    Enum,
    Struct,
    System
}

internal record ParallelNodeConfig
{
    public string ChildFinishPolicy { get; init; } = "";

    // public string ExitPolicy { get; init; } = "";
    public string SuccessPolicy { get; init; } = "";
    public string FailPolicy { get; init; } = "";
}