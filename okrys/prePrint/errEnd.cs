using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace en_PrePrint
{
    class errEnd
    {
        //途中終了時の処理
        public static void Exit()
        {
            MessageBox.Show ("処理は中断されました。",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Information);
            Environment.Exit(0);
        }
    }
}
