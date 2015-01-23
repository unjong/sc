using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Core
{
    public abstract class HeaderParser
    {
        protected HeaderParser ()
	    {

	    }
        public abstract void ParseRow(string[] columnArg, DataRow dr);

        public static HeaderParser CreateParser(string line)
        {
            if (line.Contains(".InitialHeader"))
                return new InitialHeader();
            else if (line.Contains(".InitialDateTimeHeader"))
                return new InitialDateTimeHeader();
            else if (line.Contains(".InitialMaskEditHeader"))
                return new InitialMaskEditHeader();
            else if (line.Contains(".InitialMoneyHeader"))
                return new InitialMoneyHeader();
            else if (line.Contains(".InitialValuedComboHeader"))
                return new InitialValuedComboHeader();
            else
                return new InitialHeader();
        }
        public static void ReplaceColumnInfo(string[] columnInfo)
        {
			for (int i = 0; i < columnInfo.Length; i++)
			{
                columnInfo[i] = columnInfo[i].Trim().Replace("\"", "").Replace(");", "").Replace("CommonModule.CellTypeProperty.", "").Replace("CommonModule.Alignment.", "");
                columnInfo[i] = columnInfo[i].Replace("HIS.WinUI.Controls.Spread.CommonModule.CellTypeProperty.", "")
                                               .Replace("HIS.WinUI.Controls.Spread.CommonModule.Alignment.", "")
                                               .Replace("HIS.WinUI.Controls.Spread.", "");
                if (columnInfo[i].LastIndexOf("//") > 0)
                    columnInfo[i].Remove(columnInfo[i].LastIndexOf("//"));
			}
        }
    }

    public class InitialDateTimeHeader : HeaderParser
    {
        public override void ParseRow(string[] columnInfos, DataRow dr)
        {
            ReplaceColumnInfo(columnInfos);
            if (columnInfos.Length == 6)
            {
                #region length 6
                //public void InitialDateTimeHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, CommonModule.DateTimeFormat datetimeFormat);
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["Width"] = columnInfos[3];
                    //dr["Hidden"] = columnInfos[6];
                    dr["CellType"] = "DateTime";
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[2];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Format"] = columnInfos[5];
                }
                //public void InitialDateTimeHeader(string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[5];
                    dr["Title"] = columnInfos[0];
                    dr["Width"] = columnInfos[2];
                    dr["CellType"] = "DateTime";
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[1];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Format"] = columnInfos[4];
                }
                #endregion
            }
            else if (columnInfos.Length == 7)
            {
                #region length 7
                //public void InitialDateTimeHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat);
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["Width"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[5];
                    dr["CellType"] = "DateTime";
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[2];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Format"] = columnInfos[6];
                }
                //public void InitialDateTimeHeader(string Title, bool Lock, float Width, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[6];
                    dr["Title"] = columnInfos[0];
                    dr["Width"] = columnInfos[2];
                    dr["Hidden"] = columnInfos[4];
                    dr["CellType"] = "DateTime";
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[1];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Format"] = columnInfos[5];
                }
                #endregion
            }
            else if (columnInfos.Length == 10)
            {
                int rt = 0;
                //public void InitialDateTimeHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat);
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["Format"] = columnInfos[9];
                    dr["CellType"] = "DateTime";
                }
            }

            else if (columnInfos.Length == 11)
            {
                int rt = 0;
                //public void InitialDateTimeHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment DateTimeAlignment, bool Hidden, CommonModule.DateTimeFormat datetimeFormat, string DataField);
                if (int.TryParse(columnInfos[0], out rt))
                {
                    dr["Binding"] = columnInfos[10];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["Format"] = columnInfos[9];
                    dr["CellType"] = "DateTime";
                }

            }
        }
    }
    public class InitialHeader : HeaderParser
    {
        public override void ParseRow(string[] columnInfos, DataRow dr)
        {
            ReplaceColumnInfo(columnInfos);
            if (columnInfos.Length == 3)
            {
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    dr["Title"] = columnInfos[1];
                    //dr["DataType"] = columnInfos[2];
                    dr["CellType"] = columnInfos[2];
                }
                else
                {
                    dr["Title"] = columnInfos[0];
                    //dr["DataType"] = columnInfos[1];
                    dr["CellType"] = columnInfos[1];
                    dr["Binding"] = columnInfos[2];

                }
            }
            else if (columnInfos.Length == 4)
            {
                dr["Title"] = columnInfos[0];
                //dr["DataType"] = columnInfos[1];
                dr["CellType"] = columnInfos[1];
                dr["Binding"] = columnInfos[3];
                dr["Width"] = columnInfos[2];
            }
            else if (columnInfos.Length == 7)
            {
                int rt = 0;
                //public void InitialHeader(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden);
                if (int.TryParse(columnInfos[0], out rt) || int.TryParse( columnInfos[4], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                  //  dr["DataType"] = columnInfos[2];
                    dr["Width"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[6];
                    dr["CellType"] = columnInfos[2];
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[3];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[5];
                }

                //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[6];
                    dr["Title"] = columnInfos[0];
                    //dr["DataType"] = columnInfos[1];
                    dr["Width"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[5];
                    dr["CellType"] = columnInfos[1];
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    //dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[2];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[4];
                }
            }

            else if (columnInfos.Length == 13)
            {
                int rt = 0;
                //public void InitialHeader(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden);
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[6];
                    dr["Title"] = columnInfos[1];
                    //dr["DataType"] = columnInfos[2];
                    dr["Width"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[12];
                    dr["CellType"] = columnInfos[2];
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    dr["Length"] = columnInfos[6];
                    dr["ReadOnly"] = columnInfos[4];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[11];
                }
                else
                {
                    //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                    dr["Binding"] = columnInfos[12];
                    dr["Title"] = columnInfos[0];
                    //dr["DataType"] = columnInfos[1];
                    dr["Width"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[11];
                    dr["CellType"] = columnInfos[1];
                    //dr["Unique"] = columnInfos[];
                    //dr["NotNull"] = columnInfos[7];
                    dr["Length"] = columnInfos[5];
                    dr["ReadOnly"] = columnInfos[3];
                    //dr["SaveNo"] = columnInfos[10];
                    //dr["DefaultValue"] = columnInfos[11];
                    dr["HorizontalAlignment"] = columnInfos[10];
                }
            }
        }
    }

    public class InitialMaskEditHeader : HeaderParser
    {
        public override void ParseRow(string[] columnInfos, DataRow dr)
        {
            ReplaceColumnInfo(columnInfos);
            if (columnInfos.Length ==4)
            {
            #region length 4
		      //public void InitialMaskEditHeader(int Seq, string Title, string Mask, char MaskChar);
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["DataType"] = columnInfos[2];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[3];

                }
                 //public void InitialMaskEditHeader(string Title, string Mask, char MaskChar, string DataField);
                else
                { 
                    dr["Binding"] = columnInfos[3];
                    dr["Title"] = columnInfos[0];
                    dr["DataType"] = columnInfos[1];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[2];
                } 
	        #endregion
            }
            else if(columnInfos.Length == 8)
            {
               #region Length 8
		 //public void InitialMaskEditHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[5];
                    dr["DataType"] = columnInfos[6];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[7];
                }
                    //public void InitialMaskEditHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[7];
                    dr["Title"] = columnInfos[0];
                    dr["ReadOnly"] = columnInfos[1];
                    dr["Width"] = columnInfos[2];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[4];
                    dr["DataType"] = columnInfos[5];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[6];
                } 
	#endregion
            }
            else if(columnInfos.Length == 11)
            {
               #region Length 11
		  //public void InitialMaskEditHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["DataType"] = columnInfos[9];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[10];
                }
                //public void InitialMaskEditHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string Mask, char MaskChar, string DataField);
                else
                {
                   dr["Binding"] = columnInfos[10];
                    dr["Title"] = columnInfos[0];
                    dr["BackColor"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[6];
                    dr["Hidden"] = columnInfos[7];
                    dr["DataType"] = columnInfos[8];
                    dr["CellType"] = "MaskEdit";
                    dr["Format"] = columnInfos[9];
                } 
	#endregion
            }
        }
    }

    public class InitialMoneyHeader : HeaderParser
    {
        public override void ParseRow(string[] columnInfos, DataRow dr)
        {
             if (columnInfos.Length ==4)
            {
            #region length 4
		      //public void InitialMoneyHeader(int Seq, string Title, bool ShowSeperator, bool ShowCurrencymark);
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["DataType"] = "ShowSeperator="+columnInfos[2];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[3];

                }
                //public void InitialMoneyHeader(string Title, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                else
                { 
                    dr["Binding"] = columnInfos[3];
                   dr["Title"] = columnInfos[0];
                    dr["DataType"] = "ShowSeperator="+columnInfos[1];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[2];
                } 
	        #endregion
            }
            else if(columnInfos.Length == 8)
            {
               #region Length 8
		        //public void InitialMoneyHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[5];
                    dr["DataType"] = "ShowSeperator="+columnInfos[6];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[7];
                }
                  //public void InitialMoneyHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[7];
                    dr["Title"] = columnInfos[0];
                    dr["ReadOnly"] = columnInfos[1];
                    dr["Width"] = columnInfos[2];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[4];
                      dr["DataType"] = "ShowSeperator="+columnInfos[5];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[6];
                } 
	#endregion
            }
            else if(columnInfos.Length == 11)
            {
               #region Length 11
		        //public void InitialMoneyHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                   dr["DataType"] = "ShowSeperator="+columnInfos[9];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[10];
                }
                  //public void InitialMoneyHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, bool ShowSeperator, bool ShowCurrencymark, string DataField);
                else
                {
                   dr["Binding"] = columnInfos[10];
                    dr["Title"] = columnInfos[0];
                    dr["BackColor"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[6];
                    dr["Hidden"] = columnInfos[7];
                     dr["DataType"] = "ShowSeperator="+columnInfos[8];
                    dr["CellType"] = "MoneyType";
                    dr["Format"] ="ShowCurrencymask="+columnInfos[9];
                } 
	#endregion
            }
        }
    }

    public class InitialValuedComboHeader : HeaderParser
    {
        public override void ParseRow(string[] columnInfos, DataRow dr)
        {
             if (columnInfos.Length ==4)
            {
            #region length 4
		      //public void InitialValuedComboHeader(int Seq, string Title, DataTable dt, bool AddEmptyString);
                int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["DataType"] = "DataTable="+columnInfos[2];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] ="AddEmpthString="+columnInfos[3];

                }
                //public void InitialValuedComboHeader(string Title, DataTable dt, bool AddEmptyString, string DataField);
                else
                { 
                    dr["Binding"] = columnInfos[3];
                    dr["Title"] = columnInfos[0];
                    dr["DataType"] = "DataTable=" + columnInfos[1];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[2];
                } 
	        #endregion
            }
             if (columnInfos.Length == 5)
             {
                 #region length 5
                 //public void InitialValuedComboHeader(int Seq, string Title, string[] valCodes, string[] valNames, bool AddEmptyString);
                 int rt = 0;
                 if (int.TryParse(columnInfos[0], out rt))
                 {
                     //dr["Binding"] = columnInfos[13];
                     dr["Title"] = columnInfos[1];
                     dr["DataType"] = "string[] code=" + columnInfos[2] + " string[] valCodes="+columnInfos[3];
                     dr["CellType"] = "ComboBox";
                     dr["Format"] = "AddEmptyString = " + columnInfos[3];

                 }
                 //public void InitialValuedComboHeader(string Title, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                 else
                 {
                     dr["Binding"] = columnInfos[4];
                     dr["Title"] = columnInfos[0];
                     dr["DataType"] = "string[] code=" + columnInfos[1] + " string[] valCodes=" + columnInfos[2];
                     dr["CellType"] = "ComboBox";
                     dr["Format"] = "AddEmptyString = " + columnInfos[3];
                 }
                 #endregion
             }
             if (columnInfos.Length == 6)
             {
                 #region length 6
                 //public void InitialValuedComboHeader(int Seq, string Title, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol);
                 int rt = 0;
                 if (int.TryParse(columnInfos[0], out rt))
                 {
                     //dr["Binding"] = columnInfos[13];
                     dr["Title"] = columnInfos[1];
                     dr["DataType"] = "DataTable=" + columnInfos[2];
                     dr["CellType"] = "ComboBox";
                     dr["Format"] = "AddEmpthString=" + columnInfos[3] + "; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];

                 }
                 //public void InitialValuedComboHeader(string Title, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol, string DataField);
                 else
                 {
                     dr["Binding"] = columnInfos[5];
                     dr["Title"] = columnInfos[0];
                     dr["DataType"] = "DataTable=" + columnInfos[1];
                     dr["CellType"] = "ComboBox";
                     dr["Format"] = "AddEmpthString=" + columnInfos[2] + "; CodeCol=" + columnInfos[3] +  "NameCol=" + columnInfos[4];
                 }
                 #endregion
             }
            else if(columnInfos.Length == 8)
            {
               #region Length 8
                //public void InitialValuedComboHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[5];
                    dr["DataType"] = "DataTable=" + columnInfos[6];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[7];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                }
                //public void InitialValuedComboHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[7];
                    dr["Title"] = columnInfos[0];
                    dr["ReadOnly"] = columnInfos[1];
                    dr["Width"] = columnInfos[2];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[4];
                    dr["DataType"] = "DataTable=" + columnInfos[5];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[6];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                } 
	#endregion
            }
            else if(columnInfos.Length == 9)
            {
               #region Length 9
                //public void InitialValuedComboHeader(int Seq, string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                    //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[4];
                    dr["Hidden"] = columnInfos[5];
                    dr["DataType"] = "string[] valCodes=" + columnInfos[6] + " string[] valNames="+columnInfos[7];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[8];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                }
                //public void InitialValuedComboHeader(string Title, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[8];
                    dr["Title"] = columnInfos[0];
                    dr["ReadOnly"] = columnInfos[1];
                    dr["Width"] = columnInfos[2];
                    dr["HorizontalAlignment"] = columnInfos[3];
                    dr["Hidden"] = columnInfos[4];
                     dr["DataType"] = "string[] valCodes=" + columnInfos[5] + " string[] valNames="+columnInfos[6];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[7];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                } 
	#endregion
            }
            else if(columnInfos.Length == 11)
            {
               #region Length 11
		        //public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol,
                //CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString);
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                     //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["DataType"] = "DataTable=" + columnInfos[9];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[10];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                }
               //public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[10];
                    dr["Title"] = columnInfos[0];
                    dr["BackColor"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[6];
                    dr["Hidden"] = columnInfos[7];
                    dr["DataType"] = "DataTable=" + columnInfos[8];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[9];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                } 
	            #endregion

            }
            else if(columnInfos.Length == 12)
            {
               #region Length 12
		        //public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString);
        
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                     //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["DataType"] = "string[] valCodes=" + columnInfos[9] + " string[] valNames="+columnInfos[10];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[11];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                }
                //public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string[] valCodes, string[] valNames, bool AddEmptyString, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[11];
                    dr["Title"] = columnInfos[0];
                    dr["BackColor"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[6];
                    dr["Hidden"] = columnInfos[7];
                     dr["DataType"] = "string[] valCodes=" + columnInfos[8] + " string[] valNames="+columnInfos[9];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[10];// +"; CodeCol=" + columnInfos[4] + "; NameCol=" + columnInfos[5];
                }
	            #endregion

            }
             else if(columnInfos.Length == 13)
            {
               #region Length 13
		        //public void InitialValuedComboHeader(int Seq, string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol);
        //public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, int CodeCol, int NameCol, string DataField); 
                  int rt = 0;
                if (int.TryParse(columnInfos[0], out rt))
                {
                     //dr["Binding"] = columnInfos[13];
                    dr["Title"] = columnInfos[1];
                    dr["BackColor"] = columnInfos[2];
                    dr["ReadOnly"] = columnInfos[3];
                    dr["Width"] = columnInfos[4];
                    dr["HorizontalAlignment"] = columnInfos[7];
                    dr["Hidden"] = columnInfos[8];
                    dr["DataType"] = "DataTable=" + columnInfos[9];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[10]+"; CodeCol=" + columnInfos[11] + "; NameCol=" + columnInfos[12];
                }
               //public void InitialValuedComboHeader(string Title, Color BackColor, bool Lock, float Width, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, DataTable dt, bool AddEmptyString, string DataField);
                else
                {
                    dr["Binding"] = columnInfos[12];
                    dr["Title"] = columnInfos[0];
                    dr["BackColor"] = columnInfos[1];
                    dr["ReadOnly"] = columnInfos[2];
                    dr["Width"] = columnInfos[3];
                    dr["HorizontalAlignment"] = columnInfos[6];
                    dr["Hidden"] = columnInfos[7];
                    dr["DataType"] = "DataTable=" + columnInfos[8];
                    dr["CellType"] = "ComboBox";
                    dr["Format"] = "AddEmpthString=" + columnInfos[9] +"; CodeCol=" + columnInfos[10] + "; NameCol=" + columnInfos[11];
                } 
	            #endregion

            }
        }
    }

}
