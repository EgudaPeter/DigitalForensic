using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Models.Models
{
    public class ActorsModel
    {
        public int AID { get; set; }
        public int WID { get; set; }
        public string Username { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
