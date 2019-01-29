using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digital.Forensic.Models.Models;

namespace Digital.Forensic.Repository.Repositories
{
    public class YourRepo : BaseRepository<DigitalForensicEntities>, IYourRepo
    {
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
            return DataContext.SaveChanges() > 0;
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
            return DataContext.SaveChanges() > 0;
        }

        private void SaveAll()
        {
            DataContext.SaveChanges();
        }

    }
}
