using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class ProcessJobManagement
    {
        private static ConcurrentDictionary<string, ProcessJob> ProcessJobList = new ConcurrentDictionary<string, ProcessJob>();

        public static void Initial()
        {
            ProcessJobList.Clear();
        }

        public static string GetNewID()
        {
            int currentIdx = 1;
            string key = "";
            while (true)
            {
                key = "PR" + currentIdx.ToString("000");
                if (!ProcessJobList.ContainsKey(key))
                {
                    break;
                }
                currentIdx++;
            }
            return key;
        }

        public static List<ProcessJob> GetProcessJobList()
        {

            List<ProcessJob> result = new List<ProcessJob>();
            lock (ProcessJobList)
            {
                lock (ProcessJobList)
                {
                    result = ProcessJobList.Values.ToList();
                }
            }
            return result;
        }

        public static ProcessJob Get(string ProcessJobId)
        {
            ProcessJob result = null;

            lock (ProcessJobList)
            {
                ProcessJobList.TryGetValue(ProcessJobId, out result);
            }

            return result;
        }

        public static bool Add(string ProcessJobId, ProcessJob PJ)
        {
            bool result = false;
            lock (ProcessJobList)
            {
                if (!ProcessJobList.ContainsKey(ProcessJobId))
                {
                    ProcessJobList.TryAdd(ProcessJobId, PJ);
                    result = true;
                }
            }
            return result;
        }

        public static bool Remove(string ProcessJobId)
        {
            bool result = false;
            lock (ProcessJobList)
            {
                ProcessJob tmp;
                result = ProcessJobList.TryRemove(ProcessJobId, out tmp);

            }
            return result;
        }
    }
}
