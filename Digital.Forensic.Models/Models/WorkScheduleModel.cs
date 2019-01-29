using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Models.Models
{
    public class WorkScheduleModel
    {
        public int WID { get; set; }
        public string PublicKey { get; set; }
        public int NoOfActors { get; set; }
        public string GeneratedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
