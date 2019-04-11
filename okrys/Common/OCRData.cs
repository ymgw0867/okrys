using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace okrys.Common
{
    class OCRData : Master
    {
        //public OleDbCommand sCom = new OleDbCommand();
        //protected StringBuilder sb = new StringBuilder();
        
        // ＯＣＲデータヘッダ
        public string OCRDATA_ID { get; set; }
        public string _SheetID { get; set; }
        public string _StaffCode { get; set; }
        public string _Year { get; set; }
        public string _Month { get; set; }
        public string _OrderCode { get; set; }
        public string _ShuDays { get; set; }
        public string _ImageName { get; set; }

        public class OCR_Item
        {
            public string _Day { get; set; }
            public string _Kyuka { get; set; }
            public string _Sh { get; set; }
            public string _Sm { get; set; }
            public string _eh { get; set; }
            public string _em { get; set; }
            public string _Kyukei { get; set; }
            public string _teisei { get; set; }
        }

        public OCR_Item[] itm = new OCR_Item[global._MULTIGYO];

        // 勤怠記号に該当する就業奉行の事由コードの配列
        public string[] sJiyuArray = { "21", "1", "2", "18", "17", "10", "", "20", "8", "19" };

        //出力データ
        public string sShainNo;         // 社員番号
        public string sDate;            // 日付
        public string sShift;           // シフト
        public string sJiyu;            // 事由
        public string sStime;           // 出勤時刻
        public string sEtime;           // 退出時刻
        public string sGtime;           // 外出時刻
        public string sMtime;           // 戻り時刻
        public string sWorktime;        // 出勤（実働）時間
        public string sResttime;        // 休憩時間
        public string sHolidayWork;     // 休日時間時間

        public OCRData()
        {
            // 出力情報初期化
            sShainNo = string.Empty;    //社員番号
            sDate = string.Empty;       //日付
            sShift = string.Empty;      //シフト
            sJiyu = string.Empty;       //事由
            sStime = string.Empty;      //出勤時間
            sEtime = string.Empty;      //退出時間
            sGtime = string.Empty;      //外出時間
            sMtime = string.Empty;      //戻り時間
            sWorktime = string.Empty;   // 出勤（実働）時間
            sResttime = string.Empty;   // 休憩時間
        }

        /// <summary>
        /// 勤務票ヘッダデータ新規登録
        /// </summary>
        /// <param name="sID">ID</param>
        /// <param name="sSheID">申請書種別</param>
        /// <param name="sSNo">個人番号</param>
        /// <param name="sSName">氏名</param>
        /// <param name="sYear">年</param>
        /// <param name="sMonth">月</param>
        /// <param name="sShozokuName">所属名</param>
        /// <param name="sImgName">画像名</param>
        /// <param name="sDBName">会社領域データベース名</param>
        /// <param name="sKoutsuhi">交通費計</param>
        public void HeadInsert(string sID, string sSheID, string sSNo, string sSName, string sYear, string sMonth,
                           string sShozokuName, string sImgName, string sDBName, string sKoutsuhi)
        {
            try
            {
                // 勤務票ヘッダ
                sb.Clear();
                sb.Append("insert into 勤務票ヘッダ ");
                sb.Append("(ID,申請書種別,個人番号,氏名,年,月,所属名,画像名,");
                sb.Append("データ領域名,交通費計,更新年月日) ");
                sb.Append("values (?,?,?,?,?,?,?,?,?,?,?)");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();

                sCom.Parameters.AddWithValue("@ID", sID);                       // ID                
                sCom.Parameters.AddWithValue("@ID", sSheID);                    // 申請書種別                
                sCom.Parameters.AddWithValue("@kjn", sSNo);                     // 個人番号                 
                sCom.Parameters.AddWithValue("@kjn", sSName);                   // 氏名                 
                sCom.Parameters.AddWithValue("@year", sYear);                   // 年                
                sCom.Parameters.AddWithValue("@month", sMonth);                 // 月                
                sCom.Parameters.AddWithValue("@ShozokuName", sShozokuName);     // 所属名              
                sCom.Parameters.AddWithValue("@IMG", sImgName);                 // 画像名                 
                sCom.Parameters.AddWithValue("@DBNAME", sDBName);               // データベース名              
                sCom.Parameters.AddWithValue("@KTH", sKoutsuhi);                // 交通費                                            
                sCom.Parameters.AddWithValue("@Date", DateTime.Today.ToShortDateString());  // 更新年月日

                // テーブル書き込み
                sCom.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票ヘッダ", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 勤務票ヘッダデータ更新
        /// </summary>
        /// <param name="sID">ID</param>
        /// <param name="sSNo">個人番号</param>
        /// <param name="sName">氏名</param>
        /// <param name="sYear">年</param>
        /// <param name="sMonth">月</param>
        /// <param name="sShozokuName">所属名</param>
        /// <param name="sKoutsuhi">交通費計</param>
        public void HeadUpdate(string sID, string sSNo, string sName, string sYear, string sMonth,
                           string sShozokuName, string sDBName, string sKoutsuhi)
        {
            try
            {
                // 勤務票ヘッダ
                sb.Clear();
                sb.Append("update 勤務票ヘッダ set ");
                sb.Append("個人番号=?,氏名=?,年=?,月=?,所属名=?,データ領域名=?,交通費計=?,更新年月日=? ");
                sb.Append("where ID=?");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();

                sCom.Parameters.AddWithValue("@kjn", sSNo);             // 個人番号                
                sCom.Parameters.AddWithValue("@name", sName);           // 氏名                     
                sCom.Parameters.AddWithValue("@year", sYear);           // 年                
                sCom.Parameters.AddWithValue("@month", sMonth);         // 月                
                sCom.Parameters.AddWithValue("@shozoku", sShozokuName); // 所属名                  
                sCom.Parameters.AddWithValue("@dbName", sDBName);       // データベース名                
                sCom.Parameters.AddWithValue("@Koutsuhi", sKoutsuhi);   // 交通費計          
                sCom.Parameters.AddWithValue("@Date", DateTime.Today.ToShortDateString());  // 更新年月日
                sCom.Parameters.AddWithValue("@ID", sID);   // ID

                // テーブル書き込み
                sCom.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票ヘッダ", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 出勤簿ヘッダレコードの画像ファイル名を書き換える
        /// </summary>
        /// <param name="img">画像名</param>
        /// <param name="sID">ID</param>
        public void HeadUpdate(string img, string sID)
        {
            try
            {
                // 勤務票ヘッダ
                sb.Clear();
                sb.Append("update 勤務票ヘッダ set 画像名=? ");
                sb.Append("where ID=?");

                sCom.CommandText = sb.ToString();
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@img", img);
                sCom.Parameters.AddWithValue("@ID", sID);

                // テーブル書き込み
                sCom.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票ヘッダ", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 勤務票データを削除する（ヘッダ、明細両方）
        /// </summary>
        /// <param name="_year">年</param>
        /// <param name="_month">月</param>
        /// <param name="_StaffCode">個人番号</param>
        public void DataDelete(string _year, string _month, string _StaffCode)
        {
            string sID = string.Empty;

            // 画像ファイル名を取得
            OleDbDataReader dR = HeaderSelect(_year, _month, _StaffCode);
            string sImgNm = string.Empty;

            while (dR.Read())
            {
                sID = dR["ID"].ToString();
            }
            dR.Close();

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 勤務票ヘッダ
                sCom.CommandText = "delete from 勤務票ヘッダ where ID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                sCom.ExecuteNonQuery();

                //勤務票明細データを削除します
                sCom.CommandText = "delete from 勤務票明細 where ヘッダID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                sCom.ExecuteNonQuery();

                ////////画像ファイルを削除する
                //////if (System.IO.File.Exists(_InPath + sImgNm))
                //////{
                //////    System.IO.File.Delete(_InPath + sImgNm);
                //////}

                // トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票データ削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                
                // トランザクションロールバック
                sTran.Rollback(); 
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 勤務票データを削除する（ヘッダ、明細両方）
        /// </summary>
        /// <param name="_ID">ヘッダID</param>
        public void DataDelete(string _ID)
        {
            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 勤務票ヘッダ
                sCom.CommandText = "delete from 勤務票ヘッダ where ID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", _ID);
                sCom.ExecuteNonQuery();

                //勤務票明細データを削除します
                sCom.CommandText = "delete from 勤務票明細 where ヘッダID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", _ID);
                sCom.ExecuteNonQuery();

                ////////画像ファイルを削除する
                //////if (System.IO.File.Exists(_InPath + sImgNm))
                //////{
                //////    System.IO.File.Delete(_InPath + sImgNm);
                //////}

                // トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票データ削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // トランザクションロールバック
                sTran.Rollback();
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        ///-------------------------------------------------------------
        /// <summary>
        ///     基準年月以前の過去勤務票データを削除する : 2017/09/07 </summary>
        /// <param name="sID">
        ///     ヘッダID</param>
        /// <param name="_InPath">
        ///     画像ファイルフォルダパス</param>
        ///-------------------------------------------------------------
        public void pastDataDelete(int dYYMM)
        {
            string sID = string.Empty;

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 基準年月以前の過去データを取得します
                OleDbDataReader dR = pastHeaderSelect(dYYMM);
                string sImgNm = string.Empty;

                string[] sKey = new string[1];

                int iX = 0;

                while (dR.Read())
                {
                    // IDキー配列作成
                    if (iX > 0) 
                    {
                        Array.Resize(ref sKey, iX + 1);
                    }

                    sKey[iX] = dR["ID"].ToString();

                    iX++;
                }
                dR.Close();

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[i] != null)
                    {
                        // 過去勤務票明細データを削除します : 2017/09/07
                        sCom.CommandText = "delete from 過去勤務票明細 where ヘッダID = ?";
                        sCom.Parameters.Clear();
                        sCom.Parameters.AddWithValue("@ID", sKey[i]);
                        sCom.ExecuteNonQuery();
                    }
                }

                // 過去勤務票ヘッダデータを削除します : 2017/09/07
                sCom.CommandText = "delete from 過去勤務票ヘッダ where 年*100+月 <= ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@YearMonth", dYYMM);
                sCom.ExecuteNonQuery();

                // トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "過去勤務票ＭＤＢデータ削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // トランザクションロールバック
                sTran.Rollback();
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        ///----------------------------------------------------------
        /// <summary>
        ///     基準年月以前の勤務票データを削除する : 2017/09/07 </summary>
        /// <param name="sID">
        ///     ヘッダID</param>
        /// <param name="_InPath">
        ///     画像ファイルフォルダパス</param>
        ///----------------------------------------------------------
        public void comDataDelete(int dYYMM)
        {
            string sID = string.Empty;

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 基準年月以前の過去データを取得します
                OleDbDataReader dR = comHeaderSelect(dYYMM);
                string sImgNm = string.Empty;

                string[] sKey = new string[1];

                int iX = 0;

                while (dR.Read())
                {
                    // IDキー配列作成
                    if (iX > 0)
                    {
                        Array.Resize(ref sKey, iX + 1);
                    }

                    sKey[iX] = dR["ID"].ToString();

                    iX++;
                }
                dR.Close();

                for (int i = 0; i < sKey.Length; i++)
                {
                    if (sKey[i] != null)
                    {
                        // 勤務票明細データを削除します
                        sCom.CommandText = "delete from 勤務票明細 where ヘッダID = ?";
                        sCom.Parameters.Clear();
                        sCom.Parameters.AddWithValue("@ID", sKey[i]);
                        sCom.ExecuteNonQuery();
                    }
                }

                // 勤務票ヘッダデータを削除します
                sCom.CommandText = "delete from 勤務票ヘッダ where 年*100+月 <= ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@YearMonth", dYYMM);
                sCom.ExecuteNonQuery();

                // トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票ＭＤＢデータ削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // トランザクションロールバック
                sTran.Rollback();
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 勤務表明細新規登録
        /// </summary>
        /// <param name="shID">ヘッダＩＤ</param>
        /// <param name="sDay">日付</param>
        /// <param name="sHol">休日マーク</param>
        /// <param name="sKigou">勤怠記号</param>
        /// <param name="sSh">開始時</param>
        /// <param name="sSm">開始分</param>
        /// <param name="sEh">終了時</param>
        /// <param name="sEm">終了分</param>
        /// <param name="sRh">休憩時</param>
        /// <param name="sRm">休憩分</param>
        /// <param name="sWh">実働時</param>
        /// <param name="sWm">実働分</param>
        /// <param name="sKoutsuhi">交通費計</param>
        /// <param name="steisei">訂正</param>
        public void ItemInsert(string shID, string sDay, string sHol, string sKigou, string sSh, string sSm, 
                               string sEh, string sEm, string sRh, string sRm, string sWh, string sWm, 
                               string sKoutsuhi, string steisei)
        {
            try
            {
                // 勤務票明細
                sb.Clear();
                sb.Append("insert into 勤務票明細 ");
                sb.Append("(ヘッダID,日付,休日マーク,勤怠記号,開始時,開始分,終了時,終了分,休憩時,休憩分,実働時,実働分,交通費,訂正,更新年月日) ");
                sb.Append("values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
                sCom.CommandText = sb.ToString();

                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@hID", shID);             // ヘッダID
                sCom.Parameters.AddWithValue("@Days", sDay);            // 日付 
                sCom.Parameters.AddWithValue("@Hol", sHol);             // 休日マーク 
                sCom.Parameters.AddWithValue("@kigou", sKigou);         // 勤怠記号 
                sCom.Parameters.AddWithValue("@sh", sSh);               // 開始・時間
                sCom.Parameters.AddWithValue("@sm", sSm);               // 開始・分
                sCom.Parameters.AddWithValue("@eh", sEh);               // 終了・時間
                sCom.Parameters.AddWithValue("@em", sEm);               // 終了・分
                sCom.Parameters.AddWithValue("@rh", sRh);               // 休憩・時間
                sCom.Parameters.AddWithValue("@rm", sRm);               // 休憩・分
                sCom.Parameters.AddWithValue("@wh", sWh);               // 実働・時間
                sCom.Parameters.AddWithValue("@wm", sWm);               // 実働・分
                sCom.Parameters.AddWithValue("@Koutsuhi", sKoutsuhi);   // 交通費
                sCom.Parameters.AddWithValue("@teisei", steisei);       // 訂正
                sCom.Parameters.AddWithValue("@Date", DateTime.Today.ToShortDateString());  // 更新年月日

                // テーブル書き込み
                sCom.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票明細", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 勤務表明細更新
        /// </summary>
        /// <param name="shMark">休日マーク</param>
        /// <param name="sKigou">勤怠記号</param>
        /// <param name="sSh">開始時</param>
        /// <param name="sSm">開始分</param>
        /// <param name="sEh">終了時</param>
        /// <param name="sEm">終了分</param>
        /// <param name="sRh">休憩時</param>
        /// <param name="sRm">休憩分</param>
        /// <param name="sWh">実働時</param>
        /// <param name="sWm">実働分</param>
        /// <param name="sKoutsuhi">交通費計</param>
        /// <param name="steisei">訂正</param>
        /// <param name="sID">ID</param>
        public void ItemUpdate(string shMark, string sKigou, string sSh, string sSm, string sEh, string sEm, 
                               string sRh, string sRm, string sWh, string sWm, string sKoutsuhi, string steisei, 
                               int sID)
        {
            try
            {
                // 勤務票明細
                sb.Clear();
                sb.Append("update 勤務票明細 set ");
                sb.Append("休日マーク=?,勤怠記号=?,開始時=?,開始分=?,終了時=?,終了分=?,休憩時=?,休憩分=?,");
                sb.Append("実働時=?,実働分=?,交通費=?,訂正=?,更新年月日=? ");
                sb.Append("where ID=? ");
                sCom.CommandText = sb.ToString();

                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@mark", shMark);          // 休日マーク
                sCom.Parameters.AddWithValue("@kigou", sKigou);         // 勤怠記号
                sCom.Parameters.AddWithValue("@sh", sSh);               // 開始・時間
                sCom.Parameters.AddWithValue("@sm", sSm);               // 開始・分
                sCom.Parameters.AddWithValue("@eh", sEh);               // 終了・時間
                sCom.Parameters.AddWithValue("@em", sEm);               // 終了・分
                sCom.Parameters.AddWithValue("@rh", sRh);               // 休憩・時間
                sCom.Parameters.AddWithValue("@rm", sRm);               // 休憩・分
                sCom.Parameters.AddWithValue("@wh", sWh);               // 実働・時間
                sCom.Parameters.AddWithValue("@wm", sWm);               // 実働・分
                sCom.Parameters.AddWithValue("@koutsuhi", sKoutsuhi);   // 交通費計
                sCom.Parameters.AddWithValue("@teisei", steisei);       // 訂正
                sCom.Parameters.AddWithValue("@Date", DateTime.Today.ToShortDateString());  // 更新年月日
                sCom.Parameters.AddWithValue("@ID", sID);               // ID

                // テーブル書き込み
                sCom.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票明細", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        ///-----------------------------------------------------------------
        /// <summary>
        ///     ＣＳＶデータをＭＤＢに登録する </summary>
        /// <param name="_InPath">
        ///     CSVデータパス</param>
        /// <param name="frmP">
        ///     プログレスバーフォームオブジェクト</param>
        /// <param name="dbName">
        ///     会社領域データベース名</param>
        ///-----------------------------------------------------------------
        public void CsvToMdb(string _InPath, frmPrg frmP, string dbName)
        {
            string headerKey = string.Empty;    // ヘッダキー
            string prnKBN = string.Empty;       // 申請書ID
            string sName = string.Empty;        // 社員名
            string sShozoku = string.Empty;     // 所属名
            SqlDataReader dr = null;

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 対象CSVファイル数を取得
                string [] t = System.IO.Directory.GetFiles(_InPath, "*.csv");
                int cLen = t.Length;

                //CSVデータをMDBへ取込
                int cCnt = 0;
                foreach (string files in System.IO.Directory.GetFiles(_InPath, "*.csv"))
                {
                    //件数カウント
                    cCnt++;

                    //プログレスバー表示
                    frmP.Text = "OCR変換CSVデータロード中　" + cCnt.ToString() + "/" + cLen.ToString();
                    frmP.progressValue = cCnt * 100 / cLen;
                    frmP.ProgressStep();

                    ////////OCR処理対象のCSVファイルかファイル名の文字数を検証する
                    //////string fn = Path.GetFileName(files);

                    int sDays = 0;

                    // CSVファイルインポート
                    var s = System.IO.File.ReadAllLines(files, Encoding.Default);
                    foreach (var stBuffer in s)
                    {
                        // カンマ区切りで分割して配列に格納する
                        string[] stCSV = stBuffer.Split(',');

                        // ヘッダ行
                        if (stCSV[0] == "*")
                        {
                            headerKey = Utility.GetStringSubMax(stCSV[1].Trim(), 17);   // ヘッダーキー取得
                            prnKBN = Utility.GetStringSubMax(stCSV[2].Trim(), 1);       // 申請書ID取得
                            string sNo = Utility.GetStringSubMax(stCSV[3].Trim(), 6);   // 個人番号
                            sName = string.Empty;        // 社員名
                            sShozoku = string.Empty;     // 所属名

                            // 社員（パート・アルバイト）情報を取得します
                            if (sNo != string.Empty)
                            {
                                //dbControl.DataControl dCon = new dbControl.DataControl(dbName);
                                //dr = dCon.GetEmployeeBase(global.cnfYear.ToString(), global.cnfMonth.ToString(), sNo);

                                // 勘定奉行データベース接続文字列を取得する 2017/09/07
                                string sc = SqlControl.obcConnectSting.get(dbName);

                                // 勘定奉行データベースに接続する 2017/09/04
                                SqlControl.DataControl dCon = new SqlControl.DataControl(sc);
                                
                                dr = dCon.GetEmployeeBase(global.cnfYear.ToString(), global.cnfMonth.ToString(), sNo);

                                while (dr.Read())
                                {
                                    //社員名、所属名取得
                                    sShozoku = dr["DepartmentName"].ToString().Trim();
                                    sName = dr["Name"].ToString().Trim();
                                }
                                dr.Close();
                                dCon.Close();
                            }

                            // MDBへ登録する：勤務記録ヘッダテーブル
                            if (prnKBN == global.SHAIN_ID)    // 社員申請書
                            {
                                HeadInsert(headerKey, prnKBN,
                                           sNo,
                                           sName,
                                           Utility.GetStringSubMax(stCSV[4].Trim().Replace("-", ""), 2),
                                           Utility.GetStringSubMax(stCSV[5].Trim().Replace("-", ""), 2),
                                           sShozoku,
                                           headerKey + ".tif",
                                           dbName,
                                           "0");
                            }
                            else if (prnKBN == global.PART_ID)    // パート申請書
                            {
                                HeadInsert(headerKey, prnKBN,
                                           sNo,
                                           sName,
                                           Utility.GetStringSubMax(stCSV[4].Trim().Replace("-", ""), 2),
                                           Utility.GetStringSubMax(stCSV[5].Trim().Replace("-", ""), 2),
                                           sShozoku,
                                           headerKey + ".tif",
                                           dbName,
                                           Utility.StrtoInt(Utility.GetStringSubMax(stCSV[6].Trim().Replace("-", ""), 5)).ToString());
                            }
                        }
                        else
                        {
                            // 勤務票明細テーブル
                            DateTime dt;

                            sCom.Parameters.Clear();
                            sDays++;

                            // 存在する日付のときにMDBへ登録する
                            string tempDt = (global.cnfYear + Properties.Settings.Default.RekiHosei).ToString() + "/" + global.cnfMonth.ToString() + "/" + sDays.ToString();

                            if (DateTime.TryParse(tempDt, out dt))
                            {
                                if (prnKBN == global.SHAIN_ID)    // 社員申請書
                                {
                                    ItemInsert(headerKey,
                                                sDays.ToString(),
                                                Utility.GetStringSubMax(stCSV[0].Trim().Replace("-", ""), 1),
                                                Utility.GetStringSubMax(stCSV[1].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[2].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[3].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[4].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[5].Trim().Replace("-", ""), 2),
                                                string.Empty,string.Empty,
                                                Utility.GetStringSubMax(stCSV[6].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[7].Trim().Replace("-", ""), 2),
                                                "0",
                                                Utility.GetStringSubMax(stCSV[8].Trim(), 1));
                                }
                                else if (prnKBN == global.PART_ID)    // パート申請書
                                {
                                    ItemInsert(headerKey,
                                                sDays.ToString(),
                                                Utility.GetStringSubMax(stCSV[0].Trim().Replace("-", ""), 1),
                                                Utility.GetStringSubMax(stCSV[1].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[2].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[3].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[4].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[5].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[6].Trim().Replace("-", ""), 1),
                                                Utility.GetStringSubMax(stCSV[7].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[8].Trim().Replace("-", ""), 2),
                                                Utility.GetStringSubMax(stCSV[9].Trim().Replace("-", ""), 2),
                                                Utility.StrtoInt(Utility.GetStringSubMax(stCSV[10].Trim().Replace("-", ""), 4)).ToString(),
                                                Utility.GetStringSubMax(stCSV[11].Trim(), 1));
                                }
                            }
                        }
                    }
                }

                // トランザクションコミット
                sTran.Commit();

                //CSVファイルを削除する
                foreach (string files in System.IO.Directory.GetFiles(_InPath, "*.csv"))
                {
                    System.IO.File.Delete(files);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票CSVインポート処理", MessageBoxButtons.OK);

                // トランザクションロールバック
                sTran.Rollback();
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        ///// <summary>
        ///// OCRDATAクラスから勤務票データをＭＤＢに登録する
        ///// </summary>
        ///// <param name="_InPath">CSVデータパス</param>
        ///// <param name="frmP">プログレスバーフォームオブジェクト</param>
        ///// <param name="cLen">CSVデータ件数</param>
        //public void ClsToMdb(OCRData [] ocr, int idx)
        //{
        //    //トランザクション開始
        //    OleDbTransaction sTran = null;
        //    sTran = sCom.Connection.BeginTransaction();
        //    sCom.Transaction = sTran;

        //    try
        //    {
        //        // MDBへ登録する
        //        // 勤務記録ヘッダテーブル
        //        int yy = int.Parse(ocr[idx]._Year) + Properties.Settings.Default.RekiHosei;
        //        HeadInsert(ocr[idx].OCRDATA_ID, ocr[idx]._SheetID, ocr[idx]._StaffCode, yy.ToString(),
        //                    ocr[idx]._Month, ocr[idx]._OrderCode, ocr[idx]._ShuDays, ocr[idx]._ImageName);

        //        // 勤務票明細テーブル
        //        DateTime dt;

        //        for (int i = 0; i < global._MULTIGYO; i++)
        //        {
        //            // 存在する日付のときにMDBへ登録する
        //            string tempDt = (global.sYear + Properties.Settings.Default.RekiHosei).ToString() + "/" + global.sMonth.ToString() + "/" + (i + 1).ToString();
        //            if (DateTime.TryParse(tempDt, out dt))
        //            {
        //                ItemInsert(ocr[idx].OCRDATA_ID,
        //                    (i + 1).ToString(),
        //                    ocr[idx].itm[i]._Sh,
        //                    ocr[idx].itm[i]._Sm,
        //                    ocr[idx].itm[i]._eh,
        //                    ocr[idx].itm[i]._em,
        //                    ocr[idx].itm[i]._Kyuka,
        //                    ocr[idx].itm[i]._Kyukei,
        //                    ocr[idx].itm[i]._teisei);
        //            }                            
        //        }

        //        // トランザクションコミット
        //        sTran.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "勤務票インポート処理", MessageBoxButtons.OK);

        //        // トランザクションロールバック
        //        sTran.Rollback();
        //    }
        //    finally
        //    {
        //        if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
        //    }
        //}

        /// <summary>
        /// MDBヘッダデータの件数をカウントする
        /// </summary>
        /// <returns>件数</returns>
        public int CountMDB()
        {
            int rCnt = 0;
            OleDbDataReader dR;
            sCom.CommandText = "select count(ID) as cnt from 勤務票ヘッダ";
            dR = sCom.ExecuteReader();

            while (dR.Read())
            {
                //データ件数取得
                rCnt = int.Parse(dR["cnt"].ToString());
                //rCnt++;
            }

            dR.Close();
            //sCom.Connection.Close();

            return rCnt;
        }

        /// <summary>
        /// MDB明細データの件数をカウントする
        /// </summary>
        /// <returns>件数</returns>
        public int CountItemMDB()
        {
            int rCnt = 0;
            OleDbDataReader dR;
            sCom.CommandText = "select count(ID) as cnt from 勤務票明細";
            dR = sCom.ExecuteReader();

            while (dR.Read())
            {
                //データ件数取得
                rCnt = int.Parse(dR["cnt"].ToString());
            }

            dR.Close();

            return rCnt;
        }
        
        /// <summary>
        /// 勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <returns>データリーダー</returns>
        public OleDbDataReader HeaderSelect()
        {
            OleDbDataReader dR;
            sCom.CommandText = "select * from 勤務票ヘッダ order by ID";
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 指定したIDの勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns>データリーダー</returns>
        public OleDbDataReader HeaderSelect(string sID)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select 勤務票ヘッダ.* from 勤務票ヘッダ ");
            sb.Append("where 勤務票ヘッダ.ID=?");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", sID);
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 指定した年・月・スタッフコードの勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns>データリーダー</returns>
        public OleDbDataReader HeaderSelect(string _sYear, string _sMonth, string _sStaffCode)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 勤務票ヘッダ ");
            sb.Append("where 年=? and 月=? and 個人番号=?");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", _sYear);
            sCom.Parameters.AddWithValue("@2", _sMonth);
            sCom.Parameters.AddWithValue("@3", _sStaffCode);
            dR = sCom.ExecuteReader();

            return dR;
        }
        
        ///---------------------------------------------------------------------
        /// <summary>
        ///     指定した年月以前の過去勤務票ヘッダのデータリーダーを取得する </summary>
        /// <param name="dYYMM">
        ///     年月</param>
        /// <returns>
        ///     データリーダー</returns>
        ///---------------------------------------------------------------------
        public OleDbDataReader pastHeaderSelect(int dYYMM)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 過去勤務票ヘッダ ");
            sb.Append("where 年*100+月 <= ?");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", dYYMM);
            dR = sCom.ExecuteReader();

            return dR;
        }

        ///---------------------------------------------------------------------
        /// <summary>
        ///     指定した年月以前の勤務票ヘッダのデータリーダーを取得する : 2017/09/07</summary>
        /// <param name="dYYMM">
        ///     年月</param>
        /// <returns>
        ///     データリーダー</returns>
        ///---------------------------------------------------------------------
        public OleDbDataReader comHeaderSelect(int dYYMM)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 勤務票ヘッダ ");
            sb.Append("where 年*100+月 <= ?");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", dYYMM);
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 過去勤務票ヘッダデータリーダーより所属名を取得する
        /// </summary>
        /// <returns>データリーダー</returns>
        public OleDbDataReader HeaderShozokuSelect()
        {
            OleDbDataReader dR;

            // 過去出勤簿ヘッダデータリーダーより所属名を取得する
            sb.Clear();
            sb.Append("select distinct 所属名 from 過去勤務票ヘッダ ");
            sCom.CommandText = sb.ToString();
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 指定したIDの勤務表明細のデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns>データリーダー</returns>
        public OleDbDataReader itemSelect(string sID)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select 勤務票明細.* from 勤務票明細 ");
            sb.Append("where ヘッダID=? order by ID");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", sID);
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 個人番号付きの勤務表明細のデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns>データリーダー</returns>
        public OleDbDataReader itemSelect()
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select 勤務票ヘッダ.個人番号,勤務票ヘッダ.年,勤務票ヘッダ.月,勤務票ヘッダ.申請書種別,勤務票明細.* from ");
            sb.Append("勤務票ヘッダ inner join 勤務票明細 ");
            sb.Append("on 勤務票ヘッダ.ID = 勤務票明細.ヘッダID ");
            sb.Append("order by 勤務票明細.ID");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            dR = sCom.ExecuteReader();

            return dR;
        }
        
        /// <summary>
        /// 指定した年・月・スタッフコードの過去勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <param name="_sYear">年</param>
        /// <param name="_sMonth">月</param>
        /// <param name="_sStaffCode">スタッフコード</param>
        /// <param name="_dbName">データ領域名</param>
        /// <returns>データリーダー</returns>
        public OleDbDataReader lastHeaderSelect(string _sYear, string _sMonth, string _sStaffCode, string _dbName)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 過去勤務票ヘッダ ");
            sb.Append("where 年=? and 月=? and 個人番号=? and データ領域名=?");

            sCom.CommandText = sb.ToString();
            sCom.Parameters.Clear();
            sCom.Parameters.AddWithValue("@1", _sYear);
            sCom.Parameters.AddWithValue("@2", _sMonth);
            sCom.Parameters.AddWithValue("@3", _sStaffCode);
            sCom.Parameters.AddWithValue("@4", _dbName);
            dR = sCom.ExecuteReader();

            return dR;
        }
        
        /// <summary>
        /// 指定した年・月・所属名の過去勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <param name="_sYear">年</param>
        /// <param name="_sMonth">月</param>
        /// <param name="_SzName">所属名</param>
        /// <returns></returns>
        public OleDbDataReader lastHeaderSelect(int _sYear, int _sMonth, string _SzName, string _DBName)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 過去勤務票ヘッダ where データ領域名 = '" + _DBName + "' ");
            if (_sYear != 0) sb.Append("and 年 = " + _sYear + " "); // 年
            if (_sMonth != 0) sb.Append("and 月 = " + _sMonth + " "); // 月
            if (_SzName != string.Empty) sb.Append("and 所属名 = '" + _SzName + "' ");     // 所属名
            sb.Append("order by 年, 月, 所属名, 個人番号");

            sCom.CommandText = sb.ToString();
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 指定したＩＤの過去勤務票ヘッダのデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns></returns>
        public OleDbDataReader lastHeaderSelect(string sID)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 過去勤務票ヘッダ where ID = '" + sID + "' ");
            sCom.CommandText = sb.ToString();
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 指定したヘッダIDの過去勤務票明細のデータリーダーを取得する
        /// </summary>
        /// <param name="sID">ID</param>
        /// <returns></returns>
        public OleDbDataReader lastItemSelect(string sID)
        {
            OleDbDataReader dR;

            sb.Clear();
            sb.Append("select * from 過去勤務票明細 where ヘッダID = '" + sID + "' ");
            sb.Append("order by ID");
            sCom.CommandText = sb.ToString();
            dR = sCom.ExecuteReader();

            return dR;
        }

        /// <summary>
        /// 過去勤務票データを削除する（ヘッダ、明細両方）
        /// </summary>
        /// <param name="_year">年</param>
        /// <param name="_month">月</param>
        /// <param name="_StaffCode">個人番号</param>
        public void lastDataDelete(string _year, string _month, string _StaffCode, string dbName)
        {
            string sID = string.Empty;

            // データリーダーを取得
            OleDbDataReader dR = lastHeaderSelect(_year, _month, _StaffCode, dbName);
            string sImgNm = string.Empty;

            while (dR.Read())
            {
                sID = dR["ID"].ToString();
            }
            dR.Close();

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = sCom.Connection.BeginTransaction();
            sCom.Transaction = sTran;

            try
            {
                // 過去勤務票ヘッダ削除
                sCom.CommandText = "delete from 過去勤務票ヘッダ where ID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                sCom.ExecuteNonQuery();

                // 過去勤務票明細削除
                sCom.CommandText = "delete from 過去勤務票明細 where ヘッダID = ?";
                sCom.Parameters.Clear();
                sCom.Parameters.AddWithValue("@ID", sID);
                sCom.ExecuteNonQuery();

                // トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "勤務票データ削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // トランザクションロールバック
                sTran.Rollback();
                return;
            }
            finally
            {
                //if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 過去出勤簿ヘッダデータ作成
        /// </summary>
        public void lastHeadInsert()
        {
            // 過去出勤簿ヘッダレコードを作成します
            sb.Clear();
            sb.Append("insert into 過去勤務票ヘッダ ");
            sb.Append("select * from 勤務票ヘッダ ");
            sCom.CommandText = sb.ToString();
            sCom.ExecuteNonQuery();
        }

        /// <summary>
        /// 和暦の過去出勤簿ヘッダの年を西暦に変換
        /// </summary>
        public void lastHeadYearRekiHenkan()
        {
            // 和暦の過去出勤簿ヘッダの年を西暦に変換
            sb.Clear();
            sb.Append("update 過去勤務票ヘッダ ");
            sb.Append("set 年 = 年 + " + Properties.Settings.Default.RekiHosei.ToString() + ",");
            sb.Append("更新年月日 = '" + DateTime.Now.ToString() + "'" );
            sb.Append(" where 年 < 100 ");
            sCom.CommandText = sb.ToString();
            sCom.ExecuteNonQuery();
        }

        /// <summary>
        /// 過去出勤簿明細データ作成
        /// </summary>
        public void lastItemInsert()
        {
            // 出勤簿明細のデータリーダーを取得します
            OleDbDataReader dR = itemSelect();

            // 過去出勤簿明細レコード作成用SQL文定義
            sb.Clear();
            sb.Append("insert into 過去勤務票明細 (");
            sb.Append("ヘッダID,日付,休日マーク,勤怠記号,開始時,開始分,終了時,終了分,休憩時,休憩分,");
            sb.Append("実働時,実働分,交通費,訂正,更新年月日) ");
            sb.Append("values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");

            // 過去出勤簿明細レコード作成用のコネクション
            OCRData r = new OCRData();
            r.dbConnect();
            r.sCom.CommandText = sb.ToString();

            // 過去出勤簿明細レコードを作成します
            string _hd = string.Empty;
            while (dR.Read())
            {
                // ヘッダIDの検証
                r.sCom.Parameters.Clear();
                r.sCom.Parameters.AddWithValue("@HDID", dR["ヘッダID"].ToString());
                r.sCom.Parameters.AddWithValue("@DAY", dR["日付"].ToString());
                r.sCom.Parameters.AddWithValue("@hDAY", dR["休日マーク"].ToString());
                r.sCom.Parameters.AddWithValue("@YU", dR["勤怠記号"].ToString());
                r.sCom.Parameters.AddWithValue("@T4", dR["開始時"].ToString());
                r.sCom.Parameters.AddWithValue("@T5", dR["開始分"].ToString());
                r.sCom.Parameters.AddWithValue("@T6", dR["終了時"].ToString());
                r.sCom.Parameters.AddWithValue("@T7", dR["終了分"].ToString());
                r.sCom.Parameters.AddWithValue("@T8", dR["休憩時"].ToString());
                r.sCom.Parameters.AddWithValue("@T9", dR["休憩分"].ToString());
                r.sCom.Parameters.AddWithValue("@T10", dR["実働時"].ToString());
                r.sCom.Parameters.AddWithValue("@T11", dR["実働分"].ToString());
                r.sCom.Parameters.AddWithValue("@T12", dR["交通費"].ToString());
                r.sCom.Parameters.AddWithValue("@T13", dR["訂正"].ToString());
                r.sCom.Parameters.AddWithValue("@UPDAY", DateTime.Today.ToShortDateString());
                r.sCom.ExecuteNonQuery();
            }
            dR.Close();

            r.sCom.Connection.Close();
        }
    }
}
