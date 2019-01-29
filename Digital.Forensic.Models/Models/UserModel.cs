using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Models.Models
{
    public class UserModel
    {
        public int UID { get; set; }
        public string Surname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CapturedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> isActivated { get; set; }
        public Nullable<bool> HasChangedPassword { get; set; }
        public string UserRole { get; set; }
        public string Email { get; set; }
        public Nullable<System.Guid> ActivationCode { get; set; }
    }
}
