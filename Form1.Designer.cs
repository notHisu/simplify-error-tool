namespace ErrorTool
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnFetchLog = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            tcLogs = new TabControl();
            tpLogs = new TabPage();
            dgvErrorLogs = new DataGridView();
            tpVan = new TabPage();
            dgvVan = new DataGridView();
            tpVanDetails = new TabPage();
            txtConnection = new TextBox();
            label1 = new Label();
            dgvVanDetails = new DataGridView();
            txtMailbox = new TextBox();
            txtUser = new TextBox();
            lbMailbox = new Label();
            lbUser = new Label();
            menuStrip1.SuspendLayout();
            tcLogs.SuspendLayout();
            tpLogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvErrorLogs).BeginInit();
            tpVan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVan).BeginInit();
            tpVanDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVanDetails).BeginInit();
            SuspendLayout();
            // 
            // btnFetchLog
            // 
            btnFetchLog.Location = new Point(12, 27);
            btnFetchLog.Name = "btnFetchLog";
            btnFetchLog.Size = new Size(75, 23);
            btnFetchLog.TabIndex = 0;
            btnFetchLog.Text = "Fetch Log";
            btnFetchLog.UseVisualStyleBackColor = true;
            btnFetchLog.Click += btnFetchLog_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(801, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "About";
            // 
            // tcLogs
            // 
            tcLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tcLogs.Controls.Add(tpLogs);
            tcLogs.Controls.Add(tpVan);
            tcLogs.Controls.Add(tpVanDetails);
            tcLogs.Location = new Point(0, 56);
            tcLogs.Name = "tcLogs";
            tcLogs.SelectedIndex = 0;
            tcLogs.Size = new Size(801, 390);
            tcLogs.TabIndex = 3;
            // 
            // tpLogs
            // 
            tpLogs.Controls.Add(dgvErrorLogs);
            tpLogs.Location = new Point(4, 24);
            tpLogs.Name = "tpLogs";
            tpLogs.Padding = new Padding(3);
            tpLogs.Size = new Size(793, 362);
            tpLogs.TabIndex = 0;
            tpLogs.Text = "ErrorLogs";
            tpLogs.UseVisualStyleBackColor = true;
            // 
            // dgvErrorLogs
            // 
            dgvErrorLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvErrorLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvErrorLogs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvErrorLogs.Location = new Point(3, 3);
            dgvErrorLogs.Name = "dgvErrorLogs";
            dgvErrorLogs.Size = new Size(787, 356);
            dgvErrorLogs.TabIndex = 0;
            // 
            // tpVan
            // 
            tpVan.Controls.Add(dgvVan);
            tpVan.Location = new Point(4, 24);
            tpVan.Name = "tpVan";
            tpVan.Padding = new Padding(3);
            tpVan.Size = new Size(793, 362);
            tpVan.TabIndex = 1;
            tpVan.Text = "VanStuck";
            tpVan.UseVisualStyleBackColor = true;
            // 
            // dgvVan
            // 
            dgvVan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVan.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVan.Dock = DockStyle.Fill;
            dgvVan.Location = new Point(3, 3);
            dgvVan.Name = "dgvVan";
            dgvVan.Size = new Size(787, 356);
            dgvVan.TabIndex = 0;
            // 
            // tpVanDetails
            // 
            tpVanDetails.Controls.Add(txtConnection);
            tpVanDetails.Controls.Add(label1);
            tpVanDetails.Controls.Add(dgvVanDetails);
            tpVanDetails.Controls.Add(txtMailbox);
            tpVanDetails.Controls.Add(txtUser);
            tpVanDetails.Controls.Add(lbMailbox);
            tpVanDetails.Controls.Add(lbUser);
            tpVanDetails.Location = new Point(4, 24);
            tpVanDetails.Name = "tpVanDetails";
            tpVanDetails.Padding = new Padding(3);
            tpVanDetails.Size = new Size(793, 362);
            tpVanDetails.TabIndex = 2;
            tpVanDetails.Text = "VanDetails";
            tpVanDetails.UseVisualStyleBackColor = true;
            // 
            // txtConnection
            // 
            txtConnection.Location = new Point(285, 15);
            txtConnection.Name = "txtConnection";
            txtConnection.ReadOnly = true;
            txtConnection.Size = new Size(139, 23);
            txtConnection.TabIndex = 6;
            txtConnection.Text = "Placeholder";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(207, 18);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 5;
            label1.Text = "Connection:";
            // 
            // dgvVanDetails
            // 
            dgvVanDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvVanDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVanDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVanDetails.Location = new Point(3, 93);
            dgvVanDetails.Name = "dgvVanDetails";
            dgvVanDetails.Size = new Size(787, 266);
            dgvVanDetails.TabIndex = 4;
            // 
            // txtMailbox
            // 
            txtMailbox.Location = new Point(66, 57);
            txtMailbox.Name = "txtMailbox";
            txtMailbox.Size = new Size(100, 23);
            txtMailbox.TabIndex = 3;
            txtMailbox.Text = "Placeholder";
            // 
            // txtUser
            // 
            txtUser.Location = new Point(66, 15);
            txtUser.Name = "txtUser";
            txtUser.ReadOnly = true;
            txtUser.Size = new Size(100, 23);
            txtUser.TabIndex = 2;
            txtUser.Text = "Placeholder";
            // 
            // lbMailbox
            // 
            lbMailbox.AutoSize = true;
            lbMailbox.Location = new Point(8, 60);
            lbMailbox.Name = "lbMailbox";
            lbMailbox.Size = new Size(52, 15);
            lbMailbox.TabIndex = 1;
            lbMailbox.Text = "Mailbox:";
            // 
            // lbUser
            // 
            lbUser.AutoSize = true;
            lbUser.Location = new Point(8, 18);
            lbUser.Name = "lbUser";
            lbUser.Size = new Size(33, 15);
            lbUser.TabIndex = 0;
            lbUser.Text = "User:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(801, 446);
            Controls.Add(tcLogs);
            Controls.Add(btnFetchLog);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Error Tool";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tcLogs.ResumeLayout(false);
            tpLogs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvErrorLogs).EndInit();
            tpVan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVan).EndInit();
            tpVanDetails.ResumeLayout(false);
            tpVanDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVanDetails).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnFetchLog;
        private DataGridView dgvLogs;
        private DataGridView dgvVan;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private TabControl tcLogs;
        private TabPage tpLogs;
        private TabPage tpVan;
        private DataGridView dgvErrorLogs;
        private TabPage tpVanDetails;
        private Label lbUser;
        private TextBox txtUser;
        private Label lbMailbox;
        private DataGridView dgvVanDetails;
        private TextBox txtMailbox;
        private TextBox txtConnection;
        private Label label1;
    }
}
