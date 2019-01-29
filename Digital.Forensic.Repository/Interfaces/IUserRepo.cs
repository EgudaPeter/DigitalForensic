using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Repository.Interfaces
{
    public interface IUserRepo
    {
        IQueryable<User> GetAllUsers();
        bool AuthenticateUser(string username, string password);
        User FindUser(string username);
        string[] RolesOfUser(User user);
        bool ChangePassword(string oldPassword, string newPassword);
        bool ActivateUserAccount(string username);
        bool CheckIfEmailIsUnique(string email);
        bool CheckIfUserNameIsUnique(string username);
        int AddUser(UserModel model);
        int UpdateUser(UserModel model);
        bool VerifyAccount(string ID);
        void DeleteASingleUser(int id);
        void DeleteAGroupOfUsers(string[] IDs);
        void RollBackTransaction(int id);
    }
}
