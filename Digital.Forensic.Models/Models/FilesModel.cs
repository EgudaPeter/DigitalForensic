using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Models.Models
{
    public class FilesModel
    {
        public int FileID { get; set; }
        public int WID { get; set; }
        public string FileData { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileHashedValue { get; set; }
        public string DoCFileName { get; set; }
        public string DoCFileExtension { get; set; }
        public string DoCFileData { get; set; }
        public string FileDataEnc { get; set; }
        public string DoCFileDataEnc { get; set; }
    }
}
