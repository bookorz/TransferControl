using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class RobotPoint
    {
        public string NodeName { get; set; }
        public string Position { get; set; }
        public string MappingPoint { get; set; }
        public string PreMappingPoint { get; set; }
        public string PositionType { get; set; }
        public string Point { get; set; }
        public int Offset { get; set; }
        public string Point2 { get; set; }
        /// <summary>
        /// Clamp 下取/下放 點位
        /// </summary>
        public string DownClampPoint { get; set; }
        /// <summary>
        /// Clamp 上取/上放 點位
        /// </summary>
        public string UpClampPoint { get; set; }
        /// <summary>
        /// Vacuum 下取/下放 點位
        /// </summary>
        public string DownVacuumPoint { get; set; }
        /// <summary>
        /// Vacuum 上取/上放 點位
        /// </summary>
        public string UpVacuumPoint { get; set; }

    }
}
