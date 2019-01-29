using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Repository.Interfaces;
using System;
using System.Linq;
using Digital.Forensic.Models.Models;

namespace Digital.Forensic.Repository.Repositories
{
    public class WorkScheduleRepo : BaseRepository<DigitalForensicEntities>, IWorkScheduleRepo
    {

        private void SaveAll()
        {
            DataContext.SaveChanges();
        }

        public int AddSchedule(WorkScheduleModel model)
        {
            var schedule = new WorkSchedule()
            {
                PublicKey = model.PublicKey,
                NoOfActors = model.NoOfActors,
                GeneratedBy = model.GeneratedBy,
                CreatedDate = model.CreatedDate
            };
            DataContext.WorkSchedules.Add(schedule);
            SaveAll();
            return schedule.WID;
        }

        public bool SaveActors(int WID, string[] IDs)
        {
            foreach (var item in IDs)
            {
                var record = new Actor();
                int ID = int.Parse(item);
                var user = DataContext.Users.Where(x => x.UID == ID).FirstOrDefault();
                if (user != null)
                {
                    record.WID = WID;
                    record.Username = user.Username;
                    record.CreatedDate = DateTime.Now;
                    DataContext.Actors.Add(record);
                }
            }
            return DataContext.SaveChanges() > 0;
        }

        public IQueryable<WorkSchedule> GetAllSchedules()
        {
            return DataContext.WorkSchedules;
        }

        public IQueryable<Actor> GetAllActors()
        {
            return DataContext.Actors;
        }

        public bool AddDE(FilesModel model)
        {
            if (DataContext.DigitalEvidences.Any(x => x.WID == model.WID))
            {
                var file = DataContext.DigitalEvidences.Where(x => x.WID == model.WID).FirstOrDefault();
                file.FileData = model.FileData;
                file.FileDataEnc = model.FileDataEnc;
                file.FileName = model.FileName;
                file.FileExtension = model.FileExtension;
            }
            else
            {
                var record = new DigitalEvidence()
                {
                    WID = model.WID,
                    FileData = model.FileData,
                    FileDataEnc = model.FileDataEnc,
                    FileName = model.FileName,
                    FileExtension = model.FileExtension,
                    FileHashedValue = model.FileHashedValue
                };
                DataContext.DigitalEvidences.Add(record);
            }
            return DataContext.SaveChanges() > 0;
        }

        public bool AddDCoC(FilesModel model)
        {
            if (DataContext.DCoCs.Any(x => x.WID == model.WID))
            {
                var file = DataContext.DCoCs.Where(x => x.WID == model.WID).FirstOrDefault();
                file.DCoCFileData = model.FileData;
                file.DCoCFileDataEnc = model.FileDataEnc;
                file.DCoCFileName = model.FileName;
                file.DCoCFileExtension = model.FileExtension;
            }
            else
            {
                var record = new DCoC()
                {
                    WID = model.WID,
                    DCoCFileData = model.FileData,
                    DCoCFileDataEnc = model.FileDataEnc,
                    DCoCFileName = model.FileName,
                    DCoCFileExtension = model.FileExtension
                };
                DataContext.DCoCs.Add(record);
            }
            return DataContext.SaveChanges() > 0;
        }

        public void RollTransaction(string key)
        {
            var widRecord = DataContext.WorkSchedules.Where(x => x.PublicKey == key).FirstOrDefault();
            if(widRecord != null)
            {
                var actors = DataContext.Actors.Where(x => x.WID == widRecord.WID);
                if(actors != null)
                {
                    foreach(var actor in actors)
                    {
                        DataContext.Actors.Remove(actor);
                    }
                }
                DataContext.WorkSchedules.Remove(widRecord);
                SaveAll();
            }
        }

        public DigitalEvidence GetDE(int id)
        {
            return DataContext.DigitalEvidences.Where(x => x.WID == id).FirstOrDefault();
        }

        public DCoC GetDCoC(int id)
        {
            return DataContext.DCoCs.Where(x => x.WID == id).FirstOrDefault();
        }
    }
}
