namespace BehaviacXmlTrans;

public static class CSharpStrings
{
    public static string BtStatusEnumName { get; } = "EBTStatus";
    public static string Success { get; } = $"{BtStatusEnumName}.BT_SUCCESS";
    public static string Fail { get; } = $"{BtStatusEnumName}.BT_FAILURE";
    public static string Running { get; } = $"{BtStatusEnumName}.BT_RUNNING";
    public static string Invalid { get; } = $"{BtStatusEnumName}.BT_INVALID";
    public static string RunTimeInterface => ":IBTree";
    public static string RootRunningNodeString => "rootRunningNode";

    public static Dictionary<string, (string CsharpType, bool IsNewType)> TypeDic => new()
    {
        {"int", ("int", false)}
    };

    public static string RemoveParameterAndActionHead(string s)
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
        Console.Out.WriteLine("is Custom Type");
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
            _ => throw new ArgumentOutOfRangeException()
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
            _ => throw new Exception()
        };
    }

    public static string FindReturnTypeAndEtc(string name, string objName, out string methodName)
    {
        var findClass = FindClassAndEtc(name, out var mName, out var parameters);


        var xElements = Configs.MetaXml.Element("agents")
            ?.Elements().ToArray();
        var xElement = xElements
            ?.Where(element =>
                element.Attribute("classfullname")?.Value.ToString() == findClass
            ).ToArray();
        if (xElement == null) throw new NullReferenceException($"no method name {name} find class {findClass}");
        {
            {
                foreach (var element in xElement)
                {
                    var firstOrDefault = element.Elements("Method")
                        .FirstOrDefault(x => x.Attribute("Name")?.Value.ToString() == mName);
                    if (firstOrDefault == null) continue;
                    var returnTypeAndEtc = firstOrDefault.Attribute("ReturnType")?.Value ??
                                           throw new NullReferenceException();
                    var xAttributes = firstOrDefault.Elements("Param").Select(x =>
                        x.Attribute("Type")?.Value ?? throw new NullReferenceException()).Zip(parameters);
                    var s1 = xAttributes.Aggregate(" ", (s, x) => s + FuncParameterFix(x.First, x.Second) + ',')[..^1];

                    methodName = objName + "." + mName + "(" + s1 +
                                 ")";
                    return returnTypeAndEtc;
                }
            }

            throw new NullReferenceException($"no method name {name}");
        }
    }

    private static string FuncParameterFix(string typeString, string argString)
    {
        if (!argString.StartsWith('"') && argString.Contains("::")) //非字符串并且带有::认为是变量，有问题后面再传入变量名
        {
            return RemoveParameterAndActionHead(argString);
        }

        //通过类型来给出正确的常量参数写法
        //错误1 不给枚举写枚举类型 Self.SGame::InGame::GameLogic::ObjAgent::GetSkillAttackRange(SLOT_SKILL_2)
        //错误2 浮点数没有f结尾 Self.SGame::InGame::GameLogic::ObjAgent::PlayAnimation(&quot;Idle&quot;,0.15,0,true)
        var findParamType = Tools.FindParamType(typeString, true);
        switch (findParamType)
        {
            case PType.Enum:
                var lastIndexOf = typeString.LastIndexOf(':')+1;
                return typeString[lastIndexOf..] + '.' + argString;
        }

        return typeString switch
        {
            "float" => argString + 'f',

            _ => argString
        };
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
}