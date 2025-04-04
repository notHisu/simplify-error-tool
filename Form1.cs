using ErrorTool.Config;
using ErrorTool.Interfaces;
using ErrorTool.Models;
using ErrorTool.Services;
using System.Diagnostics;

namespace ErrorTool
{
    public partial class MainForm : Form
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IParcelService _parcelService;

        public MainForm(IElasticSearchService elasticSearchService, IParcelService parcelService)
        {
            InitializeComponent();

            var config = new ElasticConfig();
            var dbConfig = new DatabaseConfig();

            _elasticSearchService = elasticSearchService ?? throw new ArgumentNullException(nameof(elasticSearchService), "ElasticSearchService cannot be null.");
            _parcelService = parcelService ?? throw new ArgumentNullException(nameof(parcelService), "ParcelService cannot be null.");

            ConfigureErrorDataGridView();
            ConfigureVanDataGridView();
            ConfigureVanDetailsGridView();
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

            // Configure the grid for variable row heights
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

            txtConnection.Text = vanStuckModel.ConnectionName;

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
                ViewParcelDataAsync(parcelId);
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

        private async Task ViewParcelDataAsync(object parcelId)
        {
            try
            {
                // Get the parent row to find the username and connection
                var selectedRow = GetParentRowForParcelId(Convert.ToInt64(parcelId));
                if (selectedRow == null)
                {
                    MessageBox.Show("Could not find parent information for this parcel.");
                    return;
                }

                // Show loading indicator
                using (var loadingForm = new Form())
                {
                    loadingForm.Text = "Loading";
                    loadingForm.Size = new Size(300, 120);
                    loadingForm.StartPosition = FormStartPosition.CenterParent;

                    var label = new Label { Text = "Fetching parcel data...", Location = new Point(50, 20) };
                    loadingForm.Controls.Add(label);

                    var progress = new ProgressBar
                    {
                        Style = ProgressBarStyle.Marquee,
                        Location = new Point(50, 50),
                        Size = new Size(200, 20)
                    };
                    loadingForm.Controls.Add(progress);

                    loadingForm.Show(this);

                    try
                    {
                        // Use the service
                        string content = await _parcelService.GetParcelContent(
                            Convert.ToInt64(parcelId),
                            selectedRow.UserName,
                            selectedRow.ConnectionName);

                        loadingForm.Close();

                        // Display the content
                        DisplayParcelContent(content);
                    }
                    catch
                    {
                        loadingForm.Close();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("View Parcel Error", $"Failed to retrieve parcel data: {ex.Message}");
            }
        }

        // Helper method to find the parent row
        private VanStuckViewModel GetParentRowForParcelId(long parcelId)
        {
            if (dgvVan.DataSource is List<VanStuckViewModel> vanData)
            {
                return vanData.FirstOrDefault(row => row.RawParcelIds.Contains(parcelId));
            }
            return null;
        }

        // Helper method to display content
        private void DisplayParcelContent(string content)
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