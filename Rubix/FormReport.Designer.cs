namespace Rubix
{
    partial class FormReport
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
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.LotDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.PRS92DataSet = new Rubix.PRS92DataSet();
            this.LotDataTableAdapter = new Rubix.PRS92DataSetTableAdapters.LotDataTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.LotDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PRS92DataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // reportViewer1
            // 
            this.reportViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "DataSet1";
            reportDataSource1.Value = this.LotDataBindingSource;
            this.reportViewer1.LocalReport.DataSources.Add(reportDataSource1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "Rubix.Report1.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(0, 0);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.Size = new System.Drawing.Size(1062, 805);
            this.reportViewer1.TabIndex = 0;
            this.reportViewer1.Load += new System.EventHandler(this.reportViewer1_Load_1);
            // 
            // PRS92DataSet
            // 
            this.PRS92DataSet.DataSetName = "PRS92DataSet";
            this.PRS92DataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // LotDataTableAdapter
            // 
            this.LotDataTableAdapter.ClearBeforeFill = true;
            // 
            // FormReport
            // 
            this.ClientSize = new System.Drawing.Size(1062, 805);
            this.Controls.Add(this.reportViewer1);
            this.Name = "FormReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.LotDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PRS92DataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.BindingSource LotDataBindingSource;
        private PRS92DataSet PRS92DataSet;
        private PRS92DataSetTableAdapters.LotDataTableAdapter LotDataTableAdapter;
     
        //private DataSet1TableAdapters.LotDataTableAdapter LotDataTableAdapter;

       
       
        
    }
}