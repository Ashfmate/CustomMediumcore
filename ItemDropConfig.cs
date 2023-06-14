using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Program;
using Newtonsoft.Json;

namespace Program
{
    public class ItemDropConfig
    {
        public Dictionary<int, string> Items = new();

        public void Write()
        {
            File.WriteAllText(CustomMedium.path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ItemDropConfig Read() 
        {
            if (!File.Exists(CustomMedium.path)) 
            {
                return new();
            }
            return JsonConvert.DeserializeObject<ItemDropConfig>(File.ReadAllText(CustomMedium.path)) ?? new();
        }
    }
}
