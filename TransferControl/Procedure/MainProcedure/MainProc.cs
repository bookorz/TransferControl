using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Procedure.MainProcedure
{
    public class MainProc
    {
        protected ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected IMainProcCallback ProcCallback;
        protected IProcReport Report;

        public bool IsRun { set; get;}
        public MainProc(IMainProcCallback prco, IProcReport report)
        {
            ProcCallback = prco;

            Report = report;

            IsRun = false;
        }
        public virtual bool Start()
        {
            IsRun = true;
            return true;
        }
        public virtual void Stop()
        {

        }

        public virtual void Pause()
        {

        }

        public virtual void Reset()
        {

        }

        public virtual bool GetPauseStatus()
        {
            return true;
        }
        public virtual void EmergencyStop()
        {

        }




    }

}
