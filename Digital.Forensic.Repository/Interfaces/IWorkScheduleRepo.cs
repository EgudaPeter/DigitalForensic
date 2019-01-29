using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Repository.Interfaces
{
    public interface IWorkScheduleRepo
    {
        int AddSchedule(WorkScheduleModel model);
        bool SaveActors(int WID, string[] IDs);
        IQueryable<WorkSchedule> GetAllSchedules();
        IQueryable<Actor> GetAllActors();
        bool AddDE(FilesModel model);
        bool AddDCoC(FilesModel model);
        void RollTransaction(string key);
        DigitalEvidence GetDE(int id);
        DCoC GetDCoC(int id);
        //int AddFile(FilesModel model);
        //WorkSchedule_Files GetFile(int id);
    }
}
