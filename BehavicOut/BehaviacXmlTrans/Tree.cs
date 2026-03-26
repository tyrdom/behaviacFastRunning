using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviacXmlTrans
{
    public  class Tree
    {
        public static Dictionary<string, VariableMethod> CustomVariableMethod = new()
        {
            {"FInt3",new VariableMethod(){ Get="GetFInt3Variable",Set="SetFInt3Variable" } },
            {"int",new VariableMethod(){ Get="GetIntVariable",Set="SetIntVariable" } },
            {"uint",new VariableMethod(){ Get="GetUintVariable",Set="SetUintVariable" } },
            {"bool",new VariableMethod(){ Get="GetBoolVariable",Set="SetBoolVariable" } },
            {"string",new VariableMethod(){ Get="GetStringVariable",Set="SetStringVariable" } },
        };

        public static Dictionary<string, TeamVariableMethod> CustomTeamVariableMethod = new()
        {
            {"FInt3",new TeamVariableMethod(){ SquadSet="SetSquadFInt3Variable",ActorSet="SetActorFInt3Variable" } },
            {"int",new TeamVariableMethod(){ SquadSet="SetSquadIntVariable",ActorSet="SetActorIntVariable" } },
            {"uint",new TeamVariableMethod(){SquadSet = "SetSquadUintVariable", ActorSet = "SetActorUintVariable"} },
            {"bool",new TeamVariableMethod(){SquadSet = "SetSquadBoolVariable", ActorSet = "SetActorBoolVariable"} },
            {"string",new TeamVariableMethod(){SquadSet = "SetSquadStringVariable", ActorSet = "SetActorStringVariable"} },
        };

        public static Dictionary<string, Tree> TreeDic = new();

        public string Path = "";
        public List<string> SubTreePath = new();
        public Dictionary<string, string> Fields = new();//key:字段名 val:type

        private string ToFieldStr()
        {
            string result = "\tpublic BTreeAgent Agent;\n";
            //foreach (var item in Fields)
            //{
            //    result += $"\tpublic {item.Value} {item.Key};\n";
            //}

            return result;
        }

        private string ToMethodStr()
        {
            string result = "";
            foreach (var item in Fields)
            {
                //Set
                result += $"\tpublic void {CustomVariableMethod[item.Value].Set}_{item.Key}({item.Value} value) \n";
                //result += "\t{\n" +$"\t\t{item.Key} = value;" + "\n\t}\n";
                result += "\t{\n" + $"\t\tAgent.{CustomVariableMethod[item.Value].Set}(\"{item.Key}\",value);" + "\n\t}\n";
                //Get
                result += $"\tpublic {item.Value} {CustomVariableMethod[item.Value].Get}_{item.Key}() \n";
                result += "\t{\n" + $"\t\treturn Agent.{CustomVariableMethod[item.Value].Get}(\"{item.Key}\");" + "\n\t}\n";

            }

            result += "    public void SetAgent(BTreeAgent agent)\r\n    {\r\n        this.Agent = agent;\r\n    }";

            return result;
        }


        public string ToInterfaceStr()
        {
            string result = $"\npublic interface IVariable_" + Path+ ": IVariable\n{\n";
            foreach (var item in Fields)
            {
                //Set
                result += $"\tpublic void {CustomVariableMethod[item.Value].Set}_{item.Key}({item.Value} value); \n";
                //Get
                result += $"\tpublic {item.Value} {CustomVariableMethod[item.Value].Get}_{item.Key}(); \n";
            }

            return result+"}\n";
        }


        public string ToVariableStr()
        {
            string result = "";
            result += ToFieldStr() + ToMethodStr();
            return result;
        }

        public string ToGenerateVariable()
        {
            //wrapper 序列化
            string str = $"using GenerateSerializer;\nusing ZGame.InGame.GameLogic;\n\n" +
                $"{CSharpStrings.GenerateFlag}\r\npublic partial class {Path} : " +
                $"{CSharpStrings.RunTimeBaseClass},{CSharpStrings.GenerateInterface}" +
                $"\r\n{{\r\n    {CSharpStrings.IGenerateSerializer}\n\r}}\n\n";

            //Variable 序列化+黑板值字段
            string subTreeInterface = $",IVariable_{Path}";
            foreach (var item in SubTreePath)
            {
                subTreeInterface += $",IVariable_{item}";
            }

            str += $"{CSharpStrings.GenerateFlag}\r\npublic class Variable_{Path} : {CSharpStrings.GenerateInterface}{subTreeInterface}" +
                $"\r\n{{\n{ToVariableStr()}\r\n    {CSharpStrings.IGenerateSerializer}\n\r}}\n\n";

            //接口
            str += ToInterfaceStr()+ "\n\n";

            return str;
        }

        public void GenSubTree()
        {
            var temp = new List<string>();
            GetSubTree(temp);
            SubTreePath = temp;
        }

        private void GetSubTree(List<string> SubTreePath)
        {
            foreach (var item in this.SubTreePath)
            {
                if (!SubTreePath.Contains(item))
                    SubTreePath.Add(item);
            }

            foreach (var item in this.SubTreePath)
            {
                if (TreeDic.TryGetValue(item, out var subTree))
                {
                    subTree.GetSubTree(SubTreePath);
                }
                else
                {
                    throw new NullReferenceException($"no find subTree {item}");
                }
            }
        }

        public void GenSubTreeField()
        {
            GenSubTreeField(Fields);
        }


        private void GenSubTreeField(Dictionary<string, string> Fields)
        {
            foreach (var field in this.Fields)
            {
                if (!Fields.ContainsKey(field.Key))
                    Fields.Add(field.Key, field.Value);
            }

            foreach (var item in SubTreePath)
            {
                if (TreeDic.TryGetValue(item, out var subTree))
                {
                    subTree.GenSubTreeField(Fields);
                }
                else
                {
                    throw new NullReferenceException($"no find subTree {item}");
                }
            }
        }
        
        public struct VariableMethod
        {
            public string Set;
            public string Get;
        }

        public struct TeamVariableMethod
        {
            public string ActorSet;
            public string SquadSet;
        }

    }


}
