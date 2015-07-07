using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Data.SqlClient;

namespace Rubix
{
    public partial class FormMain : Form
    {

        string server = string.Empty;
        string connectionString = string.Empty;
        bool cancelme;
        string _CadSurveyNo=string.Empty;

        public struct PTM
        {
            public double northing, easting;

            public PTM(double p1, double p2)
            {
                northing = p1;
                easting = p2;
            }
        }

        public struct Param
        {
            public double a, b,ce,cn;

            public Param(double A, double B,double CE,double CN)
            {
                a = A;
                b = B;
                ce = CE;
                cn = CN;
            }
        }
        

        public FormMain(string CadSurveyNo)
        {
            _CadSurveyNo = CadSurveyNo;
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            server = Registry.GetValue(@"HKEY_CURRENT_USER\Software\VB and VBA Program Settings\GNIS-X\SQLServer", "SQLServer", String.Empty).ToString();
            connectionString="server=" + server + ";Database=prs92;user id=prs92;password=prs92";

           
            comboBoxProjects.DataSource = LoadProjects();
            comboBoxProjects.DisplayMember = "CADSurveyNo";
            comboBoxProjects.Text = _CadSurveyNo;
           

        }


        private DataTable LoadProjects()
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {               

                cnn.Open(); 
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("SELECT CadSurveyNo FROM  CadSurvey ORDER BY CADSURVEYNO", cnn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                return dt;

            }
        }


        private PTM TransformPRS92(Param param,PTM old) 
        {
            PTM ptm;
                ptm.northing = Math.Round((((param.b * -1) * old.easting) + (param.a * old.northing) + param.cn),6);
                ptm.easting =Math.Round(( (param.a * old.easting) + (param.b * old.northing) + param.ce),6);
            return ptm;
        }

        private void Tranform(string project)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();                
                SqlCommand cmd = new SqlCommand("SELECT * FROM  LOTDATA WHERE CADSURVEYNO='" + project + "'", cnn);
                DataTable dt= new DataTable();
                SqlDataAdapter da= new SqlDataAdapter(cmd);
                da.Fill(dt);

                PTM ptm;
                
                Param param;

                 string a = textBoxa.Text.Replace(" ", string.Empty);
                 string b = textBoxb.Text.Replace(" ", string.Empty);
                 string ce = textBoxCE.Text.Replace(" ", string.Empty);
                 string cn = textBoxCN.Text.Replace(" ", string.Empty);

                 Double.TryParse(a, out param.a);
                 Double.TryParse(b, out param.b);
                 Double.TryParse(ce, out param.ce);
                 Double.TryParse(cn, out param.cn);

                 progressBar1.Visible = true;
                 progressBar1.Minimum = 1;
                 progressBar1.Maximum = dt.Rows.Count;

                for (int i=0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();
                    if (cancelme == true)
                    {
                        this.buttonTranforn.Text = "Transform";
                        this.progressBar1.Visible = false;
                        
                        return;
                    }
                    progressBar1.Value = i+1;

                    if (dt.Rows[i]["ptmy"] != DBNull.Value || dt.Rows[i]["ptmx"] != DBNull.Value)
                    {
                        ptm.northing = Convert.ToDouble(dt.Rows[i]["ptmy"]);
                        ptm.easting = Convert.ToDouble(dt.Rows[i]["ptmx"]);


                        PTM prs = TransformPRS92(param, ptm);

                        SqlCommand updatecmd = new SqlCommand("Update LOTDATA set prsx=" + prs.easting + "," + "prsy=" + prs.northing + " Where CadSurveyNo='" + dt.Rows[i]["CADSURVEYNO"] + "' and MunicipalityPsgc='" + dt.Rows[i]["MunicipalityPsgc"] + "' and CadCaseNo='" + dt.Rows[i]["CADCASENO"] + "' and BarangayPsgc='" + dt.Rows[i]["BARANGAYPSGC"] + "' and Quadrangle='" + dt.Rows[i]["QUADRANGLE"] + "' and SectionId='" + dt.Rows[i]["SECTIONID"] + "' and LotNo='" + dt.Rows[i]["LOTNO"] + "' and [Sequence]=" + dt.Rows[i]["SEQUENCE"], cnn);

                        try
                        {
                            updatecmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            this.textBoxLogs.AppendText(ex.Message + Environment.NewLine);
                            this.textBoxLogs.AppendText(prs.northing.ToString() + Environment.NewLine);
                            this.textBoxLogs.AppendText(prs.easting.ToString() + Environment.NewLine);
                        }


                    }

                    else
                    {
                        this.textBoxLogs.AppendText(dt.Rows[i]["CADSurveyNo"].ToString() + " - LOT " + dt.Rows[i]["LOtNo"].ToString() + " Corner " + dt.Rows[i]["SEQUENCE"].ToString() + " no ptm coordinates" + Environment.NewLine);
                    }
                   
                }

                MessageBox.Show("Finished");
                this.buttonTranforn.Text = "Transform";
                progressBar1.Visible = false;
                
            }
        }

        private void buttonTranforn_Click(object sender, EventArgs e)
        {
            if (buttonTranforn.Text == "Cancel")
            {
                cancelme = true;
            }
            else
            {
                cancelme = false;
                this.textBoxLogs.Clear();
            }

            

            if (Extensions.IsNumeric(textBoxa.Text) && Extensions.IsNumeric(textBoxb.Text) && Extensions.IsNumeric(textBoxCE.Text) && Extensions.IsNumeric(textBoxCN.Text))
            {


                buttonTranforn.Text = "Cancel";

                string project = this.comboBoxProjects.Text;
                Tranform(project);
            }

            else
            {
                MessageBox.Show("Invalid Parameters");
            }
        }

        

    }
}
