using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BehaviacXmlTrans;

public static class CSharpStrings
{
    public static string BtStatusEnumName { get; } = "EBTStatus";
    public static string Success { get; } = $"{BtStatusEnumName}.BT_SUCCESS";
    public static string Fail { get; } = $"{BtStatusEnumName}.BT_FAILURE";
    public static string Running { get; } = $"{BtStatusEnumName}.BT_RUNNING";
    public static string Invalid { get; } = $"{BtStatusEnumName}.BT_INVALID";
    public static string RunTimeBaseClass => "BTree";

    public static string IGenerateSerializer = "public void Deserialize(MemoryReaderEx memoryReaderEx, ICreateInstance handle)\r\n    {\r\n\r\n    }\r\n\r\n    public void Serialize(MemoryWriterEx memoryWriterEx, ICreateInstance handle)\r\n    {\r\n\r\n    }";
    public static string GenerateFlag = "[GenerateFormatter]";
    public static string GenerateInterface = "IGenerateSerializer";

    public static Dictionary<string, string> CSTypeShort = new()
    {
        {"System.UInt32", "uint"}, {"System.String", "string"}, {"System.Int32", "int"}, {"System.Single", "float"},
        {"System.Boolean", "bool"}, {"System.Int16", "short"}, {"System.UInt16", "ushort"}, {"System.Int64", "long"},
        {"System.UInt64", "ulong"}, {"System.Byte", "ubyte"}, {"System.SByte", "sbyte"}
    };


    public static string SimpleRemoveParameterAndActionHead(string s)
    {
        //struct常量 :const VInt3 {x=0;y=0;z=0;}
        //变量 SGame::InGame::GameLogic::ObjAgent
        //枚举 const SGame::InGame::GameLogic::SkillSlotType SLOT_SKILL_2
        //系统常量 const int 0
        //字符串
        if (s.StartsWith('"'))
        {
            return s;
        }

        if (CheckStructParamsAndFix(s, out var replace3)) return replace3;

        var lastIndexOf = s.LastIndexOf(":", StringComparison.Ordinal) + 1;
        if (lastIndexOf > 0)
        {
            var removeParameterAndActionHead = s[lastIndexOf..];
            var replace = removeParameterAndActionHead.Replace(' ', '.');
            return replace;
        }

        lastIndexOf = s.LastIndexOf(" ", StringComparison.Ordinal) + 1;

        return s[lastIndexOf..];
    }

    public static bool CheckStructParamsAndFix(string s, out string replace2)
    {
        var of = s.LastIndexOf('{');
        replace2 = "";
        if (of < 0) return false;
        // Console.Out.WriteLine("is Custom Type");
        var indexOf = s.LastIndexOf('}') - 1;
        var values = s[(of + 1)..indexOf];
        var replace = values.Replace(';', ',');
        var replace1 = of > 1 ? s[..(of - 1)].Replace("const", "new") + "()" : "";
        replace2 = replace1 + "{" + replace + "}";
        return true;
    }

    public static string GenOperator(string op)
    {
        return op switch
        {
            "Equal" => " == ",
            "GreaterEqual" => " >= ",
            "LessEqual" => " <= ",
            "Less" => " < ",
            "Greater" => " > ",
            "Add" => " + ",
            "NotEqual" => " != ",
            "Mul" => " * ",
            "Sub" => " - ",
            "Div" => " / ",
            _ => throw new ArgumentOutOfRangeException($"cant match op : {op}")
        };
    }

    public static string BoolGenStatus(string b)
    {
        return b switch
        {
            "true" => Success,
            "false" => Fail,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static string StringToEnum(string resOp)
    {
        return resOp switch
        {
            "BT_SUCCESS" => Success,
            "BT_FAILURE" => Fail,
            _ => throw new ArgumentOutOfRangeException($"行为树指定的结果不能为 {resOp}")
        };
    }

    public static string FindReturnTypeAndEtc(string name, string objName, string nodeId, out string methodName,
        out string constListParam, out string[] agesToInit)
    {
        var findClass = FindClassAndEtc(name, out var mName, out var parameters);
        var contains = mName.Contains("PlayAgeAction");
        agesToInit = contains
            ? parameters.Where(x => x.StartsWith('"') && x.EndsWith(".bytes\"")).ToArray()
            : Array.Empty<string>();

        var xElements = Configs.MetaXml.Element("agents")
            ?.Elements().ToArray();
        var xElement = xElements
            ?.Where(element =>
                element.Attribute("classfullname")?.Value.ToString() == findClass
            ).ToArray();

        if (xElement == null) throw new NullReferenceException($"no method <{name}> in find class {findClass}");
        {
            var baseTypes = xElement.Select(xE => xE.Attribute("base")?.Value.ToString() ?? "");
            var elements = xElements
                ?.Where(element => baseTypes.Contains(
                    element.Attribute("classfullname")?.Value.ToString())
                ).ToArray();
            var eee = elements != null ? xElement.Union(elements) : xElement;
            {
                foreach (var element in eee)
                {
                    var firstOrDefault = element.Elements("Method")
                        .FirstOrDefault(x => x.Attribute("Name")?.Value.ToString() == mName);
                    if (firstOrDefault == null)
                    {
                        // Console.Out.WriteLine($"not find {mName} in {element}");
                        continue;
                    }

                    var returnTypeAndEtc = firstOrDefault.Attribute("ReturnType")?.Value ??
                                           throw new NullReferenceException();
                    var xAttributes = firstOrDefault.Elements("Param").Select(
                        x => (x.Attribute("Name")?.Value ?? throw new NullReferenceException(),
                            x.Attribute("TypeFullName")?.Value ?? throw new NullReferenceException())
                    ).Zip(parameters);

                    var (s1, s2) = xAttributes.Aggregate
                    ((" ", ""), (s, x) =>
                    {
                        var t1 = s.Item1 +
                                 FuncParameterFix(x.First.Item1, x.First.Item2, x.Second, nodeId, out var cs) + ',';
                        var t2 = s.Item2 + cs;
                        return (t1, t2);
                    });
                    var value = firstOrDefault.Attribute("Static")?.Value ?? throw new NullReferenceException();
                    ;
                    var cname = value == "true" ? findClass.Replace("::", ".") : objName;
                    methodName = cname + "." + mName + "(" + s1[..^1] +
                                 ")";
                    constListParam = s2;
                    return returnTypeAndEtc;
                }
            }

            throw new NullReferenceException($"no method name {name} <{mName}> findClass is {findClass}");
        }
    }

    public static string SetCustomTeamSetActorVariableReplace(string field, string type, string methodName, string id)
    {
        try
        {
            var index = methodName.IndexOf(",") + 1;
            string[] param = methodName.Substring(index, methodName.Length - 1 - index).Split(',');
            type = type.Substring(0, 1).ToUpper() + type.Substring(1, type.Length - 1);
            string str = "var Btree#id# = agent.GetActorBTree(#param#);\r\nif(Btree#id# != null)\r\n{\r\n   " +
          "var variable = Btree#id# as IVariable_WrapperAI_Hero_HeroCommonAI;\r\n   if (variable != null)\r\n       " +
          "variable.Set#type#Variable_#field#(#val#);\r\n   else\r\n       SDebug.LogError(\"hero variable is " +
          "not \\\"IVariable_WrapperAI_Hero_HeroCommonAI\\\"\");\r\n}";

            return str.Replace("#field#", field).Replace("#type#", type).Replace("#val#", param[0]).Replace("#param#", param[1]).Replace("#id#",id);;
        }
        catch
        {
            Console.WriteLine("设置团队黑板值参数错误：" + methodName);
            return "";
        }

    }

    public static string SetCustomTeamSetSquadVariableReplace(string field,string type,string methodName,string id)
    {
        try
        {
            var index = methodName.IndexOf(",") + 1;
            type = type.Substring(0, 1).ToUpper() + type.Substring(1, type.Length - 1);
            string[] param = methodName.Substring(index, methodName.Length - 1 - index).Split(',');
            string str = "var list#id#= agent.GetSquadBTree(#param#);\r\nif(list#id# != null)\r\n{\r\n\tfor (int i = 0; " +
          "i < list#id#.Count; i++)\r\n\t{\r\n\t\tvar variable = list#id#[i].GetVariable() as IVariable_WrapperAI_Hero_HeroCommonAI;\r\n\t\tif " +
          "(variable != null)\r\n            variable.Set#type#Variable_#field#(#val#);\r\n        else\r\n           " +
          "SDebug.LogError(\"hero variable is not \\\"IVariable_WrapperAI_Hero_HeroCommonAI\\\"\");\r\n    }\r\n}";

            return str.Replace("#field#", field).Replace("#type#", type).Replace("#val#", param[0]).Replace("#param#", param[1]).Replace("#id#",id);
        }
        catch
        {
            Console.WriteLine("设置团队黑板值参数错误："+ methodName);
            return "";
        }  
    }

    public static string SetCustomVariableReplace(string str)
    {
       return str.Replace("\",", "(").Replace("( \"", "_").Replace("agent", "variable");
    }

    public static string GetCustomVariableReplace(string str)
    {
        return str.Replace("\")", "()").Replace("( \"", "_").Replace("agent", "variable");
    }

    public static bool FindCustomVariable(string str, out string field, out string type)
    {
        type = "";
        field = "";

        foreach (var item in Tree.CustomVariableMethod)
        {
            if (str.Contains(item.Value.Set)|| str.Contains(item.Value.Get))
            {
                type = item.Key;
                var startIndex = str.IndexOf("\"") + 1;
                var endIndex = str.LastIndexOf("\"");
                if (startIndex == -1 || endIndex == -1)
                {
                    throw new NullReferenceException($"no support variable str: {str}  field:{field}");
                    //return false;
                }

                field = str[startIndex..endIndex];
                return true;
            }
        }

        return false;
    }


    public static bool FindSetTeamVariable(string str, out string field, out string type,out bool setActor)
    {
        type = "";
        field = "";
        setActor = false;

        foreach (var item in Tree.CustomTeamVariableMethod)
        {
            if (str.Contains(item.Value.ActorSet) || str.Contains(item.Value.SquadSet))
            {
                setActor = str.Contains(item.Value.ActorSet);
                type = item.Key;
                var startIndex = str.IndexOf("\"") + 1;
                var endIndex = str.LastIndexOf("\"");
                if (startIndex == -1 || endIndex == -1)
                {
                    throw new NullReferenceException($"no support variable str: {str}  field:{field}");
                    //return false;
                }

                field = str[startIndex..endIndex];
                return true;
            }
        }

        return false;
    }

    private static string FuncParameterFix(string paramName, string typeString, string argString, string nodeId,
        out string constList)
    {
        constList = "";
        if (!argString.StartsWith('"') && argString.Contains("::")) //非字符串并且带有::认为是变量，有问题后面再传入变量名到参数表里找
        {
            return SimpleRemoveParameterAndActionHead(argString);
        }

        //通过类型来给出正确的常量参数写法
        // //错误1 不给枚举写枚举类型 Self.SGame::InGame::GameLogic::ObjAgent::GetSkillAttackRange(SLOT_SKILL_2)
        // //错误2 浮点数没有f结尾 Self.SGame::InGame::GameLogic::ObjAgent::PlayAnimation(&quot;Idle&quot;,0.15,0,true)
        var paramType = Tools.FindParamType(typeString, out typeString);

        // var findParamType = Tools.FindCustomParamType(typeString, out var typeString2);

        switch (paramType)
        {
            case PType.Enum:
                return typeString + '.' + argString;
            case PType.Struct:
                return "new" + "()" +
                       (CheckStructParamsAndFix(argString, out var value2) ? value2 : "");
            case PType.System:
                return typeString switch
                {
                    "float" => argString + 'f',

                    _ => argString
                };
            case PType.Array:
                var indexOf = argString.IndexOf(':') + 1;
                var s2 = argString[indexOf..];
                var findParamType = Tools.FindParamType(typeString, out var typeString2);
                var varName = nodeId + "Func" + paramName;
                if (findParamType == PType.Array)
                {
                    throw new NotSupportedException("not support array in array");
                }

                if (s2 == "")
                {
                    constList =
                        $"private List<{typeString2}> {varName} = new List<{typeString2}>(){{}};//常数数组参数只用创建1次\n";
                    return varName;
                }

                var replace = s2.Split('|');
                var s1 = replace.Select(x => FuncParameterFix(paramName, typeString, x, nodeId, out _))
                    .Aggregate("", (s, x) => s + ',' + x)[1..];
                constList =
                    $"private List<{typeString2}> {varName} = new List<{typeString2}>(){{{s1}}};//常数数组参数只用创建1次\n";
                return varName;

            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new NotSupportedException($"cant find {typeString}");
    }

    private static string FindClassAndEtc(string name, out string methodName, out string[] parameters)
    {
        parameters = Array.Empty<string>();
        var var = name.IndexOf(".", StringComparison.Ordinal) + 1;
        var var2 = name.IndexOf("(", StringComparison.Ordinal);
        var v3 = name.IndexOf(")", StringComparison.Ordinal);
        var fullname = name[..var2];
        var lastIndexOf = fullname.LastIndexOf(":", StringComparison.Ordinal);
        var ls = lastIndexOf - 1;
        methodName = fullname[(lastIndexOf + 1)..];
        var enumerable = name[var..ls];
        var p = name[(var2 + 1)..v3];
        var strings = p.Split(','); //先简单切分，认为字符串中没有,
        parameters = strings;
        return enumerable;
    }

    public static string GetBreakPointStr(string nodeType,string nodeId)
    {
        return ("CHECK_BREAKPOINT(\"#nodeType#\", "+"#nodeId#);").Replace("#nodeType#", nodeType).Replace("#nodeId#", nodeId);
    }
}