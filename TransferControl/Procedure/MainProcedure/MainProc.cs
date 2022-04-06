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
        public MainProc(IMainProcCallback prco)
        {
            ProcCallback = prco;
        }
        public virtual bool Start()
        {
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
    }

}
