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
            var aa = tuple.Item2 ? tuple.Item1 + "_" + s1 : tuple.Item1;
            var bb = s1 == Configs.EditDir || tuple.Item2;
            return (aa, bb);
        });
        var fileName1 = valueTuple.Item1;
        var lastIndexOf = fileName1[1..fileName1.LastIndexOf(".", StringComparison.Ordinal)];
        csName = lastIndexOf + ".cs";
        var s = "using behaviac;\nusing SGame.InGame.GameLogic;\n\npublic class " + lastIndexOf +
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
        var constructor = $"public {lastIndexOf}({className} a)\n{{\n{className} = a; \n}}\n";
        s += constructor;
        var enumerable = element.Elements();
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

                    var result = ConnectorTransFromRoot(xElement, className, out var countParams);
                    s += countParams;
                    s += result;
                    break;
            }
        }


        s += "\n}";

        return s;
    }

    public static string ConnectorTransFromRoot(XElement xElement, string agentObjName,
        out string treeStatusParamsAndAgentObj)
    {
        treeStatusParamsAndAgentObj = "\n";
        var head = $"public {CSharpStrings.BtStatusEnumName} Tick()\n{{\n";
        var res = "";
        var node = xElement.Elements().FirstOrDefault(x => x.Name == "Node") ?? throw new NullReferenceException();
        var runningSwitch = "";
        const string rootStatus = "\n int nowRunningNode{get;set;} = -1;\n";
        var s = TransNode(node, "root", "Root", agentObjName, out var tsp,
            out var aRunningSwitch, out var nodeResultInitString) + "\n";
        var behaviortreestatus = "// BehaviorTreeStatus\n";
        treeStatusParamsAndAgentObj += behaviortreestatus + rootStatus + tsp;
        runningSwitch += aRunningSwitch;
        res += s;


        if (runningSwitch == "") return head + res + "\n}";

        var switchString = nodeResultInitString + "switch (nowRunningNode)\n{\n" + runningSwitch + "\n}\n";
        return head + switchString + res + "\n}";
    }

    private static string TransNode(XElement node, string parentIdString, string parentTypeString,
        string agentObjName,
        out string treeStatusValues,
        out string headRunningSwitch, out string
            nodeResultInitString)
    {
        treeStatusValues = "";
        headRunningSwitch = "";
        nodeResultInitString = "";
        var firstOrDefault = node.Elements("Comment").FirstOrDefault();
        var value = firstOrDefault?.Attribute("Text")?.Value;
        var res = "//" + value + "\n";
        var enable = node.Attribute("Enable")?.Value;
        if (enable == "false")
        {
            return res;
        }

        var id = node.Attribute("Id")?.Value;
        var idString = "Node" + id;
        var nodeType = node.Attribute("Class")?.Value;

        if (nodeType == null) throw new NullReferenceException($"{idString} type is Null");

        res += "//" + nodeType + "\n";

        // if (id == "5")
        // {
        //     Console.Out.WriteLine($"{id}");
        // }


        res = res + "//" + idString + "\n";
        var xElements = node.Elements("Connector");


        var s = "";
        // 向子树递归收集
        foreach (var connector in xElements)
        {
            var s1 = TransConnector(connector, idString, nodeType, agentObjName, out var acp,
                out var aRunningSwitch,
                out var irs);
            s += s1;
            treeStatusValues += acp;
            headRunningSwitch += aRunningSwitch;
            nodeResultInitString += irs;
        }

        s = "\n" + s +
            "\n";

        //与父节点关系
        var needResult = true;
        var parentEndStringGoto = parentIdString + "Out;\n";
        var parentVarString = parentIdString + "Result";
        var resultVarString = idString + "Result";
        var tail = "";
        var head = "";
        switch (parentTypeString)
        {
            case "Root":

                tail = $"return {resultVarString};";
                break;
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                needResult = false;
                break;
            case "PluginBehaviac.Nodes.Sequence":

                tail = $"if({resultVarString} == {CSharpStrings.Fail})\n"
                       + "{\n"
                       + $"{parentVarString} = {CSharpStrings.Fail};\n"
                       + $"goto {parentEndStringGoto}"
                       + "}";
                break;
            case "PluginBehaviac.Nodes.Selector":
                needResult = true;
                tail = $"if({resultVarString} == {CSharpStrings.Success})\n"
                       + "{\n"
                       + $"{parentVarString} = {CSharpStrings.Success};\n"
                       + $"goto {parentEndStringGoto}"
                       + "}";
                break;
            case "PluginBehaviac.Nodes.IfElse_condition":

                tail = $"if({resultVarString} == true)\n";
                break;
            case "PluginBehaviac.Nodes.IfElse_if":
                head = "{\n";
                tail = parentVarString + " = " + resultVarString + "\n}";
                break;
            case "PluginBehaviac.Nodes.IfElse_else":
                head = "else\n{\n";
                tail = parentVarString + " = " + resultVarString + "\n}";
                break;
            case "PluginBehaviac.Nodes.WithPreconditionPrecondition":

                head = "//选择监测条件\n";
                tail = $"if({resultVarString} == {CSharpStrings.Fail})\n" +
                       "{\n"
                       + $"goto {parentEndStringGoto}"
                       + "}";
                break;
            case "PluginBehaviac.Nodes.WithPreconditionAction":
                head = "//选择监测动作\n";
                break;
        }

// 解析节点
        var enterString = idString + "Enter:\n";
        var outString = idString + "Out:\n";
        var body = "";
        var acp2 = "";
        var outPutString = "";
        var headResult = "";
        switch (nodeType)
        {
            case "PluginBehaviac.Nodes.DecoratorAlwaysSuccess":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "";
                outPutString = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "";

                break;
            case "PluginBehaviac.Nodes.Sequence":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Invalid};\n" : "\n";

                break;
            case "PluginBehaviac.Nodes.Selector":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Invalid};\n" : "\n";
                break;

            case "PluginBehaviac.Nodes.IfElse":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "\n";
                break;
            case "PluginBehaviac.Nodes.Condition":
                headResult = NodeCondition(node, needResult, resultVarString,
                    agentObjName, out body, out acp2);
                break;
            case "PluginBehaviac.Nodes.Noop":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "\n";
                break;
            case "PluginBehaviac.Nodes.DecoratorLoop":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Success};\n" : "\n";
                var countString = node.Attribute("Count")?.Value;
                headRunningSwitch += $"case {id}:\n" + $"goto {idString}Enter;\n";
                var continueString = $"nowRunningNode = {id};\n"
                                     + $"return {resultVarString};\n";
                if (countString == "const int -1")
                {
                    outPutString = continueString;
                }
                else
                {
                    treeStatusValues += idString + "nowRunTime { get; set; } = 0;\n";
                    treeStatusValues += idString + "maxRunTime { get; set; } = " + countString + ";\n";
                    outPutString = "if(" + idString + "nowRunTime <" + idString + "maxRunTime)\n{" + continueString +
                                   "}";
                }

                break;
            case "PluginBehaviac.Nodes.Action":

                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";

                headResult = needResult ? "" + resultVarString + $" = {CSharpStrings.Success};\n" : "\n";
                var method = node.Attribute("Method")?.Value ??
                             throw new NullReferenceException($"no method in action node {id}");

                var findReturnType = CSharpStrings.FindReturnTypeAndEtc(method, agentObjName, out var methodName);


                if (findReturnType == "behaviac::EBTStatus")
                {
                    var varString = resultVarString + " = " + methodName + ";\n";
                    var runningNowrunningnode =
                        " if (" + resultVarString + $" == {CSharpStrings.Running} )\n" + "{\nnowRunningNode = " + id +
                        ";\n return " + CSharpStrings.Running + ";\n }\n"
                        + "nowRunningNode = -1;";

                    headRunningSwitch += $"case {id}:\n" + $"goto {idString}Enter;\n";
                    body = varString + runningNowrunningnode;
                }
                else
                {
                    var a = methodName + ";\n";
                    var resOp = node.Attribute("ResultOption")?.Value ??
                                throw new NullReferenceException($"not Res @ {id}");
                    var stringToEnum = CSharpStrings.StringToEnum(resOp);
                    var b = resultVarString + "=" + stringToEnum + ";\n";
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
                       + "="
                       + CSharpStrings.RemoveParameterAndActionHead(p1)
                       + CSharpStrings.GenOperator(op)
                       + CSharpStrings.RemoveParameterAndActionHead(p2) + ";";
                break;
            case "PluginBehaviac.Nodes.SelectorLoop":
                acp2 = needResult
                    ? $"private {CSharpStrings.BtStatusEnumName} {resultVarString} {{ get; set; }}\n"
                    : "";
                headResult = needResult ? resultVarString + $" = {CSharpStrings.Invalid};\n" : "\n";
                break;
            case "PluginBehaviac.Nodes.WithPrecondition":

                break;
            default:
                throw new ArgumentException($"Cant Read  {id} :Type {nodeType}");
        }

        treeStatusValues += acp2;
        res += headResult + head + enterString + body + s + outString + outPutString + tail;
        return res;
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

    private static string NodeCondition(XElement node, bool needResult, string resultVarString,
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
        bb = $"({bb}) ? {CSharpStrings.Success} : {CSharpStrings.Fail};\n";

        body = needResult ? resultVarString + $" = {bb};\n" : "";
        return headResult;
    }


    private static string TransConnector(XElement connector, string idString, string parentString, string agentObjName,
        out string acp,
        out string aRunningSwitch, out string irs)
    {
        aRunningSwitch = "";
        var id = connector.Attribute("Identifier")?.Value;
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
        foreach (var xElement in xElements)
        {
            var transNode = TransNode(xElement, idString, fixParentString, agentObjName,
                                out var aStr, out var ars,
                                out var airs) +
                            "\n";
            res += transNode;
            acp += aStr;
            aRunningSwitch += ars;
            irs += airs;
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

    private static object FixParam(string type, string name, string value)
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

    private static PType FindParamType(string replace)
    {
        XElement xElement = Configs.MetaXml.Element("types") ?? throw new NullReferenceException();
        var firstOrDefault = xElement.Elements().FirstOrDefault(x => x.Attribute("Type")?.Value == replace) ??
                             throw new NullReferenceException();
        var xName = firstOrDefault.Name.LocalName;
        return xName switch
        {
            "enumtype" => PType.Enum,
            "struct" => PType.Struct,
            _ => throw new NullReferenceException()
        };
    }
}

enum PType
{
    Enum,
    Struct,
    System
}