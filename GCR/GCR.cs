using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace GLKB
{
    // Stratigraphic classification
    enum Priority
    {
        Group = 0,
        Formation = 1,
        Member = 2
    };

    // GIS Software Codes
    // 2025 Feb. 25
    enum GISSoftware
    {
        MapGIS = 0,
        ESRI = 1,
        QGIS = 2
    };

    class GCR
    {
        public string strGeoCode;              // for geological label  (Apr 19,2025)     // Geological unit code (July 23, 2016)  
        public string strEigenValue;           // for Value of VSM      (Apr 19,2025)     // Characteristic value of geological unit code (Aug 2, 2016) 
        string strRecPresentCode;              // for Typical Style     (Apr 19,2025)     // Intelligently recognized geological unit code (Aug 2, 2016)

        string[] strGeoWords;                  // Common geological unit codes (Aug 8, 2016)
        List<string> lstGeoWords = null;       // Common geological unit codes (Aug 8, 2016)
        public string strGroupValue;           // for value of Element  (Apr 19,2025)      // Records characteristic values of common geological unit codes (Aug 8, 2016)

        System.Data.DataTable tGCRKnowledge;   // Expert knowledge base (Aug 2, 2016)
        GISSoftware gisName = GISSoftware.ESRI; // ArcGIS format (Feb 25, 2025)

        // Character flags: 
        // Lowercase English: 0; Uppercase English: 1;
        // Digits: 2; Greek: 3;
        // '+': 4; '-': 5;
        // '.': 6; '∈': 7;
        // '^': 8
        string flag = "";                      // Flags geological unit code categories

        // ESRI-specific Python keywords and functions
        // This script includes commonly used terms and operations 
        // in ESRI's ArcPy library for geoprocessing and GIS analysis.
        // By wigsont, Feb. 25, 2025
        string[] esriFormattingTags = 
        {
            "<SUB>",    // Subscript start tag
            "</SUB>",   // Subscript end tag
            "<SUP>",    // Superscript start tag
            "</SUP>",   // Superscript end tag
            "<ITA>",    // Italic start tag
            "</ITA>"    // Italic end tag
        };

        public GCR()
        {
            strGeoCode = "";
            strEigenValue = "";
            ReadGCRKnowledge();
            InitialGeoWords();
            strRecPresentCode = "";
            strGroupValue = "";
        }

        public GCR(string str)
        {
            strGeoCode = str;
            strEigenValue = "";
            ReadGCRKnowledge();
            InitialGeoWords();
            strRecPresentCode = "";
            strGroupValue = "";
        }

        public GCR(string str, DBUniAccessor.IDBAccess acs)
        {
            strGeoCode = str;
            strEigenValue = "";
            ReadGCRKnowledge(acs);
            InitialGeoWords();
            strRecPresentCode = "";
            strGroupValue = "";
        }

        // Load geological label knowledge base (Aug 2, 2016)
        // Default: GCRK.accdb in the current dynamic library directory
        private void ReadGCRKnowledge()
        {
            DBUniAccessor.IDBAccess acs = null; // Database access interface (Sep 3, 2009)
            string strSQL = "";

            try
            {
                acs = DBUniAccessor.DBAccessFactory.Create(DBUniAccessor.DBType.Access);
                acs.Init(".", "\\GLKB.accdb", "", "");
                acs.Open();

                if (acs != null)
                {
                    /* update the fieldnames to fit paper */
                    //strSQL= "select ID,SourceGeoCode,GroupValue,     EigenValue,   PresentGeoCode,WordType,备注 From GCRK";

                    strSQL = "SELECT ID,[Label Text],[Value of Element],[Value of VSM],[Typical Style],[WordType],[备注] from [Characteristic Values]";
                    tGCRKnowledge = acs.RunQuery(strSQL);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load knowledge base. Reason: " + ex.Message);
            }
            finally
            {
                acs.Close();
            }
        }

        // Load geological label knowledge base (Aug 2, 2016)
        // Default: GCRK.accdb in the current dynamic library directory
        private DataTable ReadGCRKnowledge(DBUniAccessor.IDBAccess acs)
        {
            string strSQL = "";

            try
            {
                if (acs != null)
                {
                    strSQL = "SELECT ID,[Label Text],[Value of Element],[Value of VSM],[Typical Style],[WordType],[备注] from [Characteristic Values]";
                    tGCRKnowledge = acs.RunQuery(strSQL);
                }
                return tGCRKnowledge;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load knowledge base. Reason: " + ex.Message);
            }
        }

        // Save geological label knowledge base (Aug 2, 2016, modified Sep 1, 2016)
        // Default: GCRK.accdb in the current dynamic library directory
        public void WriteGCRKnowledge(string sSourceGeoCode, string sDestGeoCode)
        {
            DBUniAccessor.IDBAccess acs = null; // Database access interface (Sep 3, 2009)
            string strSQL = "";

            try
            {
                acs = DBUniAccessor.DBAccessFactory.Create(DBUniAccessor.DBType.Access);
                acs.Init(".", "\\GCRK.mdb", "", "");
                acs.Open();

                //strSQL = "Insert into GCRK(SourceGeoCode,GroupValue,EigenValue,PresentGeoCode,WordType) values ('" + sSourceGeoCode
                //            + "','" + this.GenerateGroupValue(sSourceGeoCode)
                //            + "','" + this.GenerateEigenValue(sSourceGeoCode)
                //            + "','" + sDestGeoCode
                //            + "'," + "-9" + ")";

                strSQL = "Insert into [Characteristic Values]([Label Text],[Value of Element],[Value of VSM],[Typical Style],[WordType]) values ('" + sSourceGeoCode
                            + "','" + this.GenerateGroupValue(sSourceGeoCode)
                            + "','" + this.GenerateEigenValue(sSourceGeoCode)
                            + "','" + sDestGeoCode
                            + "'," + "-9" + ")";


                acs.RunSQL(strSQL);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Warning!", ex.Message);
            }
            finally
            {
                acs.Close();
            }
        }

        private string GetFlag()
        {
            return flag;
        }

        // Check if character is Greek (Unicode range X0370-X03FF)
        private bool IsGreek(string ch)
        {
            byte[] bts = Encoding.Unicode.GetBytes(ch);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
                r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');

            int max = 0X03FF;
            int min = 0X0370;

            int value = Convert.ToInt32(r, 16);
            return !(value < min) & !(value > max);
        }

        // Check if Unicode value is Greek (X0370-X03FF)
        private bool IsGreek(int value)
        {
            int max = 0X03FF;
            int min = 0X0370;
            return !(value < min) & !(value > max);
        }

        // Check if character is uppercase English (X0041-X005A)
        private bool IsCapitalEnglish(string ch)
        {
            byte[] bts = Encoding.Unicode.GetBytes(ch);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
                r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');

            int min = 0X0041;
            int max = 0X005A;
            int value = Convert.ToInt32(r, 16);
            return !(value < min) & !(value > max);
        }

        // Check if Unicode value is uppercase English (X0041-X005A)
        private bool IsCapitalEnglish(int value)
        {
            int min = 0X0041;
            int max = 0X005A;
            return !(value < min) & !(value > max);
        }

        // Check if character is lowercase English (X0061-X007A)
        private bool IsLowerEnglish(string ch)
        {
            byte[] bts = Encoding.Unicode.GetBytes(ch);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
                r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');

            int min = 0X0061;
            int max = 0X007A;
            int value = Convert.ToInt32(r, 16);
            return !(value < min) & !(value > max);
        }

        // Check if Unicode value is lowercase English (X0061-X007A)
        private bool IsLowerEnglish(int value)
        {
            int min = 0X0061;
            int max = 0X007A;
            return !(value < min) & !(value > max);
        }

        // Convert string to Unicode escape sequence
        private static string ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
                r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') + bts[i].ToString("x").PadLeft(2, '0');
            return r;
        }

        // Check if character is number (0X0030-0X0039)
        private bool IsNumber(string ch)
        {
            byte[] bts = Encoding.Unicode.GetBytes(ch);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
                r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');

            int min = 0X0030;
            int max = 0X0039;
            int value = Convert.ToInt32(r, 16);
            return !(value < min) & !(value > max);
        }

        // Check if Unicode value is number (0X0030-0X0039)
        private bool IsNumber(int value)
        {
            int min = 0X0030;
            int max = 0X0039;
            return !(value < min) & !(value > max);
        }

        // Check if character is '+'
        private bool IsAdd(string ch)
        {
            return (ch == "+");
        }

        // Check if Unicode value is '+' (0X002B)
        private bool IsAdd(int value)
        {
            return (value == 0X002B);
        }

        // Check if character is '-'
        private bool IsConnect(string ch)
        {
            return (ch == "-");
        }

        // Check if Unicode value is '-' (0X002D)
        private bool IsConnect(int value)
        {
            return (value == 0X002D);
        }

        // Check if Unicode value is '.' (0X002E)
        private bool IsDot(int value)
        {
            return (value == 0X002E);
        }

        // Check if Unicode value is '∈' (0X2208)
        private bool IsInclueIn(int value)
        {
            return (value == 0X2208);
        }

        // Check if character is '∈'
        private bool IsIncludeIn(string ch)
        {
            return (ch == "∈");
        }

        // Check if character is '^' (U+005E)
        private bool IsZhChSh(string ch)
        {
            return (ch == "^");
        }

        // Check if Unicode value is '^' (0X005E)
        private bool IsZhChSh(int value)
        {
            return (value == 0X005E);
        }

        // Initialize common geological label  including ages, 
        // Quaternary deposit types, informal rock units, and supracrustal types
        // Aug 8, 2016
        private int InitialGeoWords()
        {
            if (tGCRKnowledge == null) return -2;

            // If no record exists for this geological unit code, generate intelligently based on characteristic values
            DataRow[] drMatchWords = tGCRKnowledge.Select("WordType=1 or WordType=2");

            lstGeoWords = new List<string>();

            for (int i = 0; i < drMatchWords.Length; i++)
            {
                lstGeoWords.Add(drMatchWords[i]["Label Text"].ToString());
            }

            return 0;
        }

        // Generate characteristic values for geological unit codes containing common codes (Aug 8, 2016)
        private string GenerateGroupValue()
        {
            List<string> lst = new List<string>();

            for (int i = 0; i < lstGeoWords.Count; i++)
            {
                if (strGeoCode.IndexOf(lstGeoWords[i]) > -1)
                {
                    lst.Add(lstGeoWords[i].ToString());
                }
            }

            int count = lst.Count;

            for (int m = 0; m < count; m++)
            {
                for (int n = 0; n < lst.Count; n++)
                {
                    if (lst[m].Contains(lst[n]) && !lst[m].Equals(lst[n]))
                    {
                        lst.RemoveAt(n);
                        m = 0;
                        count--;
                        break;
                    }
                }
            }

            string strTemp = strGeoCode;
            strGroupValue = "";

            // Mark matching parts in geological codes with '1' (Aug 8, 2016)
            for (int m = 0; m < lst.Count; m++)
            {
                int start = strTemp.IndexOf(lst[m]);
                string strFlag = new string('*', lst[m].Length); // Create marker string

                if (start > -1)
                {
                    strTemp = strTemp.Replace(lst[m], strFlag);
                }
            }

            // Mark non-matching parts in geological codes with '0' (Aug 8, 2016)
            for (int i = 0; i < strTemp.Length; i++)
            {
                strGroupValue += (strTemp[i] == '*') ? "1" : "0";
            }

            return strGroupValue;
        }

        // Overloaded version to generate characteristic values (Aug 8, 2016)
        private string GenerateGroupValue(string str)
        {
            string strGeoCode = str;
            List<string> lst = new List<string>();

            for (int i = 0; i < lstGeoWords.Count; i++)
            {
                if (strGeoCode.IndexOf(lstGeoWords[i]) > -1)
                {
                    lst.Add(lstGeoWords[i].ToString());
                }
            }

            int count = lst.Count;

            for (int m = 0; m < count; m++)
            {
                for (int n = 0; n < lst.Count; n++)
                {
                    if (lst[m].Contains(lst[n]) && !lst[m].Equals(lst[n]))
                    {
                        lst.RemoveAt(n);
                        m = 0;
                        count--;
                        break;
                    }
                }
            }

            string strTemp = strGeoCode;
            strGroupValue = "";

            // Mark matching parts with '1'
            for (int m = 0; m < lst.Count; m++)
            {
                int start = strTemp.IndexOf(lst[m]);
                string strFlag = new string('*', lst[m].Length);

                if (start > -1)
                {
                    strTemp = strTemp.Replace(lst[m], strFlag);
                }
            }

            // Mark non-matching parts with '0'
            for (int i = 0; i < strTemp.Length; i++)
            {
                strGroupValue += (strTemp[i] == '*') ? "1" : "0";
            }

            return strGroupValue;
        }

        // Get characteristic value of geological unit code (July 30, 2016)
        private string GetEigenValue()
        {
            string r = "";
            int value = -1;
            flag = "";

            for (int index = 0; index < strGeoCode.Length; index++)
            {
                r = "";
                byte[] bts = Encoding.Unicode.GetBytes(strGeoCode[index].ToString());
                for (int i = 0; i < bts.Length; i += 2)
                {
                    r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');
                }

                value = Convert.ToInt32(r, 16);

                // Character flags:
                if (IsLowerEnglish(value)) flag += "0";
                if (IsCapitalEnglish(value)) flag += "1";
                if (IsNumber(value)) flag += "2";
                if (IsGreek(value)) flag += "3";
                if (IsAdd(value)) flag += "4";
                if (IsConnect(value)) flag += "5";
                if (IsDot(value)) flag += "6";
                if (IsInclueIn(value)) flag += "7";
                if (IsZhChSh(value)) flag += "8";
            }

            return flag;
        }

        // Generate characteristic value of geological unit code (Aug 17, 2016)
        private string GenerateEigenValue(string strSourceGeoCode)
        {
            string r = "";
            int value = -1;
            flag = "";

            for (int index = 0; index < strSourceGeoCode.Length; index++)
            {
                r = "";
                byte[] bts = Encoding.Unicode.GetBytes(strSourceGeoCode[index].ToString());
                for (int i = 0; i < bts.Length; i += 2)
                {
                    r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');
                }

                value = Convert.ToInt32(r, 16);

                // Character flags:
                if (IsLowerEnglish(value)) flag += "0";
                if (IsCapitalEnglish(value)) flag += "1";
                if (IsNumber(value)) flag += "2";
                if (IsGreek(value)) flag += "3";
                if (IsAdd(value)) flag += "4";
                if (IsConnect(value)) flag += "5";
                if (IsDot(value)) flag += "6";
                if (IsInclueIn(value)) flag += "7";
                if (IsZhChSh(value)) flag += "8";
            }

            return flag;
        }

        // Split geological unit code into 0/1/2.../6 segments connected with '@' (July 25, 2016)
        private string GetSubCode(string strGeoCode)
        {
            int start = 0;
            int length = 1;
            string strRCode = "";

            for (int m = 0; m < flag.Length; m++)    // Example: γδJ3
            {
                if (flag[m] == flag[start])
                {
                    start = m;
                    strRCode += strGeoCode.Substring(start, length);
                    continue;
                }
                else
                {
                    strRCode += "@";
                    start = m;
                    m--;
                }
            }

            return strRCode;
        }

        // Match characteristic value codes (Aug 2, 2016)
        private string MatchEigenValue()
        {
            if (strEigenValue == "") return "-1";
            if (tGCRKnowledge == null) return "-2";

            // First check if knowledge base already has this geological unit code
            foreach (DataRow dr in tGCRKnowledge.Rows)
            {
                if (dr["[Label Text]"].ToString() == strGeoCode)
                {
                    return dr["[Typical Style]"].ToString();
                }
            }

            // If no record exists, generate intelligently based on characteristic values
            GenerateGroupValue();
            DataRow[] drMatchEigens = tGCRKnowledge.Select("[Value of VSM]='" + strEigenValue + "' and [Value of Element]='" + strGroupValue + "'");

            if (drMatchEigens.Length < 1)
            {
                if (System.Windows.Forms.MessageBox.Show("No matching geological unit code found. Learn?", "Prompt:",
                    System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    return "Learn Completely";
                }
                else
                {
                    return "Learn Failure";
                }
            }

            if (drMatchEigens.Length > 1)
            {
                // Further filter by common geological code combinations
                GenerateGroupValue();
                drMatchEigens = tGCRKnowledge.Select("[Value of VSM]='" + strEigenValue + "' and [Value of Element]='" + strGroupValue + "'");
            }

            string strMatchEigenValue = "";
            for (int i = 0; i < drMatchEigens.Length; i++)
            {
                strMatchEigenValue += drMatchEigens[i]["[Typical Style]"].ToString() + "\r\n";
            }

            // Replace original code with superscript/subscript/italic markers to generate new geological code (Aug 2, 2016)
            for (int i = 0; i < drMatchEigens.Length; i++)
            {
                strRecPresentCode += RepalceSourceGeoCode(drMatchEigens[i]["[Label Text]"].ToString(),
                                     drMatchEigens[i]["[Typical Style]"].ToString(),
                                     strGeoCode, gisName) + drMatchEigens[i]["备注"].ToString() + "%";
            }

            return strRecPresentCode;
        }

        // Overloaded version with priority selection for Member/Formation (Sep 2, 2016)
        private string MatchEigenValue(Priority prior)
        {
            if (strEigenValue == "") return "-1";
            if (tGCRKnowledge == null) return "-2";

            // First check if knowledge base already has this geological unit code
            foreach (DataRow dr in tGCRKnowledge.Rows)
            {
                if (dr["Label Text"].ToString() == strGeoCode)
                {
                    return dr["Typical Style"].ToString();
                }
            }

            // If no record exists, generate intelligently based on characteristic values
            GenerateGroupValue();
            DataRow[] drMatchEigens = tGCRKnowledge.Select("[Value of VSM]='" + strEigenValue + "' and [Value of Element]='" + strGroupValue + "'");

            if (drMatchEigens.Length < 1)
            {
                return null;
            }

            if (drMatchEigens.Length > 1)
            {
                // Further filter by common geological code combinations
                GenerateGroupValue();
                drMatchEigens = tGCRKnowledge.Select("[Value of VSM]='" + strEigenValue + "' and [Value of Element]='" + strGroupValue + "'");
            }

            string strMatchEigenValue = "";
            for (int i = 0; i < drMatchEigens.Length; i++)
            {
                strMatchEigenValue += drMatchEigens[i]["Typical Style"].ToString() + "\r\n";
            }

            string strTmp = "";
            if (drMatchEigens.Length > 1)
            {
                // Replace original code with formatting markers
                for (int i = 0; i < drMatchEigens.Length; i++)
                {
                    if (drMatchEigens[i]["备注"].ToString() != "")
                    {
                        strTmp = drMatchEigens[i]["备注"].ToString();
                        if (strTmp.Substring(strTmp.Length - 1) == prior.ToString())
                        {
                            strRecPresentCode = RepalceSourceGeoCode(drMatchEigens[i]["[Label Text]"].ToString(),
                                                  drMatchEigens[i]["[Typical Style]"].ToString(),
                                                  strGeoCode,
                                                  gisName);
                        }
                    }
                }
            }

            if (drMatchEigens.Length == 1)
            {
                strRecPresentCode = RepalceSourceGeoCode(drMatchEigens[0]["Label Text"].ToString(),
                                      drMatchEigens[0]["Typical Style"].ToString(),
                                      strGeoCode, gisName);
            }

            return strRecPresentCode;
        }

        // Match characteristic value codes (Aug 8, 2016)
        private string MatchEigenValue(bool IsFilter)
        {
            if (strEigenValue == "") return "-1";
            if (tGCRKnowledge == null) return "-2";

            // First check if knowledge base already has this geological unit code
            foreach (DataRow dr in tGCRKnowledge.Rows)
            {
                if (dr["Label Text"].ToString() == strGeoCode)
                {
                    return dr["Typical Style"].ToString();
                }
            }

            // If no record exists, generate intelligently based on characteristic values (Aug 8, 2016)
            DataRow[] drMatchEigens = tGCRKnowledge.Select("EigenValue='" + strEigenValue + "'");

            if (drMatchEigens.Length < 1)
            {
                if (System.Windows.Forms.MessageBox.Show("No matching geological unit code found. Learn?", "Prompt:",
                    System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    return "Learn Completely";
                }
                else
                {
                    return "Learn Failure";
                }
            }

            if (drMatchEigens.Length > 1)
            {
                System.Windows.Forms.MessageBox.Show("Found " + drMatchEigens.Length + " geological characteristic value codes", "Prompt:");
            }

            string strMatchEigenValue = "";
            for (int i = 0; i < drMatchEigens.Length; i++)
            {
                strMatchEigenValue += drMatchEigens[i]["Typical Style"].ToString() + "\r\n";
            }

            // Replace original code with formatting markers (Aug 2, 2016)
            for (int i = 0; i < drMatchEigens.Length; i++)
            {
                strRecPresentCode = RepalceSourceGeoCode(drMatchEigens[i]["Label Text"].ToString(),
                                      drMatchEigens[i]["Typical Style"].ToString(),
                                      strGeoCode, GISSoftware.ESRI);
            }

            return strRecPresentCode;
        }


        // convert raw text to typical style
        private string RepalceSourceGeoCode(string strSourceCode,  // Raw geological label in knowledge base
                                           string strSourcePresentCode, // Typical geological label in knowledge base
                                           string strDestCode     // raw text
                                           )
        {
            string strDestPresentCode = "";

            if (strSourceCode.Length != strDestCode.Length)
            {
                System.Windows.Forms.MessageBox.Show("Error", "Prompt:");
            }

            int index = -1;
            strDestPresentCode = strSourcePresentCode;

            for (int i = 0; i < strDestCode.Length; i++)
            {
                index = strDestPresentCode.IndexOf(strSourceCode[i], i);
                strDestPresentCode = strDestPresentCode.Remove(index, 1);
                strDestPresentCode = strDestPresentCode.Insert(index, strDestCode[i].ToString());
            }

            return strDestPresentCode;   // formatted text
        }

        // Override to handle compatibility with different GIS software packages. 
        // Ensures proper interpretation and handling of GIS-specific data formats 
        // and operations across multiple platforms.
        // ASCII digit codes are used here for numerbers 
        // Written by Wigsont on Feb. 25, 2025.
        // GIS software-specific version
        private string RepalceSourceGeoCode(string strSourceCode,
                                           string strSourcePresentCode,
                                           string strDestCode,
                                           GISSoftware giscode)
        {
            if (giscode != GISSoftware.ESRI) return "";

            string strDestPresentCode = "";

            if (strSourceCode.Length != strDestCode.Length)
            {
                System.Windows.Forms.MessageBox.Show("Error", "Prompt:");
            }

            int index = -1;
            strDestPresentCode = strSourcePresentCode;

            bool[] isTagCharacter = MarkTagCharacters(strSourcePresentCode, esriFormattingTags);

            if (isTagCharacter.Length == strSourcePresentCode.Length)
            {
                int j = 0;
                for (int i = 0; i < strSourcePresentCode.Length; i++)
                {
                    if (isTagCharacter[i]) continue;

                    index = strDestPresentCode.IndexOf(strSourceCode[j], i);

                    if (index < 0)
                    {
                        int m = index;
                    }

                    strDestPresentCode = strDestPresentCode.Remove(index, 1);
                    strDestPresentCode = strDestPresentCode.Insert(index, strDestCode[j].ToString());
                    j++;
                }

                return strDestPresentCode;
            }
            else
                return "";
        }

        /// <summary>
        /// Marks characters in string that belong to specified tags (Generated by ChatGPT, Feb 25, 2025)
        /// </summary>
        /// <param name="input">String to process</param>
        /// <param name="tags">Array of tag strings</param>
        /// <returns>Boolean array indicating whether each character is within tag ranges</returns>
        public static bool[] MarkTagCharacters(string input, string[] tags)
        {
            bool[] isTagCharacter = new bool[input.Length];

            foreach (var tag in tags)
            {
                int index = input.IndexOf(tag, StringComparison.Ordinal);
                while (index >= 0)
                {
                    for (int i = index; i < index + tag.Length && i < isTagCharacter.Length; i++)
                    {
                        isTagCharacter[i] = true;
                    }
                    index = input.IndexOf(tag, index + 1, StringComparison.Ordinal);
                }
            }

            return isTagCharacter;
        }

        // Parse input label using knowledge base:
        // Step 1: Get characteristic value
        // Step 2: Match knowledge base characteristic values
        // Step 3: Add parsed string/@$ identifiers
        public string GetPresetGeoCode()
        {
            strEigenValue = GetEigenValue();
            return MatchEigenValue();
        }

        // Overloaded version with priority selection (Sep 2, 2016)
        public string GetPresetGeoCode(Priority prior)
        {
            strEigenValue = GetEigenValue();
            return MatchEigenValue(prior);
        }

        private string splitSourceGCode()
        {
            string r = "";
            int value = -1;

            for (int index = 0; index < strGeoCode.Length; index++)
            {
                r = "";
                byte[] bts = Encoding.Unicode.GetBytes(strGeoCode[index].ToString());
                for (int i = 0; i < bts.Length; i += 2)
                {
                    r += "0X" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');
                }

                value = Convert.ToInt32(r, 16);

                // Character flags:
                if (IsLowerEnglish(value)) flag += "0";
                if (IsCapitalEnglish(value)) flag += "1";
                if (IsNumber(value)) flag += "2";
                if (IsGreek(value)) flag += "3";
                if (IsAdd(value)) flag += "4";
                if (IsConnect(value)) flag += "5";
                if (IsDot(value)) flag += "6";
                if (IsInclueIn(value)) flag += "7";
                if (IsZhChSh(value)) flag += "8";
            }

            return GetSubCode(strGeoCode);
        }
    }
}