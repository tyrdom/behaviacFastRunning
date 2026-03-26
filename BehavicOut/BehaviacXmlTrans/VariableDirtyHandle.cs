using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviacXmlTrans
{
    internal static class VariableDirtyHandle
    {

        public static void Handle()
        {
            var newData = ConstructTreeData();
            ReadTreeFiled(newData, out var bDirty);

            if (bDirty) 
            {
                Tools.GenerateVariable();
                if (!File.Exists(Configs.VariableDirtyPath))
                {
                    File.Create(Configs.VariableDirtyPath).Dispose();

                }
            }

            SaveTreeFiled(newData);
        
        }

        private static List<TreeData> ConstructTreeData()
        {
            List<TreeData> treeDatas = new List<TreeData>();
            foreach (var item in Tree.TreeDic)
            {
                List<string> key = new List<string>();
                foreach (var field in item.Value.Fields)
                {
                    key.Add(field.Key);
                }
                treeDatas.Add(new TreeData(item.Key, key));
            }

            return treeDatas;
        }

        private static void SaveTreeFiled(List<TreeData> data)
        {
      
            TreeDataJson json = new TreeDataJson();
            json.datas = data;

            if (!File.Exists(Configs.FiledCachePath))
            {
                File.Create(Configs.FiledCachePath).Dispose();
            }

            File.WriteAllText(Configs.FiledCachePath, JsonMapper.ToJson(json));
        }

        private static void ReadTreeFiled(List<TreeData> newData, out bool bDirty)
        {
            if (!File.Exists(Configs.FiledCachePath))
            {
                bDirty = true;
                return;
            }

            string str = File.ReadAllText(Configs.FiledCachePath);
            var oldData = JsonMapper.ToObject<TreeDataJson>(str).datas;
            if (newData.Count != oldData.Count)
            {
                bDirty = true;
                return;
            }

            for (int i = 0; i < newData.Count; i++)
            {
                var oldTree = oldData.Find(a => { return a.Name == newData[i].Name; });
                if (oldTree == null || oldTree.key.Count != newData[i].key.Count)
                {
                    bDirty = true;
                    return;
                }

                foreach (var item in newData[i].key)
                {
                    if (!oldTree.key.Contains(item))
                    {
                        bDirty = true;
                        return;
                    }
                }
            }

            bDirty = false;
        }


        [System.Serializable]
        public class TreeDataJson
        {
            public List<TreeData> datas = new List<TreeData>();
        }

        [System.Serializable]
        public class TreeData
        {
            public string Name = "";
            public List<string> key = new List<string>();

            public TreeData() { }
            public TreeData(string name, List<string> key)
            {
                Name = name;
                this.key = key;
            }
        }
    }
}
