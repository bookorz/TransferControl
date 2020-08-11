﻿
using LiteDB;
using log4net;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TransferControl.Comm;
using TransferControl.Config;

namespace TransferControl.Management
{
    public static class NodeManagement
    {
        private static ConcurrentDictionary<string, Node> NodeList;
        private static ConcurrentDictionary<string, Node> NodeListByCtrl;
        static ILog logger = LogManager.GetLogger(typeof(NodeManagement));

   

        public static void LoadConfig()
        {
            NodeList = new ConcurrentDictionary<string, Node>();
            NodeListByCtrl = new ConcurrentDictionary<string, Node>();
            //Dictionary<string, object> keyValues = new Dictionary<string, object>();
            //string Sql = @"SELECT 
            //             UPPER(t.node_id) AS name, UPPER(t.controller_id) AS controller,
            //             t.conn_address AS adrno, UPPER(t.node_type) AS TYPE, 
            //             UPPER(t.vendor) AS brand,t.bypass,t.enable_flg AS ENABLE,                        
            //             t.wafer_size as WaferSize,
            //                t.Double_Arm as DoubleArmActive,
            //                t.Notch_Angle as NotchAngle,
            //                t.R_Flip_Degree as R_Flip_Degree,
            //                t.associated_node as Associated_Node,
            //                t.r_arm as RArmActive,
            //                t.l_arm as LArmActive,
            //                t.carrier_type AS CarrierType,
            //                t.mode as Mode,
            //                t.ack_timeout as AckTimeOut,
            //                t.motion_timeout as MotionTimeOut
            //            FROM config_node t
            //            WHERE t.equipment_model_id = @equipment_model_id";
            //keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
            //DataTable dt = dBUtil.GetDataTable(Sql, keyValues);
            //string str_json = JsonConvert.SerializeObject(dt, Formatting.Indented);
            ////str_json = str_json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\\"", "\"");
            //List<Node> nodeList = JsonConvert.DeserializeObject<List<Node>>(str_json);
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                // Get customer collection
                var col = db.GetCollection<Node>("config_node");
                var result = col.Query().Where(x => x.equipment_model_id.Equals(SystemConfig.Get().SystemMode) );
                List<Node> cfgList = result.ToList();


                foreach (Node each in cfgList)
                {
                    //if (each.Enable)
                    //{
                    each.InitialObject();
                    NodeList.TryAdd(each.Name, each);
                    NodeListByCtrl.TryAdd(each.Controller + each.AdrNo, each);
                    //}
                }
            }
        }

        public static void InitialNodes()
        {
            foreach (Node each in NodeList.Values.ToList())
            {
                each.CurrentLoadPort = "";
                each.CurrentPosition = "";

                //each.InitialComplete = false;
                each.JobList.Clear();

                each.Phase = "";
                each.PutOut = false;
                //each.TransferQueue.Clear();

            }
        }

        public static bool IsRobotInitial()
        {
            bool result = false;
            var findNotInit = from node in NodeList.Values.ToList()
                              where !node.InitialComplete && node.Type.Equals("ROBOT") && !node.ByPass
                              select node;
            if (findNotInit.Count() == 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public static bool IsNeedInitial()
        {
            bool result = false;
            var findNotInit = from node in NodeList.Values.ToList()
                              where !node.InitialComplete && !node.Type.Equals("OCR") && !node.Type.Equals("SYSTEM") && !node.ByPass
                              select node;
            if (findNotInit.Count() == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static string GetCurrentState()
        {
            string result = "";
            var findAlarm = from node in NodeList.Values.ToList()
                            where node.State.Equals("Alarm") && !node.ByPass
                            select node;
            if (findAlarm.Count() != 0)
            {
                result = "Alarm";
            }
            else
            {
                var findPause = from node in NodeList.Values.ToList()
                                where node.State.Equals("Pause") && !node.ByPass
                                select node;
                if (findPause.Count() != 0)
                {
                    result = "Pause";
                }
                else
                {
                    var findRun = from node in NodeList.Values.ToList()
                                  where node.State.Equals("Run") && !node.ByPass
                                  select node;
                    if (findRun.Count() != 0)
                    {
                        result = "Run";
                    }
                    else
                    {
                        var findIdle = from node in NodeList.Values.ToList()
                                       where node.State.Equals("Idle") && !node.ByPass
                                       select node;
                        if (findIdle.Count() != 0)
                        {
                            result = "Idle";
                        }
                        else
                        {
                            var findDown = from node in NodeList.Values.ToList()
                                           where node.State.Equals("Down") && !node.ByPass
                                           select node;
                            if (findDown.Count() != 0)
                            {
                                result = "Down";
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static List<Node> GetLoadPortList()
        {
            List<Node> result = new List<Node>();

            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT")
                           select port;

            if (findPort.Count() != 0)
            {
                result = findPort.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static List<Node> GetAlignerList()
        {
            List<Node> result = new List<Node>();

            var findA = from A in NodeList.Values.ToList()
                           where A.Type.Equals("ALIGNER") && A.Enable
                           select A;

            if (findA.Count() != 0)
            {
                result = findA.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static List<Node> GetLoadPortList(string Mode)
        {
            List<Node> result = new List<Node>();

            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Mode.Equals(Mode)
                           select port;

            if (findPort.Count() != 0)
            {
                result = findPort.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static Node GetLoadPortByFoup(string FoupId)
        {
            Node result = null;
            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Enable && port.Carrier.CarrierID.ToUpper().Equals(FoupId.ToUpper())
                           select port;
            if (findPort.Count() != 0)
            {
                result = findPort.First();
            }
            return result;
        }

        public static Node GetLoadPortByPTN(int PTN)
        {
            Node result = null;
            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Enable && port.PTN.Equals(PTN)
                           select port;
            if (findPort.Count() != 0)
            {
                result = findPort.First();
            }
            return result;
        }

        public static List<Node> GetEnableRobotList()
        {
            List<Node> result = new List<Node>();

            var findRobot = from robot in NodeList.Values.ToList()
                            where robot.Type.Equals("ROBOT") && robot.Enable == true
                            select robot;

            if (findRobot.Count() != 0)
            {
                result = findRobot.ToList();
            }

            return result;
        }

        public static List<Node> GetList()
        {
            List<Node> result = NodeList.Values.ToList();

            result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });

            return result;
        }

        public static Node Get(string Name)
        {
            Node result = null;
            if (Name != null)
            {
                NodeList.TryGetValue(Name.ToUpper(), out result);
            }
            if (result == null)
            {
                logger.Error("Node not exist, Name:" + Name);
            }
            return result;
        }

        public static Node GetRobotByPosition(string Position, string filtName)
        {
            Node result = null;

            foreach (Node each in NodeList.Values.ToList())
            {

                if (each.CurrentPosition.Equals(Position) && !each.Name.Equals(filtName) && each.Type.Equals("ROBOT"))
                {
                    result = each;
                }
            }

            return result;
        }

        public static Node GetByController(string DeviceName, string NodeAdr)
        {
            Node result = null;

            NodeListByCtrl.TryGetValue(DeviceName + NodeAdr, out result);

            return result;
        }

        public static Node GetOCRByController(string DeviceName)
        {
            Node result = null;

            var node = from LD in NodeManagement.GetList()
                       where LD.Controller.Equals(DeviceName)
                       select LD;
            if (node.Count() != 0)
            {
                result = node.First();
                if (!result.Type.Equals("OCR"))
                {
                    result = null;
                }
            }
            return result;
        }

        //public static Node GetNextRobot(Node ProcessNode, Job Job)
        //{
        //    Node result = null;


        //    var findPoint = from point in PointManagement.GetPointList(ProcessNode.Name)
        //                    where point.Position.ToUpper().Equals(Job.Destination.ToUpper())
        //                    select point;
        //    if (findPoint.Count() != 0)
        //    {
        //        result = NodeManagement.Get(findPoint.First().NodeName);
        //    }
        //    return result;
        //}

        public static Node GetNextRobot(string Destination)
        {
            Node result = null;

            var findPoint = from point in PointManagement.GetPointList()
                            where point.Position.ToUpper().Equals(Destination.ToUpper())
                            select point;
            if (findPoint.Count() != 0)
            {
                result = Get(findPoint.First().NodeName);
            }

            return result;
        }

        //public static Node GetOCRByAligner(Node Aligner)
        //{
        //    Node result = null;

        //    foreach (Node.Route eachRt in Aligner.RouteTable)
        //    {
        //        if (eachRt.NodeType.Equals("OCR"))
        //        {
        //            if (NodeList.TryGetValue(eachRt.NodeName, out result))
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static Node GetAlignerByOCR(Node OCR)
        //{
        //    Node result = null;

        //    foreach (Node.Route eachRt in OCR.RouteTable)
        //    {
        //        if (eachRt.NodeType.Equals("ALIGNER"))
        //        {
        //            if (NodeList.TryGetValue(eachRt.NodeName, out result))
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static bool Add(string Name, Node Node)
        {
            bool result = false;


            if (!NodeList.ContainsKey(Name))
            {
                NodeList.TryAdd(Name, Node);
                NodeListByCtrl.TryAdd(Node.Controller + Node.AdrNo, Node);
                result = true;
            }



            return result;
        }

    }
}
