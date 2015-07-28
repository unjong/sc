using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace CsFormAnalyzer.Core
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GridColumnInfoParser
    {
        string filePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the GridColumnInfoParser class.
        /// </summary>
        public GridColumnInfoParser(string strfilePath)
        {
            filePath = strfilePath;
        }

        public List<DataTable> Parse()
        {
            List<DataTable> dtList =ColumnParse2();
             return dtList;
        }

        List<DataTable> ColumnParse1()
        {
            List<DataTable> dtList = new List<DataTable>();
            using (System.IO.StreamReader file = new System.IO.StreamReader(filePath, Encoding.Default, true))
            {
                string line = string.Empty;
                while ((line = file.ReadLine()) != null)
                {
                    
                }
            }

            return dtList;
        }

        /// <summary>
        /// HIS.WinUI.Controls.Spread.CommonModule.Header[] hd = new HIS.WinUI.Controls.Spread.CommonModule.Header[12];
        /// </summary>
        List<DataTable> ColumnParse2()
        {
            /* example
            header 클래스 를 사용 하여 설정 하는 경우
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, string DataField);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, int width, string DataField);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden);
            public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
            */
             List<DataTable> dtList = new List<DataTable>();
             using (System.IO.StreamReader file = new System.IO.StreamReader(filePath, Encoding.Default, true))
             {
                 string line = string.Empty;

                 while ((line = file.ReadLine()) != null)
                 {
                     #region CommonModule.Header[] Type
                     bool argStarted = false;
                     if (line.Trim().Contains("CommonModule.Header[]"))
                     {
                       
                         DataTable dt = GetDataTable();
                         while ((line = file.ReadLine()) != null)
                         {

                             line = line.Trim();
                             if (line.StartsWith("//") || line.Length == 0 ) continue;
                             /* 마감 */
                             if (!line.StartsWith("//") && !line.Contains("[") && !argStarted && dt.Rows.Count>0 )
                             {
                                 argStarted = false;
                                 dtList.Add(dt);
                                 break;
                             }

                             if(line.Contains("["))
                                 argStarted = true;
                             //hd[0] = new HIS.WinUI.Controls.Spread.CommonModule.Header
                             //(0, "보험유형", HIS.WinUI.Controls.Spread.CommonModule.CellTypeProperty.ValuedComboBoxCell,
                             //Color.White, true, 80, 2, 0, 0, HIS.WinUI.Controls.Spread.CommonModule.SortOrder.None,
                             //-1, HIS.WinUI.Controls.Spread.CommonModule.Alignment.Center, false);
                             if (line.StartsWith("//")) continue;
                             if (line.Contains("("))
                             {
                                 string columnArgs = line.Substring(line.IndexOf("(") + 1);
                                 if (!line.Contains(");"))  // 내려쓰기로 파라메터 정보를 쓴경우
                                 {
                                     while ((line = file.ReadLine()) != null)
                                     {
                                         if (line.Trim().StartsWith("//")) continue;
                                         columnArgs += line.Trim();
                                         if (line.Contains(");"))
                                         {
                                             argStarted = false;
                                             break;
                                         }
                                     }
                                 }
                                 else
                                 {
                                     argStarted = false;
                                 }

                                 string[] columnInfo = columnArgs.Split(',');
                                 DataRow dr = dt.Rows.Add();
                                 GetColumnInfosType1(columnInfo, dr);
                               
                             }
                             else
                             {

                             }
                         }
                     }
                     #endregion
                 }
             }
             using (System.IO.StreamReader file = new System.IO.StreamReader(filePath, Encoding.Default, true))
             {
                 string line = string.Empty;

                 bool headerStart = false;
                 string prefix = string.Empty;
                 DataTable dt2 = null;
                 while ((line = file.ReadLine()) != null)
                 {
                     #region CommonModule .InitialHeader ()   type
                     line = line.Trim();
                     #region Sample data
                     //public void InitialDateTimeHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, CommonModule.DateTimeFormat datetimeFormat);
                     //public void InitialDateTimeHeader(string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                     //public void InitialDateTimeHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat);
                     //public void InitialDateTimeHeader(string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                     //public void InitialDateTimeHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat);
                     //public void InitialDateTimeHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                     //       public void InitialMaskEditHeader(int Seq, string Title, string Mask, char MaskChar);
                     //       public void InitialMaskEditHeader(string Title, string Mask, char MaskChar, string DataField);
                     //       public void InitialMaskEditHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar);
                     //       public void InitialMaskEditHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar, string DataField);
                     //       public void InitialMaskEditHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar);
                     //       public void InitialMaskEditHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar, string DataField);
                     //       public void InitialMoneyHeader(int Seq, string Title, bool ShowSeperator, bool ShowCurrencymark);
                     //       public void InitialMoneyHeader(string Title, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                     //       public void InitialMoneyHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark);
                     //       public void InitialMoneyHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                     //       public void InitialMoneyHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark);
                     //       public void InitialMoneyHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, DataTable dt, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, DataTable dt, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, string[] valCodes, string[] valNames, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol);
                     //       public void InitialValuedComboHeader(string Title, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString);
                     //       public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                     //       public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol);
                     //       public void InitialValuedComboHeader(st 
                     #endregion
                     if (line.StartsWith("//") || line.Length == 0) continue;
                     if(line.StartsWith("/*"))
                     {
                         if (!line.EndsWith("*/"))
                         {

                             while ((line = file.ReadLine()) != null)
                             {
                                 if (line.EndsWith("*/"))
                                     break;
                             }
                         }
                         continue;
                     }

                     if (line.Contains(".InitialHeader") || line.Contains(".InitialDateTimeHeader") || line.Contains(".InitialMaskEditHeader") ||
                         line.Contains(".InitialMoneyHeader") || line.Contains(".InitialValuedComboHeader") || line.Contains(".InitialDateTimeHeader") )
                     {

                         HeaderParser parser = HeaderParser.CreateParser(line);

                         // 아래와 같이 spread 가 달라지는 경우는 다시 시작 해야함.
                         // this._spread_ORD.InitialHeader("내원번호", CommonModule.CellTypeProperty.TextCell, true, 100, CommonModule.Alignment.Left, false, "내원번호");
                         //  this._spread_OPD.InitialHeader("확인구분명", CommonModule.CellTypeProperty.ComboBoxCell, false, 100, CommonModule.Alignment.Left, false, "확인구분명");
                         string pprefix = line.Substring( 0, line.LastIndexOf("Initial")) ;
                         bool samePrefix = prefix.Equals(pprefix);

                         if (!headerStart || !samePrefix )
                         {
                             if (!samePrefix && dt2 != null && dt2.Rows.Count > 0 && prefix.Length>0)
                             {
                                 dtList.Add(dt2);
                             }
                             prefix = pprefix;
                             dt2 = GetDataTable();
                         }
                         headerStart = true;
                         
                         // "(" 가 아래서 시작 되는 경우
                         if (line.IndexOf("(") < 0)
                             line = file.ReadLine();
                         string columnArgs = line.Substring(line.IndexOf("(") + 1);
                         if (!line.Contains(");"))  // 내려쓰기로 파라메터 정보를 쓴경우
                         {
                             while ((line = file.ReadLine()) != null)
                             {
                                 if (line.Trim().StartsWith("//")) continue;
                                 columnArgs += line.Trim();
                                 if (line.Contains(");"))
                                 {
                                    
                                     break;
                                 }
                             }
                         }
                         else
                         {
                             
                         }
                         if (columnArgs.Contains("//"))
                             columnArgs = columnArgs.Remove(columnArgs.LastIndexOf("//"));
                         string[] columnInfos = columnArgs.Split(',');
                         if (columnInfos.Length >= 7)
                         {
                             HeaderParser.ReplaceColumnInfo(columnInfos);
                             DataRow dr = dt2.Rows.Add();
                             parser.ParseRow(columnInfos, dr);
                         }
                     }
                     else
                     {
                         if (line.Contains(".ColumnSpan"))
                         {

                         }
                         else
                         {
                             if (headerStart && dt2 != null && dt2.Rows.Count > 0)
                             {
                                 dtList.Add(dt2);
                                 dt2 = null;
                             }
                             headerStart = false;
                         }
                     }
                     #endregion
                 }
             }
             using (System.IO.StreamReader file = new System.IO.StreamReader(filePath, Encoding.Default, true))
             {
                 string line = string.Empty;

                 while ((line = file.ReadLine()) != null)
                 {
                     #region {"","",""}  type

                      line = line.Trim();
                     if (line.StartsWith("string[,]") || line.StartsWith("private string[,]"))
                     {
                         DataTable dt = GetDataTable();
                         while ((line = file.ReadLine()) != null)
                         {
                             line = line.Trim();
                            
                             if (line.Contains("};"))
                             {
                                 // 그리드 정보가 끝났으니 Grid 컨트롤을 추가 
                                 if(dt.Rows.Count>0)
                                    dtList.Add(dt);

                                 break;
                             }
                             if (line.StartsWith("//")) continue;
                             if (Regex.IsMatch(line.Replace(" ",""), "{\"", RegexOptions.RightToLeft))
                             {
                                
                                 string[] columnInfo = line.Remove(line.LastIndexOf("}")).Split(',');
                                 HeaderParser.ReplaceColumnInfo(columnInfo);
                                 //for (int i = 0; i < columnInfo.Length; i++)
                                 //{
                                 //    columnInfo[i] = columnInfo[i].TrimStart('{').Replace("\t", "").Replace("\"", "").Trim();
                                 //}
                                 if (columnInfo.Length > 11 )
                                 {
                                     DataRow dr = dt.Rows.Add();
                                     dr["Binding"] = columnInfo[0];
                                     dr["Title"] = columnInfo[1];
                                     dr["DataType"] = columnInfo[2];
                                     dr["Width"] = columnInfo[3];
                                     dr["Hidden"] = columnInfo[4];
                                     dr["CellType"] = columnInfo[5];
                                     //dr["Unique"] = columnInfo[6];
                                     dr["NotNull"] = columnInfo[7];
                                     dr["Length"] = columnInfo[8];
                                     dr["ReadOnly"] = columnInfo[9];
                                     dr["DefaultValue"] = columnInfo[11];
                                 }
                                 else if(columnInfo.Length == 6)
                                 {
                                     // 0.ColumnName_1.Label_2.DataType_3.Width_4.Hidden_5.CellType
			                         //   "ExamNm",		"검사명",			"String",	"80",		"",		"Text"},
                                     DataRow dr = dt.Rows.Add();
                                     dr["Binding"] = columnInfo[0];
                                     dr["Title"] = columnInfo[1];
                                     dr["DataType"] = columnInfo[2];
                                     dr["Width"] = columnInfo[3];
                                     dr["Hidden"] = columnInfo[4];
                                     dr["CellType"] = columnInfo[5];
                                 }
                                 //dr["HorizontalAlignment"] = columnInfo[12];
                             }
                         }
                     }
                     #endregion
                 }
             }
              
            


            return dtList;
        }


        private static void GetColumnInfosType1(string[] columnInfo, DataRow dr)
        {
            HeaderParser.ReplaceColumnInfo(columnInfo);


            if (columnInfo.Length == 8) // 8개짜리 Header 정보
            {

                dr["Binding"] = columnInfo[7];
                dr["Title"] = columnInfo[1];
                dr["DataType"] = columnInfo[2];
                dr["Width"] = columnInfo[4];
                dr["Hidden"] = columnInfo[6];
                dr["CellType"] = columnInfo[2];
                dr["HorizontalAlignment"] = columnInfo[5];
            }
            else if (columnInfo.Length == 3)
            {
                //dr["Binding"] = columnInfo[13];
                dr["Title"] = columnInfo[1];
                dr["CellType"] = columnInfo[2];

            }
            else if (columnInfo.Length == 4)
            {
                //dr["Binding"] = columnInfo[13];
                dr["Binding"] = columnInfo[3];
                dr["Title"] = columnInfo[1];
                dr["CellType"] = columnInfo[2];
            }
            else if (columnInfo.Length == 5)
            {
                //dr["Binding"] = columnInfo[13];
                dr["Binding"] = columnInfo[4];
                dr["Title"] = columnInfo[1];
                dr["CellType"] = columnInfo[2];
                dr["Width"] = columnInfo[3];
            }
            else if (columnInfo.Length == 7)
            {
                //dr["Binding"] = columnInfo[13];
                //dr["Binding"] = columnInfo[4];
                dr["Title"] = columnInfo[1];
                dr["CellType"] = columnInfo[2];
                dr["Width"] = columnInfo[4];
                dr["Hidden"] = columnInfo[6];
                //dr["Unique"] = columnInfo[];
                //dr["NotNull"] = columnInfo[7];
                dr["ReadOnly"] = columnInfo[3];
                dr["HorizontalAlignment"] = columnInfo[5];
            }
            else if (columnInfo.Length == 13)
            {
                int num = 0;


                //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width,
                // int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden);
               
                if (int.TryParse(columnInfo[0], out num))
                {
                    //dr["Binding"] = columnInfos[6];
                    dr["Title"] = columnInfo[1];
                    dr["DataType"] = columnInfo[2];
                    dr["CellType"] = columnInfo[2];
                    dr["BackColor"] = columnInfo[3];
                    dr["ReadOnly"] = columnInfo[4];
                    dr["Width"] = columnInfo[5];
                    dr["Length"] = columnInfo[6];
                    dr["HorizontalAlignment"] = columnInfo[11];
                    dr["Hidden"] = columnInfo[12];                 
                }
                else
                {
                    dr["Binding"] = columnInfo[12];
                    dr["Title"] = columnInfo[1];
                    dr["DataType"] = columnInfo[3];
                    dr["Width"] = columnInfo[5];
                    dr["Hidden"] = columnInfo[12];
                    dr["CellType"] = columnInfo[3];
                    //dr["Unique"] = columnInfo[];
                    //dr["NotNull"] = columnInfo[7];
                    dr["Length"] = columnInfo[6];
                    dr["ReadOnly"] = columnInfo[5];
                }
                //dr["SaveNo"] = columnInfo[10];
                //dr["DefaultValue"] = columnInfo[11];
                //dr["HorizontalAlignment"] = columnInfo[5];
            }
        }



        private static void GettColumnInfosType2(string[] columnInfos, DataRow dr, string headerType)
        {
            #region Inital header type

       
	#endregion
        }

        //public DataTable GetColumInfoDataTable()
        //{
        //    DataTable dt = new DataTable();
        //    // 0.ColumnName, 1.Label, 2.DataType, 3.Width, 4.Hidden, 5.CellType, 6.Unique, 7.NotNull, 8.Length, 9.ReadOnly, 10.SaveNo, 11.DefaultValue
        //    dt.Columns.Add("Seq");
        //    dt.Columns.Add("Title");
        //    dt.Columns.Add("CellTypeName");
        //    dt.Columns.Add("BackColor");
        //    dt.Columns.Add("Lock");
        //    dt.Columns.Add("Width");
        //    dt.Columns.Add("DataLength");
        //    dt.Columns.Add("Precision");
        //    dt.Columns.Add("NumericScale");
        //    dt.Columns.Add("Asc");
        //    dt.Columns.Add("KeyCol");
        //    dt.Columns.Add("TextAlignment");
        //    dt.Columns.Add("Hidden");
        //    dt.Columns.Add("DataField");
        //    return dt;
        //}

        public DataTable GetDataTable()
        {
            DataTable dt = new DataTable();

            // 0.ColumnName, 1.Label, 2.DataType, 3.Width, 4.Hidden, 5.CellType, 6.Unique, 7.NotNull, 8.Length, 9.ReadOnly, 10.SaveNo, 11.DefaultValue,12 HorizontalAlignment
            dt.Columns.Add("Binding");
            dt.Columns.Add("Title");
            dt.Columns.Add("DefaultValue");
            dt.Columns.Add("CellType");
            dt.Columns.Add("DataType");
            dt.Columns.Add("Hidden");
            dt.Columns.Add("NotNull");
            dt.Columns.Add("ReadOnly");
            //dt.Columns.Add("Unique");
            dt.Columns.Add("Length");
            dt.Columns.Add("Width");
            dt.Columns.Add("HorizontalAlignment");
            dt.Columns.Add("BackColor");
            dt.Columns.Add("Format");
            return dt;
        }

    }
}