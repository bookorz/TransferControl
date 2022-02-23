using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransferControl.Config;
using TransferControl.Management;
using TransferControl.Procedure.SubProcedure;

namespace TransferControl.Procedure.MainProcedure
{
    public class DemoEFEMMainProc: MainProc
    {
        private DemoLPProc LP1, LP2;
        //private Demo2ArmRobot Robot1;
        private SubProc Robot1;
        public DemoEFEMMainProc()
        {
            LP1 = new DemoLPProc(NodeManagement.Get("LOADPORT01"));
            LP2 = new DemoLPProc(NodeManagement.Get("LOADPORT02"));

            //Robot1 = new Demo2ArmRobot(NodeManagement.Get("ROBOT01"));
            Robot1 = new Demo1ArmRobot(NodeManagement.Get("ROBOT01"));
        }
        public override bool Start()
        {
            Node nodeLP1 = NodeManagement.Get("LOADPORT01");
            nodeLP1.Mode = "LD";
            if (!nodeLP1.InitialComplete || !nodeLP1.OrgSearchComplete) return false;

            Node nodeLP2 = NodeManagement.Get("LOADPORT02");
            nodeLP2.Mode = "ULD";
            if (!nodeLP2.InitialComplete || !nodeLP2.OrgSearchComplete) return false;

            Node nodeRobot1 = NodeManagement.Get("ROBOT01");
            if (!nodeRobot1.InitialComplete || !nodeRobot1.OrgSearchComplete) return false;

            bool Ret = true;

            if (!SystemConfig.Get().OfflineMode)
            {
                nodeLP1.Foup_Placement = true;
                nodeLP1.Foup_Presence = true;

                nodeLP2.Foup_Placement = true;
                nodeLP2.Foup_Presence = true;
            }


            if (!LP1.Start()) Ret = false;
            if (!LP2.Start()) Ret = false;


            if (!Robot1.Start()) Ret = false;

            if (!Ret) Stop();

            return Ret;
        }

        public override void Stop()
        {
            LP1.Stop();
            LP2.Stop();

            Robot1.Stop();
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
    }
}
