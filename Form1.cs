using ErrorTool.Config;
using ErrorTool.Interfaces;
using ErrorTool.Models;
using ErrorTool.Presenters;
using ErrorTool.Services;
using System.Diagnostics;

namespace ErrorTool
{
    public partial class MainForm : Form, IErrorLogView, IVanStuckView, IVanDetailsView
    {
        private readonly ErrorLogPresenter _errorLogPresenter;
        private readonly VanStuckPresenter _vanStuckPresenter;
        private readonly VanDetailsPresenter _vanDetailsPresenter;
        
        private List<VanStuckViewModel> _vanStuckItems = new List<VanStuckViewModel>();
        private VanStuckViewModel? _selectedVanStuckItem;

        public MainForm(IElasticSearchService elasticSearchService, IParcelService parcelService)
        {
            InitializeComponent();
            
            // Create presenters
            _errorLogPresenter = new ErrorLogPresenter(this, elasticSearchService);
            _vanStuckPresenter = new VanStuckPresenter(this, elasticSearchService);
            _vanDetailsPresenter = new VanDetailsPresenter(this, parcelService);
            
            ConfigureErrorDataGridView();
            ConfigureVanDataGridView();
            ConfigureVanDetailsGridView();
        }
        
        // IErrorLogView implementation
        public void DisplayErrorLogs(List<LogEntry> logs)
        {
            dgvErrorLogs.DataSource = logs;
        }
        
        public void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        public void SetLoadingState(bool isLoading)
        {
            btnFetchLog.Enabled = !isLoading;
        }
        
        // IVanStuckView implementation
        public void DisplayVanStuckItems(List<VanStuckViewModel> items)
        {
            _vanStuckItems = items;
            dgvVan.DataSource = items;
        }
        
        // IVanDetailsView implementation
        public void DisplayParcelDetails(VanStuckViewModel model)
        {
            _selectedVanStuckItem = model;
            txtUser.Text = model.UserName;
            txtConnection.Text = model.ConnectionName;
            txtMailbox.Text = model.MailboxId;
            
            dgvVanDetails.Rows.Clear();
            foreach (var parcelId in model.RawParcelIds)
            {
                int rowIndex = dgvVanDetails.Rows.Add();
                dgvVanDetails.Rows[rowIndex].Cells["ParcelId"].Value = parcelId;
            }
        }
        
        public void ShowParcelContent(string content)
        {
            var contentForm = new Form
            {
                Text = "Parcel Content",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Text = content
            };

            contentForm.Controls.Add(textBox);
            contentForm.ShowDialog();
        }
        
        public void ShowMessage(string title, string message, MessageType type)
        {
            MessageBoxIcon icon = MessageBoxIcon.Information;
            switch (type)
            {
                case MessageType.Error:
                    icon = MessageBoxIcon.Error;
                    break;
                case MessageType.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
            }
            
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }
        
        public string UserName => _selectedVanStuckItem?.UserName ?? string.Empty;
        public string ConnectionName => _selectedVanStuckItem?.ConnectionName ?? string.Empty;
        public string MailboxId => _selectedVanStuckItem?.MailboxId ?? string.Empty;
        
        // Event handlers
        private async void btnFetchLog_Click(object sender, EventArgs e)
        {
            await _errorLogPresenter.FetchErrorLogsAsync();
            await _vanStuckPresenter.FetchVanStuckItemsAsync();
        }
        
        private void DgvVan_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    bool isParcelIdsColumn = dgvVan.Columns[e.ColumnIndex].DataPropertyName == "ParcelIds";
                    
                    if (isParcelIdsColumn && _vanStuckItems.Count > e.RowIndex)
                    {
                        var selectedRow = _vanStuckItems[e.RowIndex];
                        if (selectedRow != null && selectedRow.RawParcelIds?.Count > 0)
                        {
                            DisplayParcelDetails(selectedRow);
                            tcLogs.SelectedTab = tpVanDetails;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DgvVan_CellClick: {ex.Message}");
            }
        }
        
        private async void DgvVanDetails_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
                
            var dgvVanDetails = sender as DataGridView;
            if (dgvVanDetails == null)
                return;
                
            var parcelIdObj = dgvVanDetails.Rows[e.RowIndex].Cells["ParcelId"].Value;
            if (parcelIdObj == null)
                return;
                
            long parcelId = Convert.ToInt64(parcelIdObj);
            
            // Handle button clicks
            if (e.ColumnIndex == dgvVanDetails.Columns["ViewDataColumn"].Index)
            {
                await _vanDetailsPresenter.ViewParcelDataAsync(parcelId);
            }
            else if (e.ColumnIndex == dgvVanDetails.Columns["ConfirmColumn"].Index)
            {
                var result = MessageBox.Show($"Are you sure you want to confirm Parcel ID: {parcelId}?",
                    "Confirm Parcel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    await _vanDetailsPresenter.ConfirmParcelAsync(parcelId);
                }
            }
            else if (e.ColumnIndex == dgvVanDetails.Columns["ProcessColumn"].Index)
            {
                var result = MessageBox.Show($"Are you sure you want to process file for Parcel ID: {parcelId}?",
                    "Process Parcel File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    await _vanDetailsPresenter.ProcessParcelAsync(parcelId);
                }
            }
        }
        
        private void ConfigureVanDataGridView()
        {
            dgvVan.AutoGenerateColumns = false;
            dgvVan.ReadOnly = true;

            dgvVan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Timestamp",
                HeaderText = "Time",
                DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm:ss" },
                Width = 150
            });

            dgvVan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserName",
                HeaderText = "User Name",
                Width = 150
            });

            dgvVan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ConnectionName",
                HeaderText = "Connection",
                Width = 150
            });

            dgvVan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MailboxId",
                HeaderText = "Mailbox ID",
                Width = 100
            });

            dgvVan.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ParcelCount",
                HeaderText = "Parcel Count",
                Width = 100
            });

            var parcelIdsColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ParcelIds",
                HeaderText = "Parcel IDs",
                Width = 200,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True
                }
            };

            dgvVan.Columns.Add(parcelIdsColumn);

            dgvVan.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dgvVan.CellClick += DgvVan_CellClick;

        }

        private void ConfigureErrorDataGridView()
        {
            dgvErrorLogs.AutoGenerateColumns = false;
            dgvErrorLogs.ReadOnly = true;

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Timestamp",
                HeaderText = "Timestamp",
                DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm:ss" },
                Width = 150
            });

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LogLevel",
                HeaderText = "Log Level",
                Width = 50
            });

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Message",
                HeaderText = "Message",
                Width = 300
            });

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Sender",
                HeaderText = "Sender",
                Width = 150
            });

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InternalTesting",
                HeaderText = "Internal Testing",
                Width = 100
            });

            dgvErrorLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ErrorMessage",
                HeaderText = "Error Message",
                Width = 200
            });

        }

        private void ConfigureVanDetailsGridView()
        {
            dgvVanDetails.AutoGenerateColumns = false;
            dgvVanDetails.ReadOnly = true;

            dgvVanDetails.Columns.Clear();

            dgvVanDetails.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ParcelId",
                HeaderText = "Parcel ID",
                Width = 100
            });

            dgvVanDetails.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "ViewDataColumn",
                HeaderText = "View Data",
                Text = "View",
                UseColumnTextForButtonValue = true,
                Width = 80
            });

            dgvVanDetails.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "ConfirmColumn",
                HeaderText = "Confirm",
                Text = "Confirm",
                UseColumnTextForButtonValue = true,
                Width = 80
            });

            dgvVanDetails.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "ProcessColumn",
                HeaderText = "Process File",
                Text = "Process",
                UseColumnTextForButtonValue = true,
                Width = 80
            });

            dgvVanDetails.CellClick += DgvVanDetails_CellClick;
        }
    }
}