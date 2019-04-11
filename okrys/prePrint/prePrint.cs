using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;    // 2017/09/04
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.Odbc;
using okrys.Common;

namespace okrys
{
    public partial class prePrint : Form
    {
        //所属コード設定桁数
        int ShozokuLen = 0;
         
        string _ComNo = string.Empty;               // 会社番号
        string _ComName = string.Empty;             // 会社名
        string _ComDatabeseName = string.Empty;     // 会社データベース名

        string appName = "勤怠申請書印刷";             // アプリケーション表題

        public prePrint()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ウィンドウズ最小サイズ
            Utility.WindowsMinSize(this, this.Size.Width, this.Size.Height);

            //ウィンドウズ最大サイズ
            Utility.WindowsMaxSize(this, this.Size.Width, this.Size.Height);

            //会社選択画面表示
            frmComSelect frm = new frmComSelect();
            frm.ShowDialog();
            _ComNo = frm._pblComNo;
            _ComName = frm._pblComName;
            _ComDatabeseName = frm._pblDbName;
            frm.Dispose();

            //キャプション
            this.Text = "勤怠申請書印刷 【" + _ComName + "】";

            ////////自分自身のバージョン情報を取得する　2011/03/25
            //////System.Diagnostics.FileVersionInfo ver =
            //////    System.Diagnostics.FileVersionInfo.GetVersionInfo(
            //////    System.Reflection.Assembly.GetExecutingAssembly().Location);

            ////////キャプションにバージョンを追加　2011/03/25
            //////this.Text += " ver " + ver.FileMajorPart.ToString() + "." + ver.FileMinorPart.ToString();

            //所属コードの桁数を取得する
            string sqlSTRING = string.Empty;

            //dbControl.DataControl sdcon = new dbControl.DataControl(_ComDatabeseName);　//2011/04/03
            //OleDbDataReader dR; //2011/04/03

            // 勘定奉行データベース接続文字列を取得する 2017/09/04
            string sc = SqlControl.obcConnectSting.get(_ComDatabeseName);

            SqlControl.DataControl sdcon = new SqlControl.DataControl(sc);

            //データリーダーを取得する
            SqlDataReader dR;

            sqlSTRING += "select ConfigXml.value('(/OBCXmlConfig/Config/Factor/@theNoOfDepartmentCodeFigure)[1]','int') as name ";
            sqlSTRING += "from tbCM_CorpOperationConfig";

            //scom.CommandText = sqlSTRING;
            //dR = scom.ExecuteReader();

            dR = sdcon.free_dsReader(sqlSTRING);

            while (dR.Read())
            {
                ShozokuLen = int.Parse(dR["name"].ToString());               
            }
            dR.Close();
            sdcon.Close(); //2011/04/03

            //開始部門コンボロード
            Utility.ComboBumon.load(cmbBumonS, ShozokuLen, _ComDatabeseName);
            cmbBumonS.MaxDropDownItems = 20;

            //終了部門コンボロード
            Utility.ComboBumon.load(cmbBumonE, ShozokuLen, _ComDatabeseName);
            cmbBumonE.MaxDropDownItems = 20;

            //DataGridViewの設定
            GridViewSetting(dg1);

            txtYear.Focus();

            //社員番号
            txtSNo.Text = string.Empty;
            txtENo.Text = string.Empty;

            //チェックボタン
            btnCheckOn.Enabled = false;
            btnCheckOff.Enabled = false;
            rbAll.Checked = true;

            //印刷ボタン
            btnPrn.Enabled = false;

            //元号表示　2011/03/24
            label5.Text = Properties.Settings.Default.gengou;
        }

        /// <summary>
        /// データグリッドビューの定義を行います
        /// </summary>
        /// <param name="tempDGV">データグリッドビューオブジェクト</param>
        public void GridViewSetting(DataGridView tempDGV)
        {
            try
            {
                //フォームサイズ定義

                // 列スタイルを変更するe

                tempDGV.EnableHeadersVisualStyles = false;

                // 列ヘッダー表示位置指定
                tempDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;

                // 列ヘッダーフォント指定
                tempDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Meiryo UI", 11, FontStyle.Regular);

                // データフォント指定
                tempDGV.DefaultCellStyle.Font = new Font("Meiryo UI", (float)11, FontStyle.Regular);

                // 行の高さ
                tempDGV.ColumnHeadersHeight = 22;
                tempDGV.RowTemplate.Height = 22;

                // 全体の高さ
                tempDGV.Height = 532;

                // 奇数行の色
                //tempDGV.AlternatingRowsDefaultCellStyle.BackColor = Color.Lavender;

                // 各列幅指定
                DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
                tempDGV.Columns.Add(column);
                tempDGV.Columns.Add("col1", "コード");
                tempDGV.Columns.Add("col2", "所属");
                tempDGV.Columns.Add("col3", "社員番号");
                tempDGV.Columns.Add("col4", "社員名");
                tempDGV.Columns.Add("col5", "区分");
                tempDGV.Columns.Add("col6", "在籍区分");

                tempDGV.Columns[0].Width = 40;
                tempDGV.Columns[1].Width = 80;
                tempDGV.Columns[2].Width = 220;
                tempDGV.Columns[3].Width = 100;
                tempDGV.Columns[4].Width = 220;
                tempDGV.Columns[5].Width = 140;
                tempDGV.Columns[6].Width = 80;

                tempDGV.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                tempDGV.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                // 編集可否
                tempDGV.ReadOnly = false;
                tempDGV.Columns[0].ReadOnly = false;
                tempDGV.Columns[1].ReadOnly = true;
                tempDGV.Columns[2].ReadOnly = true;
                tempDGV.Columns[3].ReadOnly = true;
                tempDGV.Columns[4].ReadOnly = true;
                tempDGV.Columns[5].ReadOnly = true;
                tempDGV.Columns[6].ReadOnly = true;

                // 行ヘッダを表示しない
                tempDGV.RowHeadersVisible = false;

                // 選択モード
                tempDGV.SelectionMode = DataGridViewSelectionMode.CellSelect;
                tempDGV.MultiSelect = true;

                // 追加行表示しない
                tempDGV.AllowUserToAddRows = false;

                // データグリッドビューから行削除を禁止する
                tempDGV.AllowUserToDeleteRows = false;

                // 手動による列移動の禁止
                tempDGV.AllowUserToOrderColumns = false;

                // 列サイズ変更禁止
                tempDGV.AllowUserToResizeColumns = true;

                // 行サイズ変更禁止
                tempDGV.AllowUserToResizeRows = false;

                // 行ヘッダーの自動調節
                //tempDGV.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

                //ソート機能制限
                for (int i = 0; i < tempDGV.Columns.Count; i++)
                {
                    tempDGV.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラーメッセージ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///-------------------------------------------------------------------
        /// <summary>
        ///     グリッドビューへ社員情報を表示する : 勘定奉行i10 2017/09/04 </summary>
        /// <param name="tempDGV">
        ///     DataGridViewオブジェクト名</param>
        /// <param name="tempDate">
        ///     所属基準日</param>
        /// <param name="sCode">
        ///     所属範囲開始コード</param>
        /// <param name="eCode">
        ///     所属範囲終了コード</param>
        /// <param name="sNo">
        ///     社員範囲開始コード</param>
        /// <param name="eNo">
        ///     社員範囲終了コード</param>
        ///-------------------------------------------------------------------
        private void GridViewShowData(string dbName, DataGridView tempDGV, string tempDate, string sCode, string eCode, Int64 sNo, Int64 eNo)
        {
            // 勘定奉行データベース接続文字列を取得する 2017/09/04
            string sc = SqlControl.obcConnectSting.get(dbName);

            // 勘定奉行データベースに接続する 2017/09/04
            SqlControl.DataControl sdcon = new SqlControl.DataControl(sc);

            string sqlSTRING = string.Empty;

            SqlDataReader dR;

            //データリーダーを取得する
            sqlSTRING += "select tbDepartment.DepartmentID,tbDepartment.DepartmentCode,tbDepartment.DepartmentName,";
            sqlSTRING += "tbEmployeeBase.EmployeeNo,tbEmployeeBase.Name,tbHR_DivisionCategory.CategoryName,zaiseki.CategoryName as zaisekikbn ";
            sqlSTRING += "from (((tbEmployeeBase inner join tbHR_DivisionCategory ";
            sqlSTRING += "on EmploymentDivisionID = CategoryID) inner join tbHR_DivisionCategory as zaiseki ";
            sqlSTRING += "on BeOnTheRegisterDivisionID = zaiseki.CategoryID) inner join ";
            
            sqlSTRING += "(select tbEmployeeMainDutyPersonnelChange.EmployeeID,tbEmployeeMainDutyPersonnelChange.BelongID from tbEmployeeMainDutyPersonnelChange inner join (";
            sqlSTRING += "select EmployeeID,max(AnnounceDate) as AnnounceDate from tbEmployeeMainDutyPersonnelChange ";
            sqlSTRING += "where AnnounceDate <= '" + tempDate + "'";
            sqlSTRING += "group by EmployeeID) as a ";
            sqlSTRING += "on (tbEmployeeMainDutyPersonnelChange.EmployeeID = a.EmployeeID) and ";
            sqlSTRING += "(tbEmployeeMainDutyPersonnelChange.AnnounceDate = a.AnnounceDate) ";
            sqlSTRING += "inner join tbDepartment ";
            sqlSTRING += "on tbEmployeeMainDutyPersonnelChange.BelongID = tbDepartment.DepartmentID ";
            sqlSTRING += "where (tbDepartment.DepartmentCode >= '" + sCode +"') and (tbDepartment.DepartmentCode <= '" + eCode + "')) as d ";

            sqlSTRING += "on tbEmployeeBase.EmployeeID = d.EmployeeID) left join ";
            sqlSTRING += "tbDepartment on d.BelongID = tbDepartment.DepartmentID ";

            sqlSTRING += "where (tbEmployeeBase.EmployeeNo >= " + sNo.ToString() + ") and (";
            sqlSTRING += "tbEmployeeBase.EmployeeNo <= " + eNo.ToString() + ") ";
            //sqlSTRING += "and (tbEmployeeBase.BeOnTheRegisterDivisionID != 9) ";    // 退職者を除外

            // 雇用区分指定
            if (rbAll.Checked)   // 正社員、アルバイト、パートを対象とする
            {
                sqlSTRING += "and (tbEmployeeBase.EmploymentDivisionID = 2 or tbEmployeeBase.EmploymentDivisionID = 5 or tbEmployeeBase.EmploymentDivisionID = 6) ";
            }
            else if (rbShain.Checked)   // 正社員を対象とする
            {
                sqlSTRING += "and (tbEmployeeBase.EmploymentDivisionID = 2) ";
            }
            else if (rbPart.Checked)   // パート・アルバイトを対象とする
            {
                sqlSTRING += "and (tbEmployeeBase.EmploymentDivisionID = 5 or tbEmployeeBase.EmploymentDivisionID = 6) ";
            }

            sqlSTRING += "order by tbDepartment.DepartmentCode,tbEmployeeBase.EmployeeNo";

            dR = sdcon.free_dsReader(sqlSTRING);

            try
            {
                //グリッドビューに表示する
                int iX = 0;
                tempDGV.RowCount = 0;

                while (dR.Read())
                {
                    if (dR["zaisekikbn"].ToString().Trim() != "退職")
                    {
                        //データグリッドにデータを表示する
                        tempDGV.Rows.Add();
                        GridViewCellData(tempDGV, iX, dR);
                        iX++;
                    }
                }
                tempDGV.CurrentCell = null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラー", MessageBoxButtons.OK);
            }
            finally
            {
                dR.Close();
                sdcon.Close();
            }

            //社員情報がないとき
            if (tempDGV.RowCount == 0)
            {
                MessageBox.Show("社員情報が存在しません", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                btnCheckOn.Enabled = false;
                btnCheckOff.Enabled = false;
                btnPrn.Enabled = false;
            }
            else
            {
                btnCheckOn.Enabled = true;
                btnCheckOff.Enabled = true;
                btnPrn.Enabled = true;
            }
        }

        ///-----------------------------------------------------------------
        /// <summary>
        ///     データグリッドに表示データをセットする : 
        ///     勘定奉行i10 2017/09/04 </summary>
        /// <param name="tempDGV">
        ///     datagridviewオブジェクト名</param>
        /// <param name="iX">
        ///     Row№</param>
        /// <param name="dR">
        ///     データリーダーオブジェクト名</param>
        ///-----------------------------------------------------------------
        private void GridViewCellData(DataGridView tempDGV, int iX, SqlDataReader dR)
        {
            tempDGV[0, iX].Value = true;
            //tempDGV[1, iX].Value = string.Format("{0:000000000000000}", Int64.Parse(dR["DepartmentCode"].ToString()));
            //tempDGV[1, iX].Value = (dR["DepartmentCode"].ToString() + "    ").Substring(0, ShozokuLen);
            
            if (Utility.NumericCheck(dR["DepartmentCode"].ToString()))
                tempDGV[1, iX].Value = int.Parse(dR["DepartmentCode"].ToString()).ToString().PadLeft(ShozokuLen, '0');
            else tempDGV[1, iX].Value = (dR["DepartmentCode"].ToString() + "    ").Substring(0, ShozokuLen);
            
            tempDGV[2, iX].Value = dR["DepartmentName"].ToString();
            tempDGV[3, iX].Value = int.Parse(dR["EmployeeNo"].ToString().Trim()).ToString().PadLeft(6, '0');
            tempDGV[4, iX].Value = dR["Name"].ToString().Trim();
            tempDGV[5, iX].Value = dR["CategoryName"].ToString().Trim();
            tempDGV[6, iX].Value = dR["zaisekikbn"].ToString().Trim();
        }

        private void btnCheckOn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("表示中の社員全てを印刷対象にします。よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            for (int i = 0; i < dg1.Rows.Count; i++)
            {
                dg1[0, i].Value = true;
            }

            btnPrn.Enabled = true;

        }

        private void btnCheckOff_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("表示中の社員全てを印刷対象外にします。よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            for (int i = 0; i < dg1.Rows.Count; i++)
            {
                dg1[0, i].Value = false;
            }
        }

        private void btnPrn_Click(object sender, EventArgs e)
        {
            int pCnt = 0;

            //エラーチェック
            if (ErrCheck() == false) return;

            //件数取得
            pCnt = PrintRowCount();
            if (pCnt == 0) 
            {
                MessageBox.Show("印刷対象行がありません", "印刷確認", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show(pCnt.ToString() + "件の勤怠申請書を印刷します。よろしいですか？", "印刷確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            
            sReport();

            MessageBox.Show("印刷が終了しました", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Boolean ErrCheck()
        {
            if (Utility.NumericCheck(txtYear.Text) == false)
            {
                MessageBox.Show("年は数字で入力してください", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtYear.Focus();
                return false;
            }

            if (Utility.NumericCheck(txtMonth.Text) == false)
            {
                MessageBox.Show("月は数字で入力してください", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtMonth.Focus();
                return false;
            }

            if (int.Parse(txtMonth.Text) < 1 || int.Parse(txtMonth.Text) > 12)
            {
                MessageBox.Show("月が正しくありません", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtMonth.Focus();
                return false;
            }

            if (txtSNo.Text != string.Empty && Utility.NumericCheck(txtSNo.Text) == false)
            {
                MessageBox.Show("開始社員番号は数字で入力してください", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtSNo.Focus();
                return false;
            }

            if (txtENo.Text != string.Empty && Utility.NumericCheck(txtENo.Text) == false)
            {
                MessageBox.Show("終了社員番号は数字で入力してください", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtENo.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// プリント件数取得
        /// </summary>
        /// <returns>印刷件数</returns>
        private int PrintRowCount()
        {
            int pCnt = 0;

            for (int i = 0; i < dg1.Rows.Count; i++)
            {
                if (dg1[0, i].Value.ToString() == "True") pCnt++;
            }

            return pCnt;
        }

        ///-----------------------------------------------------
        /// <summary>
        ///     社員出勤簿印刷・シート追加一括印刷 </summary>
        ///-----------------------------------------------------
        private void sReport()
        {
            const int S_GYO = 7;        //エクセルファイル日付明細開始行

            string sDate;
            DateTime eDate;

            //////const int S_ROWSMAX = 7; //エクセルファイル列最大値

            try
            {

                //マウスポインタを待機にする
                this.Cursor = Cursors.WaitCursor;

                string sAppPath = System.AppDomain.CurrentDomain.BaseDirectory;

                Excel.Application oXls = new Excel.Application();

                // 勤怠申請書テンプレートシート
                Excel.Workbook oXlsBook = (Excel.Workbook)(oXls.Workbooks.Open(Properties.Settings.Default.sxlsPath, 
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing));

                // 勤怠申請書印刷用シート
                Excel.Workbook oXlsPrintBook = (Excel.Workbook)(oXls.Workbooks.Open(Properties.Settings.Default.wxlsPath,
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                   Type.Missing, Type.Missing));

                Excel.Worksheet oxlsSheet = (Excel.Worksheet)oXlsBook.Sheets[1];    // 社員勤怠申請書
                Excel.Worksheet oxlsPart = (Excel.Worksheet)oXlsBook.Sheets[2];     // パート・アルバイト勤怠申請書
                Excel.Worksheet oxlsPrintSheet = null;    // 印刷用ワークシート

                Excel.Range[] rng = new Microsoft.Office.Interop.Excel.Range[2];

                try
                {
                    int pCnt = 0;

                    //グリッドを順番に読む
                    for (int i = 0; i < dg1.RowCount; i++)
                    {
                        //チェックがあるものを対象とする
                        if (dg1[0, i].Value.ToString() == "True")
                        {
                            pCnt++;     // ページカウント

                            int sRow = i;   // グリッド行インデックス取得

                            // 雇用区分取得
                            string Category = dg1[5, sRow].Value.ToString();

                            // 印刷用BOOKへシートを追加する 
                            if (Category == global.CATEGORY_SHAIN)  // 正社員のとき：社員勤怠申請書
                            {                                
                                oxlsSheet.Copy(Type.Missing, oXlsPrintBook.Sheets[pCnt]);
                            }
                            else if (Category == global.CATEGORY_PART || Category == global.CATEGORY_ARBEIT) // パート、アルバイトのとき：パート、アルバイト勤怠申請書
                            {
                                oxlsPart.Copy(Type.Missing, oXlsPrintBook.Sheets[pCnt]);
                            }

                            // カレントのシートを設定
                            oxlsPrintSheet = (Excel.Worksheet)oXlsPrintBook.Sheets[pCnt + 1];

                            //// 印刷2件目以降はシートを追加する
                            //if (pCnt > 1)
                            //{
                            //    oxlsSheet.Copy(Type.Missing, oXlsPrintBook.Sheets[pCnt - 1]);
                            //    oxlsSheet = (Excel.Worksheet)oXlsPrintBook.Sheets[pCnt];
                            //}

                            // 正社員のとき：社員勤怠申請書
                            if (Category == global.CATEGORY_SHAIN)
                            { 
                                //会社名   2011/02/24 
                                oxlsPrintSheet.Cells[1, 29] = _ComName;

                                //社員番号
                                oxlsPrintSheet.Cells[2, 22] = dg1[3, sRow].Value.ToString().Substring(0, 1);
                                oxlsPrintSheet.Cells[2, 24] = dg1[3, sRow].Value.ToString().Substring(1, 1);
                                oxlsPrintSheet.Cells[2, 25] = dg1[3, sRow].Value.ToString().Substring(2, 1);
                                oxlsPrintSheet.Cells[2, 26] = dg1[3, sRow].Value.ToString().Substring(3, 1);
                                oxlsPrintSheet.Cells[2, 27] = dg1[3, sRow].Value.ToString().Substring(4, 1);
                                oxlsPrintSheet.Cells[2, 28] = dg1[3, sRow].Value.ToString().Substring(5, 1);
                            }
                            else if (Category == global.CATEGORY_PART || Category == global.CATEGORY_ARBEIT) // パート、アルバイトのとき：パート、アルバイト勤怠申請書
                            {
                                //会社名   2011/02/24 
                                oxlsPrintSheet.Cells[1, 28] = _ComName;
                                    
                                //社員番号
                                oxlsPrintSheet.Cells[2, 22] = dg1[3, sRow].Value.ToString().Substring(0, 1);
                                oxlsPrintSheet.Cells[2, 23] = dg1[3, sRow].Value.ToString().Substring(1, 1);
                                oxlsPrintSheet.Cells[2, 24] = dg1[3, sRow].Value.ToString().Substring(2, 1);
                                oxlsPrintSheet.Cells[2, 25] = dg1[3, sRow].Value.ToString().Substring(3, 1);
                                oxlsPrintSheet.Cells[2, 26] = dg1[3, sRow].Value.ToString().Substring(4, 1);
                                oxlsPrintSheet.Cells[2, 27] = dg1[3, sRow].Value.ToString().Substring(5, 1);
                            }

                            //元号
                            oxlsPrintSheet.Cells[3, 1] = Properties.Settings.Default.gengou;

                            //年
                            oxlsPrintSheet.Cells[3, 4] = string.Format("{0, 2}", int.Parse(txtYear.Text)).Substring(0, 1);
                            oxlsPrintSheet.Cells[3, 5] = string.Format("{0, 2}", int.Parse(txtYear.Text)).Substring(1, 1);

                            //月
                            oxlsPrintSheet.Cells[3, 8] = string.Format("{0, 2}", int.Parse(txtMonth.Text)).Substring(0, 1);
                            oxlsPrintSheet.Cells[3, 9] = string.Format("{0, 2}", int.Parse(txtMonth.Text)).Substring(1, 1);

                            //所属
                            oxlsPrintSheet.Cells[3, 14] = dg1[2, sRow].Value.ToString();
                            
                            //氏名
                            oxlsPrintSheet.Cells[2, 14] = dg1[4, sRow].Value.ToString();

                            int addRow = 0;
                            for (int iX = global.MAX_MIN; iX <= global.MAX_GYO; iX++)
                            {
                                addRow = iX - 1;

                                //暦補正値は設定ファイルから取得する 2011/03/24
                                sDate = (int.Parse(txtYear.Text) + Properties.Settings.Default.RekiHosei).ToString() + "/" + txtMonth.Text + "/" + iX.ToString();

                                if (DateTime.TryParse(sDate, out eDate))
                                    oxlsPrintSheet.Cells[S_GYO + addRow, 2] = ("日月火水木金土").Substring(int.Parse(eDate.DayOfWeek.ToString("d")), 1);
                                else oxlsPrintSheet.Cells[S_GYO + addRow, 1] = string.Empty;
                            }
                        }
                    }

                    //マウスポインタを元に戻す
                    this.Cursor = Cursors.Default;

                    // 印刷用BOOKの1番目のシートは削除する
                    ((Excel.Worksheet)oXlsPrintBook.Sheets[1]).Delete();

                    // 確認のためExcelのウィンドウを表示する
                    //oXls.Visible = true;

                    //印刷
                    //oxlsSheet.PrintPreview(false);
                    //oxlsSheet.PrintOut(1, Type.Missing, 1, false, oXls.ActivePrinter, Type.Missing, Type.Missing, Type.Missing);
                    //oXlsBook.PrintOut();
                    oXlsPrintBook.PrintOut();

                    // ウィンドウを非表示にする
                    oXls.Visible = false;

                    //保存処理
                    oXls.DisplayAlerts = false;

                    //Bookをクローズ
                    oXlsBook.Close(Type.Missing, Type.Missing, Type.Missing);
                    oXlsPrintBook.Close(Type.Missing, Type.Missing, Type.Missing);

                    //Excelを終了
                    oXls.Quit();
                }

                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "印刷処理", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    // ウィンドウを非表示にする
                    oXls.Visible = false;

                    //保存処理
                    oXls.DisplayAlerts = false;

                    //Bookをクローズ
                    oXlsBook.Close(Type.Missing, Type.Missing, Type.Missing);

                    //Excelを終了
                    oXls.Quit();
                }

                finally
                {
                    // COM オブジェクトの参照カウントを解放する 
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oxlsSheet);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oxlsPrintSheet);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oXlsBook);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oXlsPrintBook);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oXls);

                    //マウスポインタを元に戻す
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "印刷処理", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //マウスポインタを元に戻す
            this.Cursor = Cursors.Default;
        }

        private void txtYear_Enter(object sender, EventArgs e)
        {
            TextBox txtObj = new TextBox();
            
            if (sender == txtYear) txtObj = txtYear;
            if (sender == txtMonth) txtObj = txtMonth;
            if (sender == txtSNo) txtObj = txtSNo;
            if (sender == txtENo) txtObj = txtENo;

            txtObj.SelectAll();
        }

        private void btnRtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("終了します。よろしいですか？",appName,MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            this.Dispose();
        }

        private void btnSel_Click(object sender, EventArgs e)
        {
            string sDate = string.Empty;
            string sCode;
            string eCode;
            Int64 sNo;
            Int64 eNo;

            //エラーチェック
            if (ErrCheck() == false) return;

            //基準日付
            sDate = (int.Parse(txtYear.Text) + Properties.Settings.Default.RekiHosei).ToString() + "/" + txtMonth.Text + "/01";
            
            //開始部門コード取得
            if (cmbBumonS.SelectedIndex == -1)
            {
                sCode = "";
            }
            else
            {
                Utility.ComboBumon cmbs = new Utility.ComboBumon();
                cmbs = (Utility.ComboBumon)cmbBumonS.SelectedItem;
                sCode = cmbs.ID;
            }

            //終了部門コード取得
            if (cmbBumonE.SelectedIndex == -1)
            {
                //eCode = "999999999999999";
                eCode = "zzzzzzzzzzzzzzz";
            }
            else
            {
                Utility.ComboBumon cmbe = new Utility.ComboBumon();
                cmbe = (Utility.ComboBumon)cmbBumonE.SelectedItem;
                eCode = cmbe.ID;
            }

            //開始社員番号取得
            if (txtSNo.Text == string.Empty)
            {
                sNo = 0;
            }
            else
            {
                sNo = Int64.Parse(txtSNo.Text);
            }

            //終了社員番号取得
            if (txtENo.Text == string.Empty)
            {
                eNo = 9999999999;
            }
            else
            {
                eNo = Int64.Parse(txtENo.Text);
            }

            //データ表示
            GridViewShowData(_ComDatabeseName, dg1, sDate, sCode, eCode, sNo, eNo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sqlSTRING = string.Empty;

            OdbcConnection ocn = new OdbcConnection("Dsn=ocrODBC;uid=ocrapp;pwd=ocrpass;");
            //OdbcConnectionStringBuilder oCn = new OdbcConnectionStringBuilder();
            //oCn.Dsn = global.pblUserDsn;
            //OdbcConnection ocn = new OdbcConnection(oCn.ConnectionString);
            ocn.Open();

            OdbcCommand scom = new OdbcCommand();
            OdbcDataReader dR;

            
            sqlSTRING += "SELECT EntityCode,EntityName,DatabaseName FROM tbCorpDatabaseContext ";
            sqlSTRING += "order by EntityCode";
            scom.CommandText = sqlSTRING;
            scom.Connection = ocn;
            dR = scom.ExecuteReader();

            while (dR.Read())
            {
                MessageBox.Show(dR["EntityCode"].ToString() + ":" + dR["EntityName"].ToString() + ":" + dR["DatabaseName"].ToString()); 
            }

            dR.Close();
            ocn.Close();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            //if (e.KeyCode == Keys.Enter)
            //{
            //    if (!e.Control)
            //    {
            //        this.SelectNextControl(this.ActiveControl, !e.Shift, true, true, true);
            //    }
            //}
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

            //if (e.KeyChar == (char)Keys.Enter)
            //{
            //    e.Handled = true;
            //}
        }

        private void rBtnPrn_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void prePrint_Shown(object sender, EventArgs e)
        {
            txtYear.Focus();
        }

        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b') 
                e.Handled = true;

        }

    }
}
