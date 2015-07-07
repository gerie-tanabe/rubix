using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Data.SqlClient;
using SharpMap;
using GeoAPI;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;

namespace Rubix
{
    public partial class FormViewer : Form
    {
        string server = string.Empty;
        string connectionString = string.Empty;

        public struct PointD
        {
            public double X;
            public double Y;

            public PointD(double x, double y)
            {
                X = x;
                Y = y;
            }
        }


        public FormViewer()
        {
            InitializeComponent();
        }

     

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        

        private void FormViewer_Load(object sender, EventArgs e)
        {
            server = Registry.GetValue(@"HKEY_CURRENT_USER\Software\VB and VBA Program Settings\GNIS-X\SQLServer", "SQLServer", String.Empty).ToString();
            connectionString = "server=" + server + ";Database=prs92;user id=prs92;password=prs92";
            //connectionString = "Data Source=" + "192.168.5.186\\SQLEXPRESS" + ";Initial Catalog=PRS92;User ID=prs92;Password=prs92";
            listBoxProjects.DataSource = LoadProjects();
            listBoxProjects.DisplayMember = "CADSurveyNo";
            listBoxProjects.ValueMember = "CADSurveyNo";

            
          
           
            
        }


        private DataTable LoadProjects()
        {
            //MessageBox.Show("Loading Projects");

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand("SELECT CadSurveyNo FROM  CadSurvey ORDER BY CADSURVEYNO", cnn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private DataTable LoadLots(string CADSurveyNo)
        {
            //MessageBox.Show("Loading Lots");
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand("SELECT LOTNO FROM  LOT  WHERE CADSURVEYNO=@CADSurveyNo ORDER BY CASE when PATINDEX('%[^0-9]%',LOTNO) = 0 THEN  LOTNO ELSE cast(Left(LOTNO,PATINDEX('%[^0-9]%',LOTNO)-1) as int) END", cnn))
                {
                    cmd.Parameters.AddWithValue("@CADSurveyNo", CADSurveyNo);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    cnn.Close();
                    return dt;
                }
            }
        }

        private DataTable LoadCorners(string CADSurveyNo,string LotNo)
        {
            //MessageBox.Show("Loading Corners");
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand("SELECT [SEQUENCE],PTMX,PTMY,PRSX,PRSY FROM  LOTDATA  WHERE CADSURVEYNO=@CADSurveyNo AND LOTNO=@LotNo ORDER BY [SEQUENCE]", cnn))
                {
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@CADSurveyNo", CADSurveyNo);
                    cmd.Parameters.AddWithValue("@LotNo", LotNo);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    cnn.Close();
                    return dt;
                }

            }
        }

        private void listBoxProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            //listBoxLots.DataSource = null;
            DataRowView dr = (DataRowView)listBoxProjects.SelectedItem;
            
            string CADSurveyNo = dr["CadSurveyNo"].ToString();
            listBoxLots.DataSource = LoadLots(CADSurveyNo);
            listBoxLots.DisplayMember = "LotNo";
            listBoxLots.ValueMember = "LotNo";
            
            
        }

        private void listBoxLots_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.mapBox1.Map.Layers.Clear();
            this.mapBox1.Refresh();
            //this.dataGridViewCorners.DataSource = null;
            DataRowView dr = (DataRowView)listBoxProjects.SelectedItem;
            DataRowView dr2 = (DataRowView)listBoxLots.SelectedItem;

           string CADSurveyNo = dr["CadSurveyNo"].ToString();
           string LotNo = dr2["LotNo"].ToString();
           dataGridViewCorners.DataSource = LoadCorners(CADSurveyNo, LotNo);
           dataGridViewCorners.Columns["Sequence"].DefaultCellStyle.Format = "D";
      

           List<PointD> pt = new List<PointD>();
           List<PointD> pt2 = new List<PointD>();


           DataTable tempdt = (DataTable)(this.dataGridViewCorners.DataSource);

          //Bearing Distance
           this.dataGridViewBearingDistance.DataSource = getBearingDistance(tempdt, "PTMX", "PTMY");
           this.dataGridViewBearingDistancePRS.DataSource = getBearingDistance(tempdt, "PRSX", "PRSY");

           foreach (DataRow d in tempdt.Rows)
           {
               PointD ptm = new PointD();
               PointD prs = new PointD();

               try  {ptm.X = Convert.ToDouble((d["PTMX"])); ptm.Y = Convert.ToDouble((d["PTMY"])); pt.Add(ptm);}
               catch{}

               try{prs.X = Convert.ToDouble((d["PRSX"])); prs.Y = Convert.ToDouble((d["PRSY"])); pt2.Add(prs);}
               catch{}
               

           }

           try { LoadParcels(pt,Color.Green,"PTM"); }
           catch{}

           try { LoadParcels(pt2,Color.PowderBlue,"PRS92"); }
           catch{}

           try{this.toolStripStatusLabel1.Text = "PTM Area: " + Math.Round(Area(pt),2).ToString();}
           catch{this.toolStripStatusLabel1.Text = " ";}

           try{this.toolStripStatusLabel2.Text = "PRS Area: " + Math.Round(Area(pt2),2).ToString();}
           catch{this.toolStripStatusLabel2.Text = " ";}

           try{ this.toolStripStatusLabel3.Text = "Difference: " + Math.Round(Math.Abs(Area(pt) - Area(pt2)),2).ToString();}
           catch{this.toolStripStatusLabel3.Text = " ";           }
        }



        private double Area(List<PointD> polygon)
        {
            int i, j;
            double area = 0;

            for (i = 0; i < polygon.Count; i++)
            {
                j = (i + 1) % polygon.Count;

                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }

            area /= 2;
            return (area < 0 ? -area : area);
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void transformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRowView dr = (DataRowView)this.listBoxProjects.SelectedItem;
            string CadSurveyNo = dr["CadSurveyNo"].ToString();
            FormMain formMain = new FormMain(CadSurveyNo);
            formMain.ShowDialog(this);
        }

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {


            string CadSurveyNo = this.textBoxSearch.Text;

            DataTable dt;

            dt = LoadProjects(CadSurveyNo);

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No Records Found.");
            }
            else
            {
                this.listBoxProjects.DataSource = dt;
                this.listBoxProjects.DisplayMember = "CadSurveyNo";
                this.listBoxProjects.ValueMember = "CadSurveyNo";
            }
            

        }

        private DataTable LoadProjects(string CadSurveyNo)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Open();
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("SELECT CadSurveyNo FROM  CadSurvey WHERE CadSurveyNo like @CadSurveyNo ORDER BY CADSURVEYNO", cnn);
                cmd.Parameters.AddWithValue("@CadSurveyNo", "%" + CadSurveyNo + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                cnn.Close();
                return dt;

            }
        }


        private void LoadParcels(List<PointD> pt, Color color, string labelname)
        {
            
            SharpMap.Map map = new SharpMap.Map();
            List<GeoAPI.Geometries.Coordinate> vertices = new List<GeoAPI.Geometries.Coordinate>();


            foreach (PointD i in pt)
            {
                GeoAPI.Geometries.Coordinate p = new GeoAPI.Geometries.Coordinate();
                p.X = i.X;
                p.Y = i.Y;
                vertices.Add(p);
            }
            GeoAPI.Geometries.Coordinate l = new GeoAPI.Geometries.Coordinate();
            l.X = pt[0].X;
            l.Y = pt[0].Y;
            vertices.Add(l);

            //Collection<GeoAPI.Geometries.IGeometry> geom = new Collection<GeoAPI.Geometries.IGeometry>();




            GeometryFactory gf = new NetTopologySuite.Geometries.GeometryFactory();
            GeoAPI.Geometries.ILinearRing shell = gf.CreateLinearRing(vertices.ToArray());

            GeoAPI.Geometries.IPolygon polygon = gf.CreatePolygon(shell, null);

          
            SharpMap.Data.Providers.GeometryProvider geomProvider= new SharpMap.Data.Providers.GeometryProvider(polygon);

            SharpMap.Layers.VectorLayer layerParcels = new SharpMap.Layers.VectorLayer("Parcels");

            SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
           
            
            

            style.Fill = new SolidBrush(Color.FromArgb(200,color));
            style.Outline = new Pen(new SolidBrush(color));
           
            layerParcels.Style = style;
            layerParcels.Style.EnableOutline = true;
           
           
            layerParcels.DataSource = geomProvider;            
            layerParcels.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;

            var fdt = new SharpMap.Data.FeatureDataTable();
            fdt.Columns.Add(new System.Data.DataColumn("id", typeof(uint)));
            fdt.Columns.Add(new System.Data.DataColumn("label", typeof(string)));
            var fdr = (SharpMap.Data.FeatureDataRow)fdt.NewRow();
            fdr.ItemArray = new object[] { 1, labelname };
            fdr.Geometry =  polygon;
            fdt.AddRow(fdr);

            var dataprovider = new SharpMap.Data.Providers.GeometryFeatureProvider(fdt);
          
            var ll = new SharpMap.Layers.LabelLayer("llayer");
            ll.DataSource = dataprovider;
            ll.LabelColumn = "label";
            ll.Style.Font = new Font("Eurostile Extended", 16,FontStyle.Bold);
           
            

            mapBox1.Map.Layers.Add(layerParcels);
            mapBox1.Map.Layers.Add(ll);
            



            mapBox1.Map.ZoomToExtents();
            mapBox1.Refresh();
        }


        private double GetDistance(PointD a, PointD b)
        {
            return Math.Round(Math.Sqrt(Math.Pow(a.X-b.X,2) + Math.Pow(a.Y-b.Y,2)),2);
        }

        private void button1_Click(object sender, EventArgs e)
        {

           
        }

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void lotDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRowView dr = (DataRowView)this.listBoxProjects.SelectedItem;
            string CadSurveyNo = dr["CadSurveyNo"].ToString();

            FormReport rpt = new FormReport(CadSurveyNo);
            rpt.Show();
        }

        private void mapBox1_Click(object sender, EventArgs e)
        {

        }

        private DataTable getBearingDistance(DataTable tempdt,string X,string Y)
        {
            DataTable dtBearingDistance = new DataTable();
            dtBearingDistance.Columns.Add("Distance");
            dtBearingDistance.Columns.Add("NS");
            dtBearingDistance.Columns.Add("Degree");
            dtBearingDistance.Columns.Add("Minutes");
            dtBearingDistance.Columns.Add("EW");


          
            List<PointD> pt = new List<PointD>();

          

            PointD a = new PointD();
            PointD b = new PointD();

            for (int i = 0; i < tempdt.Rows.Count; i++)
            {
                
                try
                {
                    a.X = Convert.ToDouble(tempdt.Rows[i][X]);
                    a.Y = Convert.ToDouble(tempdt.Rows[i][Y]);

                    if (i == tempdt.Rows.Count-1)
                    {
                        b.X = Convert.ToDouble(tempdt.Rows[0][X]);
                        b.Y = Convert.ToDouble(tempdt.Rows[0][Y]);
                    }
                    else
                    {
                        b.X = Convert.ToDouble(tempdt.Rows[i + 1][X]);
                        b.Y = Convert.ToDouble(tempdt.Rows[i + 1][Y]);
                    }

                    Double deltaLat = a.Y - b.Y;
                    Double changeDep = a.X - b.X;
                    Double atan = Math.Atan(changeDep / deltaLat);
                    string NS = deltaLat > 0 ? "S" : "N";
                    double o= Math.Abs((180 / Math.PI) * atan);
                    double p = (int)o;
                    double q = Math.Round((o-p)*60);
                    double deg = q == 60 ? p + 1 : p;
                    double min = q==60?0:q;
                    string EW = changeDep > 0? "W" : "E";

                    DataRow dr = dtBearingDistance.NewRow();
                    
                    dr["Distance"] = GetDistance(a, b).ToString();
                    dr["NS"] = NS;
                    dr["Degree"] = deg.ToString();//Math.Round(atan, 2).ToString();
                    dr["Minutes"] = min.ToString();
                    dr["EW"] = EW.ToString();

                    dtBearingDistance.Rows.Add(dr);
                    
                    
                   

                }
                catch
                {
                    return dtBearingDistance;

                }


            }

           
           return dtBearingDistance;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ghost_iceman@yahoo.com");
        }
        

    }
}
