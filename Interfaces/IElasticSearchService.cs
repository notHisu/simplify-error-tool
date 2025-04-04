using ErrorTool.Models;

namespace ErrorTool.Interfaces
{
    public interface IElasticSearchService
    {
        Task<List<LogEntry>> GetLogAsync(DateTime? startDate = null, DateTime? endDate = null, int size = 500);
        Task<List<VanStuckViewModel>> GetVanStuckLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int size = 500);
    }
}