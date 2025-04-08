namespace ErrorTool
{
    partial class LoadingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblFetchingData = new Label();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // lblFetchingData
            // 
            lblFetchingData.AutoSize = true;
            lblFetchingData.Location = new Point(12, 9);
            lblFetchingData.Name = "lblFetchingData";
            lblFetchingData.Size = new Size(88, 15);
            lblFetchingData.TabIndex = 0;
            lblFetchingData.Text = "Fetching data...";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 36);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(260, 23);
            progressBar1.TabIndex = 1;
            // 
            // LoadingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 81);
            Controls.Add(progressBar1);
            Controls.Add(lblFetchingData);
            Name = "LoadingForm";
            Text = "LoadingForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblFetchingData;
        private ProgressBar progressBar1;
    }
}