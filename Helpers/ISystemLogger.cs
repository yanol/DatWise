using System.Threading.Tasks;

namespace SafetyCompliance.Helpers
{
    public interface ISystemLogger
    {
        Task LogAsync(int userId, string action, string details = null);
        Task LogErrorAsync(int userId, string action, string errorMessage);
    }
}