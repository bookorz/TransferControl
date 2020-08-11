using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class RobotPoint
    {
        public string equipment_model_id { get; set; }
        [BsonField("recipe_id")]
        public string RecipeID { get; set; }
        [BsonField("node_name")]
        public string NodeName { get; set; }
        public string Position { get; set; }
        [BsonField("position_type")]
        public string PositionType { get; set; }
        public string Point { get; set; }
        public int Offset { get; set; }
    }
}
