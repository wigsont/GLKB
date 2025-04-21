using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DBUniAccessor;

using System.Diagnostics;

namespace GLKB
{
    public partial class FormBatchProcess : Form
    {
        public FormBatchProcess()
        {
            InitializeComponent();

            // 打开知识库  Open GLKB   
            try
            {
                acs = DBUniAccessor.DBAccessFactory.Create(DBUniAccessor.DBType.Access);
                //  Read Geological Label Knowledge Base---GLKB.accdb Access database
                acs.Init(".", "\\GLKB.accdb", "", "");

                acs.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("警告Warning！", "Read Knowledge Base failure 知识库打开失败。 "+ex.Message);

                acs.Close();

            }

            //  Read Geological Labels for LuoYang
            try
            {
                acs_Label = DBUniAccessor.DBAccessFactory.Create(DBUniAccessor.DBType.Access);
                acs_Label.Init(".\\", "ComplexLabels.accdb", "", ""); // Geological Labels for LuoYang 洛阳数据   2025年2月25日
                
                acs_Label.Open();

                strSQL = "SELECT FID, Label,MapLabel2 FROM QU";

                ds_Labels = acs_Label.RunSQL(strSQL);

                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Read Labels Failure 数据打开失败。 " + ex.Message, "Warning 警告！");

                acs.Close();

            }
        }


        string strSQL = "";

        DBUniAccessor.IDBAccess acs = null;         // Database Access 数据库访问接口  2009.09.03
        DataTable tGCRKnowledge = null;

        DBUniAccessor.IDBAccess acs_Label = null;   // Database Access 数据库访问接口  2009.09.03
        DataSet ds_Labels = null;                   // ComplexLabel Dataset   2025.02.14

        private void btnConvert_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            GCR gcr = new GCR("");

            GCR: Priority prior;
            prior = Priority.Member;

            string str = gcr.GetPresetGeoCode(prior);

            if (ReadKnowledgeData(acs))
            {
                foreach (DataRow dr in ds_Labels.Tables[0].Rows)
                {
                    gcr = new GCR(dr["Label"].ToString(),acs);

                    str = gcr.GetPresetGeoCode(prior);

                    acs_Label.RunSQL("UPDATE QU SET MapLabel2='" + str + "' WHERE FID=" + dr["FID"]);   //  2025年2月26日



                }

            }

            // Stop timer
            stopwatch.Stop();

            MessageBox.Show("Convert Completed.\nIt takes " + stopwatch.ElapsedMilliseconds + " ms to generate " + ds_Labels.Tables[0].Rows.Count + " geological complex labels.");  


        }


        private bool ReadKnowledgeData(DBUniAccessor.IDBAccess acs)
        {

            string strSQL = "";

            try
            {

                if (acs != null)
                {
                    strSQL = "SELECT ID,[Label Text],[Value of Element],[Value of VSM],[Typical Style],[WordType],[备注] from [Characteristic Values]";

                    tGCRKnowledge = acs.RunQuery(strSQL);

                }

            }
            catch (Exception ex)
            {
                throw new Exception("Read GLKB Failure due to 读取知识库失败,原因：" + ex.Message);

            }

            return true;
        
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            acs.Close();
        }

        private void FormBatchProcess_Load(object sender, EventArgs e)
        {

        }

        private void btnLearn_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            GCR gcr = new GCR("");

            GCR: Priority prior;
            prior = Priority.Member;

            string str = gcr.GetPresetGeoCode(prior);

            if (ReadKnowledgeData(acs))
            {
                foreach (DataRow dr in ds_Labels.Tables[0].Rows)
                {
                    gcr = new GCR(dr["Label"].ToString(),acs);

                    str = gcr.GetPresetGeoCode(prior);

                    acs_Label.RunSQL("UPDATE QU SET MapLabel2='" + str + "', a='" + gcr.strGeoCode.Length + "', b='" + gcr.strGroupValue + "', c='" + gcr.strEigenValue + "' WHERE FID=" + dr["FID"]);

                }

            }

            // Stop timer
            stopwatch.Stop();

            MessageBox.Show("Convert Completed.It takes " + stopwatch.ElapsedMilliseconds + " ms to generate " + ds_Labels.Tables[0].Rows.Count + " geological complex labels");  

        }
    }
}
