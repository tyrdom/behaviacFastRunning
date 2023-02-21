using System.Xml.Linq;

namespace BehaviacXmlTrans;

public static class Configs
{
    // public static string Dir => "F:\\SGAME_CLONE\\Project\\BTWorkspace";
    public static string Dir => Path.Combine(WorkDir,"Project\\BTWorkspace");

    public static bool DebugMode => false;

    private static string[] LocalTestDir =>
        Path.GetDirectoryName(Environment.CurrentDirectory)?.Split(Path.DirectorySeparatorChar) ??
        throw new Exception("cant find dir");

    private static string WorkDir => Path.Combine(LocalTestDir[..^7]);
    // public static string OutPutDir { get; } = GetOutputDir();
    // public static string OutPutDir => @"D:\Client\Project\Assets\Scripts\SGame\InGame\GameLogic\SimpleAI\AutoGen";
    public static string OutPutDir => Path.Combine(WorkDir,"Project\\Assets\\Scripts\\SGame\\InGame\\GameLogic\\BtSys\\AutoGen");

    private static string Methods { get; } = Path.Combine(Dir, "SGame.meta.xml");

    public static string EditDir => "behaviors";

    // Hero\AI\Decision
    // public static string TestName =>
    // $"WrapperAI{Path.DirectorySeparatorChar}Hero{Path.DirectorySeparatorChar}AI{Path.DirectorySeparatorChar}Decision{Path.DirectorySeparatorChar}Hero_Common_Equip_Operation.xml";
    // public static string TestName => $"WrapperAI{Path.DirectorySeparatorChar}Monster{Path.DirectorySeparatorChar}BTMonsterPassive.xml";
    public static string TestName =>
        $"WrapperAI{Path.DirectorySeparatorChar}NewTest{Path.DirectorySeparatorChar}TestNode3.xml";

    public static string TestDir => $"WrapperAI";

    public static XElement MetaXml { get; } =
        XElement.Load(GetMeta());

    public static string AgentBaseOrInterface { get; } = "BTBaseAgent";

    private static string GetMeta()
    {
        var dir = Path.Combine(Dir, "behaviors", "behaviac_meta");

        var directoryInfo = new DirectoryInfo(dir);
        var firstOrDefault = directoryInfo.GetFiles("*.meta.xml").FirstOrDefault() ??
                             throw new Exception($"no file match in {dir}");
        return firstOrDefault.FullName;
    }

    public static string GetTickFuncString => "(int)ObjAgent.CurFrameNum()";
    public static string DebugModeString => "DEBUG_MODE";

    public static string CurrentFileName;

    public static IEnumerable<string> Director(DirectoryInfo d)
    {
        var files = d.GetFiles(); //文件
        var directs = d.GetDirectories(); //文件夹
        var collection = files.Select(f => f.FullName);
        var selectMany = directs.SelectMany(Director);
        var enumerable = collection.Union(selectMany);
        return enumerable;
    }

    public static string DebugLogString(string agentName, int id, string nodeType)
    {
        var arr = nodeType.Split(".");
        var typeStr = arr[arr.Length - 1];

        var ts2 = Configs.CurrentFileName.Replace(".cs", "");

        return $"#if {DebugModeString}\n{agentName}.DebugModeLog({id},\"{typeStr}\",\"{ts2}\");\n#endif\n";
    }

    public static string DebugTreeLog(string agentName,string fileName)
    {
        return $"#if {DebugModeString}\n{agentName}.DebugTreeLog(\"{fileName}\");\n#endif\n";
    }
}