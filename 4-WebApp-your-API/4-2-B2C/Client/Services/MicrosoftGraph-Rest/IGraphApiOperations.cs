using System.Threading.Tasks;

namespace TodoListClient.Services
{
    public interface IGraphApiOperations
    {
        Task<dynamic> GetUserInformation(string accessToken);
        Task<string> GetPhotoAsBase64Async(string accessToken);
    }
}