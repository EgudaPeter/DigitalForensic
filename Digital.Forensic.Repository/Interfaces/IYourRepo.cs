using Digital.Forensic.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Repository.Interfaces
{
    public interface IYourRepo
    {
        bool AddDE(FilesModel model);
        bool AddDCoC(FilesModel model);
    }
}
