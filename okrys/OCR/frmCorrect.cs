using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using okrys.Common;

namespace okrys.OCR
{
    public partial class frmCorrect : Form
    {
        ///-----------------------------------------------------------
        /// <summary>
        ///     コンストラクタ : 
        ///     大蔵屋商事・勤怠 勘定奉行i10 2017/09/04 </summary>
        /// <param name="Ryouiki">
        ///     会社領域コード</param>
        /// <param name="DBName">
        ///     会社領域データベース名</param>
        /// <param name="sID">
        ///     処理モード</param>
        ///-----------------------------------------------------------
        public frmCorrect(string DBName, string ComName, string sID)
        {
            InitializeComponent();
            dID = sID;              // 処理モード
            _PCADBName = DBName;    // 会社領域データベース名
            _PCAComName = ComName;  // 会社名
        }

        // DataGridView表示行数
        const int _MULTIGYO = 31;

        // MDBデータキー配列
        string[] sID;

        //カレントデータインデックス
        int cI;

        // 社員マスターより取得した所属コード
        string mSzCode = string.Empty;

        //終了ステータス
        const string END_BUTTON = "btn";
        const string END_MAKEDATA = "data";
        const string END_CONTOROL = "close";

        //bool bDrag = false;
        //Point posStart;

        string dID = string.Empty;                  // 表示する過去データのID
        int _Ryouiki = 0;                           // 会社領域
        string _PCADBName = string.Empty;           // 会社領域データベース名
        string _PCAComName = string.Empty;          // 会社名
        int _YakushokuType = 0;                     // 表示社員の役職タイプ（１：パート、１以外：社員）
        int _ShainID = 0;                           // 社員ＩＤ

        private void frmCorrect_Load(object sender, EventArgs e)
        {
            this.pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            // フォーム最大値
            Utility.WindowsMaxSize(this, this.Width, this.Height);

            // フォーム最小値
            Utility.WindowsMinSize(this, this.Width, this.Height);

            // データグリッド定義
            GridviewSet.Setting(dataGridView1);

            //元号を取得
            label1.Text = Properties.Settings.Default.gengou;

            // 勤務データ登録
            if (dID == string.Empty)
            {
                // CSVデータをMDBへ読み込む
                GetCsvDataToMDB();

                // MDB件数カウント
                if (CountMDB() == 0)
                {
                    MessageBox.Show("対象となる出勤簿データがありません", "出勤簿データ登録", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Environment.Exit(0);     //終了処理
                }

                // キャプション
                this.Text = "勤務票データ登録【" + _PCAComName + "】";
            }
            else
            {
                // キャプション
                this.Text = "過去勤務票データ表示【" + _PCAComName + "】";
            }
            
            // MDBデータキー項目読み込み
            sID = LoadMdbID();

            // エラー情報初期化
            ErrInitial();

            // 最初のレコードを表示
            cI = 0;
            DataShow(cI, sID, this.dataGridView1);

            // tagを初期化
            this.Tag = string.Empty;
        }

        // カラム定義
        private static string cDay = "col1";
        private static string cWeek = "col2";
        private static string cKyuka = "colKyuka";
        private static string cKintai = "col3";
        private static string cSH = "col4";     // 開始時
        private static string cSE = "col16";
        private static string cSM = "col5";     // 開始分
        private static string cEH = "col6";     // 終了時
        private static string cEE = "col17";
        private static string cEM = "col7";     // 終了時
        private static string cKKH = "col8";    // 休憩時
        private static string cKKE = "col18";
        private static string cKKM = "col9";    // 休憩分
        //private static string cKSH = "col10";   
        //private static string cKSE = "col19";
        //private static string cKSM = "col11";
        private static string cTH = "col12";    // 実働時
        private static string cTE = "col20";
        private static string cTM = "col13";    // 実働分
        private static string cKoutsuhi = "colKoutsuhi";    // 交通費
        private static string cCheck = "col14";
        private static string cID = "col15";

        // データグリッドビュークラス
        private class GridviewSet
        {
            /// <summary>
            /// データグリッドビューの定義を行います
            /// </summary>
            /// <param name="tempDGV">データグリッドビューオブジェクト</param>
            public static void Setting(DataGridView tempDGV)
            {
                try
                {
                    //フォームサイズ定義

                    // 列スタイルを変更する

                    tempDGV.EnableHeadersVisualStyles = false;

                    // 列ヘッダー表示位置指定
                    tempDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;

                    // 列ヘッダーフォント指定
                    tempDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Meiryo UI", 9, FontStyle.Regular);

                    // データフォント指定
                    tempDGV.DefaultCellStyle.Font = new Font("Meiryo UI", (Single)9.5, FontStyle.Regular);

                    // 行の高さ
                    tempDGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                    tempDGV.ColumnHeadersHeight = 22;
                    tempDGV.RowTemplate.Height = 22;

                    // 全体の高さ
                    tempDGV.Height = 706;
                    // 全体の幅
                    tempDGV.Width = 480;

                    // 奇数行の色
                    //tempDGV.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;

                    //各列幅指定
                    tempDGV.Columns.Add(cDay, "日");
                    tempDGV.Columns.Add(cWeek, "曜");

                    DataGridViewCheckBoxColumn cColumn = new DataGridViewCheckBoxColumn();
                    tempDGV.Columns.Add(cColumn);
                    tempDGV.Columns[2].Name = cKyuka;
                    tempDGV.Columns[2].HeaderText = "休";

                    tempDGV.Columns.Add(cKintai, "記");
                    tempDGV.Columns.Add(cSH, "開");
                    tempDGV.Columns.Add(cSE, "");
                    tempDGV.Columns.Add(cSM, "始");
                    tempDGV.Columns.Add(cEH, "終");
                    tempDGV.Columns.Add(cEE, "");
                    tempDGV.Columns.Add(cEM, "了");
                    tempDGV.Columns.Add(cKKH, "休");
                    tempDGV.Columns.Add(cKKE, "");
                    tempDGV.Columns.Add(cKKM, "憩");
                    tempDGV.Columns.Add(cTH, "実");
                    tempDGV.Columns.Add(cTE, "");
                    tempDGV.Columns.Add(cTM, "働");
                    tempDGV.Columns.Add(cKoutsuhi, "交通費");

                    DataGridViewCheckBoxColumn cColumn2 = new DataGridViewCheckBoxColumn();
                    tempDGV.Columns.Add(cColumn2);
                    tempDGV.Columns[17].Name = cCheck;
                    tempDGV.Columns[17].HeaderText = "訂";

                    tempDGV.Columns.Add(cID, "");   // 明細ID
                    tempDGV.Columns[cID].Visible = false;

                    foreach (DataGridViewColumn c in tempDGV.Columns)
                    {
                        // 幅                       
                        if (c.Name == cSE || c.Name == cEE || c.Name == cKKE ||
                            c.Name == cTE) c.Width = 10;
                        else if (c.Name == cKoutsuhi) c.Width = 60;
                        else c.Width = 28;

                        tempDGV.Columns[cCheck].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        
                        // 表示位置
                        if (c.Index < 4 || c.Name == cSE || c.Name == cEE || c.Name == cKKE || c.Name == cTE) 
                            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
                        else c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomRight;

                        if (c.Name == cSH || c.Name == cEH || c.Name == cKKH || c.Name == cTH || c.Name == cKoutsuhi) 
                            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomRight;

                        if (c.Name == cSM || c.Name == cEM || c.Name == cKKM || c.Name == cTM)
                            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomLeft;

                        if (c.Name == cCheck) c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;

                        // 編集可否
                        if (c.Index < 2 || c.Name == cSE || c.Name == cEE || c.Name == cKKE ||c.Name == cTE) 
                            c.ReadOnly = true;
                        else c.ReadOnly = false;

                        // 区切り文字
                        if (c.Name == cSE || c.Name == cEE || c.Name == cKKE || c.Name == cTE) 
                            c.DefaultCellStyle.Font = new Font("ＭＳＰゴシック", 8, FontStyle.Regular); 
                    }

                    // 行ヘッダを表示しない
                    tempDGV.RowHeadersVisible = false;

                    // 選択モード
                    tempDGV.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    tempDGV.MultiSelect = false;

                    // 編集可とする
                    //tempDGV.ReadOnly = false;

                    // 追加行表示しない
                    tempDGV.AllowUserToAddRows = false;

                    // データグリッドビューから行削除を禁止する
                    tempDGV.AllowUserToDeleteRows = false;

                    // 手動による列移動の禁止
                    tempDGV.AllowUserToOrderColumns = false;

                    // 列サイズ変更不可
                    tempDGV.AllowUserToResizeColumns = false;

                    // 行サイズ変更禁止
                    tempDGV.AllowUserToResizeRows = false;

                    // 行ヘッダーの自動調節
                    //tempDGV.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

                    //TAB動作
                    tempDGV.StandardTab = false;

                    // ソート禁止
                    foreach (DataGridViewColumn c in tempDGV.Columns)
                    {
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    //tempDGV.Columns[cDay].SortMode = DataGridViewColumnSortMode.NotSortable;

                    // 編集モード
                    tempDGV.EditMode = DataGridViewEditMode.EditOnEnter;

                    // 入力可能桁数
                    foreach (DataGridViewColumn c in tempDGV.Columns)
                    {
                        if (c.Name != cCheck && c.Name != cKyuka)
                        {
                            DataGridViewTextBoxColumn col = (DataGridViewTextBoxColumn)c;
                            if (c.Name == cKKH) col.MaxInputLength = 1;
                            else if (c.Name == cKoutsuhi) col.MaxInputLength = 4;
                            else col.MaxInputLength = 2;
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "エラーメッセージ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// CSVデータをMDBへインサートする
        /// </summary>
        private void GetCsvDataToMDB()
        {
            //CSVファイル数をカウント
            string[] inCsv = System.IO.Directory.GetFiles(Properties.Settings.Default.DataPath, "*.csv");

            //CSVファイルがなければ終了
            int cTotal = 0;
            if (inCsv.Length == 0) return;
            else cTotal = inCsv.Length;

            //オーナーフォームを無効にする
            this.Enabled = false;

            //プログレスバーを表示する
            frmPrg frmP = new frmPrg();
            frmP.Owner = this;
            frmP.Show();

            // データベースへ接続
            OCRData ocr = new OCRData();
            ocr.dbConnect();

            // MDB取り込み処理
            ocr.CsvToMdb(Properties.Settings.Default.DataPath, frmP, _PCADBName);

            // データベース接続解除
            if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();

            // いったんオーナーをアクティブにする
            this.Activate();

            // 進行状況ダイアログを閉じる
            frmP.Close();

            // オーナーのフォームを有効に戻す
            this.Enabled = true;
        }

        /// <summary>
        /// 出勤簿ヘッダデータの件数をカウントする
        /// </summary>
        /// <returns>データ件数</returns>
        private int CountMDB()
        {
            int c = 0;
            OCRData ocr = new OCRData();

            // データベース接続
            ocr.dbConnect();

            // 出勤簿ヘッダデータの件数を取得します
            c = ocr.CountMDB();

            // データベース接続解除
            if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();

            return c;            
        }

        /// <summary>
        /// MDB明細データの件数をカウントする
        /// </summary>
        /// <returns>レコード件数</returns>
        private int CountMDBitem()
        {
            int rCnt = 0;

            SysControl.SetDBConnect dCon = new SysControl.SetDBConnect();
            OleDbCommand sCom = new OleDbCommand();
            OleDbDataReader dR;
            string mySql = string.Empty;

            mySql += "select ID from 出勤簿明細 order by ID";

            sCom.CommandText = mySql;
            sCom.Connection = dCon.cnOpen();
            dR = sCom.ExecuteReader();

            while (dR.Read())
            {
                //データ件数加算
                rCnt++;
            }

            dR.Close();
            sCom.Connection.Close();

            return rCnt;
        }

        /// <summary>
        /// MDBデータのキー項目を配列に読み込む
        /// </summary>
        /// <returns>キー配列</returns>
        private string[] LoadMdbID()
        {
            //オーナーフォームを無効にする
            this.Enabled = false;

            //プログレスバーを表示する
            frmPrg frmP = new frmPrg();
            frmP.Owner = this;
            frmP.Show();

            //レコード件数取得
            int cTotal = CountMDB();
            string [] DenID = new string[1];
            int rCnt = 1;

            // ヘッダデータ取得
            OCRData ocr = new OCRData();
            ocr.dbConnect();
            OleDbDataReader dR = ocr.HeaderSelect();

            while (dR.Read())
            {
                //プログレスバー表示
                frmP.Text = "出勤簿データロード中　" + rCnt.ToString() + "/" + cTotal.ToString();
                frmP.progressValue = rCnt * 100 / cTotal;
                frmP.ProgressStep();

                //2件目以降は要素数を追加
                if (rCnt > 1) Array.Resize(ref DenID, rCnt);
                DenID[rCnt - 1] = dR["ID"].ToString();

                //データ件数加算
                rCnt++;
            }

            dR.Close();

            // データベース接続解除
            if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();

            // いったんオーナーをアクティブにする
            this.Activate();

            // 進行状況ダイアログを閉じる
            frmP.Close();

            // オーナーのフォームを有効に戻す
            this.Enabled = true;

            return DenID;
        }

        private void ErrInitial()
        {
            //エラー情報初期化
            lblErrMsg.Visible = false;
            global.errNumber = global.eNothing;     //エラー番号
            global.errMsg = string.Empty;           //エラーメッセージ
            lblErrMsg.Text = string.Empty;
        }

        //表示初期化
        private void dataGridInitial(DataGridView dgv)
        {
            txtYear.BackColor = Color.Empty;
            txtMonth.BackColor = Color.Empty;
            txtNo.BackColor = Color.Empty;

            txtYear.ForeColor = Color.Navy;
            txtMonth.ForeColor = Color.Navy;
            txtNo.ForeColor = Color.Navy;

            dgv.Rows.Clear();                                      //行数をクリア
            dgv.RowCount = _MULTIGYO;                              //行数を設定
            dgv.RowsDefaultCellStyle.ForeColor = Color.Navy;       //テキストカラーの設定
            dgv.DefaultCellStyle.SelectionBackColor = Color.Empty;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Navy;
            lblNoImage.Visible = false;
        }

        ///----------------------------------------------------------
        /// <summary>
        ///     出勤簿データ画面表示 </summary>
        /// <param name="sIx">
        ///     データインデックス</param>
        /// <param name="rRec">
        ///     出勤簿データ配列</param>
        /// <param name="dgv">
        ///     データグリッドビューオブジェクト</param>
        ///----------------------------------------------------------
        private void DataShow(int sIx, string[] rRec, DataGridView dgv)
        {
            string SqlStr = string.Empty;
            string PRN_Type = string.Empty;         // 申請書種別
            global.pblImageFile = string.Empty;     // 画像ファイル名

            // データグリッドビュー初期化
            dataGridInitial(this.dataGridView1);

            //データ表示背景色初期化
            dsColorInitial(this.dataGridView1);

            // OCRDATAクラスインスタンス生成
            OCRData ocr = new OCRData();
            OleDbDataReader dR = null;

            try
            {
                // MDB接続
                ocr.dbConnect();

                // 勤務票データ取得  
                if (dID == string.Empty)                 
                    dR = ocr.HeaderSelect(rRec[sIx]);   // 勤務票ヘッダデータ取得
                else dR = ocr.lastHeaderSelect(dID);    // 過去勤務票ヘッダデータ取得

                while (dR.Read())
                {
                    if (dID == string.Empty)
                        txtYear.Text = Utility.EmptytoZero(dR["年"].ToString());
                    else txtYear.Text = (int.Parse(Utility.EmptytoZero(dR["年"].ToString())) - Properties.Settings.Default.RekiHosei).ToString();

                    txtMonth.Text = Utility.EmptytoZero(dR["月"].ToString());

                    global.ChangeValueStatus = false;    // チェンジバリューステータス
                    txtNo.Text = string.Empty;
                    global.ChangeValueStatus = true;    // チェンジバリューステータス
                    txtNo.Text = Utility.EmptytoZero(dR["個人番号"].ToString());

                    PRN_Type = dR["申請書種別"].ToString();      // 勤怠申請書種別取得

                    if (dID != string.Empty)
                    {
                        lblName.Text = dR["氏名"].ToString();
                        lblShozoku.Text = dR["所属名"].ToString();
                        
                        // 勤怠申請書種別表示
                        //if (PRN_Type == global.SHAIN_ID)        // 社員
                        //    lblYakushoku.Text = "社員";
                        //else if (PRN_Type == global.PART_ID)    // パート・アルバイト
                        //    lblYakushoku.Text = "パート・アルバイト";
                    }

                    // データ編集モード
                    if (dID == string.Empty)
                    {
                        lblName.Text = dR["氏名"].ToString();
                        lblShozoku.Text = dR["所属名"].ToString();
                        
                        // 勤怠申請書種別による制御
                        if (PRN_Type == global.SHAIN_ID)        // 社員
                        {
                            dgv.Columns[cKKH].ReadOnly = true;      // 休憩時カラムは入力対象外とする
                            dgv.Columns[cKKM].ReadOnly = true;      // 休憩分カラムは入力対象外とする
                            dgv.Columns[cKoutsuhi].ReadOnly = true; // 交通費カラムは入力対象外とする
                        }
                        else if (PRN_Type == global.PART_ID)    // パート・アルバイト
                        {
                            dgv.Columns[cKKH].ReadOnly = false;         // 休憩時カラムを入力対象とする
                            dgv.Columns[cKKM].ReadOnly = false;         // 休憩時カラムを入力対象とする
                            dgv.Columns[cKoutsuhi].ReadOnly = false;    // 交通費カラムを入力対象とする
                        }

                        // 画像表示
                        global.pblImageFile = Properties.Settings.Default.DataPath + dR["画像名"].ToString();
                        
                        //データ数表示
                        lblPage.Text = " (" + (cI + 1).ToString() + "/" + sID.Length.ToString() + ")";
                    }
                    else
                    {
                        // 画像表示
                        global.pblImageFile = Properties.Settings.Default.tifPath + dR["画像名"].ToString();
                        
                        //データ数表示
                        lblPage.Text = string.Empty;
                    }

                    // 勤怠申請書種別による制御
                    if (PRN_Type == global.SHAIN_ID)        // 社員
                    {
                        txtKoutsuhi.Text = string.Empty;
                        txtKoutsuhi.Enabled = false;
                    }
                    else if (PRN_Type == global.PART_ID)    // パート・アルバイト
                    {
                        txtKoutsuhi.Enabled = true;
                        txtKoutsuhi.Text = Utility.EmptytoZero(dR["交通費計"].ToString());
                    }
                }
                dR.Close();

                // 勤務票明細データ取得 
                if (dID == string.Empty)
                    dR = ocr.itemSelect(rRec[sIx]);     // 勤務票明細データ
                else dR = ocr.lastItemSelect(dID);      // 過去勤務票明細データ

                // 勤務票明細データ表示
                int r = 0;
                while (dR.Read())
                {
                    dgv[cDay, r].Value = dR["日付"];

                    if (int.Parse(dR["休日マーク"].ToString()) == global.flgOn)
                        dgv[cKyuka, r].Value = true;
                    else dgv[cKyuka, r].Value = false;

                    dgv[cKintai, r].Value = dR["勤怠記号"];
                    dgv[cSH, r].Value = dR["開始時"];
                    dgv[cSM, r].Value = dR["開始分"];
                    dgv[cEH, r].Value = dR["終了時"];
                    dgv[cEM, r].Value = dR["終了分"];

                    // パート・アルバイトのとき
                    if (PRN_Type == global.PART_ID)
                    {
                        dgv[cKKH, r].Value = dR["休憩時"];
                        dgv[cKKM, r].Value = dR["休憩分"];
                        dgv[cKoutsuhi, r].Value = dR["交通費"];
                    }

                    dgv[cTH, r].Value = dR["実働時"];
                    dgv[cTM, r].Value = dR["実働分"];

                    if (int.Parse(dR["訂正"].ToString()) == global.flgOn)
                        dgv[cCheck, r].Value = true;
                    else dgv[cCheck, r].Value = false;

                    dgv[cID, r].Value = dR["ID"].ToString();    // 明細ＩＤ

                    r++;
                }

                dR.Close();

                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
                
                //画像表示
                ShowImage(global.pblImageFile);
                
                // 勤務票データ編集のとき
                if (dID == string.Empty)
                {
                    // ヘッダ情報
                    txtYear.ReadOnly = false;
                    txtMonth.ReadOnly = false;
                    txtNo.ReadOnly = false;
                    txtKoutsuhi.ReadOnly = false;

                    // スクロールバー設定
                    hScrollBar1.Enabled = true;
                    hScrollBar1.Minimum = 0;
                    hScrollBar1.Maximum = rRec.Length - 1;
                    hScrollBar1.Value = sIx;
                    hScrollBar1.LargeChange = 1;
                    hScrollBar1.SmallChange = 1;

                    //移動ボタン制御
                    btnFirst.Enabled = true;
                    btnNext.Enabled = true;
                    btnBefore.Enabled = true;
                    btnEnd.Enabled = true;

                    //最初のレコード
                    if (sIx == 0)
                    {
                        btnBefore.Enabled = false;
                        btnFirst.Enabled = false;
                    }

                    //最終レコード
                    if ((sIx + 1) == rRec.Length)
                    {
                        btnNext.Enabled = false;
                        btnEnd.Enabled = false;
                    }

                    //カレントセル選択状態としない
                    dgv.CurrentCell = null;

                    // その他のボタンを有効とする
                    button5.Enabled = true;
                    btnErrCheck.Visible = true;
                    btnDataMake.Visible = true;
                    btnDel.Visible = true;

                    // データグリッドビュー編集可
                    dataGridView1.ReadOnly = false;

                    //エラー情報表示
                    ErrShow();
                }
                else
                {
                    // ヘッダ情報
                    txtYear.ReadOnly = true;
                    txtMonth.ReadOnly = true;
                    txtNo.ReadOnly = true;
                    txtKoutsuhi.ReadOnly = true;

                    // スクロールバー設定
                    hScrollBar1.Enabled = true;
                    hScrollBar1.Minimum = 0;
                    hScrollBar1.Maximum = 0;
                    hScrollBar1.Value = 0;
                    hScrollBar1.LargeChange = 1;
                    hScrollBar1.SmallChange = 1;

                    //移動ボタン制御
                    btnFirst.Enabled = false;
                    btnNext.Enabled = false;
                    btnBefore.Enabled = false;
                    btnEnd.Enabled = false;

                    //カレントセル選択状態としない
                    dgv.CurrentCell = null;
                    //dataGridView2.CurrentCell = null;

                    // その他のボタンを無効とする
                    button5.Enabled = false;
                    btnErrCheck.Visible = false;
                    btnDataMake.Visible = false;
                    btnDel.Visible = false;

                    // データグリッドビュー編集不可
                    dataGridView1.ReadOnly = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (!dR.IsClosed) dR.Close();
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 画像を表示する
        /// </summary>
        /// <param name="pic">pictureBoxオブジェクト</param>
        /// <param name="imgName">イメージファイルパス</param>
        /// <param name="fX">X方向のスケールファクター</param>
        /// <param name="fY">Y方向のスケールファクター</param>
        private void ImageGraphicsPaint(PictureBox pic, string imgName, float fX, float fY, int RectDest, int RectSrc)
        {
            Image _img = Image.FromFile(imgName);
            Graphics g = Graphics.FromImage(pic.Image);
            
            // 各変換設定値のリセット
            g.ResetTransform();

            // X軸とY軸の拡大率の設定
            g.ScaleTransform(fX, fY); 
            
            // 画像を表示する
            g.DrawImage(_img, RectDest, RectSrc);

            // 現在の倍率,座標を保持する
            global.ZOOM_NOW = fX;
            global.RECTD_NOW = RectDest;
            global.RECTS_NOW = RectSrc;
        }

        ///データ表示エリア背景色初期化
        private void dsColorInitial(DataGridView dgv)
        {
            txtYear.BackColor = Color.White;
            txtMonth.BackColor = Color.White;
            txtNo.BackColor = Color.White;

            for (int i = 0; i < _MULTIGYO; i++)
            {
                dgv.Rows[i].DefaultCellStyle.BackColor = Color.Empty;
            }
        }

        private void txtNo_TextChanged(object sender, EventArgs e)
        {
            // 過去データ表示のときは何もしない
            if (dID != string.Empty) return;

            // チェンジバリューステータス
            if (!global.ChangeValueStatus) return; 

            // 表示欄初期化
            this.lblShozoku.Text = string.Empty;
            this.lblName.Text = string.Empty;
            string tempDate;

            //社員番号のとき
            lblShozoku.Text = string.Empty;
            lblName.Text = string.Empty;

            if (txtNo.Text != string.Empty)
            {
                // 奉行シリーズデータベースより社員情報を取得
                //dbControl.DataControl dCon = new dbControl.DataControl(_PCADBName);
                
                // 勘定奉行データベース接続文字列を取得する 2017/09/07
                string sc = SqlControl.obcConnectSting.get(_PCADBName);

                // 勘定奉行データベースに接続する 2017/09/04
                SqlControl.DataControl dCon = new SqlControl.DataControl(sc);

                SqlDataReader dRs = dCon.GetEmployeeBase(txtYear.Text, txtMonth.Text, txtNo.Text);
                                
                while (dRs.Read())
                {
                    //社員名、区分表示
                    lblShozoku.Text = dRs["DepartmentName"].ToString().Trim();
                    //lblYakushoku.Text = dRs["CategoryName"].ToString().Trim();
                    lblName.Text = dRs["Name"].ToString().Trim();
                }

                dRs.Close();
                dCon.Close();
            }
            
            //// 休日再表示
            //for (int i = 0; i < dataGridView1.RowCount; i++)
            //{
            //    YoubiSet(i);
            //}
        }


        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                if (dataGridView1.CurrentCell.ColumnIndex != 17)
                {
                    //イベントハンドラが複数回追加されてしまうので最初に削除する
                    e.Control.KeyPress -= new KeyPressEventHandler(Control_KeyPress);
                    //イベントハンドラを追加する
                    e.Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
                }
            }
        }

        void Control_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b' && e.KeyChar != '\t')
                e.Handled = true;
        }

        /// <summary>
        /// 曜日をセットする
        /// </summary>
        /// <param name="tempRow">MultiRowのindex</param>
        private void YoubiSet(int tempRow)
        {
            string sDate;
            DateTime eDate;
            Boolean bYear = false;
            Boolean bMonth = false;

            //年月を確認
            if (txtYear.Text != string.Empty)
            {
                if (Utility.NumericCheck(txtYear.Text))
                {
                    if (int.Parse(txtYear.Text) > 0)
                    {
                        bYear = true;
                    }
                }
            }

            if (txtMonth.Text != string.Empty)
            {
                if (Utility.NumericCheck(txtMonth.Text))
                {
                    if (int.Parse(txtMonth.Text) >= 1 && int.Parse(txtMonth.Text) <= 12)
                    {
                        for (int i = 0; i < _MULTIGYO; i++)
                        {
                            bMonth = true;
                        }
                    }
                }
            }

            //年月の値がfalseのときは曜日セットは行わずに終了する
            if (bYear == false || bMonth == false) return;

            //行の色を初期化
            dataGridView1.Rows[tempRow].DefaultCellStyle.BackColor = Color.Empty;

            //Nullか？
            dataGridView1[cWeek, tempRow].Value = string.Empty;
            if (dataGridView1[cDay, tempRow].Value != null) 
            {
                if (dataGridView1[cDay, tempRow].Value.ToString() != string.Empty)
                {
                    if (Utility.NumericCheck(dataGridView1[cDay, tempRow].Value.ToString()))
                    {
                        {
                            sDate = (int.Parse(Utility.EmptytoZero(txtYear.Text)) + Properties.Settings.Default.RekiHosei).ToString() + "/" +
                                               Utility.EmptytoZero(txtMonth.Text) + "/" +
                                               Utility.EmptytoZero(dataGridView1[cDay, tempRow].Value.ToString());
                            
                            // 存在する日付と認識された場合、曜日を表示する
                            if (DateTime.TryParse(sDate, out eDate))
                            {
                                dataGridView1[cWeek, tempRow].Value = ("日月火水木金土").Substring(int.Parse(eDate.DayOfWeek.ToString("d")), 1);

                                //// 休日背景色設定・日曜日
                                //if (dataGridView1[cWeek, tempRow].Value.ToString() == "日")
                                //    dataGridView1.Rows[tempRow].DefaultCellStyle.BackColor = Color.MistyRose;

                                // 時刻区切り文字
                                dataGridView1[cSE, tempRow].Value = ":";
                                dataGridView1[cEE, tempRow].Value = ":";
                                dataGridView1[cKKE, tempRow].Value = ":";
                                //dataGridView1[cKSE, tempRow].Value = ":";
                                dataGridView1[cTE, tempRow].Value = ":";
                            }
                        }
                    }
                }
             }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!global.ChangeValueStatus) return;
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == cDay) YoubiSet(e.RowIndex);  // 日付
            
            // 出勤日数
            //txtShukkinTl.Text = getWorkDays(_YakushokuType);

            // 休日チェック
            if (colName == cKyuka || colName == cCheck)
            {
                // 休日行背景色設定
                if (dataGridView1[cKyuka, e.RowIndex].Value.ToString() == "True")
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                else dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
            }

            // 過去データ表示のときは終了
            if (dID != string.Empty) return;

            // 勤怠記号
            if (colName == cKintai) 
            {
                //txtYukyuHiTl.Text = getYukyuTotal(0);
                //txtYukyuTmTl.Text = getYukyuTotal(1);
            }

            // 深夜勤務
            if (colName == cSH || colName == cSM || colName == cEH || colName == cEM)
            {
                //txtShinyaTl.Text = getShinyaTime().ToString();
            }

            // 実労働時間
            if (colName == cTH || colName == cTM)
            {
                //double w = 0;

                //// パートタイマーは月間実労働時間合計を計算します
                //if (_YakushokuType == 1)
                //{
                //    w = getWorkTime();
                //    txtRhTl.Text = System.Math.Floor(w / 60).ToString();
                //    txtRmTl.Text = (w % 60).ToString();
                //}
                //else
                //{
                //    txtRhTl.Text = string.Empty;
                //    txtRmTl.Text = string.Empty;
                //}

            }

            // 実労働時間編集チェック
            if (colName == cCheck)
            {
                // 訂正行背景色設定
                if (dataGridView1[cCheck, e.RowIndex].Value.ToString() == "True")
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                else if (dataGridView1[cKyuka, e.RowIndex].Value.ToString() == "True")  // 休日行背景色設定
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                else dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;

                //if (dataGridView1[cCheck, e.RowIndex].Value.ToString() == "True")
                //{
                //    dataGridView1[cTH, e.RowIndex].ReadOnly = false;
                //    dataGridView1[cTM, e.RowIndex].ReadOnly = false;
                //}
                //else
                //{
                //    dataGridView1[cTH, e.RowIndex].ReadOnly = true;
                //    dataGridView1[cTM, e.RowIndex].ReadOnly = true;
                //}
            }
        }

        /// <summary>
        /// 与えられた休暇記号に該当する休暇日数取得
        /// </summary>
        /// <param name="kigou">休暇記号</param>
        /// <returns>休暇日数</returns>
        private string getKyukaTotal(string kigou)
        {
            int days = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1[cKyuka, i].Value != null)
                {
                    if (dataGridView1[cKyuka, i].Value.ToString() == kigou)
                        days++;
                }
            }

            return days.ToString();
        }

        /// <summary>
        /// 有給休暇日数・時間取得
        /// </summary>
        /// <returns></returns>
        /// <param name="Status">0:日数、1:時間</param>
        private string getYukyuTotal(int Status)
        {
            int days = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1[cKintai, i].Value != null)
                {
                    if (Status == 0)    // 有給日数
                    {
                        if (dataGridView1[cKintai, i].Value.ToString() == global.ZENNICHI_YUKYU)
                        days++;
                    }
                    else if (Status == 1)   // 有給時間
                    {
                        if (dataGridView1[cKintai, i].Value.ToString() != global.ZENNICHI_YUKYU &&
                            dataGridView1[cKintai, i].Value.ToString() != string.Empty)
                            days += int.Parse(dataGridView1[cKintai, i].Value.ToString());
                    }
                }
            }

            return days.ToString();
        }

        /// <summary>
        /// 総労働時間取得
        /// </summary>
        /// <returns>総労働時間・分</returns>
        private int getWorkTime()
        {
            int wHour = 0;
            int wMin = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                wHour += Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cTH, i].Value));
                wMin += Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cTM, i].Value));
            }

            return (wHour * 60 + wMin);
        }

        /// <summary>
        /// 出勤日数取得
        /// </summary>
        /// <returns>出勤日数</returns>
        private string getWorkDays(int yaku)
        {
            // 出勤日数
            int sDays = 0;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                // 勤務時間が記入されている行
                if ((dataGridView1[cSH, i].Value != null && dataGridView1[cSH, i].Value.ToString() != string.Empty) && 
                    (dataGridView1[cSM, i].Value != null && dataGridView1[cSM, i].Value.ToString() != string.Empty))
                {
                    // 開始時間が24時台以外のもの（24時台は前日からの通し勤務とみなし出勤日数に加えない）
                    if (dataGridView1[cSH, i].Value.ToString() != "24")
                    {
                        // 社員
                        if (yaku != global.flgOn) sDays++;
                        else if (dataGridView1[cKintai, i].Value != null)    // パート：終日有休以外のときは出勤日数としてカウントする
                        {
                            if (dataGridView1[cKintai, i].Value.ToString() != global.ZENNICHI_YUKYU)
                                sDays++;
                        }
                    }
                }
            }

            return sDays.ToString();
        }

        /// <summary>
        /// 深夜勤務時間取得(22:00～05:00)
        /// </summary>
        /// <returns>深夜勤務時間・分</returns>
        private double getShinyaTime()
        {
            int wHour = 0;
            int wMin = 0;
            int wHourk = 0;
            int wMink = 0;
            int sKyukei = 0;

            int sHour = 0;
            int sMin = 0;

            DateTime stTM;
            DateTime edTM;
            double spanMin = 0;

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                // 開始が５：００以前のとき
                if (Utility.NulltoStr(dataGridView1[cSH, i].Value) != string.Empty && 
                    Utility.NulltoStr(dataGridView1[cSM, i].Value) != string.Empty)
                {
                    wHour = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cSH, i].Value));
                    wMin = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cSM, i].Value));

                    if (wHour == 24) wHour = 0;

                    if (wHour < 5 && wMin < 60)
                    {
                        // 深夜勤務時間
                        stTM = DateTime.Parse(wHour.ToString() + ":" + wMin.ToString());
                        spanMin += Utility.GetTimeSpan(stTM, global.dt0500).TotalMinutes;
                    }
                }

                // 終了が２２：００以降のとき
                if (Utility.NulltoStr(dataGridView1[cEH, i].Value) != string.Empty && 
                    Utility.NulltoStr(dataGridView1[cEM, i].Value) != string.Empty)
                {
                    wHour = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cEH, i].Value));
                    wMin = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cEM, i].Value));

                    if (wHour >= 22)
                    {
                        // 深夜勤務時間
                        //sHour = (wHour - 22) * 60 + wMin;

                        if (wHour < 25 && wMin < 60)
                        {
                            if (wHour < 24)
                            {
                                edTM = DateTime.Parse(wHour.ToString() + ":" + wMin.ToString());
                                spanMin += Utility.GetTimeSpan(global.dt2200, edTM).TotalMinutes;
                            }
                            // 24:00のときは23:59まで計算して1分加算する
                            else if (wMin == 0)
                            {
                                edTM = DateTime.Parse("23:59");
                                spanMin += Utility.GetTimeSpan(global.dt2200, edTM).TotalMinutes + 1;
                            }
                        }
                    }
                }

                //// 深夜帯休憩時間
                //wHourk = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cKSH, i].Value));
                //wMink = Utility.StrtoInt(Utility.NulltoStr(dataGridView1[cKSM, i].Value));
                //sKyukei = wHourk * 60 + wMink;

                // 深夜勤務時間
                spanMin -= sKyukei;
            }

            return spanMin;
        }

        private void frmCorrect_Shown(object sender, EventArgs e)
        {
            if (dID != string.Empty) btnRtn.Focus();
        }

        private void dataGridView3_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                //イベントハンドラが複数回追加されてしまうので最初に削除する
                e.Control.KeyPress -= new KeyPressEventHandler(Control_KeyPress);
                //イベントハンドラを追加する
                e.Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
            }
        }

        private void dataGridView4_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                //イベントハンドラが複数回追加されてしまうので最初に削除する
                e.Control.KeyPress -= new KeyPressEventHandler(Control_KeyPress);
                //イベントハンドラを追加する
                e.Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //レコードの移動
            if (cI + 1 < sID.Length)
            {
                cI++;
                DataShow(cI, sID, dataGridView1);
            }   
        }


        /// <summary>
        ///  カレントデータの更新
        /// </summary>
        /// <param name="iX">カレントレコードのインデックス</param>
        private void CurDataUpDate(int iX)
        {
            // OCRDATAクラスインスタンス生成
            OCRData ocr = new OCRData();

            //カレントデータを更新する
            string mySql = string.Empty;

            // エラーメッセージ
            string errMsg = string.Empty;

            //MDB接続
            ocr.dbConnect();

            errMsg = "出勤簿テーブル更新";

            //トランザクション開始
            OleDbTransaction sTran = null;
            sTran = ocr.sCom.Connection.BeginTransaction();
            ocr.sCom.Transaction = sTran;

            try
            {
                // 勤務票ヘッダテーブル更新
                ocr.HeadUpdate(sID[iX], 
                    Utility.EmptytoZero(txtNo.Text), 
                    Utility.NulltoStr(lblName.Text), 
                    Utility.NulltoStr(txtYear.Text), 
                    Utility.NulltoStr(txtMonth.Text),
                    Utility.NulltoStr(lblShozoku.Text).Trim(),
                    Utility.NulltoStr(_PCADBName).Trim(),
                    Utility.EmptytoZero(txtKoutsuhi.Text));

                // 勤務票明細テーブル更新
                for (int i = 0; i < _MULTIGYO; i++)
                {
                    // 存在する日付か検証
                    if (Utility.NulltoStr(dataGridView1[cWeek, i].Value) != string.Empty)
                    {
                        ocr.ItemUpdate(booltoFlg(dataGridView1[cKyuka, i].Value.ToString()).ToString(),
                            Utility.NulltoStr(dataGridView1[cKintai, i].Value),
                            timeVal(dataGridView1[cSH, i].Value, 2),
                            timeVal(dataGridView1[cSM, i].Value, 2),
                            timeVal(dataGridView1[cEH, i].Value, 2),
                            timeVal(dataGridView1[cEM, i].Value, 2),
                            timeVal(dataGridView1[cKKH, i].Value, 1),
                            timeVal(dataGridView1[cKKM, i].Value, 2),
                            timeVal(dataGridView1[cTH, i].Value, 2),
                            timeVal(dataGridView1[cTM, i].Value, 2),
                            Utility.EmptytoZero(Utility.NulltoStr(dataGridView1[cKoutsuhi, i].Value)),
                            booltoFlg(dataGridView1[cCheck, i].Value.ToString()).ToString(),
                            int.Parse(dataGridView1[cID, i].Value.ToString()));
                    }
                }

                //トランザクションコミット
                sTran.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errMsg, MessageBoxButtons.OK);

                // トランザクションロールバック
                sTran.Rollback();
            }
            finally
            {
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 時・分の登録時のデータを返す
        /// </summary>
        /// <param name="tm">時分文字列</param>
        /// <returns></returns>
        private string timeVal(object tm, int len)
        {
            string t = Utility.NulltoStr(tm);
            if (t != string.Empty) return t.PadLeft(len, '0');
            else return t;
        }

        /// <summary>
        /// Bool値を数値に変換する
        /// </summary>
        /// <param name="b">true:1, false:0</param>
        /// <returns></returns>
        private int booltoFlg(string b)
        {
            if (b == "True") return global.flgOn;
            else return global.flgOff;
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //レコードの移動
            cI =  sID.Length - 1;
            DataShow(cI, sID, dataGridView1);
        }

        private void btnBefore_Click(object sender, EventArgs e)
        {
            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //レコードの移動
            if (cI > 0)
            {
                cI--;
                DataShow(cI, sID, dataGridView1);
            }   
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //レコードの移動
            cI = 0;
            DataShow(cI, sID, dataGridView1);
        }

        ///------------------------------------------------------------
        /// <summary>
        ///     エラーチェックメイン処理 </summary>
        /// <param name="sID">
        ///     開始ID</param>
        /// <param name="eID">
        ///     終了ID</param>
        /// <returns>
        ///     True:エラーなし、false:エラーあり</returns>
        ///------------------------------------------------------------
        private Boolean ErrCheckMain(string sIx, string eIx)
        {
            int rCnt = 0;

            //オーナーフォームを無効にする
            this.Enabled = false;

            //プログレスバーを表示する
            frmPrg frmP = new frmPrg();
            frmP.Owner = this;
            frmP.Show();

            //レコード件数取得
            int cTotal = CountMDB();

            //エラー情報初期化
            ErrInitial();

            // 出勤簿データ読み出し
            Boolean eCheck = true;

            OCRData ocr = new OCRData();
            ocr.dbConnect();
            OleDbDataReader dR = ocr.HeaderSelect();

            while (dR.Read())
            {
                //データ件数加算
                rCnt++;

                //プログレスバー表示
                frmP.Text = "エラーチェック実行中　" + rCnt.ToString() + "/" + cTotal.ToString();
                frmP.progressValue = rCnt * 100 / cTotal;
                frmP.ProgressStep();

                //指定範囲のIDならエラーチェックを実施する
                if (Int64.Parse(dR["ID"].ToString()) >= Int64.Parse(sIx) && Int64.Parse(dR["ID"].ToString()) <= Int64.Parse(eIx))
                {
                    eCheck = ErrCheckData(dR);
                    if (!eCheck) break;　//エラーがあったとき
                }
            }

            dR.Close();
            ocr.sCom.Connection.Close();

            // いったんオーナーをアクティブにする
            this.Activate();

            // 進行状況ダイアログを閉じる
            frmP.Close();

            // オーナーのフォームを有効に戻す
            this.Enabled = true;

            //エラー有りの処理
            if (!eCheck)
            {
                //エラーデータのインデックスを取得
                for (int i = 0; i < sID.Length; i++)
                {
                    if (sID[i] == global.errID)
                    {
                        //エラーデータを画面表示
                        cI = i;
                        DataShow(cI, sID, dataGridView1);
                        break;
                    }
                }
            }

            return eCheck;
        }

        ///---------------------------------------------------------------
        /// <summary>
        ///     項目別エラーチェック : 2017/09/04 </summary>
        /// <param name="cdR">
        ///     データリーダー</param>
        /// <returns>
        ///     エラーなし：true, エラー有り：false</returns>
        ///---------------------------------------------------------------
        private Boolean ErrCheckData(OleDbDataReader cdR)
        {
            string sDate;
            DateTime eDate;

            DateTime sTime;         // 開始時刻
            DateTime eTime;         // 終了時刻
            int TLkoutsuhi = 0;     // 交通費合計

            // 申請書種別を取得
            string shain_Type = cdR["申請書種別"].ToString();

            //// 未確認データ
            //if (cdR["確認"].ToString() == global.flgOff.ToString())
            //{
            //    global.errID = cdR["ID"].ToString();
            //    global.errNumber = global.eNoCheck;
            //    global.errRow = 0;
            //    global.errMsg = "未確認の出勤簿です";

            //    return false;
            //}

            // 対象年
            if (Utility.NumericCheck(cdR["年"].ToString()) == false)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eYearMonth;
                global.errRow = 0;
                global.errMsg = "年が正しくありません";

                return false;
            }

            if (int.Parse(cdR["年"].ToString()) < 1)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eYearMonth;
                global.errRow = 0;
                global.errMsg = "年が正しくありません";

                return false;
            }

            if (int.Parse(cdR["年"].ToString()) != global.cnfYear)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eYearMonth;
                global.errRow = 0;
                global.errMsg = "対象年（" + global.cnfYear + "年）と一致していません";

                return false;
            }

            // 対象月
            if (Utility.NumericCheck(cdR["月"].ToString()) == false)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eMonth;
                global.errRow = 0;
                global.errMsg = "月が正しくありません";

                return false;
            }

            if (int.Parse(cdR["月"].ToString()) < 1 || int.Parse(cdR["月"].ToString()) > 12)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eMonth;
                global.errRow = 0;
                global.errMsg = "月が正しくありません";

                return false;
            }

            if (int.Parse(cdR["月"].ToString()) != global.cnfMonth)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eMonth;
                global.errRow = 0;
                global.errMsg = "対象月（" + global.cnfMonth + "月）と一致していません";

                return false;
            }

            // 対象年月
            sDate = (int.Parse(cdR["年"].ToString()) + Properties.Settings.Default.RekiHosei).ToString() + "/" + cdR["月"].ToString() + "/01";
            if (DateTime.TryParse(sDate, out eDate) == false)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eYearMonth;
                global.errRow = 0;
                global.errMsg = "年月が正しくありません";

                return false;
            }

            // 社員番号
            // 数字以外のとき
            if (Utility.NumericCheck(cdR["個人番号"].ToString()) == false)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eShainNo;
                global.errRow = 0;
                global.errMsg = "社員番号が入力されていません";

                return false;
            }

            // 社員番号マスター登録検査
            int eCnt = 0;

            // 勘定奉行データベース接続文字列を取得する 2017/09/04
            string sc = SqlControl.obcConnectSting.get(_PCADBName);

            // 勘定奉行データベースに接続する 2017/09/04
            SqlControl.DataControl dCon = new SqlControl.DataControl(sc);

            //データリーダーを取得する
            SqlDataReader sdR;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("select EmployeeNo,RetireCorpDate from tbEmployeeBase ");
            sb.Append("where EmployeeNo = '" + string.Format("{0:0000000000}", int.Parse(cdR["個人番号"].ToString())) + "'");
            sb.Append(" and BeOnTheRegisterDivisionID != 9");

            sdR = dCon.free_dsReader(sb.ToString());

            while (sdR.Read())
            {
                //rDate = DateTime.Parse(sdR["RetireCorpDate"].ToString());
                eCnt++;
            }

            sdR.Close();
            dCon.Close();

            if (eCnt == 0)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eShainNo;
                global.errRow = 0;
                global.errMsg = "マスター未登録の社員番号です";

                return false;
            }

            // 同じ社員番号の勤務票データが複数存在するとき
            if (!errCheckSameNumber(cdR["個人番号"].ToString()))
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = global.eShainNo;
                global.errRow = 0;
                global.errMsg = "同じ社員番号のデータが複数あります";

                return false;
            }

            // 勤務票明細データ
            OCRData ocr = new OCRData();
            ocr.dbConnect();
            OleDbDataReader dR = ocr.itemSelect(cdR["ID"].ToString());

            //日付別データ
            int iX = 0;
            string k = string.Empty;    // 特別休暇記号
            string yk = string.Empty;   // 有給記号

            // 集計クラス
            //sumData sDt = new sumData();

            while (dR.Read())
            {
                // 日付インデックス加算
                iX++;

                // 日付は数字か
                if (Utility.NumericCheck(dR["日付"].ToString()) == false)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = global.eDay;
                    global.errRow = iX - 1;
                    global.errMsg = "日が正しくありません";
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                sDate = (int.Parse(cdR["年"].ToString()) + Properties.Settings.Default.RekiHosei).ToString() + "/" +
                        cdR["月"].ToString() + "/" + dR["日付"].ToString();

                // 存在しない日付に記入があるとき
                if (!DateTime.TryParse(sDate, out eDate))
                {
                    if (Utility.NulltoStr(dR["休日マーク"]) == global.FLGON || Utility.NulltoStr(dR["勤怠記号"]) != string.Empty ||
                    Utility.NulltoStr(dR["開始時"]) != string.Empty || Utility.NulltoStr(dR["開始分"]) != string.Empty ||
                    Utility.NulltoStr(dR["終了時"]) != string.Empty || Utility.NulltoStr(dR["終了分"]) != string.Empty ||
                    Utility.NulltoStr(dR["休憩時"]) != string.Empty || Utility.NulltoStr(dR["休憩分"]) != string.Empty ||
                    Utility.NulltoStr(dR["実働時"]) != string.Empty || Utility.NulltoStr(dR["実働分"]) != string.Empty ||
                    Utility.NulltoStr(dR["交通費"]) != global.FLGOFF || Utility.NulltoStr(dR["訂正"]) == global.FLGON)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eDay;
                        global.errRow = iX - 1;
                        global.errMsg = "この行には記入できません";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // 休日マーク以外が無記入の行はチェック対象外とする
                if (Utility.NulltoStr(dR["勤怠記号"]) == string.Empty &&
                    Utility.NulltoStr(dR["開始時"]) == string.Empty && Utility.NulltoStr(dR["開始分"]) == string.Empty &&
                    Utility.NulltoStr(dR["終了時"]) == string.Empty && Utility.NulltoStr(dR["終了分"]) == string.Empty &&
                    Utility.NulltoStr(dR["休憩時"]) == string.Empty && Utility.NulltoStr(dR["休憩分"]) == string.Empty &&
                    Utility.NulltoStr(dR["実働時"]) == string.Empty && Utility.NulltoStr(dR["実働分"]) == string.Empty &&
                    Utility.NulltoStr(dR["交通費"]) == global.FLGOFF && Utility.NulltoStr(dR["訂正"]) == global.FLGOFF)
                {
                    continue;
                }

                // 勤怠記号
                k = Utility.NulltoStr(dR["勤怠記号"]);
                if (k != string.Empty && k != global.K_KYUJITSUSHUKIN && k != global.K_TOKUBETSU_KYUKA &&
                    k != global.K_YUKYU_KYUKA && k != global.K_SANKYU && k != global.K_IKUKYU && 
                    k != global.K_CHISOU && k != global.K_KEKKIN && k != global.K_FURI_SHUKKIN && 
                    k != global.K_SHUCCHOU && k != global.K_FURI_KYUJITSU)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = global.eKintaiKigou;
                    global.errRow = iX - 1;
                    global.errMsg = "勤怠記号が正しくありません";
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // 休日マークと勤怠記号：休日のとき
                if (Utility.NulltoStr(dR["休日マーク"]) == global.FLGON)
                {
                    if (k == global.K_TOKUBETSU_KYUKA)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に特別休暇記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_YUKYU_KYUKA)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に有給休暇記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_SANKYU)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に産休記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_IKUKYU)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に育休記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_KEKKIN)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に欠勤記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_CHISOU)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に遅刻・早退記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    if (k == global.K_FURI_KYUJITSU)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日に振替休日記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // 開始時間・時チェック
                if (!errCheckTime(dR["開始時"], cdR, "開始時間", k, iX, "H", global.eSH))
                {
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // 開始時間・分チェック
                if (!errCheckTime(dR["開始分"], cdR, "開始時間", k, iX, "M", global.eSM))
                {
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // 終了時間・時チェック
                if (!errCheckTime(dR["終了時"], cdR, "終了時間", k, iX, "H", global.eEH))
                {
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // 終了時間・分チェック
                if (!errCheckTime(dR["終了分"], cdR, "終了時間", k, iX, "M", global.eEM))
                {
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // 終了時刻範囲
                if (Utility.StrtoInt(Utility.NulltoStr(dR["終了時"])) == 24 && 
                    Utility.StrtoInt(Utility.NulltoStr(dR["終了分"])) > 0)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = global.eEM;
                    global.errRow = iX - 1;
                    global.errMsg = "終了時刻範囲を超えています（～２４：００）";
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }

                // パート・アルバイトのとき休憩時間をチェック
                if (shain_Type == global.PART_ID)
                {
                    // 休憩・時間チェック
                    if (!errCheckKyukeiTime(dR["休憩時"], cdR, "休憩時間", k, iX, "H", global.eKKH))
                    {
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    // 休憩・分チェック
                    if (!errCheckKyukeiTime(dR["休憩分"], cdR, "休憩時間", k, iX, "M", global.eKKM))
                    {
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // 開始時刻 ⇔ 終了時刻チェック
                string sh = string.Empty;
                string eh = string.Empty;

                if (Utility.NulltoStr(dR["開始時"]) != string.Empty &&
                    Utility.NulltoStr(dR["開始分"]) != string.Empty &&
                    Utility.NulltoStr(dR["終了時"]) != string.Empty &&
                    Utility.NulltoStr(dR["終了分"]) != string.Empty)
                {
                    // 開始時刻取得
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["開始時"])) == 24)
                        sTime = DateTime.Parse("0:" + Utility.NulltoStr(dR["開始分"]));
                    else sTime = DateTime.Parse(Utility.NulltoStr(dR["開始時"]) + ":" + Utility.NulltoStr(dR["開始分"]));

                    // 終了時刻取得
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["終了時"])) == 24)
                        eTime = DateTime.Parse("23:59");
                    else eTime = DateTime.Parse(Utility.NulltoStr(dR["終了時"]) + ":" + Utility.NulltoStr(dR["終了分"]));

                    //sTime = DateTime.Parse(Utility.NulltoStr(dR["開始時"]) + ":" + Utility.NulltoStr(dR["開始分"]));
                    //eTime = DateTime.Parse(Utility.NulltoStr(dR["終了時"]) + ":" + Utility.NulltoStr(dR["終了分"]));

                    // 開始時刻 > 終了時刻のときNG
                    if (DateTime.Compare(sTime, eTime) > 0)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eEH;
                        global.errRow = iX - 1;
                        global.errMsg = "終了時刻が開始時刻以前になっています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    // 開始時刻～終了時刻と休憩時間
                    double w = 0;   // 稼働時間
                    double kk = 0;  // 休憩時間

                    // 2013/07/03 終了時間が24:00記入のときは23:59までの計算なので稼働時間1分加算する
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["終了時"])) == 24 &&
                        Utility.StrtoInt(Utility.NulltoStr(dR["終了分"])) == 0)
                        w = Utility.GetTimeSpan(sTime, eTime).TotalMinutes + 1;
                    else w = Utility.GetTimeSpan(sTime, eTime).TotalMinutes;  // 稼働時間

                    // パート・アルバイトのとき休憩時間を取得
                    if (shain_Type == global.PART_ID)
                        kk = Utility.StrtoInt(Utility.NulltoStr(dR["休憩時"])) * 60 + Utility.StrtoInt(Utility.NulltoStr(dR["休憩分"]));
                    else kk = 0;

                    if (w < kk)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKKH;
                        global.errRow = iX - 1;
                        global.errMsg = "稼働時間より休憩時間が長くなっています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                    
                    //
                    // 実働時間チェックを行う
                    //
                    // 開始時刻取得
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["開始時"])) == 24)
                        sTime = DateTime.Parse("0:" + Utility.NulltoStr(dR["開始分"]));
                    else sTime = DateTime.Parse(Utility.NulltoStr(dR["開始時"]) + ":" + Utility.NulltoStr(dR["開始分"]));

                    // 終了時刻取得
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["終了時"])) == 24)
                        eTime = DateTime.Parse("23:59");
                    else eTime = DateTime.Parse(Utility.NulltoStr(dR["終了時"]) + ":" + Utility.NulltoStr(dR["終了分"]));

                    // 終了時刻ー開始時刻
                    if (Utility.StrtoInt(Utility.NulltoStr(dR["終了時"])) == 24 &&
                        Utility.StrtoInt(Utility.NulltoStr(dR["終了分"])) == 0)
                        w = Utility.GetTimeSpan(sTime, eTime).TotalMinutes + 1;  // 終了時間が24:00記入のときは23:59まで計算して稼働時間1分加算する
                    else w = Utility.GetTimeSpan(sTime, eTime).TotalMinutes;

                    // パート・アルバイトのとき休憩時間を取得
                    if (shain_Type == global.PART_ID)
                        kk = Utility.StrtoInt(Utility.NulltoStr(dR["休憩時"])) * 60 + Utility.StrtoInt(Utility.NulltoStr(dR["休憩分"]));
                    else kk = 0;

                    double zw = w - kk;     // 実稼働時間計算
                    int zh = (int)(System.Math.Floor(zw / 60));
                    int zm = (int)(zw % 60);

                    // 記入値と比較
                    if (zh != Utility.StrtoInt(Utility.NulltoStr(dR["実働時"])) ||
                        zm != Utility.StrtoInt(Utility.NulltoStr(dR["実働分"])))
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eTH;
                        global.errRow = iX - 1;
                        global.errMsg = "実働時間が正しくありません（" + zh.ToString() + "時間 " + zm.ToString() + "分）";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                    //
                    // 交通費（パート・アルバイトのみ）
                    //
                    if (Utility.NumericCheck(dR["交通費"].ToString()) == false)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKoutsuhi;
                        global.errRow = iX - 1;
                        global.errMsg = "交通費が正しくありません";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }

                }
                else
                {
                    // 開始終了時刻が無記入で稼働時間が記入されているとき
                    if (Utility.NulltoStr(dR["実働時"]) != string.Empty ||
                        Utility.NulltoStr(dR["実働分"]) != string.Empty)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eTH;
                        global.errRow = iX - 1;
                        global.errMsg = "開始終了時刻が無記入で実働時間が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // 休日出勤チェック
                if (Utility.NulltoStr(dR["休日マーク"]) == global.FLGON)
                {
                    if (k == string.Empty && Utility.NulltoStr(dR["開始時"]) != string.Empty)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "振替出勤または休日出勤（デイリー）記号が未記入です";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // 休日マークと勤怠記号：平日のとき
                if (Utility.NulltoStr(dR["休日マーク"]) == global.FLGOFF)
                {
                    if (k == global.K_KYUJITSUSHUKIN && Utility.NulltoStr(dR["開始時"]) != string.Empty)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = global.eKintaiKigou;
                        global.errRow = iX - 1;
                        global.errMsg = "休日以外に休日出勤記号が記入されています";
                        dR.Close();
                        ocr.sCom.Connection.Close();
                        return false;
                    }
                }

                // パート・アルバイトのとき交通費累計
                if (shain_Type == global.PART_ID) TLkoutsuhi += int.Parse(Utility.NulltoStr(dR["交通費"]));

            }

            // パート・アルバイトのとき交通費合計チェック
            if (shain_Type == global.PART_ID)
            {
                if (TLkoutsuhi != int.Parse(Utility.NulltoStr(cdR["交通費計"])))
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = global.eKoutsuhiTL;
                    global.errRow = iX - 1;
                    global.errMsg = "交通費の合計が計算値と一致しません（計算値：" + TLkoutsuhi.ToString() + ")";
                    dR.Close();
                    ocr.sCom.Connection.Close();
                    return false;
                }
            }

            // 出勤簿明細データリーダークローズ
            dR.Close();
            ocr.sCom.Connection.Close();

            //// 出勤簿ヘッダデータ更新
            //SumDataUpdate(cdR["ID"].ToString());

            return true;
        }

        ///---------------------------------------------------------
        /// <summary>
        ///     同じ個人番号の勤務票データが複数存在するか調べる </summary>
        /// <param name="sNumber">
        ///     個人番号</param>
        /// <returns>
        ///     複数ない：true, 複数あり：false</returns>
        ///---------------------------------------------------------
        private bool errCheckSameNumber(string sNumber)
        {
            int cnt = 0;
            OCRData ocr = new OCRData();
            ocr.dbConnect();
            OleDbDataReader dr = ocr.HeaderSelect(global.cnfYear.ToString(), global.cnfMonth.ToString(), sNumber);

            while (dr.Read())
            {
                cnt++;
            }
            dr.Close();
            ocr.sCom.Connection.Close();

            if (cnt > 1) return false;

            return true;
        }

        ///---------------------------------------------------------
        /// <summary>
        ///     エラーチェックボタン</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///---------------------------------------------------------
        private void btnErrCheck_Click(object sender, EventArgs e)
        {
            // カレントレコード更新
            CurDataUpDate(cI);

            // エラーチェック実行①:カレントレコードから最終レコードまで
            if (ErrCheckMain(sID[cI], sID[sID.Length - 1]) == false) return;

            // エラーチェック実行②:最初のレコードからカレントレコードの前のレコードまで
            if (cI > 0)
            {
                if (ErrCheckMain(sID[0], sID[cI - 1]) == false) return;
            }

            MessageBox.Show("エラーはありませんでした", "エラーチェック", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dataGridView1.CurrentCell = null;

        }
        
        /// <summary>
        /// エラー表示
        /// </summary>
        private void ErrShow()
        {
            if (global.errNumber != global.eNothing)
            {
                lblErrMsg.Visible = true;
                lblErrMsg.Text = global.errMsg;

                // 対象年月
                if (global.errNumber == global.eYearMonth)
                {
                    txtYear.BackColor = Color.Yellow;
                    txtMonth.BackColor = Color.Yellow;
                    txtYear.Focus();
                }

                // 対象月
                if (global.errNumber == global.eMonth)
                {
                    txtMonth.BackColor = Color.Yellow;
                    txtMonth.Focus();
                }

                // 個人番号
                if (global.errNumber == global.eShainNo)
                {
                    txtNo.BackColor = Color.Yellow;
                    txtNo.Focus();
                }

                // 交通費合計
                if (global.errNumber == global.eKoutsuhiTL)
                {
                    txtKoutsuhi.BackColor = Color.Yellow;
                    txtKoutsuhi.Focus();
                }

                // 日
                if (global.errNumber == global.eDay)
                {
                    dataGridView1[cDay, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cDay, global.errRow];
                }

                // 勤怠記号
                if (global.errNumber == global.eKintaiKigou)
                {
                    dataGridView1[cKintai, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cKyuka, global.errRow];
                }

                // 開始時
                if (global.errNumber == global.eSH)
                {
                    dataGridView1[cSH, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cSH, global.errRow];
                }

                // 開始分
                if (global.errNumber == global.eSM)
                {
                    dataGridView1[cSM, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cSM, global.errRow];
                }

                // 終了時
                if (global.errNumber == global.eEH)
                {
                    dataGridView1[cEH, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cEH, global.errRow];
                }

                // 終了分
                if (global.errNumber == global.eEM)
                {
                    dataGridView1[cEM, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cEM, global.errRow];
                }

                // 休憩・時
                if (global.errNumber == global.eKKH)
                {
                    dataGridView1[cKKH, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cKKH, global.errRow];
                }

                // 休憩・分
                if (global.errNumber == global.eKKM)
                {
                    dataGridView1[cKKM, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cKKM, global.errRow];
                }

                // 実働時
                if (global.errNumber == global.eTH)
                {
                    dataGridView1[cTH, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cTH, global.errRow];
                }

                // 実働分
                if (global.errNumber == global.eTM)
                {
                    dataGridView1[cTM, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cTM, global.errRow];
                }

                // 交通費
                if (global.errNumber == global.eKoutsuhi)
                {
                    dataGridView1[cKoutsuhi, global.errRow].Style.BackColor = Color.Yellow;
                    dataGridView1.Focus();
                    dataGridView1.CurrentCell = dataGridView1[cKoutsuhi, global.errRow];
                }
            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //レコードの移動
            cI = hScrollBar1.Value;
            DataShow(cI, sID, dataGridView1);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("表示中の勤務票データを削除します。よろしいですか", "削除確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            // レコードと画像ファイルを削除する
            DataDelete();

            //テーブル件数カウント：ゼロならばプログラム終了
            if (CountMDB() == 0)
            {
                MessageBox.Show("全ての勤務票データが削除されました。処理を終了します。", "勤務票削除", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //終了処理
                Environment.Exit(0);
            }

            //テーブルデータキー項目読み込み
            sID = LoadMdbID();

            //エラー情報初期化
            ErrInitial();

            //レコードを表示
            if (sID.Length - 1 < cI) cI = sID.Length - 1;
            DataShow(cI, sID, dataGridView1);
        }

        private void DataDelete()
        {
            //カレントデータを削除します
            //MDB接続
            OCRData ocr = new OCRData();
            ocr.dbConnect();

            // 画像ファイル名を取得します
            string sImgNm = string.Empty;
            OleDbDataReader dR = ocr.HeaderSelect(sID[cI]);
            while (dR.Read())
            {
                sImgNm = dR["画像名"].ToString();
            }
            dR.Close();

            try
            {
                // 勤務票ヘッダデータと明細データを削除します
                ocr.DataDelete(sID[cI]);

                // 画像ファイルを削除する
                if (System.IO.File.Exists(Properties.Settings.Default.DataPath + sImgNm))
                {
                    System.IO.File.Delete(Properties.Settings.Default.DataPath + sImgNm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("勤務票の削除に失敗しました" + Environment.NewLine + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }
        }

        private void btnRtn_Click(object sender, EventArgs e)
        {
            // フォームを閉じる
            this.Tag = END_BUTTON;
            this.Close();
        }

        private void frmCorrect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Tag.ToString() != END_MAKEDATA)
            {
                if (MessageBox.Show("終了します。よろしいですか", "終了確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                // カレントデータ更新
                if (dID == string.Empty) CurDataUpDate(cI);
            }

            // 解放する
            this.Dispose();
        }

        private void btnDataMake_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("就業奉行受け渡しデータを作成します。よろしいですか", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            // カレントレコード更新
            CurDataUpDate(cI);

            // エラーチェック実行①:カレントレコードから最終レコードまで
            if (ErrCheckMain(sID[cI], sID[sID.Length - 1]) == false) return;

            // エラーチェック実行②:最初のレコードからカレントレコードの前のレコードまで
            if (cI > 0)
            {
                if (ErrCheckMain(sID[0], sID[cI - 1]) == false) return;
            }

            // 汎用データ作成
            SaveData();

            // 交通費データ作成
            SaveKoutsuhi();

            // 画像ファイル退避
            tifFileMove();

            // 過去データ作成
            SaveLastData();

            // 設定月数分経過した過去画像,過去データ、現出勤簿データを削除する
            imageDelete();

            // MDBファイル最適化
            mdbCompact();

            //終了
            MessageBox.Show("終了しました。就業奉行で勤務データ受け入れを行ってください。", "就業奉行受け入れデータ作成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Tag = END_MAKEDATA;
            this.Close();
        }

        ///// <summary>
        ///// ＰＣＡ給与勤怠データ作成
        ///// </summary>
        //private void SaveData()
        //{
        //    // 出力データ生成
        //    SysControl.SetDBConnect Con = new SysControl.SetDBConnect();
        //    OleDbCommand sCom = new OleDbCommand();
        //    OleDbDataReader dR = null;

        //    try
        //    {
        //        //オーナーフォームを無効にする
        //        this.Enabled = false;

        //        // 社員ID
        //        string wsID = string.Empty;

        //        //プログレスバーを表示する
        //        frmPrg frmP = new frmPrg();
        //        frmP.Owner = this;
        //        frmP.Show();

        //        //レコード件数取得
        //        int cTotal = CountMDBitem();
        //        int rCnt = 1;

        //        // データベース接続
        //        sCom.Connection = Con.cnOpen();

        //        string pyymm = string.Empty;

        //        // 出勤簿ヘッダデータリーダーを取得します
        //        StringBuilder sb = new StringBuilder();
        //        sb.Clear();
        //        sb.Append("SELECT 出勤簿ヘッダ.* from 出勤簿ヘッダ ");
        //        sb.Append("order by 出勤簿ヘッダ.社員ID,出勤簿ヘッダ.ID ");
        //        sCom.CommandText = sb.ToString();
        //        dR = sCom.ExecuteReader();

        //        ////出力先フォルダがあるか？なければ作成する
        //        if (!System.IO.Directory.Exists(global.cnfPath))
        //            System.IO.Directory.CreateDirectory(global.cnfPath);

        //        //出力ファイルインスタンス作成
        //        string iFile = global.OKFILE + "_";
        //        iFile += DateTime.Today.Year.ToString() + string.Format("{0:00}", DateTime.Today.Month) + string.Format("{0:00}", DateTime.Today.Day);
        //        iFile += string.Format("{0:00}", DateTime.Now.Hour) + string.Format("{0:00}", DateTime.Now.Minute) + string.Format("{0:00}", DateTime.Now.Second);
        //        iFile += ".dat";

        //        StreamWriter outFile = new StreamWriter(global.cnfPath + iFile, false, System.Text.Encoding.GetEncoding(932));

        //        // 明細書き出し
        //        //sumData sd = null;
        //        while (dR.Read())
        //        {
        //            //プログレスバー表示
        //            frmP.Text = "汎用データ作成中です・・・" + rCnt.ToString() + "/" + cTotal.ToString();
        //            frmP.progressValue = rCnt / cTotal * 100;
        //            frmP.ProgressStep();

        //            // 汎用データの出力
        //            if (wsID != string.Empty && wsID != dR["社員ID"].ToString())
        //            {
        //                //sd.SaveDatacsv(outFile);
        //            }

        //            // 合計クラス
        //            if (wsID != dR["社員ID"].ToString())
        //                //sd = new sumData();

        //            // 社員毎に集計
        //            //sd.Caltotal(dR);

        //            //sd.c1 = dR["個人番号"].ToString() + ",";
        //            ////sd.c4 = dR["出勤日数合計"].ToString().Replace("0", string.Empty) + ",";
        //            //string sN = getShukkinNisu(dR["社員ID"].ToString(), int.Parse(dR["給与区分"].ToString()));
        //            //sd.c4 = sN + ",";
        //            //sd.c5 = dR["総労働"].ToString() + ":" + Utility.StrtoInt(dR["総労働分"].ToString()).ToString().PadLeft(2, '0') + ",";
        //            //sd.c7 = dR["欠勤日数合計"].ToString().Replace("0", string.Empty) + ",";
        //            //sd.c8 = (Utility.StrtoInt(dR["特休日数合計"].ToString()) + Utility.StrtoInt(dR["振休日数合計"].ToString())).ToString() + ",";
        //            //sd.c12 = dR["残業時"].ToString() + ":" + Utility.StrtoInt(dR["残業分"].ToString()).ToString().PadLeft(2, '0') + ",";
                    
        //            //double sh = System.Math.Floor(double.Parse(dR["深夜勤務時間合計"].ToString()) / 60);
        //            //int sm = int.Parse(dR["深夜勤務時間合計"].ToString()) % 60;
        //            //sd.c13 = sh.ToString() + ":" + sm.ToString().PadLeft(2, '0') + ",";

        //            //sd.c18 = dR["遅刻早退回数"].ToString().Replace("0", string.Empty) + ",";
        //            //sd.c20 = dR["有休日数合計"].ToString().Replace("0", string.Empty) + ",";
        //            //sd.c21 = dR["有休時間合計"].ToString().Replace("0", string.Empty) + ",";

        //            //// 要勤務日数
        //            //sd.c2 = (int.Parse(sN) + Utility.StrtoInt(dR["有休日数合計"].ToString())).ToString() + ",";

        //            // 社員ＩＤ
        //            wsID = dR["社員ID"].ToString();
        //        }

        //        //sd.SaveDatacsv(outFile);

        //        // データリーダーをクローズ
        //        dR.Close();

        //        // 出力ファイルをクローズ
        //        outFile.Close();

        //        // いったんオーナーをアクティブにする
        //        this.Activate();

        //        // 進行状況ダイアログを閉じる
        //        frmP.Close();

        //        // オーナーのフォームを有効に戻す
        //        this.Enabled = true;

        //        // 画像ファイル退避
        //        tifFileMove();

        //        // 過去データ作成
        //        SaveLastData();

        //        // 出勤簿ヘッダレコード削除
        //        sCom.CommandText = "delete from 出勤簿ヘッダ";
        //        sCom.ExecuteNonQuery();

        //        // 出勤簿明細レコード削除
        //        sCom.CommandText = "delete from 出勤簿明細";
        //        sCom.ExecuteNonQuery();

        //        ////設定月数分経過した過去画像およびデータを削除する
        //        imageDelete();

        //        //終了
        //        MessageBox.Show("終了しました。PCA給与Xでデータの受け入れを行ってください。", "給与計算用勤怠データ作成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        this.Tag = END_MAKEDATA;
        //        this.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        if (!dR.IsClosed) dR.Close();
        //        if (sCom.Connection.State == ConnectionState.Open) sCom.Connection.Close();

        //        //MDBファイル最適化
        //        mdbCompact();
        //    }
        //}

        /// <summary>
        /// 画像ファイル退避処理
        /// </summary>
        private void tifFileMove()
        {
            // ローカルmdb接続
            OCRData ocr = new OCRData();
            ocr.dbConnect();

            // 移動先フォルダがあるか？なければ作成する（TIFフォルダ）
            if (!System.IO.Directory.Exists(Properties.Settings.Default.tifPath))
                System.IO.Directory.CreateDirectory(Properties.Settings.Default.tifPath);

            // 出勤簿ヘッダのデータリーダーを取得する
            OleDbDataReader dR = ocr.HeaderSelect();

            while (dR.Read())
            {
                string NewFilenameYearMonth = (int.Parse(dR["年"].ToString()) + Properties.Settings.Default.RekiHosei).ToString() + 
                                              dR["月"].ToString().PadLeft(2, '0');

                // 画像ファイルパスを取得する
                string fromImg = Properties.Settings.Default.DataPath + dR["画像名"].ToString();

                //
                //  tifファイル処理
                //

                // ファイル名を「対象年月個人番号」に変えて退避先フォルダへ移動する
                string NewFilename = _PCADBName + "_" + NewFilenameYearMonth + dR["個人番号"].ToString().PadLeft(6, '0') + ".tif";
                string toImg = Properties.Settings.Default.tifPath + NewFilename;

                // 同名ファイルが既に登録済みのときは削除する
                if (System.IO.File.Exists(toImg)) System.IO.File.Delete(toImg);

                // ファイルを移動する
                if (System.IO.File.Exists(fromImg)) System.IO.File.Move(fromImg, toImg);

                // 出勤簿ヘッダレコードの画像ファイル名を書き換える
                OCRData r = new OCRData();
                r.dbConnect();
                r.HeadUpdate(NewFilename, dR["ID"].ToString());
                r.sCom.Connection.Close();
            }

            dR.Close();
            ocr.sCom.Connection.Close();
        }

        /// <summary>
        /// 受渡データ出力
        /// </summary>
        /// <param name="outFile">出力するStreamWriterオブジェクト</param>
        /// <param name="sd">集計データクラス</param>
        private void SaveDatacsv(StreamWriter outFile)
        {
            //// CSVファイルを書き出す
            //StringBuilder sb = new StringBuilder();
            //sb.Clear();
            //sb.Append(sd.c1 + sd.c2 + sd.c3 + sd.c4 + sd.c5 + sd.c6 + sd.c7 + sd.c8 + sd.c9 + sd.c10);
            //sb.Append(sd.c11 + sd.c12 + sd.c13 + sd.c14 + sd.c15 + sd.c16 + sd.c17 + sd.c18 + sd.c19 + sd.c20);
            //sb.Append(sd.c21 + sd.c22 + sd.c23 + sd.c24 + sd.c25 + sd.c26 + sd.c27 + sd.c28 + sd.c29 + sd.c30);
            //sb.Append(sd.c31 + sd.c32 + sd.c33 + sd.c34 + sd.c35 + sd.c36 + sd.c37 + sd.c38 + sd.c39 + sd.c40);

            //for (int i = 0; i < sd.c41.Length; i++)
            //{
            //    sb.Append(sd.c41[i]);
                
            //}

            ////明細ファイル出力
            //outFile.WriteLine(sb.ToString());
        }

        /// <summary>
        /// MDBファイルを最適化する
        /// </summary>
        private void mdbCompact()
        {
            try
            {
                JRO.JetEngine jro = new JRO.JetEngine();
                string OldDb = Properties.Settings.Default.mdbOlePath;
                string NewDb = Properties.Settings.Default.mdbPathTemp;

                jro.CompactDatabase(OldDb, NewDb);

                //今までのバックアップファイルを削除する
                System.IO.File.Delete(Properties.Settings.Default.mdbPath + global.MDBBACK);

                //今までのファイルをバックアップとする
                System.IO.File.Move(Properties.Settings.Default.mdbPath + global.MDBFILE, Properties.Settings.Default.mdbPath + global.MDBBACK);

                //一時ファイルをMDBファイルとする
                System.IO.File.Move(Properties.Settings.Default.mdbPath + global.MDBTEMP, Properties.Settings.Default.mdbPath + global.MDBFILE);
            }
            catch (Exception e)
            {
                MessageBox.Show("MDB最適化中" + Environment.NewLine + e.Message, "エラー", MessageBoxButtons.OK);
            }
        }
        
        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (leadImg.ScaleFactor < global.ZOOM_MAX)
            {
                leadImg.ScaleFactor += global.ZOOM_STEP;
            }
            global.miMdlZoomRate = (float)leadImg.ScaleFactor;
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (leadImg.ScaleFactor > global.ZOOM_MIN)
            {
                leadImg.ScaleFactor -= global.ZOOM_STEP;
            }
            global.miMdlZoomRate = (float)leadImg.ScaleFactor;
        }

        ///-------------------------------------------------------------
        /// <summary>
        ///     設定月数分経過した過去画像を削除する : 2017/09/07 </summary>
        ///-------------------------------------------------------------
        private void imageDelete()
        {
            // 削除月設定が0のとき、「過去画像削除しない」とみなし終了する
            if (global.cnfArchived == global.flgOff) return;

            OCRData ocr = new OCRData();

            try
            {
                //削除年月の取得
                DateTime dt = DateTime.Parse(DateTime.Today.Year.ToString() + "/" + DateTime.Today.Month.ToString() + "/01");
                DateTime delDate = dt.AddMonths(global.cnfArchived * (-1));
                int _dYY = delDate.Year;            //基準年
                int _dMM = delDate.Month;           //基準月
                int _dYYMM = _dYY * 100 + _dMM;     //基準年月
                int _waYYMM = (delDate.Year - Properties.Settings.Default.RekiHosei) * 100 + _dMM;   //基準年月(和暦）
                int _DataYYMM;
                string fileYYMM;

                //設定月数分経過した過去画像を削除する            
                foreach (string files in System.IO.Directory.GetFiles(Properties.Settings.Default.tifPath, "*.tif"))
                {
                    // ファイル名が規定外のファイルは読み飛ばします
                    if (System.IO.Path.GetFileName(files).Length < 24) continue;

                    ////ファイル名より年月を取得する : 2017/09/07
                    //fileYYMM = System.IO.Path.GetFileName(files).Substring(9, 6);

                    //if (Utility.NumericCheck(fileYYMM))
                    //{
                    //    _DataYYMM = int.Parse(fileYYMM);

                    //    //基準年月以前なら削除する
                    //    if (_DataYYMM <= _dYYMM) File.Delete(files);
                    //}

                    //ファイル名より年月を取得する : 2017/09/07
                    string[] sn = System.IO.Path.GetFileName(files).Split('_');
                    if (sn.Length > 1)
                    {
                        if (sn[1].Length > 5)
                        {
                            _DataYYMM = int.Parse(sn[1].Substring(0, 6));

                            //基準年月以前なら削除する
                            if (_DataYYMM <= _dYYMM)
                            {
                                File.Delete(files);
                            }
                        }
                    }
                }

                // 過去データを削除する
                ocr.dbConnect();
                ocr.pastDataDelete(_dYYMM);

                // 勤務票ヘッダ、勤務票明細を削除する : 2017/09/07
                ocr.comDataDelete(_dYYMM);
            }
            catch (Exception e)
            {
                MessageBox.Show("過去画像削除中" + Environment.NewLine + e.Message, "エラー", MessageBoxButtons.OK);
                return;
            }
            finally
            {
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //選択画面表示
            this.Hide();
            frmShainSel frm = new frmShainSel();
            frm.ShowDialog();
            string selID = frm._ID;
            frm.Dispose();
            this.Show();

            //勤務票が選択されていないときは終了
            if (selID == string.Empty) return;

            //カレントデータの更新
            CurDataUpDate(cI);

            //エラー情報初期化
            ErrInitial();

            //選択されたレコードへ移動する
            for (int i = 0; i < sID.Length; i++)
            {
                if (sID[i] == selID)
                {
                    cI = i;                             //カレントレコードindexをセット
                    DataShow(cI, sID, dataGridView1);    //データ表示
                    break;
                }
            }
        }
        
        /// <summary>
        /// 過去出勤簿データ登録
        /// </summary>
        private void SaveLastData()
        {
            StringBuilder sb = new StringBuilder();
            OleDbDataReader dR = null;
            OCRData ocr = new OCRData();
            ocr.dbConnect();

            try
            {
                // 領域名、年月、個人番号が一致する過去データを削除します
                dR = ocr.HeaderSelect();

                while (dR.Read())
                {
                    OCRData r = new OCRData();
                    r.dbConnect();
                    r.lastDataDelete((int.Parse(dR["年"].ToString()) + Properties.Settings.Default.RekiHosei).ToString(), 
                        dR["月"].ToString(), dR["個人番号"].ToString(), _PCADBName);
                    r.sCom.Connection.Close();
                }
                dR.Close();

                // 過去勤務票ヘッダレコードを作成します
                ocr.lastHeadInsert();

                // 和暦の過去勤務票ヘッダの年を西暦に変換
                ocr.lastHeadYearRekiHenkan();

                // 過去勤務票明細レコードを作成します
                ocr.lastItemInsert();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "過去勤務票ヘッダ作成エラー", MessageBoxButtons.OK);
            }
            finally
            {
                if (dR.IsClosed == false) dR.Close();
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == cSH || colName == cSE || colName == cEH || colName == cEE ||
                colName == cKKH || colName == cKKE || colName == cTH || colName == cTE)
            {
                e.AdvancedBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            string colName = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Name;
            if (colName == cKyuka || colName == cCheck)
            {
                if (dataGridView1.IsCurrentCellDirty)
                {
                    dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    dataGridView1.RefreshEdit();
                }
            }
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellEnter_1(object sender, DataGridViewCellEventArgs e)
        {
            string ColH = string.Empty;
            string ColM = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Name;

            // 開始時間または終了時間を判断
            if (ColM == cSM)        // 開始時刻
            {
                ColH = cSH;
            }
            else if (ColM == cEM)   // 終了時刻
            {
                ColH = cEH;
            }
            else if (ColM == cKKM)  // 規定内
            {
                ColH = cKKH;
            }
            //else if (ColM == cKSM)  // 深夜帯
            //{
            //    ColH = cKSH;
            //}
            else
            {
                return;
            }

            // 開始時、終了時が入力済みで開始分、終了分が未入力のとき"00"を表示します
            if (dataGridView1[ColH, dataGridView1.CurrentRow.Index].Value != null)
            {
                if (dataGridView1[ColH, dataGridView1.CurrentRow.Index].Value.ToString().Trim() != string.Empty)
                {
                    if (dataGridView1[ColM, dataGridView1.CurrentRow.Index].Value == null)
                    {
                        dataGridView1[ColM, dataGridView1.CurrentRow.Index].Value = "00";
                    }
                    else if (dataGridView1[ColM, dataGridView1.CurrentRow.Index].Value.ToString().Trim() == string.Empty)
                    {
                        dataGridView1[ColM, dataGridView1.CurrentRow.Index].Value = "00";
                    }
                }
            }
        }

        /// <summary>
        /// 時間記入チェック
        /// </summary>
        /// <param name="obj">データーリーダー項目オブジェクト</param>
        /// <param name="cdR">出勤簿データリーダーオブジェクト</param>
        /// <param name="tittle">チェック項目名称</param>
        /// <param name="k">勤怠記号</param>
        /// <param name="iX">日付を表すインデックス</param>
        /// <param name="Mode">時間：H, 分:M</param>
        /// <param name="errNum">エラー箇所番号</param>
        /// <returns>エラーなし：true, エラーあり：false</returns>
        private bool errCheckTime(object obj, OleDbDataReader cdR, string tittle, string k, int iX, string Mode, int errNum)
        {
            bool rtn = true;

            // 無記入のとき
            if (Utility.NulltoStr(obj) == string.Empty)
            {
                // 特別休暇・有給休暇・産休・育休・出張・欠勤・振替休日、及び、勤怠記号無記入以外で無記入のときNGとする
                if (k != global.K_TOKUBETSU_KYUKA && k != global.K_YUKYU_KYUKA &&
                    k != global.K_SANKYU && k != global.K_IKUKYU && k != global.K_SHUCCHOU && 
                    k != global.K_KEKKIN && k != global.K_FURI_KYUJITSU)
                {
                    global.errMsg = tittle + "が未入力です";
                    rtn = false;
                }
            }
            else
            {
                // 欠勤で記入されているときNGとする
                if (k == global.K_KEKKIN)
                {
                    global.errMsg = "欠勤で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 振休で記入されているときNGとする
                if (k == global.K_FURI_KYUJITSU)
                {
                    global.errMsg = "振替休日で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 特別休暇で記入されているときNGとする
                if (k == global.K_TOKUBETSU_KYUKA)
                {
                    global.errMsg = "特別休暇で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 有給休暇で記入されているときNGとする
                if (k == global.K_YUKYU_KYUKA)
                {
                    global.errMsg = "有給休暇で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 産休で記入されているときNGとする
                if (k == global.K_SANKYU)
                {
                    global.errMsg = "産休で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 育休で記入されているときNGとする
                if (k == global.K_IKUKYU)
                {
                    global.errMsg = "育休で" + tittle + "が入力されています";
                    rtn = false;
                }

                // 数字以外の記入
                if (!Utility.NumericCheck(obj.ToString()))
                {
                    global.errMsg = tittle + "が正しくありません";
                    rtn = false;
                }

                if (Mode == "H")    // 時間のチェック
                {
                    if (int.Parse(obj.ToString()) < 0 || int.Parse(obj.ToString()) > 24)
                    {
                        global.errMsg = tittle + "が正しくありません";
                        rtn = false;
                    }
                }
                else if (Mode == "M")   // 分のチェック
                {
                    if (int.Parse(obj.ToString()) < 0 || int.Parse(obj.ToString()) > 59)
                    {
                        global.errMsg = tittle + "が正しくありません";
                        rtn = false;
                    }
                }
            }

            // 戻り値
            if (!rtn)
            {
                global.errID = cdR["ID"].ToString();
                global.errNumber = errNum;
                global.errRow = iX - 1;
                return false;
            }
            else return true;
        }

        /// <summary>
        /// 休憩時間記入チェック
        /// </summary>
        /// <param name="obj">データーリーダー項目オブジェクト</param>
        /// <param name="cdR">出勤簿データリーダーオブジェクト</param>
        /// <param name="tittle">チェック項目名称</param>
        /// <param name="k">勤怠記号</param>
        /// <param name="iX">日付を表すインデックス</param>
        /// <param name="Mode">時間：H, 分:M</param>
        /// <param name="errNum">エラー箇所番号</param>
        /// <returns>エラーなし：true, エラーあり：false</returns>
        private bool errCheckKyukeiTime(object obj, OleDbDataReader cdR, string tittle, string k, int iX, string Mode, int errNum)
        {
            // 無記入のとき
            if (Utility.NulltoStr(obj) != string.Empty)
            {
                // 特別休暇で記入されているときNGとする
                if (k == global.K_TOKUBETSU_KYUKA)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "特別休暇で" + tittle + "が入力されています";
                    return false;
                }

                // 有給休暇で記入されているときNGとする
                if (k == global.K_YUKYU_KYUKA)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "有給休暇で" + tittle + "が入力されています";
                    return false;
                }

                // 産休で記入されているときNGとする
                if (k == global.K_SANKYU)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "産休で" + tittle + "が入力されています";
                    return false;
                }

                // 育休で記入されているときNGとする
                if (k == global.K_IKUKYU)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "育休で" + tittle + "が入力されています";
                    return false;
                }

                // 欠勤で記入されているときNGとする
                if (k == global.K_KEKKIN)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "欠勤で" + tittle + "が入力されています";
                    return false;
                }

                // 振替休日で記入されているときNGとする
                if (k == global.K_FURI_KYUJITSU)
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = "振替休日で" + tittle + "が入力されています";
                    return false;
                }

                // 社員で特休または有給で記入されているときNGとする
                if (cdR["給与区分"].ToString() != "1")
                {
                }

                // 数字以外の記入
                if (!Utility.NumericCheck(obj.ToString()))
                {
                    global.errID = cdR["ID"].ToString();
                    global.errNumber = errNum;
                    global.errRow = iX - 1;
                    global.errMsg = tittle + "が正しくありません";
                    return false;
                }

                if (Mode == "H")
                {
                    if (int.Parse(obj.ToString()) < 0 || int.Parse(obj.ToString()) > 24)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = errNum;
                        global.errRow = iX - 1;
                        global.errMsg = tittle + "が正しくありません";
                        return false;
                    }
                }
                else if (Mode == "M")
                {
                    if (int.Parse(obj.ToString()) < 0 || int.Parse(obj.ToString()) > 59)
                    {
                        global.errID = cdR["ID"].ToString();
                        global.errNumber = errNum;
                        global.errRow = iX - 1;
                        global.errMsg = tittle + "が正しくありません";
                        return false;
                    }
                }
            }
            return true;
        }

        private void txtShozokuCode_TextChanged(object sender, EventArgs e)
        {
            //this.lblShozoku.Text = string.Empty;

            //// SQLServer接続
            //dbControl.DataControl dCon = new dbControl.DataControl(_PCADBName);
            //OleDbDataReader dR;

            //// 部門データリーダーを取得する
            //StringBuilder sb = new StringBuilder();
            //sb.Append("select Bumon.Name from Bumon ");
            //sb.Append("where Bumon.Code = '" + txtShozokuCode.Text.Trim().PadLeft(3, '0') + "'");

            //dR = dCon.FreeReader(sb.ToString());

            //while (dR.Read())
            //{
            //    this.lblShozoku.Text = dR["Name"].ToString().Trim();
            //}

            //dR.Close();
            //dCon.Close();

        }

        /// <summary>
        /// 伝票画像表示
        /// </summary>
        /// <param name="iX">現在の伝票</param>
        /// <param name="tempImgName">画像名</param>
        public void ShowImage(string tempImgName)
        {
            //修正画面へ組み入れた画像フォームの表示    
            //画像の出力が無い場合は、画像表示をしない。
            if (tempImgName == string.Empty)
            {
                leadImg.Visible = false;
                lblNoImage.Visible = false;
                global.pblImageFile = string.Empty;
                return;
            }

            //画像ファイルがあるとき表示
            if (File.Exists(tempImgName))
            {
                lblNoImage.Visible = false;
                leadImg.Visible = true;

                // 画像操作ボタン
                btnPlus.Enabled = true;
                btnMinus.Enabled = true;

                //画像ロード
                Leadtools.Codecs.RasterCodecs.Startup();
                Leadtools.Codecs.RasterCodecs cs = new Leadtools.Codecs.RasterCodecs();

                // 描画時に使用される速度、品質、およびスタイルを制御します。 
                Leadtools.RasterPaintProperties prop = new Leadtools.RasterPaintProperties();
                prop = Leadtools.RasterPaintProperties.Default;
                prop.PaintDisplayMode = Leadtools.RasterPaintDisplayModeFlags.Resample;
                leadImg.PaintProperties = prop;

                leadImg.Image = cs.Load(tempImgName, 0, Leadtools.Codecs.CodecsLoadByteOrder.BgrOrGray, 1, 1);

                //画像表示倍率設定
                if (global.miMdlZoomRate == 0f)
                {
                    leadImg.ScaleFactor *= global.ZOOM_RATE;
                }
                else
                {
                    leadImg.ScaleFactor *= global.miMdlZoomRate;
                }

                //画像のマウスによる移動を可能とする
                leadImg.InteractiveMode = Leadtools.WinForms.RasterViewerInteractiveMode.Pan;

                // グレースケールに変換
                Leadtools.ImageProcessing.GrayscaleCommand grayScaleCommand = new Leadtools.ImageProcessing.GrayscaleCommand();
                grayScaleCommand.BitsPerPixel = 8;
                grayScaleCommand.Run(leadImg.Image);
                leadImg.Refresh();

                cs.Dispose();
                Leadtools.Codecs.RasterCodecs.Shutdown();
                global.pblImageFile = tempImgName;
            }
            else
            {
                //画像ファイルがないとき
                lblNoImage.Visible = true;

                // 画像操作ボタン
                btnPlus.Enabled = false;
                btnMinus.Enabled = false;

                leadImg.Visible = false;
                global.pblImageFile = string.Empty;
            }
        }

        private void leadImg_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void leadImg_MouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 就業奉行汎用データ作成
        /// </summary>
        private void SaveData()
        {
            string wrkOutputData;
            Boolean pblFirstGyouFlg = true;
            OCRData OutData = new OCRData();
            OutData.dbConnect();

            ////出力先フォルダがあるか？なければ作成する
            string cPath = global.cnfPath + _PCAComName + @"\";
            if (System.IO.Directory.Exists(cPath) == false)
            {
                System.IO.Directory.CreateDirectory(cPath);
            }

            //出力ファイルインスタンス作成
            string iFile = @"\" + global.OKFILE + "_";
            iFile += DateTime.Today.Year.ToString() + string.Format("{0:00}", DateTime.Today.Month) + string.Format("{0:00}", DateTime.Today.Day);
            iFile += string.Format("{0:00}", DateTime.Now.Hour) + string.Format("{0:00}", DateTime.Now.Minute) + string.Format("{0:00}", DateTime.Now.Second);
            iFile += ".csv";

            StreamWriter outFile = new StreamWriter(cPath + @"\" + iFile, false, System.Text.Encoding.GetEncoding(932));

            try
            {
                //オーナーフォームを無効にする
                this.Enabled = false;

                //プログレスバーを表示する
                frmPrg frmP = new frmPrg();
                frmP.Owner = this;
                frmP.Show();

                //レコード件数取得
                int cTotal = OutData.CountItemMDB();
                int rCnt = 1;

                //伝票最初行フラグ
                pblFirstGyouFlg = true;

                // 出勤簿明細データリーダー取得
                OleDbDataReader dR = OutData.itemSelect();
                while (dR.Read())
                {
                    //プログレスバー表示
                    frmP.Text = "汎用データ作成中です・・・" + rCnt.ToString() + "/" + cTotal.ToString();
                    frmP.progressValue = rCnt / cTotal * 100;
                    frmP.ProgressStep();

                    // 有効データの日付を対象とします
                    if (dR["勤怠記号"].ToString() != string.Empty ||
                        dR["開始時"].ToString() != string.Empty ||
                        dR["開始分"].ToString() != string.Empty ||
                        dR["終了時"].ToString() != string.Empty ||
                        dR["終了分"].ToString() != string.Empty ||
                        dR["休憩時"].ToString() != string.Empty ||
                        dR["休憩分"].ToString() != string.Empty ||
                        dR["実働時"].ToString() != string.Empty ||
                        dR["実働分"].ToString() != string.Empty ||
                        dR["交通費"].ToString() != string.Empty)
                    {
                        //出力データ初期化
                        InitOutRec(OutData);

                        //ヘッダファイル出力
                        if (pblFirstGyouFlg == true)
                        {
                            wrkOutputData = string.Empty;
                            wrkOutputData += global.H1 + ",";
                            wrkOutputData += global.H2 + ",";
                            wrkOutputData += global.H3 + ",";
                            wrkOutputData += global.H4 + ",";
                            wrkOutputData += global.H5 + ",";
                            wrkOutputData += global.H6 + ",";
                            wrkOutputData += global.H9 + ",";
                            wrkOutputData += global.H10 + ",";
                            wrkOutputData += global.H14 + ",";
                            wrkOutputData += global.H11 + ",";
                            wrkOutputData += global.H12 + ",";
                            wrkOutputData += global.H15;
                            //wrkOutputData += global.H7 + ",";
                            //wrkOutputData += global.H8;
                            outFile.WriteLine(wrkOutputData);
                        }

                        //出力データ作成
                        wrkOutputData = SetData(dR, OutData);

                        //明細ファイル出力
                        outFile.WriteLine(wrkOutputData);
                        pblFirstGyouFlg = false;
                    }

                    //データ件数加算
                    rCnt++;
                }

                dR.Close();

                //ファイルクローズ
                outFile.Close();

                // いったんオーナーをアクティブにする
                this.Activate();

                // 進行状況ダイアログを閉じる
                frmP.Close();

                // オーナーのフォームを有効に戻す
                this.Enabled = true;

            }
            catch (Exception e)
            {
                MessageBox.Show("就業奉行汎用データ作成中" + Environment.NewLine + e.Message, "エラー", MessageBoxButtons.OK);
            }
            finally
            {
                if (OutData.sCom.Connection.State == ConnectionState.Open) OutData.sCom.Connection.Close();
            }
        }

        /// <summary>
        /// 社員別交通費データ作成
        /// </summary>
        private void SaveKoutsuhi()
        {
            string wrkOutputData;
            Boolean pblFirstGyouFlg = true;

            OCRData OutData = new OCRData();
            OutData.dbConnect();

            ////出力先フォルダがあるか？なければ作成する
            string cPath = global.cnfPath + _PCAComName + @"\";
            if (System.IO.Directory.Exists(cPath) == false)
            {
                System.IO.Directory.CreateDirectory(cPath);
            }

            //出力ファイルインスタンス作成
            string iFile = @"\" + global.KOTSUHIFILE + "_";
            iFile += DateTime.Today.Year.ToString() + string.Format("{0:00}", DateTime.Today.Month) + string.Format("{0:00}", DateTime.Today.Day);
            iFile += string.Format("{0:00}", DateTime.Now.Hour) + string.Format("{0:00}", DateTime.Now.Minute) + string.Format("{0:00}", DateTime.Now.Second);
            iFile += ".csv";

            StreamWriter outFile = new StreamWriter(cPath + @"\" + iFile, false, System.Text.Encoding.GetEncoding(932));

            try
            {
                //オーナーフォームを無効にする
                this.Enabled = false;

                //プログレスバーを表示する
                frmPrg frmP = new frmPrg();
                frmP.Owner = this;
                frmP.Show();

                //レコード件数取得
                int cTotal = OutData.CountMDB();

                //伝票最初行フラグ
                pblFirstGyouFlg = true;
                int rCnt = 1;

                // 出勤簿ヘッダデータリーダー取得
                OleDbDataReader dR = OutData.HeaderSelect();
                while (dR.Read())
                {
                    //プログレスバー表示
                    frmP.Text = "交通費データ作成中です・・・" + rCnt.ToString() + "/" + cTotal.ToString();
                    frmP.progressValue = rCnt / cTotal * 100;
                    frmP.ProgressStep();

                    // 有効データの日付を対象とします
                    if (dR["申請書種別"].ToString() == global.PART_ID)
                    {
                        //出力データ初期化
                        //InitOutRec(OutData);

                        //ヘッダファイル出力
                        if (pblFirstGyouFlg == true)
                        {
                            wrkOutputData = string.Empty;
                            wrkOutputData += global.H1 + ",";
                            wrkOutputData += global.H13;
                            outFile.WriteLine(wrkOutputData);
                        }

                        //出力データ作成
                        wrkOutputData = dR["個人番号"].ToString() + "," + dR["交通費計"].ToString();

                        //明細ファイル出力
                        outFile.WriteLine(wrkOutputData);
                        pblFirstGyouFlg = false;
                    }

                    //データ件数加算
                    rCnt++;
                }

                dR.Close();

                //ファイルクローズ
                outFile.Close();

                // いったんオーナーをアクティブにする
                this.Activate();

                // 進行状況ダイアログを閉じる
                frmP.Close();

                // オーナーのフォームを有効に戻す
                this.Enabled = true;

            }
            catch (Exception e)
            {
                MessageBox.Show("交通費データ作成中" + Environment.NewLine + e.Message, "エラー", MessageBoxButtons.OK);
            }
            finally
            {
                if (OutData.sCom.Connection.State == ConnectionState.Open) OutData.sCom.Connection.Close();

                // 出力行がない場合はファイルを削除します
                if (pblFirstGyouFlg == true)
                {
                    if (System.IO.File.Exists(cPath + @"\" + iFile))
                        System.IO.File.Delete(cPath + @"\" + iFile);
                }
            }
        }

        /// <summary>
        /// 汎用データ作成
        /// </summary>
        /// <param name="iX">日index</param>
        /// <param name="dR">勤務票データリーダー</param>
        /// <param name="OutData">出力フィールド</param>
        /// <returns></returns>
        private string SetData(OleDbDataReader dR, OCRData OutData)
        {
            int sDate = 0;
            int eDate = 0;
            int sYear;

            try
            {
                //社員番号
                OutData.sShainNo = dR["個人番号"].ToString();

                //日付
                sYear = int.Parse(dR["年"].ToString()) + Properties.Settings.Default.RekiHosei;

                OutData.sDate = sYear.ToString() + "/" + dR["月"].ToString().PadLeft(2, '0') + "/" + 
                                dR["日付"].ToString().PadLeft(2, '0');

                //勤務体系コード 2013/11/11 勤務があるときは"001"を渡す
                if (dR["開始時"].ToString() != string.Empty)
                    OutData.sShift = "001";
                else OutData.sShift = string.Empty;

                // 勤怠記号から事由コードへ変換
                if (dR["勤怠記号"].ToString() == string.Empty)
                    OutData.sJiyu = string.Empty;
                else OutData.sJiyu = OutData.sJiyuArray[int.Parse(dR["勤怠記号"].ToString()) - 1];

                //出勤時刻
                if (dR["開始時"].ToString() != string.Empty && dR["開始分"].ToString() != string.Empty)
                {
                    sDate = int.Parse(dR["開始時"].ToString()) * 100 + int.Parse(dR["開始分"].ToString());
                    OutData.sStime = dR["開始時"].ToString() + ":" + dR["開始分"].ToString().PadLeft(2, '0');
                }

                //退出時刻
                if (dR["終了時"].ToString() != string.Empty && dR["終了分"].ToString() != string.Empty)
                {
                    eDate = int.Parse(dR["終了時"].ToString()) * 100 + int.Parse(dR["終了分"].ToString());
                    if (sDate >= eDate) OutData.sEtime = "翌日";
                    OutData.sEtime += dR["終了時"].ToString() + ":" + dR["終了分"].ToString().PadLeft(2, '0');
                }

                // パート・アルバイトのとき
                if (dR["申請書種別"].ToString() == global.PART_ID)
                {
                    // 出勤（実働)時間
                    if (dR["実働時"].ToString() != string.Empty || dR["実働分"].ToString() != string.Empty)
                    {
                        OutData.sWorktime = Utility.StrtoInt(dR["実働時"].ToString()).ToString() + "." + Utility.StrtoInt(dR["実働分"].ToString()).ToString().PadLeft(2, '0');
                    }
                    else if (dR["開始時"].ToString() != string.Empty)
                    {
                        // 出勤記録があるときはゼロとする
                        OutData.sWorktime = "0";
                    }

                    // 休憩時間
                    if (dR["休憩時"].ToString() != string.Empty || dR["休憩分"].ToString() != string.Empty)
                    {
                        OutData.sResttime = Utility.StrtoInt(dR["休憩時"].ToString()).ToString() + "." + Utility.StrtoInt(dR["休憩分"].ToString()).ToString();
                    }
                    else if (dR["開始時"].ToString() != string.Empty)
                    {
                        // 出勤記録があって実働時分がないときははゼロを渡す
                        OutData.sResttime = "0";
                    }
                }
                    // 社員のとき
                else if (dR["申請書種別"].ToString() == global.SHAIN_ID)
                {
                    int wh = (Utility.StrtoInt(dR["実働時"].ToString()));

                    // 出勤（実働)時間
                    if (dR["実働時"].ToString() != string.Empty || dR["実働分"].ToString() != string.Empty)
                    {
                        // 実働時間が1時間以上のときは休憩時間として1H差し引く
                        if (wh > 1) OutData.sWorktime = (wh - 1).ToString() + "." + Utility.StrtoInt(dR["実働分"].ToString()).ToString().PadLeft(2, '0');
                        else OutData.sWorktime = Utility.StrtoInt(dR["実働時"].ToString()).ToString() + "." + Utility.StrtoInt(dR["実働分"].ToString()).ToString().PadLeft(2, '0');
                    }
                    else if (dR["開始時"].ToString() != string.Empty)
                    {
                        // 出勤記録があって実働時分がないときははゼロを渡す
                        OutData.sWorktime = "0";
                    }

                   // 休憩時間
                    if (dR["開始時"].ToString() != string.Empty)
                    {
                        if (wh > 1) OutData.sResttime = "1.0";      // １Hをセットする
                        else OutData.sResttime = "0";               // 出勤記録があるときはゼロとする                        
                    }
                }

                // 休日勤務時間　2013/11/11
                if (OutData.sJiyu == "21")  // 休日出勤デイリーのとき休日勤務時間に標記する
                {
                    OutData.sShift = "090"; // "休日"の勤務体系コードを渡す
                    OutData.sHolidayWork = OutData.sWorktime;
                    OutData.sWorktime = string.Empty; 
                }
                else OutData.sHolidayWork = string.Empty;
                
                //出力文字列作成
                StringBuilder sb = new StringBuilder();
                sb.Append(OutData.sShainNo).Append(",");
                sb.Append(OutData.sDate).Append(",");
                sb.Append(OutData.sShift).Append(",");
                sb.Append(OutData.sJiyu).Append(",");
                sb.Append(OutData.sStime).Append(",");
                sb.Append(OutData.sEtime).Append(",");
                sb.Append(Properties.Settings.Default.ShukkinCode).Append(",");
                sb.Append(Properties.Settings.Default.KyuukeiCode).Append(",");
                sb.Append("031").Append(",");       // 休日勤務時間 2013/11/11
                sb.Append(OutData.sWorktime).Append(",");
                sb.Append(OutData.sResttime).Append(",");
                sb.Append(OutData.sHolidayWork);    // 休日勤務時間 2013/11/11
                //sb.Append(OutData.sGtime).Append(",");
                //sb.Append(OutData.sMtime);

                return sb.ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("就業奉行汎用データ作成中" + Environment.NewLine + e.Message, "エラー", MessageBoxButtons.OK);
                return string.Empty;
            }
        }

        /// <summary>
        /// 出力用データ初期化
        /// </summary>
        /// <param name="OutData">出力用データ</param>
        private void InitOutRec(OCRData OutData)
        {
            OutData.sShainNo = string.Empty;
            OutData.sDate = string.Empty;
            OutData.sShift = string.Empty;
            OutData.sJiyu = string.Empty;
            OutData.sStime = string.Empty;
            OutData.sEtime = string.Empty;
            OutData.sGtime = string.Empty;
            OutData.sMtime = string.Empty;
            OutData.sResttime = string.Empty;
            OutData.sWorktime = string.Empty;
        }
    }
}
