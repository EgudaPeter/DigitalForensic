using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Models.Models;
using Digital.Forensic.Repository.Interfaces;

namespace Digital.Forensic.Repository.Repositories
{
    public class UserRepo : BaseRepository<DigitalForensicEntities>, IUserRepo
    {
        private void SaveAll()
        {
            DataContext.SaveChanges();
        }
        public bool ActivateUserAccount(string username)
        {
            var user = DataContext.Users.Where(x => x.Username == username).FirstOrDefault();
            if(user != null)
            {
                user.isActivated = true;
                SaveAll();
                return true;
            }
            return false;
        }

        public bool AuthenticateUser(string username, string password)
        {
            if(DataContext.Users.Any(x=>x.Username == username && x.Password == password))
            {
                return true;
            }
            return false;
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            var user = DataContext.Users.Where(x => x.Password == oldPassword).FirstOrDefault();
            if(user != null)
            {
                user.Password = newPassword;
                user.HasChangedPassword = true;
                SaveAll();
                return true;
            }
            return false;
        }

        public User FindUser(string username)
        {
            return DataContext.Users.Where(x => x.Username == username).FirstOrDefault();
        }

        public string[] RolesOfUser(User user)
        {
            string[] roles = { user.UserRole };
            return roles;
        }

        public IQueryable<User> GetAllUsers()
        {
            return DataContext.Users;
        }

        public bool CheckIfEmailIsUnique(string email)
        {
           if(DataContext.Users.Any(x=>x.Email == email))
            {
                return false;
            }
            return true;
        }

        public bool CheckIfUserNameIsUnique(string username)
        {
            if (DataContext.Users.Any(x => x.Username == username))
            {
                return false;
            }
            return true;
        }

        public int AddUser(UserModel model)
        {
            var record = new User()
            {
                Surname = model.Surname,
                Firstname = model.Firstname,
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
                CapturedBy = model.CapturedBy,
                CreatedDate = model.CreatedDate,
                isActivated = model.isActivated,
                HasChangedPassword = model.HasChangedPassword,
                UserRole = model.UserRole,
                ActivationCode = model.ActivationCode
            };
            DataContext.Users.Add(record);
            SaveAll();
            return record.UID;
        }

        public int UpdateUser(UserModel model)
        {
            var recordToUpdate = DataContext.Users.Find(model.UID);
            recordToUpdate.Surname = model.Surname;
            recordToUpdate.Firstname = model.Firstname;
            recordToUpdate.Username = model.Username;
            recordToUpdate.Password = model.Password;
            recordToUpdate.Email = model.Email;
            recordToUpdate.isActivated = model.isActivated;
            recordToUpdate.HasChangedPassword = model.HasChangedPassword;
            recordToUpdate.ActivationCode = model.ActivationCode;
            recordToUpdate.CapturedBy = model.CapturedBy;
            SaveAll();
            return recordToUpdate.UID;
        }

        public bool VerifyAccount(string ID)
        {
            DataContext.Configuration.ValidateOnSaveEnabled = false;
            var account = DataContext.Users.Where(x => x.ActivationCode == new Guid(ID)).FirstOrDefault();
            if (account != null)
            {
                account.isActivated = true;
                SaveAll();
                return true;
            }
            return false;
        }

        public void DeleteASingleUser(int id)
        {
            var record = DataContext.Users.Find(id);
            DataContext.Users.Remove(record);
            SaveAll();
        }

        public void DeleteAGroupOfUsers(string[] IDs)
        {
            foreach(var item in IDs)
            {
                int ID = int.Parse(item);
                var record = DataContext.Users.Find(ID);
                DataContext.Users.Remove(record);
            }
            SaveAll();
        }

        public void RollBackTransaction(int id)
        {
            var user = DataContext.Users.Find(id);
            if(user != null)
            {
                DataContext.Users.Remove(user);
            }
        }
    }
}
