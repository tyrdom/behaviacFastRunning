// See https://aka.ms/new-console-template for more information


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;


namespace BehaviacXmlTrans
{
    class Program
    {
        static List<(string name, string objTypeName)> valueTuples = new List<(string name, string objTypeName)>();

        static void Main(string[] args)
        {
            // try
            {
                var path1 = Configs.Dir;
                var editDir = Configs.EditDir;
                var p = Path.Combine(path1, editDir);
                // var path = Path.Combine(Configs.Dir, Configs.EditDir, Configs.TestName);
                //
                // var convertAXmlFile = Tools.ConvertAXmlFile(path, p, out var outFileName);
                // var outPutFile = Path.Combine(Configs.OutPutDir, outFileName);
                // var writeFile = WriteFile(outPutFile, convertAXmlFile);
                // // Console.Out.WriteLine($"{convertAXmlFile}");


                var dirPath = Path.Combine(path1, editDir, Configs.TestDir);

             


                var directoryInfo = new DirectoryInfo(dirPath);

                var enumerable = Configs.Director(directoryInfo).Where(x => x.EndsWith(".xml")).ToArray();

                foreach (var x in enumerable)
                {
                    GenTree(x);
                }

                var select = valueTuples.Select(x => (x.name.Replace(p, "").Replace(".xml", ""),Configs.GetAgentStr(x.objTypeName) ));
                var genConstructor = Tools.GenConstructor(select);
                var output2 = Path.Combine(Configs.OutPutDir, "BTreeStandard.cs");
                var writeFile3 = WriteFile(output2, genConstructor);

                //VariableDirtyHandle.Handle();

                Tools.GenerateVariable();
                if (!File.Exists(Configs.VariableDirtyPath))
                {
                    File.Create(Configs.VariableDirtyPath).Dispose();
                }

                if (writeFile3.IsCompletedSuccessfully)
                {
                    Console.Out.WriteLine("Complete");
                }

                Console.WriteLine("Press any key to continue . . . ");

                Console.ReadKey(true);
            }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            //     Console.WriteLine("Press any key to continue . . . ");
            //     Console.ReadKey(true);
            // }
        }

        public static void GenTree(string x)
        {
            var replace = x.Replace(Path.Combine(Configs.Dir, Configs.EditDir), "");
            if(replace.Contains($"{Path.DirectorySeparatorChar}."))
                return;
            var fileName1 = replace
                .Replace(Path.DirectorySeparatorChar, '_');
            var path = fileName1[1..].Replace(".xml", "");

            if (Tree.TreeDic.TryGetValue(path, out var tree))
                return;

            tree = new Tree();
            Tree.TreeDic.Add(path, tree);
            tree.Path = path;
            Console.Out.WriteLine($"NOW Cov {x}");
            var aXmlFile = Tools.ConvertAXmlFile(x, Path.Combine(Configs.Dir, Configs.EditDir), out var saveFileName,
                out string objTypeName, tree);

            valueTuples.Add((x, objTypeName));
            var output = Path.Combine(Configs.OutPutDir, saveFileName);
            File.WriteAllText(output, aXmlFile);
            Console.Out.WriteLine("Complete" + aXmlFile);
        }


        private static async Task WriteFile(string fileName, string contents)
        {
            await File.WriteAllTextAsync(fileName, contents);
        }
    }
}