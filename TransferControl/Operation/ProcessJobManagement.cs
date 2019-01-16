using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Operation
{
    public class ProcessJobManagement
    {
        private static ConcurrentDictionary<string, ProcessJob> ProcessJobList = new ConcurrentDictionary<string, ProcessJob>();

        private const int SpaceNum = 8;

        public static void Initial()
        {
            ProcessJobList.Clear();
        }

        public static int GetRemainOfCount()
        {
            int result = 0;
            result = SpaceNum - ProcessJobList.Count;
            return result;
        }

        public static string GetNewID()
        {
            int currentIdx = 1;
            string key = "";
            lock (ProcessJobList)
            {
                while (true)
                {
                    key = "PR" + currentIdx.ToString("000");
                    if (!ProcessJobList.ContainsKey(key))
                    {
                        break;
                    }
                    currentIdx++;
                }
            }
            return key;
        }

        public static List<ProcessJob> FindByCarrierID(string CarrierID)
        {
            List<ProcessJob> result = null;
            lock (ProcessJobList)
            {
                var find = from each in ProcessJobList.Values
                           where each.CarrierID.ToUpper().Equals(CarrierID.ToUpper())
                           select each;
                if (find.Count() != 0)
                {
                    result = find.ToList();
                }
            }
            return result;
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
                if (ProcessJobList.Count > SpaceNum)
                {//超過限制數量
                    result = false;
                }
                else
                {
                    if (!ProcessJobList.ContainsKey(ProcessJobId))
                    {
                        lock (ProcessJobList)
                        {
                            var find = from each in ProcessJobList.Values
                                       where each.Seq >= PJ.Seq
                                       select each;
                            if (find.Count() != 0)
                            {//流水號不能重複，且要在之前的後面
                                List<ProcessJob> pjList = find.ToList();
                                pjList.Sort((x, y) => { return -x.Seq.CompareTo(y.Seq); });
                                PJ.Seq = pjList.First().Seq + 1;
                            }

                            ProcessJobList.TryAdd(ProcessJobId, PJ);
                        }
                        result = true;
                    }
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
