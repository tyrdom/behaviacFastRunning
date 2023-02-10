using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.VisualBasic;

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
        var s =
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
        var className = CSharpStrings.SimpleRemoveParameterAndActionHead(value);
        var name = $"private {className} {className}  " + ";\n";
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
        var rootRunningEnum = " Node" + result + "Running";
        const string tickCount = "\nprivate int NowLocalTick  = -1;\n";
        var rootStatus = $"{tickCount}\nprivate int {rootRunningNodeString} = -1;\n";

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
        return head + switchString + res + "\n}\n";
    }

    private static string TransNode(XElement node, int parentId, int extraId, string parentTypeString,
        string agentObjName, int runningGoNode,
        INodeConfig? nodeConfig,
        out string treeStatusValues,
        out string headRunningSwitch, out string nodeResultInitString, out string subTreeConstruct)
    {
        treeStatusValues = "";
        headRunningSwitch = "";
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
        var skipString = "";
        var onEnter = "";

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

                outOpNeed = $"if({resultVarString} == {CSharpStrings.Fail})\n{{\ngoto Node{extraId}Run;\n}}";
                break;
            case "PluginBehaviac.Nodes.IfElse_if":
                onEnter = "// IfElse_if分支\n";
                parentVarNeedString = parentVarString + " = " + resultVarString + ";\n";
                outOpNeed = $"goto Node{parentId}Out;\n";
                break;
            case "PluginBehaviac.Nodes.IfElse_else":
                parentVarNeedString = parentVarString + " = " + resultVarString + ";\n";
                break;
            case "PluginBehaviac.Nodes.WithPreconditionPrecondition":

                onEnter = "//选择监测条件之下\n";
                parentVarNeedString = $"if({resultVarString} == {CSharpStrings.Fail})\n" +
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
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                onEnter = "";
                parentVarNeedString = $"if({resultVarString} != {CSharpStrings.Invalid})\n" +
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
                    $"if(Node{parentId}RandomNowNum >= {aExp})\n{{\nNode{intId}RunningNode = -1;\ngoto Node{intId}Out;\n\n}}\n";
                localSwitch = true;
                runningGoNode = intId;
                break;
            case "PluginBehaviac.Nodes.SelectorStochastic":
                onEnter = "//随机选择之下\n";
                var selectorStochasticConfig = (SelectorProbabilityConfig) nodeConfig!;
                var aExp2 = selectorStochasticConfig.IdToExpression.TryGetValue(intId, out var exp2)
                    ? exp2
                    : throw new KeyNotFoundException();

                onEnter +=
                    $"if(Node{extraId}WhichBranchEnter != {aExp2})\n{{\nNode{intId}RunningNode = -1;\ngoto Node{intId}Skip;\n\n}}\n";
                localSwitch = true;
                runningGoNode = intId;
                parentVarNeedString = $"Node{extraId}Result = {resultVarString};\n";
                skipString = skipLabel;
                break;
            case "PluginBehaviac.Nodes.DecoratorWeight":
                onEnter = "//概率权重节点之下\n";
                parentVarNeedString = $"Node{extraId}Result = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                onEnter = "//循环节点下\n";
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                onEnter = "//循环直到之下，如果下面有running状态的节点，父节点也会running，会合并，会直接跳到running的情况\n";
                parentVarNeedString = $"{parentVarString} = {resultVarString};\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                onEnter = "//循环直到成功或运行中之下\n";
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
            ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString};\n"
            : "";
        var mustStatusVar =
            $"private {CSharpStrings.BtStatusEnumName} {resultVarString};\n"; //有子节点的情况，虽然父节点不要求状态，但是字节点需要

        var successString = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "";
        var failString = needResult ? resultVarString + $" = {CSharpStrings.Fail};\n" : "";
        var continueRunningString = $"{runningNodeString} = {id};\n"
                                    + $"{goNode}Result = {CSharpStrings.Running};\n"
                                    + $"goto Node{runningGoNode}Out;\n";
        var outRunningString = $"{runningNodeString} = -1;\n";
        string countString;
        var runningSwitch = $"case {id}:\n" + $"goto {idString}Run;\n";
        INodeConfig? extraConfig = null;
        var loopInOneTick = false; // 循环类 一个tick内完成
        switch (nodeType)
        {
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                acp2 = mustStatusVar;
                enterDo = successString;
                outPutString = successString;
                break;
            case "PluginBehaviac.Nodes.DecoratorAlwaysFailure":
                acp2 = mustStatusVar;
                enterDo = failString;
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
                    body = needResult ? resultVarString + $" = {okk[0]};\n" : "";
                    ;
                }
                else if (countString.StartsWith("const")) //常量次数，直接写死最大次数
                {
                    var removeParameterAndActionHead = CSharpStrings.SimpleRemoveParameterAndActionHead(countString);
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    body = $"if({idString}NowRunTime < {removeParameterAndActionHead})\n{{\n" +
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
                    body = $"if({idString}NowRunTime < {idString}MaxRunTime)\n{{\n" +
                           s1;
                }

                break;
            case "PluginBehaviac.Nodes.DecoratorNot":
                acp2 = mustStatusVar;
                break;
            case "PluginBehaviac.Nodes.Sequence" or "PluginBehaviac.Nodes.And":
                acp2 = mustStatusVar;
                enterDo = successString;
                break;
            case "PluginBehaviac.Nodes.Selector" or "PluginBehaviac.Nodes.Or":
                acp2 = mustStatusVar;
                enterDo = failString;
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
                ConditionConvert(node, needResult, resultVarString, idString,
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

                var findReturnType =
                    CSharpStrings.FindReturnTypeAndEtc(method, agentObjName, idString, out var methodName,
                        out var constListP);
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
                var rr = ConvertArmToFuncOrParam(agentObjName, r, idString, out var constListParam);
                acp2 += constListParam;
                body = CSharpStrings.SimpleRemoveParameterAndActionHead(l) + " = " +
                       rr + ";\n";
                parentVarNeedString = "";
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
                body = CSharpStrings.SimpleRemoveParameterAndActionHead(o)
                       + " = "
                       + ConvertArmToFuncOrParam(agentObjName, p1, idString + "Op1", out var cLsp1)
                       + CSharpStrings.GenOperator(op)
                       + ConvertArmToFuncOrParam(agentObjName, p2, idString + "Op2", out var cLsp2) + ";\n";
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
                enterDo = "\n";
                // tail = "";
                extraId = intId;
                break;
            case "PluginBehaviac.Nodes.SelectorStochastic":
                acp2 = mustStatusVar;
                acp2 += $"private int {idString}WhichBranchEnter = -1;\n";
                var allChildrenIds = GetAllChildrenAttr(node, "Id");
                var length = allChildrenIds.Length;
                enterDo = $"{idString}WhichBranchEnter = FrameRandom.Random({length});\n";
                var dictionary1 = allChildrenIds.Select(int.Parse).Zip(Enumerable.Range(0, length))
                    .ToDictionary(p => p.First, p => p.Second.ToString());
                extraConfig = new SelectorProbabilityConfig(dictionary1);
                extraId = intId;
                body =
                    $"if({resultVarString} == {CSharpStrings.Running})\n{{\n{continueRunningString}\n}}\n{outRunningString}";
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

                enterDo = $"{idString}RandomMaxNum = (uint)({foo});//获得权重总数\n";
                enterDo += $"{idString}RandomNowNum = FrameRandom.Random({idString}RandomMaxNum);\n";
                extraId = intId;

                body =
                    $"if({resultVarString} == {CSharpStrings.Running})\n{{\n{continueRunningString}\n}}\n{outRunningString}";
                break;
            case "PluginBehaviac.Nodes.DecoratorWeight":
                acp2 = mustStatusVar;
                enterDo = "//这个必然会在概率选择之下\n";
                break;

            case "PluginBehaviac.Nodes.DecoratorLoopUntil" or "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                acp2 = mustStatusVar;
                countString = node.Attribute("Count")?.Value ?? throw new NullReferenceException();

                string? s2;
                switch (nodeType)
                {
                    case "PluginBehaviac.Nodes.DecoratorLoopUntil":
                        body += "//循环直到默认不能是一帧内完成循环\n";
                        var untilString = nodeType == "PluginBehaviac.Nodes.DecoratorLoopUntil"
                            ? node.Attribute("Until")?.Value ?? throw new NullReferenceException()
                            : "false";
                        var boolGenStatus = CSharpStrings.BoolGenStatus(untilString);
                        s2 = $"{resultVarString} != {boolGenStatus}";
                        break;
                    case "PluginBehaviac.Nodes.DecoratorLoopUntilSuccessOrRunning":
                        loopInOneTick = true;
                        body += "//循环直到返回成功或者运行中只能是一帧内完成循环\n";
                        s2 =
                            $"({resultVarString} != {CSharpStrings.Success} && {resultVarString} != {CSharpStrings.Running})";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!loopInOneTick)
                {
                    headRunningSwitch += runningSwitch;
                }
                else
                {
                    enterDo += "//配置为一帧内循环，不会记录此running跳转\n";
                }

                if (countString == "const int -1")
                {
                    // if (Node51Result != EBTStatus.BT_SUCCESS)
                    // {
                    //     Node51Result = EBTStatus.BT_RUNNING;
                    //     Node0RunningNode = 51;
                    //     Node0Result = EBTStatus.BT_RUNNING;
                    //     goto Node0Out;
                    // }
                    body += loopInOneTick
                        ? "if(" + s2 + ")\n{\n" + $"goto {idString}Run;\n" + "}\n" +
                          $"if({resultVarString} == {CSharpStrings.Running})"
                        : "if(" + s2 + ")";
                    body += "\n{\n" + runString +
                            continueRunningString +
                            "}\n" + outRunningString;
                }
                else
                {
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime = -1;\n";
                    enterDo =
                        $"{idString}NowRunTime = 0;\n{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName, countString, idString, out var acpAdd)};\n";
                    acp2 += acpAdd;
                    body += loopInOneTick
                        ? "if(" + idString + "NowRunTime < " + idString +
                          $"MaxRunTime && {s2})" + "\n{\n"
                          + $"{idString}NowRunTime++;\n" + $"goto {idString}Run;\n}}\n" +
                          $"if({resultVarString} == {CSharpStrings.Running})\n{{\n"
                        : "if(" + idString + "NowRunTime < " + idString +
                          $"MaxRunTime && {s2})\n{{\n" + $"{idString}NowRunTime++;\n";
                    body +=
                        $"{runString}"
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
                    acp2 += "private int " + idString + "NowRunTime = 0;\n";
                    acp2 += "private int " + idString + "MaxRunTime = -1;\n";
                    enterDo =
                        $"{idString}NowRunTime = 0;\n{idString}MaxRunTime = {ConvertArmToFuncOrParam(agentObjName, countString, idString, out var acpA)};\n";
                    acp2 += acpA;
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
                    $"private {CSharpStrings.BtStatusEnumName} {resultVarString} ;\n";
                acp2 += $"private int {idString}StartFrame  = -1;\n";
                var waitTickString = node.Attribute("Frames")?.Value ?? throw new NullReferenceException();
                var waitTick = ConvertArmToFuncOrParam(agentObjName, waitTickString, idString, out var acp2a);
                acp2 += acp2a;
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
                enterDo =
                    $"{idString}ParallelSuccess = {successPolicyInit};\n{idString}ParallelFail = {failurePolicyInit};\n{idString}ParallelRunning = false;\n" +
                    aggregate;
                extraConfig = new ParallelNodeConfig
                    (childFinishPolicy, successPolicy, failurePolicy); //子节点一些运行策略依赖父节点配置，需要把设置传下去
                // Node47Result = Node47ParallelFail ? EBTStatus.BT_FAILURE :
                // Node47ParallelSuccess ? EBTStatus.BT_SUCCESS :
                //     Node47ParallelRunning ? EBTStatus.BT_RUNNING : EBTStatus.BT_FAILURE;
                outPutString =
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
                var subTreeName = CSharpStrings.SimpleRemoveParameterAndActionHead(subTreeValue);
                var subTreeType = subTreeName[1..^1].Replace('/', '_');
                // private IBTree Node9SubTree { get; }
                acp2 += $"private {subTreeType} {idString}SubTree;\n";
                body = (needResult ? $"{resultVarString} = " : "") + $"{idString}SubTree.Tick();\n";
                // Node9SubTree = new WrapperAI_NewTest_TestNode(a);
                subTreeConstruct += $"{idString}SubTree = new {subTreeType}(a);\n";
                if (needResult)
                {
                    var runningNow =
                        " if (" + resultVarString + $" == {CSharpStrings.Running})\n" +
                        "{\n" + continueRunningString
                        + "}\n"
                        + outRunningString;

                    headRunningSwitch += runningSwitch;
                    body += runningNow;
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
            acp2 += $"private int Node{runningGoNode}RunningNode = -1;\n";
            localS = $"switch (Node{runningGoNode}RunningNode)\n{{\n" + headRunningSwitch + "\n}\n";
            headRunningSwitch = "";
        }

        treeStatusValues += acp2;
        res += onEnter + localS + enterDo + runLabel + s + body + outLabel + outPutString + parentVarNeedString +
               outOpNeed +
               skipString;
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
        out string constListParam)
    {
        string rr;
        constListParam = "";
        if (r.Contains('(')) //是函数
        {
            CSharpStrings.FindReturnTypeAndEtc(r, agentObjName, nodeId, out var meName, out constListParam);
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
        out string body, out string acp2)
    {
        var btStatusEnumName = CSharpStrings.BtStatusEnumName;
        acp2 = needResult ? $"private {btStatusEnumName} {resultVarString};\n" : "";
        var success = CSharpStrings.Success;
        // var headResult = needResult ? "" + resultVarString + $" = {success};\n" : "\n";
        var op = node.Attribute("Operator")?.Value ?? throw new Exception("parameter null");
        var left = node.Attribute("Opl")?.Value ?? throw new Exception("parameter null");
        var right = node.Attribute("Opr")?.Value ?? throw new Exception("parameter null");
        var link = CSharpStrings.GenOperator(op);
        var bb = ConvertArmToFuncOrParam(agentObjName, left, idString + "Opl", out var acp2P1) + link +
                 ConvertArmToFuncOrParam(agentObjName, right, idString + "Opr", out var acp2P2);
        acp2 += acp2P1;
        acp2 += acp2P2;
        bb = $"{bb} ? {CSharpStrings.Success} : {CSharpStrings.Fail};\n";
        body = needResult ? resultVarString + $" = {bb}\n" : "";
        // return headResult;
    }


    private static string TransConnector(XElement connector, int parentId, int extraId, string parentString,
        string agentObjName,
        int nowCheckRunningNode,
        INodeConfig? parallelNodeConfig,
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
                return t + " " + name + " = " + value;
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
    // public string ExitPolicy { get; init; } 
}

internal record SelectorProbabilityConfig(IReadOnlyDictionary<int, string> IdToExpression) : INodeConfig;