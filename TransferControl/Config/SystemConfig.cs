using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Config
{
    public class SystemConfig
    {
        private static SystemConfig Content;
        public string OCR1ImgSourcePath { get; set; }
        public string OCR1ImgToJpgPath { get; set; }
        public string OCR2ImgSourcePath { get; set; }
        public string OCR2ImgToJpgPath { get; set; }
        public string OCR1ExePath { get; set; }
        public string OCR2ExePath { get; set; }
        public string EFEMInterfaceConn { get; set; }
        public int EFEMInterfaceConn_Port { get; set; }
        public bool SaftyCheckByPass { get; set; }
        public string AdminPassword { get; set; }
        public string CurrentRecipe { get; set; }
        public string FoupTxfLogPath { get; set; }
        public string NoticeInitFin { get; set; }
        public string NoticeProcFin { get; set; }
        public bool DummyMappingData { get; set; }
        public string TaskFlow { get; set; }
        public string FakeDataP1 { get; set; }
        public string FakeDataP2 { get; set; }
        public string FakeDataP3 { get; set; }
        public string FakeDataP4 { get; set; }
        public string Language { get; set; }

        public bool OfflineMode { get; set; } = false;

        public bool E84StartAutoMode { get; set; } = false;

        public string User { get; set; } = "Sanwa";

        public string ConfigVer { get; set; } = "Undefined";

        public int LogKeepDays { get; set; } = 30;


        public static SystemConfig Get()
        {
            if(Content == null)
            {
                ConfigTool<SystemConfig> SysCfg = new ConfigTool<SystemConfig>();
                Content = SysCfg.ReadFile("config/SystemConfig.json");
            }
            return Content;
        }
        public void Save()
        {
            ConfigTool<SystemConfig> SysCfg = new ConfigTool<SystemConfig>();
            SysCfg.WriteFile("config/SystemConfig.json", this);
        }
    }
}
