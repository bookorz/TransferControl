using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public static class JobManagement
    {
        private static ConcurrentDictionary<string, Job> JobList = new ConcurrentDictionary<string, Job>();
        static int seriesNum = 1;
        public static void Initial()
        {
            JobList.Clear();
        }

        public static string GetNewID()
        {
            int currentIdx = 1;
            string key = "";
            while (true)
            {
                key = "Wafer" + currentIdx.ToString("000");
                if (!JobList.ContainsKey(key))
                {
                    break;
                }
                currentIdx++;
            }
            return key;
        }



        public static List<Job> GetJobList()
        {

            List<Job> result = new List<Job>();
            lock (JobList)
            {
                lock (JobList)
                {
                    result = JobList.Values.ToList();
                    result.Sort((x, y) => { return -x.Position.CompareTo(y.Position); });
                }
            }
            return result;
        }


        public static Job Get(string U_Id)
        {
            Job result = null;

            lock (JobList)
            {
                JobList.TryGetValue(U_Id, out result);
            }

            return result;
        }
        public static Job Get(string NodeName, string Slot)
        {
            Job result = null;

            lock (JobList)
            {
                result = (from each in JobList.Values
                          where each.Position.Equals(NodeName) && Convert.ToInt16(each.Slot).ToString().Equals(Convert.ToInt16(Slot).ToString())
                          select each).Count() == 0 ? null : (from each in JobList.Values
                                                              where each.Position.Equals(NodeName) && Convert.ToInt16(each.Slot).ToString().Equals(Convert.ToInt16(Slot).ToString())
                                                              select each).First();
            }

            return result;
        }
        public static List<Job> GetByNode(string NodeName)
        {
            List<Job> result = null;

            lock (JobList)
            {
                result = (from each in JobList.Values
                          where each.Position.Equals(NodeName)
                          select each).ToList();
            }

            return result;
        }
        public static Job Add()
        {
            //Job result = new Job();
            //result.Uid = GetNewID();

            lock (JobList)
            {

                Job result = new Job();
                result.Uid = GetNewID();

                if (!JobList.ContainsKey(result.Uid))
                {
                    JobList.TryAdd(result.Uid, result);
                    result.Host_Job_Id = "Wafer Exist";
                    if (seriesNum >= 1000)
                    {
                        seriesNum = 1;
                    }
                    else
                    {
                        seriesNum++;
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }

        }
        public static bool Remove(Job Job)
        {
            bool result = false;
            lock (JobList)
            {
                Job tmp;
                result = JobList.TryRemove(Job.Uid, out tmp);


            }
            return result;
        }
    }
}
