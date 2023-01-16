﻿using System.Xml.Linq;

namespace BehaviacXmlTrans;

public static class Configs
{
    public static string Dir => "D:\\Client\\Project\\BTWorkspace";

    private static string LocalTestDir =>Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory))) ??
                                         throw new Exception("cant find dir");


    public static string OutPutDir { get; } = GetOutputDir();

    private static string GetOutputDir()
    {
        var directoryName = Path.GetDirectoryName(LocalTestDir) ?? throw new Exception("cant find dir");
        var combine = Path.Combine(directoryName, "BehaviacRunTest", "AutoGen");
        return combine;
    }

    private static string Methods { get; } = Path.Combine(Dir, "SGame.meta.xml");

    public static string EditDir => "behaviors";
    public static string TestName => "WrapperAI\\NewTest\\TestNode.xml";
    // public static string TestFile { get; } = Path.Combine(Dir, EditDir, TestName);

    public static XElement MetaXml { get; } =
        XElement.Load(GetMeta());


    static string GetMeta()
    {
        var dir = Path.Combine(Dir, "behaviors", "behaviac_meta");
        
        var directoryInfo = new DirectoryInfo(dir);
        var firstOrDefault = directoryInfo.GetFiles("*.meta.xml").FirstOrDefault() ??
                             throw new Exception($"no file match in {dir}");
        return firstOrDefault.FullName;
    }
}