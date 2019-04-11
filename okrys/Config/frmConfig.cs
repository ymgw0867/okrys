﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using okrys.Common;
using System.Data.OleDb;

namespace okrys.Config
{
    public partial class frmConfig : Form
    {
        public frmConfig()
        {
            InitializeComponent();
            label5.Text = Properties.Settings.Default.gengou;

            ms.dbConnect();
            dR = ms.Select(global.configKEY);

            while (dR.Read())
            {
                txtYear.Text = dR["年"].ToString();
                txtMonth.Text = dR["月"].ToString();
                lblPath.Text = dR["受け渡しデータ作成パス"].ToString();
                txtArchive.Text = dR["データ保存月数"].ToString();
            }

            dR.Close();
        }

        msConfig ms = new msConfig();
        OleDbDataReader dR = null;

        private void frmConfig_Load(object sender, EventArgs e)
        {
            Utility.WindowsMaxSize(this, this.Width, this.Height);
            Utility.WindowsMinSize(this, this.Width, this.Height);
        }


        /// <summary>
        /// フォルダダイアログ選択
        /// </summary>
        /// <returns>フォルダー名</returns>
        private string userFolderSelect()
        {
            string fName = string.Empty;

            //出力フォルダの選択ダイアログの表示
            // FolderBrowserDialog の新しいインスタンスを生成する (デザイナから追加している場合は必要ない)
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

            // ダイアログの説明を設定する
            folderBrowserDialog1.Description = "フォルダを選択してください";

            // ルートになる特殊フォルダを設定する (初期値 SpecialFolder.Desktop)
            folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.Desktop;

            // 初期選択するパスを設定する
            folderBrowserDialog1.SelectedPath = @"C:\";

            // [新しいフォルダ] ボタンを表示する (初期値 true)
            folderBrowserDialog1.ShowNewFolderButton = true;

            // ダイアログを表示し、戻り値が [OK] の場合は、選択したディレクトリを表示する
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                fName = folderBrowserDialog1.SelectedPath + @"\";
            }
            else
            {
                // 不要になった時点で破棄する
                folderBrowserDialog1.Dispose();
                return fName;
            }

            // 不要になった時点で破棄する
            folderBrowserDialog1.Dispose();

            return fName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //フォルダーを選択する
            lblPath.Text = userFolderSelect();
        }

        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // データ更新
            DataUpdate();
        }

        private void DataUpdate()
        {
            if (MessageBox.Show("データを更新してよろしいですか","確認",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;

            // エラーチェック
            if (!errCheck()) return;

            // データ更新
            ms.UpDate(txtYear.Text, txtMonth.Text, lblPath.Text, txtArchive.Text);

            global.cnfYear = int.Parse(txtYear.Text);
            global.cnfMonth = int.Parse(txtMonth.Text);
            global.cnfPath = lblPath.Text;
 
            // 終了
            this.Close();
        }

        private bool errCheck()
        {
            // 処理年
            if (!Utility.NumericCheck(txtYear.Text))
            {
                MessageBox.Show("処理年が正しくありません","エラー",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                txtYear.Focus();
                return false;
            }

            // 処理月
            if (!Utility.NumericCheck(txtMonth.Text))
            {
                MessageBox.Show("処理月が正しくありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtMonth.Focus();
                return false;
            }

            // パス
            if (lblPath.Text.Trim() == string.Empty)
            {
                MessageBox.Show("受け渡しデータ作成パスが指定されていません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                lblPath.Focus();
                return false;
            }

            // データ保存期間
            if (!Utility.NumericCheck(txtArchive.Text))
            {
                MessageBox.Show("履歴データ保存期間が正しくありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtMonth.Focus();
                return false;
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            // データベース接続を解除
            if (ms.sCom.Connection.State == ConnectionState.Open)
                ms.sCom.Connection.Close();

            // 後片付け
            this.Dispose();
        }

    }
}
