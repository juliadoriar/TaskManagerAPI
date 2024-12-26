using TaskManagerAPI.Models;
using System.ServiceModel;
namespace TaskManagerAPI.Interfaces
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        bool RegisterUser(User user);

        [OperationContract]
        string LoginUser(string email, string password);

        [OperationContract]
        User GetUser(int id);

        [OperationContract]
        IEnumerable<User> ListUsers();

        [OperationContract]
        bool UpdateUser(User user);

        [OperationContract]
        bool DeleteUser(int id);
    }
}
