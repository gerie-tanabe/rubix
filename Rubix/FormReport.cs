using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Data.SqlClient;

namespace Rubix
{
    public partial class FormReport : Form
    {

        public string _CadSurveyNo = string.Empty;
        public string connectionString = string.Empty;

        public FormReport(string CadSurveyNo)
        {
            _CadSurveyNo = CadSurveyNo;
            InitializeComponent();
        }

        private void FormReport_Load(object sender, EventArgs e)
        {
// TODO: This line of code loads data into the 'DataSet1.LotData' table. You can move, or remove it, as needed.
           
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {

        }

       

        private void reportViewer1_Load_1(object sender, EventArgs e)
        {
            string server = Registry.GetValue(@"HKEY_CURRENT_USER\Software\VB and VBA Program Settings\GNIS-X\SQLServer", "SQLServer", String.Empty).ToString();
            connectionString = "server=" + server + ";Database=prs92;user id=prs92;password=prs92";
            

            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", LoadLots(_CadSurveyNo)));
            
            reportViewer1.LocalReport.Refresh();
            reportViewer1.RefreshReport();
            
        }

        private DataTable LoadLots(string CADSurveyNo)
        {
            
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM  LOTDATA  WHERE CADSURVEYNO=@CADSurveyNo ORDER BY CASE when PATINDEX('%[^0-9]%',LOTNO) = 0 THEN  LOTNO ELSE cast(Left(LOTNO,PATINDEX('%[^0-9]%',LOTNO)-1) as int) END", cnn))
                {
                    cmd.Parameters.AddWithValue("@CADSurveyNo", CADSurveyNo);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    cnn.Close();
                    return dt;
                }
            }
        }

       
    }
}
