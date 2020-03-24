using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Config;

namespace TransferControl.Config.Authorization
{
    public class FunctionGroup
    {
        public string id { get; set; }
        public string name { get; set; }
        public string form { get; set; }
        public string reference { get; set; }

        public static List<FunctionGroup> GetList()
        {
            return new ConfigTool<List<FunctionGroup>>().ReadFile("config/FunctionGroup.json");
        }

        public static FunctionGroup Get(string Id)
        {
            List<FunctionGroup> GroupList = GetList();
            var groupInfo = from grp in GroupList
                           where grp.id.ToUpper().Equals(Id.ToUpper()) 
                            select grp;
            if (groupInfo.Count() != 0)
            {
                return groupInfo.First();
            }
            else
            {
                return null;
            }
        }
    }
}
