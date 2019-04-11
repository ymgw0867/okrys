using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace okrys.Common
{
    public class Master
    {
        public OleDbCommand sCom = new OleDbCommand();
        protected StringBuilder sb = new StringBuilder();

        public Master()
        {
        }

        public void dbConnect()
        {
            // データベース接続文字列
            OleDbConnection Cn = new OleDbConnection();
            sb.Clear();
            sb.Append("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=");
            sb.Append(Properties.Settings.Default.mdbPath);
            sb.Append(global.MDBFILE);
            Cn.ConnectionString = sb.ToString();
            Cn.Open();

            sCom.Connection = Cn;
        }
    }

    public class msConfig : Master
    {
        /// <summary>
        /// 環境設定マスター新規登録
        /// </summary>
        /// <param name="sID">キー</param>
        /// <param name="sSYEAR">年</param>
        /// <param name="sSMONTH">月</param>
        /// <param name="sPath">受け渡しデータ作成先パス</param>
        public void Insert(int sID, string sSYEAR, string sSMONTH, string sPath, string sArchived)
        {
            try
            {
                sb.Clear();
                sb.Append("insert into 環境設定 (");
                sb.Append("ID,年,月,受け渡しデータ作成パス,データ保存月数,更新年月日) values (");
                sb.Append("?,?,?,?,?,?)");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                sCom.Parameters.AddWithValue("@year", sSYEAR);
                sCom.Parameters.AddWithValue("@Month", sSMONTH);
                sCom.Parameters.AddWithValue("@Path", sPath);
                sCom.Parameters.AddWithValue("@Arc", sArchived);
                sCom.Parameters.AddWithValue("@update", DateTime.Today.ToShortDateString());
                sCom.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "環境設定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }            
        }

        /// <summary>
        /// 環境設定マスター更新
        /// </summary>
        /// <param name="sSYEAR">年</param>
        /// <param name="sSMONTH">月</param>
        /// <param name="sPath">受け渡しデータ作成先パス</param>
        public void UpDate(string sSYEAR, string sSMONTH, string sPath, string sArchived)
        {
            try
            {
                sb.Clear();
                sb.Append("update 環境設定 set ");
                sb.Append("年=?,月=?,受け渡しデータ作成パス=?,データ保存月数=?,更新年月日=?");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@year", sSYEAR);
                sCom.Parameters.AddWithValue("@Month", sSMONTH);
                sCom.Parameters.AddWithValue("@path", sPath);
                sCom.Parameters.AddWithValue("@arc", sArchived);
                sCom.Parameters.AddWithValue("@update", DateTime.Today.ToShortDateString());

                sCom.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "環境設定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }
        
        /// <summary>
        /// 環境設定マスター取得
        /// </summary>
        /// <param name="sID">環境設定キー</param>
        public OleDbDataReader Select(int sID)
        {
            OleDbDataReader dr = null;

            try
            {
                sb.Clear();
                sb.Append("select * from 環境設定 ");
                sb.Append("where ID = ?");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                dr = sCom.ExecuteReader();
                return dr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "環境設定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return dr;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 環境設定データを取得する
        /// </summary>
        public void GetCommonYearMonth()
        {
            OleDbDataReader dr = Select(global.configKEY);

            try
            {
                while (dr.Read())
                {
                    global.cnfYear = int.Parse(dr["年"].ToString());
                    global.cnfMonth = int.Parse(dr["月"].ToString());
                    global.cnfPath = dr["受け渡しデータ作成パス"].ToString();
                    global.cnfArchived = int.Parse(dr["データ保存月数"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "環境設定年月取得", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                if (dr.IsClosed == false) dr.Close();
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }
    }
}
