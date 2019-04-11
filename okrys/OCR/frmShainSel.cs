using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using okrys.Common;

namespace okrys.OCR
{
    public partial class frmShainSel : Form
    {
        public frmShainSel()
        {
            InitializeComponent();
        }

        private void frmShainSel_Load(object sender, EventArgs e)
        {

            //ウィンドウズ最小サイズ
            Utility.WindowsMinSize(this, this.Size.Width, this.Size.Height);

            //ウィンドウズ最大サイズ
            Utility.WindowsMaxSize(this, this.Size.Width, this.Size.Height);

            //データグリッド初期化
            GridViewSetting(dg1);

            //データ表示
            GridViewShowData(dg1);

            //受け渡しIDの初期化
            _ID = string.Empty;
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

                // 列スタイルを変更する

                tempDGV.EnableHeadersVisualStyles = false;

                // 列ヘッダー表示位置指定
                tempDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;

                // 列ヘッダーフォント指定
                tempDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Meiryo UI", 10, FontStyle.Regular);

                // データフォント指定
                tempDGV.DefaultCellStyle.Font = new Font("Meiryo UI", 10, FontStyle.Regular);

                // 行の高さ
                tempDGV.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                tempDGV.ColumnHeadersHeight = 22;
                tempDGV.RowTemplate.Height = 22;

                // 全体の高さ
                tempDGV.Height = 583;

                // 奇数行の色
                //tempDGV.AlternatingRowsDefaultCellStyle.BackColor = Color.Lavender;

                // 各列幅指定
                tempDGV.Columns.Add("col1", "ID");
                tempDGV.Columns.Add("col2", "所属");
                tempDGV.Columns.Add("col4", "個人番号");
                tempDGV.Columns.Add("col5", "社員名");
                tempDGV.Columns.Add("col6", "申請書区分");

                tempDGV.Columns[0].Width = 180;
                tempDGV.Columns[1].Width = 200;
                tempDGV.Columns[2].Width = 80;
                tempDGV.Columns[3].Width = 200;
                tempDGV.Columns[4].Width = 200;

                //tempDGV.Columns[0].Visible = false;

                tempDGV.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                tempDGV.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                tempDGV.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                tempDGV.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                // 編集可否
                tempDGV.ReadOnly = true;

                // 行ヘッダを表示しない
                tempDGV.RowHeadersVisible = false;

                // 選択モード
                tempDGV.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                tempDGV.MultiSelect = false;

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

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラーメッセージ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// グリッドビューへ社員情報を表示する
        /// </summary>
        /// <param name="sConnect">接続文字列</param>
        /// <param name="tempDGV">DataGridViewオブジェクト名</param>
        private void GridViewShowData(DataGridView g)
        {
            //カレントカーソルを保持
            Cursor preCursor = Cursor.Current;

            //待機カーソル
            Cursor.Current = Cursors.WaitCursor;

            //MDBを読み出し
            OCRData ocr = new OCRData();
            ocr.dbConnect();
            OleDbDataReader mdR = ocr.HeaderSelect();

            try
            {
                g.RowCount = 0;

                while (mdR.Read())
                {
                    g.Rows.Add();
                    g[0, g.RowCount - 1].Value = mdR["ID"].ToString();
                    g[1, g.RowCount - 1].Value = mdR["所属名"].ToString();
                    g[2, g.RowCount - 1].Value = mdR["個人番号"].ToString().PadLeft(6, '0');
                    g[3, g.RowCount - 1].Value = mdR["氏名"].ToString();

                    if (mdR["申請書種別"].ToString() == global.SHAIN_ID)
                    {
                            g[4, g.RowCount - 1].Value = "社員";
                    }
                    else if  (mdR["申請書種別"].ToString() == global.PART_ID)
                    {
                            g[4, g.RowCount - 1].Value = "パート・アルバイト";
                    }
                    else 
                    {
                            g[4, g.RowCount - 1].Value = string.Empty;
                    }
                }
                mdR.Close();
                ocr.sCom.Connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "エラー", MessageBoxButtons.OK);
            }
            finally
            {
                if (!mdR.IsClosed) mdR.Close();
                if (ocr.sCom.Connection.State == ConnectionState.Open) ocr.sCom.Connection.Close();
            }

            g.CurrentCell = null;

            //カーソルを元に戻す
            Cursor.Current = preCursor;
        }

        private void btnRtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmShainSel_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btnSel_Click(object sender, EventArgs e)
        {
            //データ選択確認
            dataSelect();
        }

        private void dataSelect()
        {
            if (dg1.SelectedRows.Count == 0)
            {
                MessageBox.Show("勤務票データが選択されていません", "選択確認", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string sID = dg1[0, dg1.SelectedRows[0].Index].Value.ToString();        //ID
            string sShozoku = dg1[1, dg1.SelectedRows[0].Index].Value.ToString();   //所属
            string sName = dg1[3, dg1.SelectedRows[0].Index].Value.ToString();      //社員名
            string sMsg = string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("以下の勤務票が選択されました。よろしいですか。");
            sb.Append(Environment.NewLine).Append(Environment.NewLine);
            sb.Append("所属：").Append(sShozoku).Append(Environment.NewLine);
            sb.Append("氏名：").Append(sName);

            if (MessageBox.Show(sb.ToString(), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return; ;
            _ID = sID;
            this.Close();
        }

        //IDの取得
        public string _ID { get; set; }

        private void dg1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //データ選択確認
            dataSelect();
        }
    }
}
