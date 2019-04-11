using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace okrys
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // フォルダ作成
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.DataPath);
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.Image_WinoutPath);
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.Scan_WinoutPath);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form frm = new prePrint();
            frm.ShowDialog();
            this.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 環境設定情報取得
            Common.msConfig conf = new Common.msConfig();
            conf.dbConnect();
            conf.GetCommonYearMonth();
            conf.sCom.Connection.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form frm = new OCR.frmOCR();
            frm.ShowDialog();
            this.Show();
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form frm = new Config.frmConfig();
            frm.ShowDialog();
            this.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 環境設定年月の確認
            string msg = "処理対象年月は " + Properties.Settings.Default.gengou + global.cnfYear.ToString() + "年 " + global.cnfMonth.ToString() + "月です。よろしいですか？";
            if (MessageBox.Show(msg, "勤務データ登録", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;

            this.Hide();
            frmComSelect frm = new frmComSelect();
            frm.ShowDialog();

            if (frm._pblDbName != string.Empty)
            {
                // 選択領域のデータベース名を取得します
                string _ComName = frm._pblComName;
                string _ComDBName = frm._pblDbName;
                frm.Dispose();

                // 出勤簿データ作成画面
                OCR.frmCorrect frmg = new OCR.frmCorrect(_ComDBName, _ComName, string.Empty);
                frmg.ShowDialog();
            }
            else frm.Dispose();

            this.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmComSelect frm = new frmComSelect();
            frm.ShowDialog();

            if (frm._pblDbName != string.Empty)
            {
                // 選択領域のデータベース名を取得します
                string _ComName = frm._pblComName;
                string _ComDBName = frm._pblDbName;
                frm.Dispose();

                // 出勤簿ビューワ画面
                OCR.frmUnSubmit frmg = new OCR.frmUnSubmit(_ComDBName, _ComName); 
                frmg.ShowDialog();
            }
            else frm.Dispose();

            this.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmRecovery frm = new frmRecovery();
            frm.ShowDialog();
            this.Show();
        }
    }
}
