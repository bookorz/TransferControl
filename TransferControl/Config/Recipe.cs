using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Comm;
using TransferControl.Management;

namespace TransferControl.Config
{
    public class Recipe
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Recipe));
        //id
        public string recipe_id { get; set; }
        public string recipe_name { get; set; }


        //port_type: Load, Unload, Both
        public string port1_type { get; set; }
        public string port2_type { get; set; }
        public string port3_type { get; set; }
        public string port4_type { get; set; }
        public string port5_type { get; set; }
        public string port6_type { get; set; }
        public string port7_type { get; set; }
        public string port8_type { get; set; }
        //port_priority: 1~8
        public int port1_priority { get; set; }
        public int port2_priority { get; set; }
        public int port3_priority { get; set; }
        public int port4_priority { get; set; }
        public int port5_priority { get; set; }
        public int port6_priority { get; set; }
        public int port7_priority { get; set; }
        public int port8_priority { get; set; }

        //port_carrier_type: FOUP, FOSB, 200_Adapter, 300_Adapter
        public string port1_carrier_type { get; set; }
        public string port2_carrier_type { get; set; }
        public string port3_carrier_type { get; set; }
        public string port4_carrier_type { get; set; }
        public string port5_carrier_type { get; set; }
        public string port6_carrier_type { get; set; }
        public string port7_carrier_type { get; set; }
        public string port8__carriertype { get; set; }

        //robot_speed: 1~100
        public string robot1_speed { get; set; }
        public string robot2_speed { get; set; }

        //aligner_speed: 1~100
        public string aligner1_speed { get; set; }
        public string aligner2_speed { get; set; }

        //aligner_speed: 1~100
        public string aligner1_angle { get; set; }
        public string aligner2_angle { get; set; }
        public string notch_angle { get; set; }

        public string ocr_ttl_config { get; set; }
        public Boolean is_use_ocr_ttl { get; set; }
        public string ocr_t7_config { get; set; }
        public Boolean is_use_ocr_t7 { get; set; }
        public string ocr_m12_config { get; set; }
        public Boolean is_use_ocr_m12 { get; set; }
        public Boolean is_use_aligner1 { get; set; }
        public Boolean is_use_aligner2 { get; set; }
        public Boolean is_use_exchange { get; set; }
        public Boolean is_use_burnin { get; set; }
        public string ocr_type { get; set; }
        public string ocr_check_Rule { get; set; }
        public string get_slot_order { get; set; }
        public string put_slot_order { get; set; }
        public string wafer_size { get; set; }
        ////ocr_config
        //public string ocr1_config { get; set; }
        //public string ocr2_config { get; set; }

        //uac_check_CIDRW
        public string uac_check_CIDRW { get; set; }

        //input_proc_fin:
        public string input_proc_fin { get; set; }

        //output_proc_fin
        public string output_proc_fin { get; set; }

        //auto_proc_fin
        public string auto_proc_fin { get; set; }

        //manual_proc_fin
        public string manual_proc_fin { get; set; }

        //auto_get_constrict
        public string auto_get_constrict { get; set; }

        //auto_put_constrict
        public string auto_put_constrict { get; set; }

        //manual_get_constrict
        public string manual_get_constrict { get; set; }

        //manual_put_constrict
        public string manual_put_constrict { get; set; }

        //auto_fin_unclamp
        public string auto_fin_unclamp { get; set; }

        //manual_fin_unclamp
        public string manual_fin_unclamp { get; set; }

        //log_path
        public string log_path { get; set; }

        //equip_id
        //public string equip_id { get; set; }//20190613 移到 sysconfig

        public string ffu_rpm_open { get; set; }
        public string ffu_rpm_close { get; set; }

        public Boolean is_use_double_arm { get; set; }
        public Boolean is_use_r_arm { get; set; }
        public Boolean is_use_l_arm { get; set; }

        public static Recipe CurrentRecipe;

        static Dictionary<string, Recipe> tmp = new Dictionary<string, Recipe>();

        public string RunMode { get; set; } = "";

        public void Reload()
        {
            Recipe Content;
            tmp.Remove(this.recipe_id);

            if (!File.Exists(string.Format(@"D:\recipe\{0}.json", recipe_id)))
            {
                if (!Directory.Exists(@"D:\recipe"))
                    Directory.CreateDirectory(@"D:\recipe");

                File.Copy("recipe/" + recipe_id + ".json", string.Format(@"D:\recipe\{0}.json", recipe_id), true);
            }

            ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
            Content = SysCfg.ReadFile(string.Format(@"D:\recipe\{0}.json", recipe_id));

            if (Content != null)
                tmp.Add(this.recipe_id, Content);
        }
        public static Recipe Get(string fileName)
        {
            Recipe Content;
            if (!tmp.TryGetValue(fileName, out Content))
            {
                if (!File.Exists(string.Format(@"D:\recipe\{0}.json", fileName)))
                {
                    if (!Directory.Exists(@"D:\recipe"))
                        Directory.CreateDirectory(@"D:\recipe");

                    File.Copy("recipe/" + fileName + ".json", string.Format(@"D:\recipe\{0}.json", fileName), true);
                }

                ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
                Content = SysCfg.ReadFile(string.Format(@"D:\recipe\{0}.json", fileName));


                if (Content != null)
                    tmp.Add(fileName, Content);
            }

            CurrentRecipe = Content;
            return Content;
        }
        public static void Set(string fileName, Recipe recipe)
        {
            if (recipe != null)
            {
                ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
                SysCfg.WriteFile(string.Format(@"D:\recipe\{0}.json", fileName), recipe);

                tmp.Remove(fileName);
                tmp.Add(fileName, (Recipe)recipe.MemberwiseClone());
            }
        }
        public static Boolean Delete(string fileName)
        {
            try
            {
                string date = System.DateTime.Now.ToString("yyyyMMdd");
                string time = System.DateTime.Now.ToString("HHmmss");

                //string oldName = "recipe/" + fileName + ".json";
                string oldName = string.Format(@"D:\recipe\{0}.json", fileName);
               // string newName = "recipe/" + fileName + "_" + date + "_" + time + ".del";
                string newName = string.Format(@"D:\recipe\{0}_{1}_{2}.del", fileName, date, time);
                System.IO.File.Move(oldName, newName);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message + " " + e.StackTrace);
                return false;
            }
        }

        public static void updateDBConfig(string recipeName)
        {
            try
            {
                Recipe recipe = Recipe.Get(recipeName);

                try
                {
                    foreach (Node node in NodeManagement.GetList())
                    {
                        switch (node.Name.ToUpper())
                        {
                            case "LOADPORT01":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port1_carrier_type;
                                node.Mode = getPortType(recipe.port1_type);
                                node.Enable = getEnable(recipe.port1_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT02":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port2_carrier_type;
                                node.Mode = getPortType(recipe.port2_type);
                                node.Enable = getEnable(recipe.port2_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT03":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port3_carrier_type;
                                node.Mode = getPortType(recipe.port3_type);
                                node.Enable = getEnable(recipe.port3_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT04":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port4_carrier_type;
                                node.Mode = getPortType(recipe.port4_type);
                                node.Enable = getEnable(recipe.port4_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "ROBOT01":
                                node.WaferSize = recipe.wafer_size;
                                node.RArmActive = recipe.is_use_r_arm;
                                node.LArmActive = recipe.is_use_l_arm;
                                node.DoubleArmActive = recipe.is_use_double_arm;
                                break;
                            case "ALIGNER01":
                                node.WaferSize = recipe.wafer_size;
                                node.Enable = recipe.is_use_aligner1;
                                break;
                            case "ALIGNER02":
                                node.WaferSize = recipe.wafer_size;
                                node.Enable = recipe.is_use_aligner2;
                                break;
                        }
                    }
                    NodeManagement.Save();
                }
                catch (Exception ex)
                {
                    logger.Error("Update load port 資訊失敗! " + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
            }
        }
        private static string getPortType(string port_type)
        {
            string result = "";
            switch (port_type)
            {
                case "L":
                    result = "LD";
                    break;
                case "U":
                    result = "ULD";
                    break;
                case "N":
                    result = "";
                    break;
            }
            return result;
        }
        private static int getEnable(string port_type)
        {
            int result = 0;
            switch (port_type)
            {
                case "L":
                case "U":
                    result = 1;
                    break;
                case "N":
                    result = 0;
                    break;
            }
            return result;
        }
    }
}
