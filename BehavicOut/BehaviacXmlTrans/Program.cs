// See https://aka.ms/new-console-template for more information


using System.Xml.Linq;

namespace BehaviacXmlTrans;

class Program
{
    static void Main(string[] args)
    {
        var path = Path.Combine(Configs.Dir, Configs.EditDir, Configs.TestName);
        var convertAXmlFile = Tools.ConvertAXmlFile(path, out var outFileName);
        var outPutFile = Path.Combine(Configs.OutPutDir, outFileName);
        var writeFile = WriteFile(outPutFile, convertAXmlFile);
        Console.Out.WriteLine($"{convertAXmlFile}");
        if (writeFile.IsCompletedSuccessfully)
        {
            Console.Out.WriteLine("Complete");
        }
    }
    private static async Task WriteFile(string fileName, string contents)
    {
        await File.WriteAllTextAsync(fileName, contents);
    }

    
}