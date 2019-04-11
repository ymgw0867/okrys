using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;    // 2017/09/03
using okrys.Common;

namespace okrys.Common
{
    class SqlControl
    {
        ///----------------------------------------------------------
        /// <summary>
        ///     DataControlクラスの基本クラス : 2017/09/04 </summary>
        ///----------------------------------------------------------
        public class BaseControl
        {
            private Utility.SQLDBConnect dbConnect;

            ///------------------------------------------------------------------------------
            /// <summary>
            ///     BaseControlのコンストラクタ。DBConnectクラスのインスタンスを作成します。</summary>
            ///------------------------------------------------------------------------------
            public BaseControl(string sConnect)
            {
                dbConnect = new Utility.SQLDBConnect(sConnect);
            }

            ///------------------------------------------------------------------------------
            /// <summary>
            ///     データベース接続メソッド</summary>
            /// <returns>
            ///     データベース接続情報を取得します</returns>
            ///------------------------------------------------------------------------------
            public SqlConnection GetConnection()
            {
                return dbConnect.Cn;
            }
        }

        /// <summary>
        /// データコントロールクラス BaseControlを継承する
        /// </summary>
        public class DataControl : BaseControl
        {

            private Access.DataAccess dAccess;
            public SqlConnection Cn = new SqlConnection();

            /// <summary>
            /// DataControlクラスのコンストラクタ。データアクセスクラスのインスタンスを作成します。
            /// </summary>
            public DataControl(string sConnect)
                : base(sConnect)
            {
                // データアクセスクラスのインスタンスを作成する
                dAccess = new Access.DataAccess();
            }

            /// <summary>
            /// データベースの接続を解除します
            /// </summary>
            public void Close()
            {
                if (Cn.State == System.Data.ConnectionState.Open)
                {
                    Cn.Close();
                }
            }

            ///-------------------------------------------------------------------
            /// <summary>
            ///     条件付きデータリーダー取得インターフェイスを引数としたメソッド </summary>
            /// <param name="IDSR">
            ///     データリーダーを取得するインターフェイス</param>
            /// <param name="tempString">
            ///     SQL文のwhere以下の条件を記述した文字列</param>
            /// <returns>
            ///     条件式に一致する引数で指定されたマスターのデータリーダー</returns>
            ///-------------------------------------------------------------------
            public SqlDataReader FillByAccess(Access.DataAccess.IFillBy IDSR, string tempString)
            {
                // データベース接続情報を取得する
                Cn = this.GetConnection();

                return IDSR.GetdsReader(Cn, tempString);
            }

            ///--------------------------------------------------------
            /// <summary>
            ///     条件付きデータリーダを取得します </summary>
            /// <param name="tempString">
            ///     SQL文を記述した文字列</param>
            /// <returns>
            ///     SqlDataReader データリーダー 2017/09/03</returns>
            ///--------------------------------------------------------
            public SqlDataReader free_dsReader(string tempString)
            {
                try
                {
                    return FillByAccess(new Access.DataAccess.free_dsReader(), tempString);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            ///---------------------------------------------------------
            /// <summary>
            ///     社員情報を取得します : 勘定奉行i10 2017/09/04 </summary>
            /// <param name="sYY">
            ///     基準年</param>
            /// <param name="sMM">
            ///     基準月</param>
            /// <returns>
            ///     データリーダー</returns>
            ///---------------------------------------------------------
            public SqlDataReader GetEmployeeBase(string sYY, string sMM, string sNo)
            {
                string tempDate;

                //基準年月日
                string sDate = (int.Parse(sYY) + Properties.Settings.Default.RekiHosei).ToString() + "/" + sMM + "/01";
                DateTime eDate;
                if (DateTime.TryParse(sDate, out eDate)) tempDate = eDate.ToShortDateString();   //日付を返す
                else tempDate = DateTime.Today.ToShortDateString();　　//当日日付を返す

                //// SQLServer接続
                ////dbControl.DataControl dCon = new dbControl.DataControl(_PCADBName);
                SqlDataReader dRs;
                StringBuilder sb = new StringBuilder();
                string SqlStr = string.Empty;

                sb.Append("select tbDepartment.DepartmentID,tbDepartment.DepartmentCode,tbDepartment.DepartmentName,tbEmployeeBase.EmployeeNo,tbEmployeeBase.Name,tbHR_DivisionCategory.CategoryName ");
                sb.Append("from ((tbEmployeeBase inner join tbHR_DivisionCategory ");
                sb.Append("on EmploymentDivisionID = CategoryID) left join ");

                sb.Append("(select tbEmployeeMainDutyPersonnelChange.EmployeeID,tbEmployeeMainDutyPersonnelChange.BelongID from tbEmployeeMainDutyPersonnelChange inner join (");
                sb.Append("select EmployeeID,max(AnnounceDate) as AnnounceDate from tbEmployeeMainDutyPersonnelChange ");
                sb.Append("where AnnounceDate <= '" + tempDate + "'");
                sb.Append("group by EmployeeID) as a ");
                sb.Append("on (tbEmployeeMainDutyPersonnelChange.EmployeeID = a.EmployeeID) and ");
                sb.Append("(tbEmployeeMainDutyPersonnelChange.AnnounceDate = a.AnnounceDate) ");
                sb.Append("inner join tbDepartment ");
                sb.Append("on tbEmployeeMainDutyPersonnelChange.BelongID = tbDepartment.DepartmentID ");
                sb.Append(") as d ");

                sb.Append("on tbEmployeeBase.EmployeeID = d.EmployeeID) left join ");
                sb.Append("tbDepartment on d.BelongID = tbDepartment.DepartmentID ");

                sb.Append("where tbEmployeeBase.EmployeeNo = '" + string.Format("{0:0000000000}", int.Parse(sNo)) + "' ");
                sb.Append(" and BeOnTheRegisterDivisionID != 9");

                dRs = FreeReader(sb.ToString());
                return dRs;
            }


            ///-------------------------------------------------------------
            /// <summary>
            ///     データリーダーを取得する : SQLClient 2017/09/04 </summary>
            /// <param name="tempSQL">
            ///     SQL文</param>
            /// <returns>
            ///     データリーダー</returns>
            ///-------------------------------------------------------------
            public SqlDataReader FreeReader(string tempSQL)
            {
                SqlCommand sCom = new SqlCommand();
                sCom.CommandText = tempSQL;
                sCom.Connection = GetConnection();
                SqlDataReader dR = sCom.ExecuteReader();

                return dR;
            }

        }


        ///---------------------------------------------------------------
        /// <summary>
        ///     勘定奉行データベースへの接続文字列を取得する </summary>
        ///     2017/09/03
        ///---------------------------------------------------------------
        public class obcConnectSting
        {
            ///---------------------------------------------------------------------
            /// <summary>
            ///     勘定奉行データベースへの接続文字列を取得する </summary>
            /// <param name="dbName">
            ///     接続データベース名</param>
            /// <returns>
            ///     接続文字列</returns>
            ///     
            ///     2017/09/03
            ///---------------------------------------------------------------------
            public static string get(string dbName)
            {
                SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder();
                cb.DataSource = Properties.Settings.Default.SQLServerName;
                cb.InitialCatalog = dbName;
                cb.IntegratedSecurity = false;
                cb.UserID = Properties.Settings.Default.sqlUID;
                cb.Password = Properties.Settings.Default.sqlPWD;

                return cb.ToString();
            }
        }

    }
}
