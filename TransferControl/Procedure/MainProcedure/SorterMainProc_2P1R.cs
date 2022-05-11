using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TransferControl.CommandConvert;
using TransferControl.Config;
using TransferControl.Engine;
using TransferControl.Management;
using TransferControl.Procedure.SubProcedure;

namespace TransferControl.Procedure.MainProcedure
{
    public class SorterMainProc_2P1R : MainProc
    {
        private ManualResetEvent[] ProcFinishedEvt = new ManualResetEvent[3];

        private LPProc LP1, LP2;
        private Demo2ArmRobot Robot1;

        enum Proc
        {
            LP1 = 0,
            LP2,
            ROB,
            TOTAL
        }
        public SorterMainProc_2P1R(IMainProcCallback prco, IProcReport report) : base(prco, report)
        {

        }
        public override bool Start()
        {
            Node nodeRobot1 = NodeManagement.Get("ROBOT01");

            Node nodeLP1 = NodeManagement.Get("LOADPORT01");
            LP1 = new LPProc(nodeLP1, Report);
            ProcFinishedEvt[(int)Proc.LP1] = LP1.ProcFinishedEvt;
            if (!nodeLP1.InitialComplete || !nodeLP1.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeLP1.Name + "InitialFail!");
                return false;
            }


            Node nodeLP2 = NodeManagement.Get("LOADPORT02");
            LP2 = new LPProc(nodeLP2, Report);
            ProcFinishedEvt[(int)Proc.LP2] = LP2.ProcFinishedEvt;
            if (!nodeLP2.InitialComplete || !nodeLP2.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeLP2.Name + "InitialFail!");
                return false;
            }



            Robot1 = new Demo2ArmRobot(nodeRobot1, Report);
            ProcFinishedEvt[(int)Proc.ROB] = Robot1.ProcFinishedEvt;
            if (!nodeRobot1.InitialComplete || !nodeRobot1.OrgSearchComplete)
            {
                logger.Debug("Node name :" + nodeRobot1.Name + "InitialFail!");
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

            if (!Robot1.Start()) Ret = false;

            if (!Ret) Stop();

            IsRun = Ret;

            if (IsRun)
                TimerManagement.Initial();

            return Ret;
        }
        bool IsStopped = false;
        public override void Stop()
        {
            if (!IsStopped)
            {
                IsStopped = true;
                Robot1.Stop();

                ThreadPool.QueueUserWorkItem(new WaitCallback(StartToStopProc), null);
            }

        }

        public override void Pause()
        {
            Robot1.Pause();

            if (IsRun)
                TimerManagement.Pause();
        }

        public override void Reset()
        {
            Robot1.Reset();

            if (IsRun)
                TimerManagement.Start();
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

            WaitHandle.WaitAll(ProcFinishedEvt, Timeout.Infinite);


            ProcCallback.EndOfProcCallback();

            IsStopped = false;
            IsRun = false;
        }
        public override void EmergencyStop()
        {
            LP1?.EmergencyStop();
            LP2?.EmergencyStop();
            Robot1?.EmergencyStop();

            TimerManagement.Pause();
        }

    }
}
