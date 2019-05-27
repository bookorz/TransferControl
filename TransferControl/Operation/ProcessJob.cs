using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Operation
{
    public class ProcessJob: IXfeStateReport
    {
        //Create Data
        public string PJ_ObjID;
        public int CarrierQuantity;
        public string CarrierID;
        public int SlopNumber;
        public int[] SlotID;
        public int PRRecipeMethod;
        public string RcpSpec;
        public int ProcessStart;
        public long Seq;

        //run time data
        public int PJState;
        private XfeCrossZone xfe;
        IProcessJobReport _report;

        public ProcessJob()
        {
           
            CarrierQuantity = 25;
            PJ_ObjID = "";
            CarrierID = "";
            SlopNumber = 0;
            PRRecipeMethod = 0;
            RcpSpec = "";
            ProcessStart = 0;
            SlotID = new int[0];
            Seq = DateTime.Now.Ticks;
            PJState = 0;
            xfe = new XfeCrossZone(this);
        }

        public void SetReport(IProcessJobReport report)
        {
            _report = report;
        }

        public bool Start()
        {
            return xfe.Start(CarrierID);
        }

        public void On_Transfer_Complete(XfeCrossZone xfe)
        {
            if (_report != null)
            {
                _report.On_PJ_Complete(this);
            }
            ////XfeCrossZone.Stop();
            //double wph = (xfe.ProcessCount / xfe.ProcessTime) * 3600.0 * 1000.0;
            
            ////Reverse Foup
            //string startPort = "";
            //foreach (Job each in JobManagement.GetJobList())
            //{
            //    startPort = each.Destination;
            //    //string from = each.FromPort;

            //    var p = from port in NodeManagement.GetLoadPortList()
            //            where port.IsMapping && !port.Name.Equals(each.Destination) && !port.Name.Equals(each.FromPort)
            //            select port;
            //    string from = "";
            //    if (p.Count() != 0)
            //    {
            //        from = p.First().Name;
            //    }
            //    else
            //    {
            //        from = each.FromPort;
            //    }


            //    string fromSlot = each.FromPortSlot;
            //    each.FromPort = each.Destination;
            //    each.FromPortSlot = each.DestinationSlot;
            //    each.AssignPort(from, fromSlot);
            //    each.NeedProcess = true;
            //}
            //xfe.Start(startPort);
        }

        
        public void On_LoadPort_Complete(Node Port)
        {
            throw new NotImplementedException();
        }

        public void On_UnLoadPort_Complete(Node Port)
        {
            throw new NotImplementedException();
        }
    }
}
