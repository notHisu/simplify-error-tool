using ErrorTool.Models;

namespace ErrorTool.Interfaces
{
    public interface IErrorLogView
    {
        void DisplayErrorLogs(List<LogEntry> logs);
        void ShowErrorMessage(string title, string message);
        void SetLoadingState(bool isLoading);
    }
    
    public interface IVanStuckView
    {
        void DisplayVanStuckItems(List<VanStuckViewModel> items);
        void ShowErrorMessage(string title, string message);
    }
    
    public interface IVanDetailsView
    {
        void DisplayParcelDetails(VanStuckViewModel model);
        void ShowParcelContent(string content);
        void ShowMessage(string title, string message, MessageType type);
        
        string UserName { get; }
        string ConnectionName { get; }
        string MailboxId { get; }
    }
    
    public enum MessageType
    {
        Information,
        Error,
        Warning
    }
}