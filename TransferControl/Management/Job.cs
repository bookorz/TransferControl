using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class Job
    {
        ILog logger = LogManager.GetLogger(typeof(Job));
        public string Slot { get; set; }
        public string MappingValue { get; set; }
        public string Uid { get; set; }
        public string Host_Job_Id { get; set; }
        public string Host_Lot_Id { get; set; }
        public List<OCRInfo> OcrCodeList { get; set; }
        public bool InProcess { get; set; }
        public bool NeedProcess { get; set; }
        public bool AbortProcess { get; set; }
        public string LastNode { get; set; }
        public string LastSlot { get; set; }
        public bool ProcessFlag { get; set; }
        public bool AlignerFlag { get; set; }
        public bool OCRFlag { get; set; }
        public string OCRImgPath { get; set; }
        public string OCRScore { get; set; }
        public string OCRResult { get; set; }
        public bool OCRPass { get; set; }
        public string OCR_M12_ImgPath { get; set; }
        public string OCR_M12_Score { get; set; }
        public string OCR_M12_Result { get; set; }
        public bool OCR_M12_Pass { get; set; }
        public string OCR_T7_ImgPath { get; set; }
        public string OCR_T7_Score { get; set; }
        public string OCR_T7_Result { get; set; }
        public bool OCR_T7_Pass { get; set; }
        public string Position { get; set; }           
        public string RecipeID { get; set; }
        public bool ErrPosition { get; set; }
        public bool MapFlag { get; set; }
        public int Offset { get; set; }
       
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string WaferSize { get; set; }
        
        public MapStatus Status { get; set; }
        public enum MapStatus
        {
            Normal,
            Empty,
            Crossed,
            Undefined,
            Double
        }

        public Job()
        {
           
          
            Host_Lot_Id = "";
          
            OCRImgPath = "";
            OCR_M12_ImgPath = "";
            OCR_T7_ImgPath = "";
            RecipeID = "";
            ProcessFlag = false;
            MapFlag = false;
           
            AlignerFlag = false;
            NeedProcess = false;
            AbortProcess = false;
            OCRFlag = false;
            InProcess = false;
           
            OcrCodeList = new List<OCRInfo>();
           
           
            WaferSize = "";
            MappingValue = "";
        }

       
    }
}
