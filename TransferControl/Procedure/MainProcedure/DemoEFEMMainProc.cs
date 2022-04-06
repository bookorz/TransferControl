using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TransferControl.Config;
using TransferControl.Management;
using TransferControl.Procedure.SubProcedure;

namespace TransferControl.Procedure.MainProcedure
{
    public class DemoEFEMMainProc: MainProc
    {
        private DemoLPProc LP1, LP2;
        private AlignerProc AL1;
        private Demo2ArmRobot Robot1;
        private ManualResetEvent[] ProcFinishedEvt = new ManualResetEvent[4];

        enum Proc
        {
            LP1 = 0,
            LP2,
            AL,
            ROB,
            TOTAL
        }

        //private SubProc Robot1;
        public DemoEFEMMainProc(IMainProcCallback prco):base(prco)
        {
            LP1 = new DemoLPProc(NodeManagement.Get("LOADPORT01"));
            ProcFinishedEvt[(int)Proc.LP1] = LP1.ProcFinishedEvt;

            LP2 = new DemoLPProc(NodeManagement.Get("LOADPORT02"));
            ProcFinishedEvt[(int)Proc.LP2] = LP2.ProcFinishedEvt;

            AL1 = new AlignerProc(NodeManagement.Get("ALIGNER01"));
            ProcFinishedEvt[(int)Proc.AL] = AL1.ProcFinishedEvt;

            Robot1 = new Demo2ArmRobot(NodeManagement.Get("ROBOT01"));
            ProcFinishedEvt[(int)Proc.ROB] = Robot1.ProcFinishedEvt;
        }
        public override bool Start()
        {
            Node nodeLP1 = NodeManagement.Get("LOADPORT01");
            nodeLP1.Mode = "LD";
            if (!nodeLP1.InitialComplete || !nodeLP1.OrgSearchComplete)
            {
                logger.Debug("Node name :"+ nodeLP1.Name + "InitialFail!");
                return false;
            }


            Node nodeLP2 = NodeManagement.Get("LOADPORT02");
            nodeLP2.Mode = "ULD";
            if (!nodeLP2.InitialComplete || !nodeLP2.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeLP2.Name + "InitialFail!");
                return false;
            }


            Node nodeRobot1 = NodeManagement.Get("ROBOT01");
            if (!nodeRobot1.InitialComplete || !nodeRobot1.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeRobot1.Name + "InitialFail!");
                return false;
            }



            Node nodeAligner1 = NodeManagement.Get("ALIGNER01");
            if (!nodeAligner1.InitialComplete || !nodeAligner1.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeAligner1.Name + "InitialFail!");
                return false;
            }


            bool Ret = true;

            if (!SystemConfig.Get().OfflineMode)
            {
                nodeLP1.Foup_Placement = true;
                nodeLP1.Foup_Presence = true;

                nodeLP2.Foup_Placement = true;
                nodeLP2.Foup_Presence = true;
            }

            for (int i = 0; i < ProcFinishedEvt.Length; i++)
                ProcFinishedEvt[i].Reset();


            if (!LP1.Start()) Ret = false;
            if (!LP2.Start()) Ret = false;

            if (!AL1.Start()) Ret = false;

            if (!Robot1.Start()) Ret = false;

            if (!Ret) Stop();

            return Ret;
        }
        bool IsStopped = false;
        public override void Stop()
        {
            if(!IsStopped)
            {
                IsStopped = true;
                Robot1.Stop();

                ThreadPool.QueueUserWorkItem(new WaitCallback(StartToStopProc), null);
            }

        }

        public override void Pause()
        {
            Robot1.Pause();
        }

        public override void Reset()
        {
            Robot1.Reset();
        }
        public override bool GetPauseStatus()
        {
            return Robot1.IsPause();
        }

        private void StartToStopProc(object node)
        {
            ProcFinishedEvt[(int)Proc.ROB].WaitOne();

            LP1.Stop();
            LP2.Stop();
            AL1.Stop();

            WaitHandle.WaitAll(ProcFinishedEvt, Timeout.Infinite);


            ProcCallback.EndOfProcCallback();
            IsStopped = false;
        }
    }
}
