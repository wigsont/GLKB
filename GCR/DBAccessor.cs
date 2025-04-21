using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows.Forms;

// 
//   Interfaces for Database access
//   Note: The original sourcecodes was from a contributor in 2019. If anyone knows details, please let us know.
//   Note: Code Description: The content related to the DBUniAccessor interface was modified by @wigsont in 2009
//   Referenced by mainForm and GCR




namespace DBUniAccessor
{
    public enum DBType
    {
        Access,
        SQL,
        Excel, // 2012.02.15 ����

        // Not implemented
        DB2,
        Oracle,
        MySQL
    }

    public enum FieldType
    { 
        CHAR=0,
        INT=1,
        LONG,
        DOUBLE,
        FLOAT,
        DATE
    }

    // �ֶ���  2012.02.14
    public class Field
    {
        public Field()
        { 
        }

        public Field(string name,FieldType type,int len)
        {
            fName = name;
            fType = type;
            fLength = len;
        }
        public string fName;
        public FieldType fType;
        public int fLength;
    
    }

    public interface IDBAccess
    {
        void Init(string strServer, string strDataBase, string strUser, string strPwd);
        void Open();
        void Close();
        bool TestConn();

        int RunNoQuery(string strCmd);
        DataTable RunQuery(string strCmd);
        DataTable OpenTable(string tblName);
        DataTable OpenTableHD(string tblName);
        DataSet RunQuery_DS(string strCmd);// ����� 2009.08.19
        DataSet RunSQL(string strSQL);     // Ϊ�˼��ݾɰ汾��������  2009.09.03
        bool ExecuteProcedure(string procedureName,string databaseName,string strDataFileDir,string strLogFileDir);
        bool LinkedStatus();               // ����������״̬     2009.09.09
        DBType DBType { get;}
        int GetFiledMax(string strTable, string strField);

        DataTable Tables { get; }
        DataTable GetColumns();
        DataTable GetColumns(string strTable);
        
        string[] GetTableNames();
        bool CheckTableNameExist(string tblname);
        string[] GetFieldNames(string strTable);
        string[] GetFieldNamesAndTypes(string strTable,ref int[] num);

     //  string[] GetFieldTypes();
       string[] GetFieldTypes(string strTable);       

        // ���Ӽ�¼
        DataRow AddNew(string strTable/* ������ */);
        bool StoreDataRow(string strTable/* ������ */,DataRow dr);
        bool AddRowAndUpdate(string strTable/* ������ */, DataRow dr);
        bool AddTableAndUpdate(string strTableName, DataTable dt);

        // �洢��    2015.06.04
        bool StoreDataTable(string strTableName, DataTable dt, Field[] fds);
        bool StoreDataTable(string strTableName, DataTable dt);


        // �õ�DataTable����ֶ�����  2015.06.04
        Field[] GetFields(DataTable dt);
        FieldType ConvertField(String flds);

       // ����Geoxpl���ݱ�box
        bool LoadBaseTablesToList(ComboBox cboBox);
        bool LoadBaseTablesToList2(ListBox lstBox);

        // ������
        bool CreateTable(string strTable);// Field[] fields);

        // ɾ����
        bool DelTable(string strTable /* ������ */);

        // �����ֶ�
        bool AddField(string strTableName/* ������ */, Field f);
        bool AddColumn(string TableName, string ColumnName, string ColumnType);//�������ֶ������ֶ����� Boolean Byte .... Text(10)
        // ����GeoExpl�����ֶ�
        bool AddGeoExplMainField(string strTableName);

        // ɾ���ֶ�
        bool DelField(string strTableName, string fName);

        // �����ֶ�
        bool AlterField(string strTableName, Field f);
        
    }

    public static class DBAccessFactory
    {
        public static IDBAccess Create(DBType type)
        {
            IDBAccess IRet = null;
            switch (type)
            {
                case DBType.Access:
                    IRet = new Access(type);
                    break;

                case DBType.SQL:
                    IRet = new SQL(type);
                    break;

                case DBType.Excel :
                    IRet = new Excel(type);
                    break;

                default:
                    break;
            }
            return IRet;
        }

        private abstract class DBAccess : IDBAccess
        {
            protected DbConnection m_oConn = null;
            protected const string CON_strServer = "Server";
            protected const string CON_strDataBase = "Data Source";
            protected const string CON_strUser = "UID";
            protected const string CON_strPwd = "PWD";
            protected const string CON_strConnTimeOut = "Connect Timeout = 2";
            private DBType m_eDBType = DBType.Access;

            protected DbDataAdapter m_oAdpt = null;    // �����  2012.02.10 
            protected DbCommandBuilder m_oCB = null;   // �����  2012.02.10

            protected DBAccess(DBType type)
            {
                this.m_eDBType = type;
            }

            public DBType DBType
            {
                get { return this.m_eDBType; }
            }

            public void Init(string strServer, string strDataBase, string strUser, string strPwd)
            {
                this.InitConn(strServer, strDataBase, strUser, strPwd);
            }

            public void Open()
            {
                if (this.m_oConn != null)
                {
                    this.m_oConn.Open();
                }
            }

            public int RunNoQuery(string strCmd)
            {
                int iRet = 0;
                try
                {
                    DbCommand oCmd = this.GetCmd(strCmd);
                    if (oCmd != null)
                    {
                        iRet = oCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw (new Exception(ex.Message));
                }
                return iRet;
            }

            public int GetFiledMax(string strTable, string strField)
            {
                int iRet = -1;
                DataTable dt = this.RunQuery("Select Max(" + strField + ") From " + strTable);
                if (dt != null && dt.Rows.Count == 1)
                {
                    iRet = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                }
                return iRet;
            }

            public DataTable RunQuery(string strCmd)
            {
                DataTable dt = new DataTable();
                DbDataAdapter adp = this.DbAdp;
                adp.SelectCommand = this.GetCmd(strCmd);
                adp.Fill(dt);
                return dt;
            }
            public DataTable OpenTable(string tblName)
            {
                DataTable dt = this.RunQuery("Select * From " +tblName); //new DataTable();
               // DbDataAdapter adp = this.DbAdp;
              //  adp.SelectCommand = this.GetCmd(strCmd);
              //  adp.Fill(dt);
                return dt;
            }
            public DataTable OpenTableHD(string tblName)
            {
                DataTable dt = this.RunQuery("Select * From " + tblName+" Where GeoExpl_ID=1"); //new DataTable();
                // DbDataAdapter adp = this.DbAdp;
                //  adp.SelectCommand = this.GetCmd(strCmd);
                //  adp.Fill(dt);
                return dt;
            }
            public DataSet RunQuery_DS(string strCmd)   // ����� 
            {
                DataSet ds = new DataSet();
                DbDataAdapter adp = this.DbAdp;
                adp.SelectCommand = this.GetCmd(strCmd);
                adp.Fill(ds);
                return ds;

            }

            public DataSet RunSQL(string strSQL)       // Ϊ�˼��ݾɰ汾  2009.09.02
            {
                return RunQuery_DS(strSQL);            
            }

          
            public bool ExecuteProcedure(string storename, string databaseName,string strDataFileDir,string strLogFileDir)       // ����� 2009.09.02
            {
                return RunStore(storename, databaseName,strDataFileDir,strLogFileDir);                
            }


            public void Close()
            {
                if (this.m_oConn != null && this.m_oConn.State == System.Data.ConnectionState.Open)
                {
                    this.m_oConn.Close();
                }
            }

            public bool LinkedStatus()  // ����״̬  2009.09.09
            {
                if (this.m_oConn.State == System.Data.ConnectionState.Open)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            
            }

            public bool TestConn()
            {
                bool bRet = true;
                try
                {
                    if (this.m_oConn.State != System.Data.ConnectionState.Open)
                    {
                        this.m_oConn.Open();
                    }
                    bRet = this.m_oConn.State == System.Data.ConnectionState.Open;
                }
                catch
                {
                    bRet = false;
                }
                this.Close();
                return bRet;
            }

            public  DataRow AddNew(string strTableName)
            { 
                    return RunSQL("SELECT * FROM [" + strTableName + "]").Tables[0].NewRow ();       
            }

            public bool LoadBaseTablesToList(ComboBox cboBox)
            {
                DataTable dt;
                try
                {
                    dt = this.OpenTable("DataTableInfo000");
                    int i;
                    for (i = 0; i < dt.Rows.Count; i++)
                    {
                        cboBox.Items.Add(dt.Rows[i]["TableName"]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());   
                   return false;
                }
                return true;

            }
            public bool LoadBaseTablesToList2(ListBox lstBox)
            {
                DataTable dt;
                try
                {
                    dt = this.OpenTable("DataTableInfo000");
                    int i;
                    for (i = 0; i < dt.Rows.Count; i++)
                    {
                        lstBox.Items.Add(dt.Rows[i]["TableName"]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString()); 
                    return false;
                }
                return true;

            }            
            
            public abstract DataTable Tables { get; }
            public abstract DataTable GetColumns();
            public abstract DataTable GetColumns(string strTable);

            protected abstract void InitConn(string strServer, string strDataBase, string strUser, string strPwd);
            protected abstract DbCommand GetCmd(string strCmd);
            protected abstract DbDataAdapter DbAdp { get;}

            protected abstract bool RunStore(string storename, string databaseName, string fileDataDir,string fileLogDir);// ����� 2009.09.02
            
            // �жϱ��Ƿ����   2012.02.15
            public abstract bool ExistTable(string strTable);

            // �ж��ֶ��Ƿ�����ڱ�
            public abstract bool ExistField(string strTableName, string strFieldName);

            protected abstract DbCommandBuilder GetCB { get; }

            // ��Access�����һ�м�¼  �����   2012.02.10
            public bool AddRowAndUpdate(string strTableName, DataRow dr)

              {
               //  connectionString=(OleDbConnection)base.m_oConn ;//this.GetCmd();
                  OleDbConnection connection = (OleDbConnection)m_oConn;// )// new OleDbConnection(connectionString))
                 // {
                     OleDbDataAdapter adapter = new OleDbDataAdapter();
                      adapter.SelectCommand = new OleDbCommand("SELECT * FROM [" + strTableName + "]", connection);
                      OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
                      builder.QuotePrefix = "[";
                       builder.QuoteSuffix = "]";
   //builder.QuotePrefix
                      try
                      {
                         // connection.Open();

                          DataSet customers = new DataSet();
                          adapter.Fill(customers);

                          //code to modify data in dataset here
                          customers.Tables[0].Rows.Add(dr.ItemArray);
                        //  adapter.UpdateBatchSize = customers.Tables[0].Rows.Count; 
                          adapter.Update(customers);
                          customers.AcceptChanges();
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex.ToString());
                          return false;
                      }

                      return true;
                 // }
              }

            public bool AddTableAndUpdate(string strTableName, DataTable dt)
            {
                //  connectionString=(OleDbConnection)base.m_oConn ;//this.GetCmd();
                OleDbConnection connection = (OleDbConnection)m_oConn;// )// new OleDbConnection(connectionString))
                // {
                OleDbDataAdapter adapter = new OleDbDataAdapter();
                adapter.SelectCommand = new OleDbCommand("SELECT * FROM [" + strTableName + "]", connection);
                OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";
            //    DataRow dr;
            //    int i;
                //builder.QuotePrefix
                try
                {
           

                   // DataSet customers = new DataSet();
                  //  adapter.Fill(customers);

                    //code to modify data in dataset here
                  //  customers.Tables[0].Rows.Add(dr.ItemArray);

                  //  adapter.Update(customers);
                //    customers.AcceptChanges();
                    adapter.Update(dt);
                    dt.AcceptChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());                    
                    return false;
                }

                return true;
                // }
            }

            public bool StoreDataRow(string strTableName, DataRow dr)
            {
                DataSet ds = new DataSet();
                DbDataAdapter adp = this.DbAdp;
                adp.SelectCommand = this.GetCmd("SELECT * FROM [" + strTableName + "]");
                m_oCB = this.GetCB;
 
                adp.Fill(ds);
                DataRow[] drs;
                int i;
                try
                {
                    ds.Tables[0].Rows.Add(dr.ItemArray);
                    drs=new DataRow[ds.Tables[0].Rows.Count];
                    for(i=0;i<ds.Tables[0].Rows.Count;i++)
                        drs[i]=ds.Tables[0].Rows[i];

                    adp.Update(drs);//  (ds, ds.Tables[0].TableName);
                    ds.AcceptChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());                
                    return false;
                }

                return true;
            }

            //�洢dataTable�����   2015.06.04
            public bool StoreDataTable(string strTableName, DataTable pdt)
            {

                for (int i = 0; i < pdt.Rows.Count; i++)
                {
                    DataRow dr = pdt.Rows[i];
                    if (AddRowAndUpdate(strTableName, dr) == false)
                    {
                        return false;
                    }

                }
                return true;
            }

            // ���ش洢���ֶ��������͵�����  �����  2015.06.04  
            public Field[] GetFields(DataTable dt)
            {
                if (dt == null) return null;

                int i = dt.Columns.Count;
                Field[] fields = new Field[i];

                FieldType fType = 0;
                int length = 0;
                for (int cnt = 0; cnt < dt.Columns.Count; cnt++)
                {


                    if (dt.Columns[cnt].DataType.ToString() == "System.String")
                    {
                        fType = FieldType.CHAR;

                    }
                    else if (dt.Columns[cnt].DataType.ToString() == "System.Int32" || dt.Columns[cnt].DataType.ToString() == "System.Int16")
                    {
                        fType = FieldType.INT;


                    }
                    else if (dt.Columns[cnt].DataType.ToString() == "System.Int64")
                    {
                        fType = FieldType.LONG;

                    }
                    else if (dt.Columns[cnt].DataType.ToString() == "System.Double")
                    {

                        fType = FieldType.DOUBLE;

                    }
                    else if (dt.Columns[cnt].DataType.ToString() == "System.Single")
                    {

                        fType = FieldType.FLOAT;

                    }
                    else if (dt.Columns[cnt].DataType.ToString() == "System.DateTime")
                    {

                        fType = FieldType.DATE;

                    }

                    if (dt.Columns[cnt].MaxLength == -1)
                    {
                        length = 255;
                    }


                    fields[cnt] = new Field(dt.Columns[cnt].ColumnName, fType, length);
                }


                return fields;

            }
            // ���ش洢���ֶ��������͵�����  �����  2015.06.04  
            public FieldType ConvertField(String flds)
            {
                FieldType ftype=0;
               switch(flds)
               {
                   case "String":  //Text Note
                  
                        ftype = FieldType.CHAR;
                       break;
                   case "Int32":
                   case "Int16":
                        ftype = FieldType.INT;
                       break;
                   case "Int64":
                       ftype = FieldType.LONG;
                       break;
                   case "Double": 
                       ftype = FieldType.DOUBLE;
                       break;
                   case "Single": 
                       ftype = FieldType.FLOAT;
                       break;
                   case "DateTime": 
                       ftype = FieldType.DATE;
                       break;
                 /*  case "Boolean":
                      ftype = FieldType.;
                       break;
                   case "Byte":
                       ftype = FieldType.;
                       break;
                   case "Decimal":  //����
                       ftype = FieldType.;
                       break;*/
 
                }   
                return ftype;

            }
            //��һ�δ洢dataTable�����  �����  2015.06.04
            public bool StoreDataTable(string strTableName, DataTable pdt, Field[] fds)
            {
                int i;
                OleDbConnection connection = (OleDbConnection)m_oConn;

                if (!CreateTable(strTableName)) return false;
                for (i = 0; i < fds.Length; i++)
                {
                    AddField(strTableName, fds[i]);
                }

                OleDbDataAdapter adapter = new OleDbDataAdapter();
                adapter.SelectCommand = new OleDbCommand("SELECT * FROM [" + strTableName + "]", connection);
                OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                for (i = 0; i < pdt.Rows.Count; i++)
                {
                    DataRow dr = pdt.Rows[i];

                    try
                    {
                        DataSet customers = new DataSet();
                        adapter.Fill(customers);

                        //code to modify data in dataset here
                        customers.Tables[0].Rows.Add(dr.ItemArray);
                        //  adapter.UpdateBatchSize = customers.Tables[0].Rows.Count; 
                        adapter.Update(customers);
                        customers.AcceptChanges();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return false;
                    }


                }
                return true;
            }

            // �����ݿ��д�����+GeoExpl_ID����      2015.06.8
            public bool CreateTable(string strTable)
            {
                // �ж��Ƿ���ڱ�

                if (ExistTable(strTable))
                {
 
                    return false;
                }


                 string strSQL="Create Table " + strTable + "(";
                 strSQL += "GeoExpl_ID int identity(1,1) primary key)";
                /* if (fields.Length == 0)
                 {
                     return false;
                 }*/

                /* for (int i = 0; i < fields.Length; i++)
                 {
                     if (fields[i].fType == FieldType.CHAR)
                     {
                         strSQL += fields[i].fName + " " + fields[i].fType.ToString() + "(" + fields[i].fLength + "),";
                     }
                     if (fields[i].fType == FieldType.INT || fields[i].fType == FieldType.DOUBLE || fields[i].fType == FieldType.FLOAT )
                     {
                         if(fields[i].fName=="GeoExpl_ID")
                            strSQL += fields[i].fName + " int identity(1,1) primary key,";
                         // strSQL += "add GeoExpl_ID int identity(1,1) primary key" ;
                         else
                            strSQL += fields[i].fName + " " + fields[i].fType.ToString() + ",";
                     }
                 }

                strSQL = strSQL.Substring(0, strSQL.Length -1);
                strSQL += ")";*/

                try
                {
                    this.RunNoQuery (strSQL );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString()); 
                    return false;
                }


                 return true;
            }

            // ɾ����
            public bool DelTable(string strTableName)
            {
                if (ExistTable(strTableName))
                {
                    this.RunNoQuery("Drop Table " + strTableName);

                    return true;
                }

                return false;
            }


            // �����ֶ�
            public bool AddField(string strTableName/* ������ */, Field f)
            {
                if (ExistTable(strTableName))
                {
                    if (ExistField(strTableName, f.fName)) return false;

                    string strSQL = "alter Table " + strTableName + " \n";
                    strSQL += "add [" + f.fName + "] " ;

                    if (f.fType == FieldType.CHAR)
                    {
                        strSQL += f.fType.ToString() + "(" + f.fLength + ")";
                    }

                    if (f.fType == FieldType.INT || f.fType == FieldType.DOUBLE || f.fType == FieldType.FLOAT)
                    {
                        strSQL += f.fType.ToString();
                    }

                    this.RunNoQuery(strSQL);

                    return true;
                }

                return false;
            }
            //������ֶ�
            public bool AddColumn(string TableName, string ColumnName, string ColumnType)
            {
                bool returnValue;
                string sqlstr;
                returnValue = false;
                try
                {

                    //object null_object = null;
                    sqlstr="Alter Table [" + TableName + "] Add [" + ColumnName + "] " + ColumnType + "";//, out null_object, -1); //�����ı��ֶ�  ColumnType=Text(20)
                    this.RunNoQuery(sqlstr);
                    returnValue = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return returnValue;
            }
            // ���GeoExpl�����ֶ�
            public bool AddGeoExplMainField(string strTableName)
            {
                if (ExistTable(strTableName))
                {
                    if (ExistField(strTableName, "GeoExpl_ID")) return false;

                    string strSQL = "alter Table " + strTableName + " \n";
                    strSQL += "add GeoExpl_ID int identity(1,1) primary key" ;
                    try
                    {
                        this.RunNoQuery(strSQL);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return false;
                    }

                    return true;
                }

                return false;
            }
            // ɾ���ֶ�
            public bool DelField(string strTableName, string fName)
            {
                if (ExistTable(strTableName) && ExistField (strTableName ,fName))
                {
                    string strSQL = "alter Table " + strTableName + " \n";
                    strSQL += "drop column [" + fName + "] " ;

                    this.RunNoQuery (strSQL );

                }
                else
                {
                    return false;
                }
                return true;
            }

            // �����ֶ�
            public bool AlterField(string strTableName, Field f)
            {
                if (ExistTable(strTableName) && ExistField(strTableName, f.fName))
                {
                    string strSQL = "alter Table " + strTableName + " \n";
                    strSQL += "alter column " + f.fName + " ";

                    if (f.fType == FieldType.CHAR)
                    {
                        strSQL += f.fType.ToString() + "(" + f.fLength + ")";
                    }

                    if (f.fType == FieldType.INT || f.fType == FieldType.DOUBLE || f.fType == FieldType.FLOAT)
                    {
                        strSQL += f.fType.ToString();
                    }

                    this.RunNoQuery(strSQL);

                }
                else
                {
                    return false;
                }

                return true;
            }


            // �õ����ݿ������б�����   2012.02.17
            public string[] GetTableNames()
            { 
                DataTable t= this.Tables;

                string[] strTableNames=new string[t.Rows.Count];

                for (int i=0; i<t.Rows.Count;i++ )
                {
                    strTableNames.SetValue (t.Rows[i]["Table_Name"],i);
                    
                }
                
                return strTableNames;

            }
            // ������ݱ�������   2015.05.27
            public bool CheckTableNameExist(string tblname)
            {
                DataTable t = this.Tables;

               // string[] strTableNames = new string[t.Rows.Count];

                for (int i = 0; i < t.Rows.Count; i++)
                {
                    if (t.Rows[i][0].ToString() == tblname)
                    {
                        return true;
                    }

                }

                return false;

            }
            // �õ���strTable���е��ֶ�����   2012.02.17
            public string[] GetFieldNames(string strTable)
            { 
                DataTable t= this.GetColumns (strTable );

                string[] strFieldNames=new string[t.Rows.Count];

                for (int i=0; i<t.Rows.Count;i++ )
                {
                    strFieldNames.SetValue(t.Rows[i]["COLUMN_NAME"], i);
                    
                }

                return strFieldNames;
            }
            // �õ���strTable���е��ֶ�����������   2013.08.24 xiang

            public string[] GetFieldNamesAndTypes(string strTable,ref int[] num)
            {
               // string tmpstr;
                DataTable t = this.GetColumns(strTable);

                string[] strFieldNames = new string[t.Rows.Count];
                num=new int[t.Rows.Count];
                for (int i = 0; i < t.Rows.Count; i++)
                {
         //           tmpstr=t.Rows[i]["COLUMN_NAME"]+","+t.Rows[i]["DATA_TYPE"].ToString();
          //          strFieldNames.SetValue(tmpstr, i);
                    //           tmpstr=t.Rows[i]["COLUMN_NAME"]+","+t.Rows[i]["DATA_TYPE"].ToString();
                    strFieldNames.SetValue(t.Rows[i]["COLUMN_NAME"], i);
                    num[i] = (int)t.Rows[i]["DATA_TYPE"];

                }

                return strFieldNames;
            }
            // �õ���strTable���е��ֶ�����  2012.02.17
            public string[] GetFieldTypes(string strTable)
            {
                DataTable t = this.GetColumns(strTable);

                string[] strFieldNames = new string[t.Rows.Count];

                for (int i = 0; i < t.Rows.Count; i++)
                {
                   // strFieldNames[i]=t.Rows[i]["DATA_TYPE"].ToString;
                    strFieldNames[i]=t.Rows[i]["DATA_TYPE"].ToString();

                }

                return strFieldNames;
            }
        }

        #region Access ʵ��
        private class Access : DBAccess
        {
            public Access(DBType type)
                : base(type)
            {
            }

            protected override void InitConn(string strServer, string strDataBase, string strUser, string strPwd)
            {
                string strConn = "Provider = ";
                switch (strDataBase.Substring(strDataBase.LastIndexOf(".") + 1).ToLower())
                {
                    case "mdb":     // 2000, 2003
                        strConn += "Microsoft.Jet.OleDb.4.0;";
                        break;

                    case "accdb":   // 2007
                        strConn += "Microsoft.ACE.OLEDB.12.0;";
                        break;

                    case "dll":     // 2000, 2003
                        strConn += "Microsoft.ACE.OLEDB.12.0;";
                        break;

                    default:
                        throw (new Exception("Unknown Access Version."));
                    //break;
                }

                if(strDataBase!="")
                    strConn += CON_strDataBase + " = " +strServer+"\\"+ strDataBase +";";
                if (strUser != "" && strUser != null)
                    strConn += CON_strUser + " = " + strUser + ";";
                if (strPwd != "" && strPwd != null)
                    strConn += "Jet OLEDB:Database Password" + " = " + strPwd;

                base.m_oConn = new OleDbConnection(strConn);
            }

            // Access�洢���� ����  �����  2009.09.02
            protected override bool RunStore(string storename, string databseName, string fileDataDir,string fileLogDir)
            {
                return true;
            }

            
            protected override DbCommand GetCmd(string strCmd)
            {
                return new OleDbCommand(strCmd, (OleDbConnection)base.m_oConn);
            }

            protected override DbDataAdapter DbAdp
            {
                get { return this.m_oAdpt = new OleDbDataAdapter(); }
            }

            protected override DbCommandBuilder GetCB
            {
                get { return this.m_oCB = new OleDbCommandBuilder((OleDbDataAdapter)base.m_oAdpt); }
                
            }

            public override DataTable Tables
            {
                get
                {
                    return ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
                }
            }

            public override DataTable GetColumns()
            {
                DataTable dt = new DataTable();
                foreach (DataRow row in this.Tables.Rows)
                {
                    dt.Merge(this.GetColumns(row["TABLE_NAME"].ToString()));
                }
                return dt;
            }

            public override DataTable GetColumns(string strTable)
            {
              //  return ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strTable, null });
                return ((OleDbConnection)base.m_oConn).GetSchema("columns", new string[] { null, null, strTable, null });
            }

            public override bool ExistTable(string strTable)
            {
                DataTable t = ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, strTable, "Table" });

                return (t.Rows.Count>0 ? true:false);
              //  DataTable t = ((OleDbConnection)base.m_oConn).GetSchema(
            }

            public override bool ExistField(string strTableName, string strFieldName)
            {

                DataTable t = ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strTableName,null });

                DataRow[] dr = t.Select("COLUMN_NAME='" + strFieldName + "'");
                
                return (dr.Length > 0 ? true : false);
            }

        }
        #endregion // Access

        #region Excel ʵ��
        private class Excel : DBAccess
        {
            public Excel(DBType type)
                : base(type)
            {
            }

            protected override void InitConn(string strServer, string strDataBase, string strUser, string strPwd)
            {
                string strConn = "Provider = ";
                string strExcelV = "";
                switch (strDataBase.Substring(strDataBase.LastIndexOf(".") + 1).ToLower())
                {
                    case "xls":     // 2000, 2003
                        strConn += "Microsoft.Jet.OleDb.4.0;";
                        strExcelV = "Excel 8.0";
                        break;

                    case "xlsx":   // 2007
                        strConn += "Microsoft.ACE.OLEDB.12.0;";  //Provider=Microsoft.ACE.OLEDB.12.0;Data Source=
                        strExcelV = "Excel 12.0 XML";
                        break;

                    default:
                        throw (new Exception("Unknown Access Version."));
                        //break;
                }

                if(strDataBase!="")
                    strConn += CON_strDataBase + " = " +strServer+"\\"+ strDataBase +";";
                if (strUser != "" && strUser != null)
                    strConn += CON_strUser + " = " + strUser + ";";
                if (strPwd != "" && strPwd != null)
                    strConn += "Jet OLEDB:Database Password" + " = " + strPwd;

                strConn += "Extended Properties='" + strExcelV + ";HDR=Yes;IMEX=2';";

                base.m_oConn = new OleDbConnection(strConn);
            }

            // Access�洢���� ����  �����  2009.09.02
            protected override bool RunStore(string storename, string databseName, string fileDataDir, string fileLogDir)
            {
                return true;
            }


            protected override DbCommand GetCmd(string strCmd)
            {
                return new OleDbCommand(strCmd, (OleDbConnection)base.m_oConn);
            }

            protected override DbDataAdapter DbAdp
            {
                get { return this.m_oAdpt = new OleDbDataAdapter(); }
            }

            protected override DbCommandBuilder GetCB
            {
                get { return this.m_oCB = new OleDbCommandBuilder((OleDbDataAdapter)base.m_oAdpt); }

            }

            public override DataTable Tables
            {
                get
                {
                    return ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
                }
            }

            public override DataTable GetColumns()
            {
                DataTable dt = new DataTable();
                foreach (DataRow row in this.Tables.Rows)
                {
                    dt.Merge(this.GetColumns(row["TABLE_NAME"].ToString()));
                }
                return dt;
            }

            public override DataTable GetColumns(string strTable)
            {
                return ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strTable, null });
            }

            public override bool ExistTable(string strTable)
            {
                DataTable t = ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, strTable, "Table" });

                return (t.Rows.Count > 0 ? true : false);
            }

            public override bool ExistField(string strTableName, string strFieldName)
            {

                DataTable t = ((OleDbConnection)base.m_oConn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strTableName, null });

                DataRow[] dr = t.Select("COLUMN_NAME='" + strFieldName + "'");

                return (dr.Length > 0 ? true : false);
            }

        
        
        
        }
        #endregion // Excel

        #region SQLServer ʵ��
        private class SQL : DBAccess
        {
            public SQL(DBType type)
                : base(type)
            {
            }

            protected override void InitConn(string strServer, string strDataBase, string strUser, string strPwd)
            {
                string strConn = "Data Source= " + strServer + ";";
                strConn += "Initial Catalog= " + strDataBase + ";";
                strConn += CON_strUser + " = " + strUser + ";";
                strConn += CON_strPwd + " = " + strPwd + ";";
                strConn += CON_strConnTimeOut;
                base.m_oConn = new SqlConnection(strConn);
            }

            protected override bool RunStore(string storename, string databaseName, string strMdfDir,string strLdfDir)
            {
                bool flag = true;
                SqlConnection conn = (SqlConnection)base.m_oConn;
                //SqlTransaction tran = conn.BeginTransaction(IsolationLevel.ReadCommitted, storename);

                try
                {
                    SqlCommand cmd = new SqlCommand(storename, conn);

                    //cmd.Transaction = tran;  �������ݿ���䲻������������(Transaction)

                    cmd.CommandType = CommandType.StoredProcedure;                    

                    cmd.Parameters.Add(new SqlParameter("@database_Name",SqlDbType.VarChar,50));
                    cmd.Parameters[0].Value = databaseName;
                    cmd.Parameters.Add(new SqlParameter("@database_mdf",SqlDbType.VarChar,50));
                    cmd.Parameters[1].Value = strMdfDir;
                    cmd.Parameters.Add(new SqlParameter("@database_log", SqlDbType.VarChar,50));
                    cmd.Parameters[2].Value = strLdfDir;

                    cmd.ExecuteNonQuery();
                    //tran.Commit();
                }
                catch (Exception ex)
                {
                    flag = false;

                    //tran.Rollback();

                    throw ex;

                }
                finally
                {

                }
                return flag;

            }

            protected override DbCommand GetCmd(string strCmd)
            {
                return new SqlCommand(strCmd, (SqlConnection)base.m_oConn);
            }

            protected override DbDataAdapter DbAdp
            {
                get { return this.m_oAdpt = new SqlDataAdapter(); }
            }

            public override DataTable Tables
            {
                get { return ((SqlConnection)base.m_oConn).GetSchema("Tables", null); }
            }

            public override DataTable GetColumns()
            {
                return ((SqlConnection)base.m_oConn).GetSchema("Columns", null);
            }

            public override DataTable GetColumns(string strTable)
            {
                return ((SqlConnection)base.m_oConn).GetSchema("Columns", new string[] { null, null, strTable, null });
            }

            protected override DbCommandBuilder GetCB
            {
                get { return this.m_oCB = new SqlCommandBuilder((SqlDataAdapter )base.m_oAdpt); }

            }

            public override bool ExistTable(string strTable)
            {
                DataTable t = ((SqlConnection)base.m_oConn).GetSchema("Columns", new string[] { null, null, strTable, null });

                return (t.Rows.Count > 0 ? true : false);
            }

            public override bool ExistField(string strTableName, string strFieldName)
            {

                DataTable t = ((SqlConnection)base.m_oConn).GetSchema("Columns", new string[] { null, null, strTableName, null });

                DataRow[] dr = t.Select("COLUMN_NAME='" + strFieldName + "'");

                return (dr.Length > 0 ? true : false);
            }

        }




        #endregion // SQLServer
    }
}


