using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Config;

namespace TransferControl.Config.Authorization
{
    public class UserAccount
    {
        public string userId { get; set; }
        public string password { get; set; }
        public string groupId { get; set; }
        public bool active { get; set; }

        public static UserAccount Verification(string userId, string password)
        {
            UserAccount result = null;

            var UserInfo = from usr in new ConfigTool<List<UserAccount>>().ReadFile("config/Account.json")
                           where usr.active && usr.userId.ToUpper().Equals(userId.ToUpper()) && password.ToMD5().Equals(usr.password.ToUpper())
                           select usr;
            if (UserInfo.Count() != 0)
            {
                result = UserInfo.First();
            }
            return result;
        }

        public static UserAccount Get(string userId)
        {
            UserAccount result = null;

            var UserInfo = from usr in new ConfigTool<List<UserAccount>>().ReadFile("config/Account.json")
                           where usr.userId.ToUpper().Equals(userId.ToUpper())
                           select usr;
            if (UserInfo.Count() != 0)
            {
                result = UserInfo.First();
            }
            return result;
        }

        public static List<UserAccount> GetList()
        {
            return new ConfigTool<List<UserAccount>>().ReadFile("config/Account.json");
        }

        public static bool Update(UserAccount user)
        {
            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() != 0)
            {
                UserInfo.First().active = user.active;
                UserInfo.First().groupId = user.groupId;
                UserInfo.First().password = user.password.ToMD5();
                new ConfigTool<List<UserAccount>>().WriteFile("config/Account.json", userList);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Create(UserAccount user)
        {
            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() == 0)
            {
                user.password = user.password.ToMD5();
                userList.Add(user);
                new ConfigTool<List<UserAccount>>().WriteFile("config/Account.json", userList);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Delete(UserAccount user)
        {
            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() != 0)
            {
                userList.Remove(UserInfo.First());
                new ConfigTool<List<UserAccount>>().WriteFile("config/Account.json", userList);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
