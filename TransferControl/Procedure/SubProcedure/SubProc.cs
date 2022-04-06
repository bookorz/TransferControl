using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Procedure.SubProcedure
{
    public class SubProc
    {
        public ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected Node ProcNode = null;
        protected bool IsProcExit = false;

        public bool IsProcBusy = false;
        public bool IsProcStop = false;
        public bool IsProcPause = false;

        public ManualResetEvent ProcFinishedEvt = new ManualResetEvent(false);

        public SubProc(Node node)
        {
            ProcNode = node;
        }
        ~SubProc()
        {
            IsProcExit = true;
        }
        public bool IsNodeEnabled(string NodeName)
        {
            return NodeManagement.Get(NodeName) != null ? NodeManagement.Get(NodeName).Enable : false;
        }
        public bool IsPause()
        {
            return IsProcPause;
        }
        public void Pause()
        {
            IsProcPause = true;
        }
        public void Reset()
        {
            IsProcPause = false;
        }
        public bool Start()
        {
            bool Ret = false;
            if (!IsProcBusy && ProcNode != null)
            {
                IsProcBusy = true;
                IsProcStop = false;
                IsProcPause = false;

                ProcFinishedEvt.Reset();

                ProcNode.ProcStatus = Node.ProcedureStatus.Idle;


                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcRun), ProcNode);
                Ret = true;
            }

            return Ret;
        }
        public virtual void Stop()
        {
            IsProcStop = true;
            IsProcPause = false;
        }
        public virtual void Run(Node procNode)
        {
            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    break;
                case Node.ProcedureStatus.Stop:

                    IsProcStop = true;
                    break;
                default:
                    throw new NotSupportedException();

            }
        }
        public void ProcRun(object node)
        {
            Node procNode = (Node)node;
            while (!IsProcExit && !IsProcStop)
            {
                try
                {
                    Run(procNode);

                    SpinWait.SpinUntil(() => !IsProcPause, Timeout.Infinite);
                }
                catch(Exception e)
                {
                    logger.Error("ProcRun fail Node Name:" + procNode.Name +", Node ProcStatus"+ procNode.ProcStatus + " exception: " + e.StackTrace);
                    IsProcBusy = false;
                }
            }

            IsProcBusy = false;
            ProcFinishedEvt.Set();
        }
    }
}
