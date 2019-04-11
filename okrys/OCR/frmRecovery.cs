using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlClient;
using okrys.Common;

namespace okrys
{
    public partial class frmRecovery : Form
    {
        //所属コード設定桁数
        int ShozokuLen = 0;
         
        string _ComNo = string.Empty;                   // 会社番号
        string _ComName = string.Empty;                 // 会社名
        string _ComDatabeseName = string.Empty;         // 会社データベース名

        const int MODE_ALL = 0;                         // 全員
        const int MODE_MIKAISHU = 1;                    // 未回収
        const int MODE_SHORICHU = 2;                    // 処理中
        const int MODE_SHORIZUMI = 3;                   // 処理済み
        string appName = "勤怠申請書OCR処理実施状況";      // アプリケーション表題

        public frmRecovery()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ウィンドウズ最小サイズ
            Utility.WindowsMinSize(this, this.Size.Width, this.Size.Height);

            //ウィンドウズ最大サイズ
            //Utility.WindowsMaxSize(this, this.Size.Width, this.Size.Height);

            //会社選択画面表示
            frmComSelect frm = new frmComSelect();
            frm.ShowDialog();
            _ComNo = frm._pblComNo;
            _ComName = frm._pblComName;
            _ComDatabeseName = frm._pblDbName;
            frm.Dispose();

            //キャプション
            this.Text = "勤怠申請書OCR処理実施状況 【" + _ComName + "】";

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

            // 勘定奉行データベースに接続する 2017/09/04
            SqlControl.DataControl sdcon = new SqlControl.DataControl(sc);

            //データリーダーを取得する 2017/09/04
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

            // 部門コンボロード
            Utility.ComboBumon.load(cmbBumonS, ShozokuLen, _ComDatabeseName);
            cmbBumonS.MaxDropDownItems = 20;

            //DataGridViewの設定
            GridViewSetting(dg1);

            // 対象年月を取得
            txtYear.Text = global.cnfYear.ToString();
            txtMonth.Text = global.cnfMonth.ToString();

            txtYear.Focus();
            
            //元号表示　2011/03/24
            label5.Text = Properties.Settings.Default.gengou;

            // 表示選択コンボボックス
            comboBox1.Items.Add("すべて表示");
            comboBox1.Items.Add("ＯＣＲ未実施");
            comboBox1.Items.Add("処理中");
            comboBox1.Items.Add("受け渡しデータ作成済み");
            comboBox1.SelectedIndex = 0;
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
                tempDGV.Columns.Add("col1", "コード");
                tempDGV.Columns.Add("col2", "所属");
                tempDGV.Columns.Add("col3", "社員番号");
                tempDGV.Columns.Add("col4", "社員名");
                tempDGV.Columns.Add("col5", "区分");
                tempDGV.Columns.Add("col6", "OCR");
                tempDGV.Columns.Add("col7", "状況");
                tempDGV.Columns.Add("col8", "作成日");

                tempDGV.Columns[0].Width = 80;
                tempDGV.Columns[1].Width = 220;
                tempDGV.Columns[2].Width = 100;
                tempDGV.Columns[3].Width = 180;
                tempDGV.Columns[4].Width = 100;
                tempDGV.Columns[5].Width = 80;
                tempDGV.Columns[6].Width = 160;
                tempDGV.Columns[7].Width = 170;

                tempDGV.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                tempDGV.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // 編集可否
                tempDGV.ReadOnly = false;
                tempDGV.Columns[0].ReadOnly = true;
                tempDGV.Columns[1].ReadOnly = true;
                tempDGV.Columns[2].ReadOnly = true;
                tempDGV.Columns[3].ReadOnly = true;
                tempDGV.Columns[4].ReadOnly = true;
                tempDGV.Columns[5].ReadOnly = true;
                tempDGV.Columns[6].ReadOnly = true;

                // 行ヘッダを表示しない
                tempDGV.RowHeadersVisible = false;

                // 選択モード
                tempDGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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

                ////ソート機能制限
                //for (int i = 0; i < tempDGV.Columns.Count; i++)
                //{
                //    tempDGV.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //}

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラーメッセージ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///---------------------------------------------------------------
        /// <summary>
        ///     グリッドビューへ社員情報を表示する : 
        ///         勘定奉行i10 2017/09/04 </summary>
        /// <param name="tempDGV">
        ///     DataGridViewオブジェクト名</param>
        /// <param name="tempDate">
        ///     所属基準日</param>
        /// <param name="sCode">
        ///     指定所属コード</param>
        ///---------------------------------------------------------------
        private void GridViewShowData(string dbName, DataGridView tempDGV, string tempDate, string sCode, int sMode)
        {
            // カーソル待機中
            this.Cursor = Cursors.WaitCursor;

            // 奉行シリーズデータベース接続
            string sqlSTRING = string.Empty;

            //dbControl.DataControl sdcon = new dbControl.DataControl(dbName);
            //OleDbDataReader dR;

            // 勘定奉行データベース接続文字列を取得する 2017/09/04
            string sc = SqlControl.obcConnectSting.get(dbName);

            // 勘定奉行データベースに接続する 2017/09/04
            SqlControl.DataControl sdcon = new SqlControl.DataControl(sc);

            //データリーダーを取得する
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

            // 所属指定
            if (sCode != string.Empty)
                sqlSTRING += "where (tbDepartment.DepartmentCode = '" + sCode +"')) as d ";
            else sqlSTRING += ") as d ";

            sqlSTRING += "on tbEmployeeBase.EmployeeID = d.EmployeeID) left join ";
            sqlSTRING += "tbDepartment on d.BelongID = tbDepartment.DepartmentID ";

            // 雇用区分指定 正社員、アルバイト、パートを対象とする
            sqlSTRING += "where (tbEmployeeBase.EmploymentDivisionID = 2 or tbEmployeeBase.EmploymentDivisionID = 5 or tbEmployeeBase.EmploymentDivisionID = 6) ";

            //sqlSTRING += "and (tbEmployeeBase.BeOnTheRegisterDivisionID != 9) ";    // 退職者を除外

            sqlSTRING += "order by tbDepartment.DepartmentCode,tbEmployeeBase.EmployeeNo";

            dR = sdcon.free_dsReader(sqlSTRING);

            try
            {
                //グリッドビューに表示する
                int iX = 0;
                tempDGV.RowCount = 0;

                int dMode = 0;

                while (dR.Read())
                {
                    bool rec = false;
                    string kDt = string.Empty;

                    if (dR["zaisekikbn"].ToString().Trim() != "退職")
                    {
                        // 処理済みか調べる
                        OCRData ocr = new OCRData();
                        ocr.dbConnect();
                        OleDbDataReader r = ocr.lastHeaderSelect((int.Parse(txtYear.Text) + Properties.Settings.Default.RekiHosei).ToString(),
                            txtMonth.Text, int.Parse(dR["EmployeeNo"].ToString()).ToString(), _ComDatabeseName);
                       
                        while (r.Read())
                        {
                            kDt = r["更新年月日"].ToString();
                            dMode = MODE_SHORIZUMI;
                            rec = true;
                        }

                        r.Close();

                        // 処理中か調べる
                        r = ocr.HeaderSelect(txtYear.Text, txtMonth.Text, int.Parse(dR["EmployeeNo"].ToString()).ToString());
                        if (r.HasRows)
                        {
                            dMode = MODE_SHORICHU;
                            rec = true;
                        }
                        r.Close();
                        ocr.sCom.Connection.Close();

                        // 未回収
                        if (!rec)
                        {
                            dMode = MODE_MIKAISHU;
                        }

                        // データ表示
                        switch (sMode)
                        {
                            case MODE_MIKAISHU: // 未回収のみ
                                if (dMode == MODE_MIKAISHU)
                                {
                                    //データグリッドにデータを表示する
                                    tempDGV.Rows.Add();
                                    GridViewCellData(tempDGV, iX, dR);
                                    GridRowData(tempDGV, iX, dMode, kDt);
                                    iX++;
                                }
                                break;

                            case MODE_SHORICHU: // 処理中のみ
                                if (dMode == MODE_SHORICHU)
                                {
                                    //データグリッドにデータを表示する
                                    tempDGV.Rows.Add();
                                    GridViewCellData(tempDGV, iX, dR);
                                    GridRowData(tempDGV, iX, dMode, kDt);
                                    iX++;
                                }
                                break;

                            case MODE_SHORIZUMI:    // 処理済みのみ
                                if (dMode == MODE_SHORIZUMI)
                                {
                                    //データグリッドにデータを表示する
                                    tempDGV.Rows.Add();
                                    GridViewCellData(tempDGV, iX, dR);
                                    GridRowData(tempDGV, iX, dMode, kDt);
                                    iX++;
                                }
                                break;

                            case MODE_ALL:  // 全員表示
                                //データグリッドにデータを表示する
                                tempDGV.Rows.Add();
                                GridViewCellData(tempDGV, iX, dR);
                                GridRowData(tempDGV, iX, dMode, kDt);
                                iX++;
                                break;
                        }
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

                // カーソルを戻す
                this.Cursor = Cursors.Default;
            }

            // 該当するデータがないとき
            if (tempDGV.RowCount == 0)
            {
                MessageBox.Show("該当するデータはありませんでした", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);      
            }
        }

        /// <summary>
        /// 回収状況項目をグリッドに表示する
        /// </summary>
        /// <param name="dg">データグリッドオブジェクト</param>
        /// <param name="r">行インデックス</param>
        /// <param name="sMode">回収ステータス</param>
        /// <param name="sDate">処理完了日付</param>
        private void GridRowData(DataGridView dg, int r, int sMode, string sDate)
        {
            switch (sMode)
            {
                case MODE_MIKAISHU:
                    dg[5, r].Value = "×";
                    dg[6, r].Value = "OCR未実施";
                    dg[7, r].Value = sDate;
                    dg.Rows[r].DefaultCellStyle.ForeColor = Color.Red;
                    break;

                case MODE_SHORICHU:
                    dg[5, r].Value = "○";
                    dg[6, r].Value = "処理中";
                    dg[7, r].Value = sDate;
                    dg.Rows[r].DefaultCellStyle.ForeColor = Color.Blue;
                    break;

                case MODE_SHORIZUMI:
                    dg[5, r].Value = "○";
                    dg[6, r].Value = "受け渡しデータ作成済み";
                    dg[7, r].Value = sDate;
                    dg.Rows[r].DefaultCellStyle.ForeColor = Color.Black;
                    break;
            }

        }

        ///--------------------------------------------------------------
        /// <summary>
        ///     データグリッドに表示データをセットする : 
        ///     勘定奉行i10 2017/09/04 </summary>
        /// <param name="tempDGV">
        ///     datagridviewオブジェクト名</param>
        /// <param name="iX">
        ///     Row№</param>
        /// <param name="dR">
        ///     データリーダーオブジェクト名</param>
        ///--------------------------------------------------------------
        private void GridViewCellData(DataGridView tempDGV, int iX, SqlDataReader dR)
        {
            if (Utility.NumericCheck(dR["DepartmentCode"].ToString()))
                tempDGV[0, iX].Value = int.Parse(dR["DepartmentCode"].ToString()).ToString().PadLeft(ShozokuLen, '0');
            else tempDGV[0, iX].Value = (dR["DepartmentCode"].ToString() + "    ").Substring(0, ShozokuLen);
            
            tempDGV[1, iX].Value = dR["DepartmentName"].ToString();
            tempDGV[2, iX].Value = int.Parse(dR["EmployeeNo"].ToString().Trim()).ToString().PadLeft(6, '0');
            tempDGV[3, iX].Value = dR["Name"].ToString().Trim();
            tempDGV[4, iX].Value = dR["CategoryName"].ToString().Trim();
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

            return true;
        }

        private void txtYear_Enter(object sender, EventArgs e)
        {
            TextBox txtObj = new TextBox();
            
            if (sender == txtYear) txtObj = txtYear;
            if (sender == txtMonth) txtObj = txtMonth;

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
            DataSelect(comboBox1.SelectedIndex);
        }

        private void DataSelect(int sMode)
        {
            string sDate = string.Empty;
            string sCode;

            //エラーチェック
            if (ErrCheck() == false) return;

            //基準日付
            sDate = (int.Parse(txtYear.Text) + Properties.Settings.Default.RekiHosei).ToString() + "/" + txtMonth.Text + "/01";

            // 部門コード取得
            if (cmbBumonS.Text == string.Empty)
            {
                sCode = "";
            }
            else
            {
                Utility.ComboBumon cmbs = new Utility.ComboBumon();
                cmbs = (Utility.ComboBumon)cmbBumonS.SelectedItem;
                sCode = cmbs.ID;
            }

            //データ表示
            GridViewShowData(_ComDatabeseName, dg1, sDate, sCode, sMode);
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

    }
}
