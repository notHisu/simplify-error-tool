using ErrorTool.Config;
using ErrorTool.Models;
using ErrorTool.Services;
using System.Diagnostics;

namespace ErrorTool
{
    public partial class MainForm : Form
    {
        private ElasticSearchService _elasticSearchService;

        public MainForm()
        {
            InitializeComponent();
            InitializeElasticsearch();
            ConfigureDataGridView();
            ConfigureVanDetailsGridView();
        }

        private void InitializeElasticsearch()
        {
            try
            {
                // Create configuration only once
                var config = new ElasticConfig();

                if (config.IsConfigured)
                {
                    _elasticSearchService = new ElasticSearchService(config);
                    MessageBox.Show("Elasticsearch is configured successfully.",
                        "Configuration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Elasticsearch is not configured. Please check your .env file.",
                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnFetchLog.Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                HandleError("Initialization Error", $"Failed to initialize Elasticsearch: {ex.Message}");
                btnFetchLog.Enabled = false;
            }
        }

        private void HandleError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void btnFetchLog_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnFetchLog.Enabled = false;

                var errorLogs = await _elasticSearchService.GetLogAsync();
                var van = await _elasticSearchService.GetVanStuckLogsAsync();

                foreach (var item in van)
                {
                    // Make sure the initial display is collapsed
                    item.ParcelIds = string.Join(", ", item.RawParcelIds.Take(5)) +
                        (item.RawParcelIds.Count > 5 ? $" and {item.RawParcelIds.Count - 5} more" : "");
                }

                dgvErrorLogs.DataSource = errorLogs;
                dgvVan.DataSource = van;
            }
            catch (Exception ex)
            {
                HandleError("Fetch Error", $"Failed to retrieve logs: {ex.Message}");
            }
            finally
            {
                // Re-enable UI elements
                btnFetchLog.Enabled = true;
                // statusLabel.Text = "Ready";
            }
        }

        private void ConfigureDataGridView()
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

            // Configure the grid for variable row heights
            dgvVan.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dgvVan.CellClick += DgvVan_CellClick;
 
        }

        private void ConfigureVanDetailsGridView()
        {
            // Configure the DataGridView for parcel details
            dgvVanDetails.AutoGenerateColumns = false;
            dgvVanDetails.ReadOnly = true;
            
            // Clear existing columns
            dgvVanDetails.Columns.Clear();
            
            // Add parcel ID column
            dgvVanDetails.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ParcelId",
                HeaderText = "Parcel ID",
                Width = 100
            });
            
            // Add action button columns
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
            
            // Register the CellClick event handler
            dgvVanDetails.CellClick += DgvVanDetails_CellClick;
        }

        private void DgvVan_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if it's a valid row
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    // Get the column by its DataPropertyName instead of Name
                    bool isParcelIdsColumn = dgvVan.Columns[e.ColumnIndex].DataPropertyName == "ParcelIds";
                    
                    if (isParcelIdsColumn)
                    {
                        var selectedRow = dgvVan.Rows[e.RowIndex].DataBoundItem as VanStuckViewModel;
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

        private void DisplayParcelDetails(VanStuckViewModel vanStuckModel)
        {
            if (vanStuckModel == null || vanStuckModel.RawParcelIds == null || !vanStuckModel.RawParcelIds.Any())
                return;
            
            // Find the DataGridView in the VanDetails tab
            var dgvVanDetails = tpVanDetails.Controls.OfType<DataGridView>().FirstOrDefault();
            if (dgvVanDetails == null)
                return;
                
            var txtUser = tpVanDetails.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == "txtUser");
            if (txtUser == null)
                return;
            txtUser.Text = vanStuckModel.UserName;

            // Clear existing data
            dgvVanDetails.Rows.Clear();
            
            // Add each parcel ID as a row
            foreach (var parcelId in vanStuckModel.RawParcelIds)
            {
                int rowIndex = dgvVanDetails.Rows.Add();
                dgvVanDetails.Rows[rowIndex].Cells["ParcelId"].Value = parcelId;
            }
        }

        private void DgvVanDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var dgvVanDetails = sender as DataGridView;
            if (dgvVanDetails == null)
                return;

            var parcelId = dgvVanDetails.Rows[e.RowIndex].Cells["ParcelId"].Value;

            // Handle button clicks
            if (e.ColumnIndex == dgvVanDetails.Columns["ViewDataColumn"].Index)
            {
                ViewParcelData(parcelId);
            }
            else if (e.ColumnIndex == dgvVanDetails.Columns["ConfirmColumn"].Index)
            {
                ConfirmParcel(parcelId);
            }
            else if (e.ColumnIndex == dgvVanDetails.Columns["ProcessColumn"].Index)
            {
                ProcessParcelFile(parcelId);
            }
        }

        private void ViewParcelData(object parcelId)
        {
            MessageBox.Show($"View data action for Parcel ID: {parcelId}",
                "View Parcel Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Implement actual view functionality here
            // This could open a new form or dialog showing the parcel data details
        }

        private void ConfirmParcel(object parcelId)
        {
            var result = MessageBox.Show($"Are you sure you want to confirm Parcel ID: {parcelId}?",
                "Confirm Parcel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Implement actual confirmation logic here
                MessageBox.Show($"Parcel ID: {parcelId} confirmed successfully",
                    "Confirmation Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ProcessParcelFile(object parcelId)
        {
            var result = MessageBox.Show($"Are you sure you want to process file for Parcel ID: {parcelId}?",
                "Process Parcel File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Implement actual file processing logic here

                    MessageBox.Show($"Parcel ID: {parcelId} file processed successfully",
                        "Processing Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    HandleError("File Processing Error", $"Failed to process parcel file: {ex.Message}");
                }
            }
        }
    }
}