using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Config;

namespace TransferControl.Config.Authorization
{
    public class UserGroupFunction
    {
        public string userGroupId { get; set; }
        public string functionId { get; set; }
        public bool active { get; set; }

       
        public static List<UserGroupFunction> Get(string groupId)
        {
            List<UserGroupFunction> GroupList = new ConfigTool<List<UserGroupFunction>>().ReadFile("config/UserGroupFunction.json");
            var groupInfo = from grp in GroupList
                            where grp.userGroupId.ToUpper().Equals(groupId.ToUpper())
                            select grp;
            if (groupInfo.Count() != 0)
            {
                return groupInfo.ToList();
            }
            else
            {
                return null;
            }
        }

        public static bool Update(UserGroupFunction usrGroup)
        {
            List<UserGroupFunction> GroupList = new ConfigTool<List<UserGroupFunction>>().ReadFile("config/UserGroupFunction.json");
            var groupInfo = from grp in GroupList
                            where grp.userGroupId.ToUpper().Equals(usrGroup.userGroupId.ToUpper()) && grp.functionId.ToUpper().Equals(usrGroup.functionId.ToUpper())
                            select grp;
            if (groupInfo.Count() != 0)
            {
                groupInfo.First().active = usrGroup.active;
                new ConfigTool<List<UserGroupFunction>>().WriteFile("config/UserGroupFunction.json", GroupList);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool Create(UserGroupFunction usrGroup)
        {
            List<UserGroupFunction> GroupList = new ConfigTool<List<UserGroupFunction>>().ReadFile("config/UserGroupFunction.json");
            var groupInfo = from grp in GroupList
                            where grp.userGroupId.ToUpper().Equals(usrGroup.userGroupId.ToUpper()) && grp.functionId.ToUpper().Equals(usrGroup.functionId.ToUpper())
                            select grp;
            if (groupInfo.Count() == 0)
            {
                GroupList.Add(usrGroup);
                new ConfigTool<List<UserGroupFunction>>().WriteFile("config/UserGroupFunction.json", GroupList);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool Delete(UserGroupFunction usrGroup)
        {
            List<UserGroupFunction> GroupList = new ConfigTool<List<UserGroupFunction>>().ReadFile("config/UserGroupFunction.json");
            var groupInfo = from grp in GroupList
                            where grp.userGroupId.ToUpper().Equals(usrGroup.userGroupId.ToUpper()) && grp.functionId.ToUpper().Equals(usrGroup.functionId.ToUpper())
                            select grp;
            if (groupInfo.Count() != 0)
            {
                GroupList.Remove(groupInfo.First());
                new ConfigTool<List<UserGroupFunction>>().WriteFile("config/UserGroupFunction.json", GroupList);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
