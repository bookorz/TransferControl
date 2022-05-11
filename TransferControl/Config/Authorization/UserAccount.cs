using System;
using System.Collections.Generic;
using System.IO;
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

        public static UserAccount NowUser = null;

        private static string FileName = @"D:\config\Account.json";

        public static UserAccount Verification(string userId, string password)
        {
            UserAccount result = null;

            if (!File.Exists(FileName))
            {
                if(!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config/Account.json", FileName, true);
            }


            var UserInfo = from usr in new ConfigTool<List<UserAccount>>().ReadFile(FileName)
                           where usr.active && usr.userId.ToUpper().Equals(userId.ToUpper()) && password.ToMD5().Equals(usr.password.ToUpper())
                           select usr;
            if (UserInfo.Count() != 0)
            {
                result = UserInfo.First();

                NowUser = result;
            }
            return result;
        }
        public static UserAccount Verification(string userId)
        {
            UserAccount result = null;

            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config/Account.json", FileName, true);
            }


            var UserInfo = from usr in new ConfigTool<List<UserAccount>>().ReadFile(FileName)
                           where usr.active && usr.userId.ToUpper().Equals(userId.ToUpper())
                           select usr;
            if (UserInfo.Count() != 0)
            {
                result = UserInfo.First();

                NowUser = result;
            }
            return result;
        }

        public static UserAccount Get(string userId)
        {
            UserAccount result = null;

            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config /Account.json", FileName, true);
            }

            var UserInfo = from usr in new ConfigTool<List<UserAccount>>().ReadFile(FileName)
                           where usr.userId.ToUpper().Equals(userId.ToUpper())
                           select usr;
            if (UserInfo.Count() != 0)
            {
                result = UserInfo.First();
                NowUser = result;
            }
            return result;
        }

        public static List<UserAccount> GetList()
        {
            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config /Account.json", FileName, true);
            }
            return new ConfigTool<List<UserAccount>>().ReadFile(FileName);
        }

        public static bool Update(UserAccount user)
        {
            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config/Account.json", FileName, true);
            }

            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() != 0)
            {
                UserInfo.First().active = user.active;
                UserInfo.First().groupId = user.groupId;
                UserInfo.First().password = user.password.ToMD5();
                new ConfigTool<List<UserAccount>>().WriteFile(FileName, userList);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Create(UserAccount user)
        {
            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config/Account.json", FileName, true);
            }

            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() == 0)
            {
                user.password = user.password.ToMD5();
                userList.Add(user);
                new ConfigTool<List<UserAccount>>().WriteFile(FileName, userList);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Delete(UserAccount user)
        {
            if (!File.Exists(FileName))
            {
                if (!Directory.Exists(@"D:\config"))
                    Directory.CreateDirectory(@"D:\config");

                File.Copy("config /Account.json", FileName, true);
            }

            List<UserAccount> userList = GetList();
            var UserInfo = from usr in userList
                           where usr.userId.Equals(user.userId)
                           select usr;
            if (UserInfo.Count() != 0)
            {
                userList.Remove(UserInfo.First());
                new ConfigTool<List<UserAccount>>().WriteFile(FileName, userList);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
