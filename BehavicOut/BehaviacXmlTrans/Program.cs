// See https://aka.ms/new-console-template for more information


using System.Xml.Linq;

namespace BehaviacXmlTrans;

class Program
{
    static void Main(string[] args)
    {
        var p = Path.Combine(Configs.Dir, Configs.EditDir);
        // var path = Path.Combine(Configs.Dir, Configs.EditDir, Configs.TestName);
        //
        // var convertAXmlFile = Tools.ConvertAXmlFile(path, p, out var outFileName);
        // var outPutFile = Path.Combine(Configs.OutPutDir, outFileName);
        // var writeFile = WriteFile(outPutFile, convertAXmlFile);
        // // Console.Out.WriteLine($"{convertAXmlFile}");


        var dirPath = Path.Combine(Configs.Dir, Configs.EditDir, Configs.TestDir);
        var directoryInfo = new DirectoryInfo(dirPath);
        var enumerable = Configs.Director(directoryInfo).Where(x => x.EndsWith(".xml")).ToArray();


        var valueTuples = new List<(string name, string objTypeName)>();
        foreach (var x in enumerable)
        {
            Console.Out.WriteLine($"NOW Cov {x}");
            var aXmlFile = Tools.ConvertAXmlFile(x, p, out var saveFileName, out var objTypeName);
            valueTuples.Add((x, objTypeName));
            var output = Path.Combine(Configs.OutPutDir, saveFileName);

            var writeFile2 = WriteFile(output, aXmlFile);
            if (writeFile2.IsCompletedSuccessfully)
            {
                Console.Out.WriteLine("Complete");
            }
        }

        var select = valueTuples.Select(x => (x.name.Replace(p, "").Replace(".xml", ""), x.objTypeName));
        var genConstructor = Tools.GenConstructor(select);
        var output2 = Path.Combine(Configs.OutPutDir, "IBTree.cs");
        var writeFile3 = WriteFile(output2, genConstructor);
        if (writeFile3.IsCompletedSuccessfully)
        {
            Console.Out.WriteLine("Complete");
        }
    }


    private static async Task WriteFile(string fileName, string contents)
    {
        await File.WriteAllTextAsync(fileName, contents);
    }
}