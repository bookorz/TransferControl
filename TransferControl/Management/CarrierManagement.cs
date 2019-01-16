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
        private static List<Carrier> CarrierList = new List<Carrier>();

        public static void Initial()
        {
            CarrierList.Clear();
        }

        public static int GetNewIndex()
        {
            int currentIdx = 1;
            
            lock (CarrierList)
            {
                while (true)
                {
                    var find = from each in CarrierList
                               where each.CarrierIndex == currentIdx
                               select each;
                    if (find.Count() == 0)
                    {
                        return currentIdx;
                    }
                    currentIdx++;
                }
            }
           
        }

        public static List<Carrier> GetCarrierList()
        {

            List<Carrier> result = new List<Carrier>();
            lock (CarrierList)
            {
                lock (CarrierList)
                {
                    result = CarrierList;                    
                }
            }
            return result;
        }

        public static Carrier FindByLocation(string PortName)
        {
            Carrier result = null;
            lock (CarrierList)
            {
                var find = from each in CarrierList
                           where each.LocationID.ToUpper().Equals(PortName.ToUpper())
                           select each;
                if (find.Count() != 0)
                {
                    result = find.First();
                }
            }
            return result;
        }

        public static Carrier Get(string CarrierId)
        {
            Carrier result = null;

            lock (CarrierList)
            {
                var find = from each in CarrierList
                           where each.CarrierID.ToUpper().Equals(CarrierId.ToUpper())
                           select each;
                if (find.Count() != 0)
                {
                    result = find.First();
                }
            }

            return result;
        }

        public static bool Add(Carrier Carrier)
        {
            bool result = false;
            lock (CarrierList)
            {
                
                    CarrierList.Add(Carrier);
                    result = true;
                
            }
            return result;
        }

        public static bool Remove(Carrier Carrier)
        {
            bool result = false;
            lock (CarrierList)
            {
                Carrier tmp;
                result = CarrierList.Remove(Carrier);

            }
            return result;
        }

    }
}
