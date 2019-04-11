using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace okrys
{
    class global
    {
        public static string pblImageFile;

        //表示関係
        public static float miMdlZoomRate = 0;      //現在の表示倍率

        //表示倍率（%）
        public static float ZOOM_RATE = 0.23f;      // 標準倍率
        public static float ZOOM_MAX = 2.00f;       // 最大倍率
        public static float ZOOM_MIN = 0.05f;       // 最小倍率
        public static float ZOOM_STEP = 0.02f;      // ステップ倍率
        public static float ZOOM_NOW;               // 現在の倍率

        public static int RECTD_NOW;                // 現在の座標
        public static int RECTS_NOW;                // 現在の座標
        public static int RECT_STEP = 20;           // ステップ座標

        //和暦西暦変換
        public const int rekiCnv = 1988;    //西暦、和暦変換

        //エラーチェック関連
        public static string errID;         //エラーデータID
        public static int errNumber;        //エラー項目番号
        public static int errRow;           //エラー行
        public static string errMsg;        //エラーメッセージ

        //エラー項目番号
        public const int eNothing = 0;      // エラーなし
        public const int eYearMonth = 1;    // 対象年月
        public const int eMonth = 2;        // 対象月
        public const int eShainNo = 3;      // 個人番号
        public const int eShozoku = 4;      // 所属コード
        public const int eDay = 6;          // 日
        public const int eKintaiKigou = 7;  // 勤怠記号
        public const int eYukyu = 8;        // 有給休暇
        public const int eSH = 9;           // 開始時
        public const int eSM = 10;          // 開始分
        public const int eEH = 11;          // 終了時
        public const int eEM = 12;          // 終了分
        public const int eKKH = 13;         // 休憩・時間
        public const int eKKM = 14;         // 休憩・分
        public const int eTH = 15;          // 実働時
        public const int eTM = 16;          // 実働分
        public const int eNoCheck = 17;     // 未チェック出勤簿
        public const int eKoutsuhi = 18;    // 交通費
        public const int eKoutsuhiTL = 19;  // 交通費合計

        //汎用データファイル名
        public static string OKFILE = "勤怠データ";
        public static string KOTSUHIFILE = "交通費データ";

        // 就業奉行汎用データヘッダ項目
        public const string H1 = @"""EBAS001""";    // 社員番号
        public const string H2 = @"""LTLT001""";    // 日付
        public const string H3 = @"""LTLT003""";    // 勤務体系コード（使用 2013/11/11）: "001"
        public const string H4 = @"""LTLT004""";    // 事由コード
        public const string H5 = @"""LTDT001""";    // 出勤時刻
        public const string H6 = @"""LTDT002""";    // 退出時刻
        public const string H7 = @"""LTDT003""";    // 外出時刻（未使用）
        public const string H8 = @"""LTDT004""";    // 戻入時刻（未使用）
        public const string H9 = @"""LTTC001""";    // 勤怠時間項目コード１：出勤時間
        public const string H10 = @"""LTTC002""";   // 勤怠時間項目コード２：休憩時間
        public const string H14 = @"""LTTC003""";   // 勤怠時間項目コード３：休日勤務時間
        public const string H11 = @"""LTTS001""";   // 時間１：出勤時間
        public const string H12 = @"""LTTS002""";   // 時間２：休憩時間
        public const string H15 = @"""LTTS003""";   // 時間３：休日勤務時間

        // 給与奉行汎用データヘッダ項目
        public const string H13 = @"""SPPM280""";   // 通勤手当

        //ローカルMDB関連
        public const string MDBFILE = "okrys.mdb";         //MDBファイル名
        public const string MDBTEMP = "okrys_Temp.mdb";    //最適化一時ファイル名
        public const string MDBBACK = "okrys_Back.mdb";    //最適化後バックアップファイル名

        public static int flgOn = 1;            //フラグ有り(1)
        public static int flgOff = 0;           //フラグなし(0)
        public static string FLGON = "1";
        public static string FLGOFF = "0";
        public static int pblDenNum;            // データ数

        public static int configKEY = 1;        // 環境設定データキー

        //ＯＣＲ処理ＣＳＶデータの検証要素
        public static int CSVLENGTH = 197;          //データフィールド数 2011/06/11
        public static int CSVFILENAMELENGTH = 21;   //ファイル名の文字数 2011/06/11  
 
        // 勤務記録表
        public static int STARTTIME = 8;            // 単位記入開始時間帯
        public static int ENDTIME = 22;             // 単位記入終了時間帯
        public static int TANNIMAX = 4;             // 単位最大値
        public static int WEEKLIMIT = 160;          // 週労働時間基準単位：40時間
        public static int DAYLIMIT = 32;            // 一日あたり労働時間基準単位：8時間

        // 環境設定項目
        public static int cnfYear;                  // 対象年
        public static int cnfMonth;                 // 対象月
        public static string cnfPath;               // 受け渡しデータ作成パス
        public static int cnfArchived;              // データ保管期間（月数）

        public static int ShozokuLength = 0;        // 所属コード桁数
        public static int ShainLength = 0;          // 社員コード桁数
        public static int ShozokuMaxLength = 4;     // 所属コードＭＡＸ桁数
        public static int ShainMaxLength = 5;       // 社員コードＭＡＸ桁数

        // 勤怠記号
        public static string K_KYUJITSUSHUKIN = "1";        // 休日出勤（デイリー）
        public static string K_TOKUBETSU_KYUKA = "2";       // 特別休暇
        public static string K_YUKYU_KYUKA = "3";           // 有休休暇
        public static string K_SANKYU = "4";                // 産休
        public static string K_IKUKYU = "5";                // 育休
        public static string K_KEKKIN = "6";                // 欠勤
        public static string K_CHISOU = "7";                // 遅刻・早退
        public static string K_FURI_SHUKKIN = "8";          // 振替出勤
        public static string K_SHUCCHOU = "9";              // 出張
        public static string K_FURI_KYUJITSU = "10";        // 振替休日

        // 有給記号
        public static string ZENNICHI_YUKYU = "0";      // 全日有給
        public static string H1_YUKYU = "1";            // 1H有給
        public static string H2_YUKYU = "2";            // 2H有給
        public static string H3_YUKYU = "3";            // 3H有給
        public static string H4_YUKYU = "4";            // 4H有給
        public static string H5_YUKYU = "5";            // 5H有給
        public static string H6_YUKYU = "6";            // 6H有給
        public static string H7_YUKYU = "7";            // 7H有給

        // 深夜時間帯
        public static DateTime dt2200 = DateTime.Parse("22:00");
        public static DateTime dt0500 = DateTime.Parse("05:00");
        public static DateTime dt0800 = DateTime.Parse("08:00");

        // ChangeValueStatus
        public static bool ChangeValueStatus = true;

        public static int MAX_GYO = 31;
        public static int MAX_MIN = 1;

        // 雇用区分
        public static string CATEGORY_SHAIN = "正社員";
        public static string CATEGORY_PART = "パート";
        public static string CATEGORY_ARBEIT = "アルバイト";

        // ＯＣＲモード
        public static string OCR_SCAN = "1";
        public static string OCR_IMAGE = "2";

        // 勤怠申請書種別ID
        public static string SHAIN_ID = "1";
        public static string PART_ID = "3";

        //datagridview表示行数
        public static int _MULTIGYO = 31;
    }
}
