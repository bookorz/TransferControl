using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class CarrierManagement
    {
        private static ConcurrentDictionary<string, Carrier> CarrierList = new ConcurrentDictionary<string, Carrier>();

        public static void Initial()
        {
            CarrierList.Clear();
        }

        public static string GetNewID()
        {
            int currentIdx = 1;
            string key = "";
            while (true)
            {
                key = "CST" + currentIdx.ToString("000");
                if (!CarrierList.ContainsKey(key))
                {
                    break;
                }
                currentIdx++;
            }
            return key;
        }

        public static List<Carrier> GetCarrierList()
        {

            List<Carrier> result = new List<Carrier>();
            lock (CarrierList)
            {
                lock (CarrierList)
                {
                    result = CarrierList.Values.ToList();                    
                }
            }
            return result;
        }

        public static Carrier Get(string CarrierId)
        {
            Carrier result = null;

            lock (CarrierList)
            {
                CarrierList.TryGetValue(CarrierId, out result);
            }

            return result;
        }

        public static bool Add(string CarrierId, Carrier Carrier)
        {
            bool result = false;
            lock (CarrierList)
            {
                if (!CarrierList.ContainsKey(CarrierId))
                {
                    CarrierList.TryAdd(CarrierId, Carrier);
                    result = true;
                }
            }
            return result;
        }

        public static bool Remove(string CarrierId)
        {
            bool result = false;
            lock (CarrierList)
            {
                Carrier tmp;
                result = CarrierList.TryRemove(CarrierId, out tmp);

            }
            return result;
        }

    }
}
