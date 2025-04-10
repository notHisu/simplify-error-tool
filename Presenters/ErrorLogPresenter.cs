using ErrorTool.Interfaces;
using ErrorTool.Models;

namespace ErrorTool.Presenters
{
    public class ErrorLogPresenter
    {
        private readonly IErrorLogView _view;
        private readonly IElasticSearchService _elasticSearchService;
        
        public ErrorLogPresenter(IErrorLogView view, IElasticSearchService elasticSearchService)
        {
            _view = view;
            _elasticSearchService = elasticSearchService;
        }
        
        public async Task FetchErrorLogsAsync()
        {
            try
            {
                _view.SetLoadingState(true);
                var logs = await _elasticSearchService.GetLogAsync();
                _view.DisplayErrorLogs(logs);
            }
            catch (Exception ex)
            {
                _view.ShowErrorMessage("Fetch Error", $"Failed to retrieve logs: {ex.Message}");
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }
    }
    
    public class VanStuckPresenter
    {
        private readonly IVanStuckView _view;
        private readonly IElasticSearchService _elasticSearchService;
        
        public VanStuckPresenter(IVanStuckView view, IElasticSearchService elasticSearchService)
        {
            _view = view;
            _elasticSearchService = elasticSearchService;
        }
        
        public async Task FetchVanStuckItemsAsync()
        {
            try
            {
                var vanStuckItems = await _elasticSearchService.GetVanStuckLogsAsync();
                
                // Format the parcel IDs before displaying
                foreach (var item in vanStuckItems)
                {
                    item.ParcelIds = string.Join(", ", item.RawParcelIds.Take(5)) +
                        (item.RawParcelIds.Count > 5 ? $" and {item.RawParcelIds.Count - 5} more" : "");
                }
                
                _view.DisplayVanStuckItems(vanStuckItems);
            }
            catch (Exception ex)
            {
                _view.ShowErrorMessage("Fetch Error", $"Failed to retrieve van stuck items: {ex.Message}");
            }
        }
    }
    
    public class VanDetailsPresenter
    {
        private readonly IVanDetailsView _view;
        private readonly IParcelService _parcelService;
        
        public VanDetailsPresenter(IVanDetailsView view, IParcelService parcelService)
        {
            _view = view;
            _parcelService = parcelService;
        }
        
        public async Task ViewParcelDataAsync(long parcelId)
        {
            try
            {
                string? content = await _parcelService.GetParcelContent(
                    parcelId,
                    _view.UserName,
                    _view.ConnectionName);

                //string content = "Sample content for parcel ID: " + parcelId; // Placeholder for actual content retrieval

                _view.ShowParcelContent(content);
            }
            catch (Exception ex)
            {
                _view.ShowMessage("View Parcel Error", 
                    $"Failed to retrieve parcel data: {ex.Message}", 
                    MessageType.Error);
            }
        }
        
        public async Task ConfirmParcelAsync(long parcelId)
        {
            try
            {
                //var result = await _parcelService.ConfirmParcel(
                //    parcelId,
                //    _view.UserName,
                //    _view.ConnectionName);

                var result = "Sample confirmation result"; // Placeholder for actual confirmation result

                _view.ShowMessage("Confirmation Success", 
                    $"Parcel ID: {parcelId} confirmed successfully\nResponse: {result}", 
                    MessageType.Information);
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Confirmation Error", ex.Message, MessageType.Error);
            }
        }
        
        public async Task ProcessParcelAsync(long parcelId)
        {
            try
            {
                if (string.IsNullOrEmpty(_view.MailboxId))
                {
                    _view.ShowMessage("Missing Information", 
                        "Mailbox ID is missing. Cannot process the parcel.", 
                        MessageType.Error);
                    return;
                }

                //var result = await _parcelService.ProcessParcel(
                //    parcelId,
                //    _view.MailboxId);

                var result = "Sample processing result"; // Placeholder for actual processing result

                _view.ShowMessage("Processing Success", 
                    $"Parcel ID: {parcelId} file processed successfully\nResponse: {result}", 
                    MessageType.Information);
            }
            catch (Exception ex)
            {
                _view.ShowMessage("File Processing Error", ex.Message, MessageType.Error);
            }
        }
    }
}