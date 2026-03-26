using System.Xml.Linq;

using System.Runtime.InteropServices;
namespace BehaviacXmlTrans;

public static class Configs
{
    public static readonly char sep = Path.DirectorySeparatorChar;

    // public static string Dir => "F:{sep}SGAME_CLONE{sep}Project{sep}BTWorkspace";
    public static string Dir
    {
        get
        {
            var workDir = WorkDir;
            var combine = Path.Combine(workDir, "Project","BTWorkspace");
        
            return combine;
        }
    }

    public static bool DebugMode => false;

    private static string[] LocalTestDir
    {
        get
        {
            return Path.GetDirectoryName(Environment.CurrentDirectory)?.Split(Path.DirectorySeparatorChar)
                   ??
                   throw new Exception("cant find dir");
        }
    }

    private static string WorkDir
    {
        get {
          

            var workDir = Path.Combine(LocalTestDir[..^7]);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var isPathRooted = Path.IsPathRooted(workDir);
                workDir = Path.DirectorySeparatorChar + workDir;
                var pathRooted = Path.IsPathRooted(workDir);
            }
            return workDir; }
    }

    // public static string OutPutDir { get; } = GetOutputDir();
    // public static string OutPutDir => @"D:\Client\Project\Assets\Scripts\SGame\InGame\GameLogic\SimpleAI\AutoGen";
    public static string OutPutDir =>
        Path.Combine(WorkDir,
            $"Project{sep}Assets{sep}Scripts{sep}InGame{sep}GameLogic{sep}BtSys{sep}AutoGen");
// /Volumes/OneTb/time/client/trunk/Project/Assets/Scripts/InGame/GameLogic/BtSys/AutoGen
    public static string OutPutSerializerDir =>
        Path.Combine(OutPutDir,
            "Variable");

    private static string Methods { get; } = Path.Combine(Dir, "SGame.meta.xml");

    public static string EditDir => "behaviors";

    public static string RootDir => Dir.Replace($"Project{sep}BTWorkspace", $"Behaviac{sep}New{sep}");

    public static string FiledCachePath => Path.Combine(RootDir, "FiledCache.txt");

    public static string VariableDirtyPath => Path.Combine(RootDir, "VariableDirty");

    // Hero\AI\Decision
    // public static string TestName =>
    // $"WrapperAI{Path.DirectorySeparatorChar}Hero{Path.DirectorySeparatorChar}AI{Path.DirectorySeparatorChar}Decision{Path.DirectorySeparatorChar}Hero_Common_Equip_Operation.xml";
    // public static string TestName => $"WrapperAI{Path.DirectorySeparatorChar}Monster{Path.DirectorySeparatorChar}BTMonsterPassive.xml";
    public static string TestName =>
        $"WrapperAI{Path.DirectorySeparatorChar}NewTest{Path.DirectorySeparatorChar}TestNode3.xml";

    public static string TestDir => $"WrapperAI";

    public static XElement MetaXml { get; } =
        XElement.Load(GetMeta());

    public static string AgentBaseOrInterface { get; } = "BTreeAgent";
 
    private static string GetMeta()
    {
        var dir = Path.Combine(Dir, "behaviors", "behaviac_meta");

        var directoryInfo = new DirectoryInfo(dir);
        var fileInfos = directoryInfo.GetFiles("*.meta.xml");
        var firstOrDefault = fileInfos.FirstOrDefault() ??
                             throw new Exception($"no file match in {dir}");
        return firstOrDefault.FullName;
    }

    public static string GetTickFuncString => "(int)BTreeAgent.CurFrameNum()";
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

    public static string DebugTreeLog(string agentName, string fileName)
    {
        return $"#if {DebugModeString}\n{agentName}.DebugTreeLog(\"{fileName}\");\n#endif\n";
    }

    public static string GetAgentStr(string agent)
    {
        return "BTreeAgent";
    }
}