using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TransferControl.Management
{
    public class TimerManagement
    {
        static private Stopwatch sw = new Stopwatch();
        static private long Counter = 0;
        static private long ElapsedMilliseconds = 0;
        private static object CountLock = new object();
        private static bool IsRun = false;

        static public void Initial()
        {
            lock (CountLock)
            {
                Counter = 0;

                if (sw.IsRunning)
                    sw.Stop();

                sw.Restart();
            }



        }
        static public void Start()
        {
            lock (CountLock)
            {
                if (sw.IsRunning)
                    sw.Stop();

                sw.Restart();
            }

        }
        static public void Pause()
        {
            lock (CountLock)
            {
                if (sw.IsRunning)
                {
                    sw.Stop();
                    ElapsedMilliseconds += sw.ElapsedMilliseconds;
                }

            }
        }
        static public void Record()
        {
            lock (CountLock)
            {
                if (sw.IsRunning)
                {
                    sw.Stop();
                    ElapsedMilliseconds += sw.ElapsedMilliseconds;
                }

                sw.Restart();
            }
        }


        static public void Add(int count)
        {
            lock(CountLock)
            {
                Counter += count;
            }
        }

        static public long GetCount()
        {
            return Counter;
        }

        static public long GetElapsedMilliseconds()
        {
            return ElapsedMilliseconds;
        }
    }
}
