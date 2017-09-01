/*
TODO: 

    -------------------------------------------------    
    Legend: 
        . pending
        - canceled
        / in progress, tentative, needs testing, etc.
        + completed
    -------------------------------------------------

    . binary, handle misalignment where len(find) - len(rep) % len(padding) != 0 (GetBytes() encoded length)

    . 32-bit and 64-bit applications see different registries, need an option
    
    + with binary replacement, only use string functions on the section of the buffer that contains the result,
        don't try to convert the whole thing to UTF-16 and back
        Right now, buffer copying needs work, keep track of offset differences

    + Optional root path

    + with case insensitive searches, determine the case of the detection and substitute the proper case tranformed replacement string

    + case sensitivity

    + handle set default value

    + Set default value for a key from powershell
        maybe this can help:
        (get-itemproperty -literalpath HKCU:\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice).'(default)'
        remember that if the value is not set it returns $null then also your method return the correct value ;)
        Forgot to say that HKCR is not defined at default, use:
        New-PSDrive -Name HKCR -PSProvider Registry -Root HKEY_CLASSES_ROOT
        then you can do correctly:
        (get-itemproperty -literalpath HKCR:\http\shell\open\command\).'(default)'

    + Refactor data scan to work on default same as named vals

    + hexifyPS(binData) lol slow, avoid string concatenation plz

    + pad binary replacement, warn if replacement string is longer than original and binary option is checked
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Design;




namespace RegReplace
{
    public partial class Form1 : Form
    {
        const string DefaultRegistryValueSentinelStringConstant = "RegReplace_dot_Form1_Dot_DefaultRegistryValueSentinelStringConstant_7af4b33f-3a4d-4b8a-9f40-8c01bbc27b14";

        bool m_userCancel = false;
        TimeSpan m_lastRefreshTime = new TimeSpan();
        TimeSpan m_refreshInterval = new TimeSpan(100);
        System.Diagnostics.Stopwatch m_runningTime = new System.Diagnostics.Stopwatch();

        [Flags]
        enum SearchOptionFlags
        {
            ZERO = 0,
            KEYS = 1,
            VALUES = 2,
            DATA = 4,
            HKCR = 8,
            HKU = 0x10,
            HKCU = 0x20,
            HKLM = 0x40,
            HKCC = 0x80,
            HKPD = 0x100,
            STRING = 0x200,
            MULTISTRING = 0x400,
            EXPANDSTRING = 0x800,
            BINARY = 0x1000,
            CASE_SENSITIVE = 0x2000,
            MATCH_RESULT_CASE = 0x4000
        }
        struct SearchOptions
        {
            public SearchOptionFlags flags;
            public StringComparison stringCompareStyle;
            public System.Globalization.CultureInfo culture;
            public string rootPath;
            public string searchString;
            public byte[] searchStringBinA;
            public byte[] searchStringBinW;
            public string replacementString;
            public byte[] replacementStringBinA;
            public byte[] replacementStringBinW;
            public byte[] replacementStringUpperBinA;
            public byte[] replacementStringUpperBinW;
            public byte[] replacementStringLowerBinA;
            public byte[] replacementStringLowerBinW;
        }

        enum StringCaseType
        {
            MIXED,
            UPPER,
            LOWER
        }

        [Flags]
        enum MatchTypeEnum
        {
            ZERO = 0,
            KEY = 1,
            VALUE = 2,
            DATA = 4,
            DEFAULT = 8,
            ASCII = 0x10,
            UNICODE = 0x20
        }

        class KeyMatch
        {
            public string path; // includes name as last element in path
            public string name; // redundant but provides easy pre-parsed data
            public string newName;
            public string newPath;
        }
        class ValueMatch
        {
            public string path;
            public string valueName;
            public string rep;
        }
        class DataMatch
        {
            public string path;
            public string valueName;
            public RegistryValueKind kind;
            public MatchTypeEnum matchType;
            //public object data;
            //public object newData;
            public bool defaultValue;
        }

        char padchar = '\0';

        //delegate void MatchHandler(string keyPath, string valueName, object data, RegistryValueKind kind, MatchTypeEnum matchType );
        delegate void MatchHandler(string keyPath, string valueName, RegistryValueKind kind, MatchTypeEnum matchType);





        public Form1()
        {
            InitializeComponent();
        }

        StringCaseType determineCase(string s, SearchOptions op)
        {
            string u, l;
            //System.Globalization.CultureInfo ci;

            switch (op.stringCompareStyle)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    l = s.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                    break;
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    l = s.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                default:
                    l = s.ToLower();
                    break;
            }
            if (s == l) { return StringCaseType.LOWER; }

            switch (op.stringCompareStyle)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    //ci = System.Globalization.CultureInfo.CurrentCulture;
                    u = s.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
                    l = s.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                    break;
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    //ci = System.Globalization.CultureInfo.InvariantCulture;
                    u = s.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                    l = s.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                default:
                    //ci = System.Globalization.CultureInfo.InstalledUICulture;
                    u = s.ToUpper();
                    l = s.ToLower();
                    break;
            }
            if (s == u) { return StringCaseType.UPPER; }

            return StringCaseType.MIXED;
        }

        string getFileOpen()
        {
            var dlg = new OpenFileDialog();
            var r = dlg.ShowDialog();
            if (r == DialogResult.Cancel) { return ""; }
            return dlg.FileName;
        }
        string getFileSave()
        {
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.DefaultExt = "ps1";
            dlg.Filter = "PowerShell Scripts (*.ps1)|*.ps1";
            var r = dlg.ShowDialog();
            if (r == DialogResult.Cancel) { return ""; }
            return dlg.FileName;
        }
        string getFolder()
        {
            var dlg = new FolderBrowserDialog();
            var r = dlg.ShowDialog();
            if (r == DialogResult.Cancel) { return ""; }
            return dlg.SelectedPath;
        }

        private void btnFindFile_Click(object sender, EventArgs e)
        {
            var s = getFileOpen();
            if (s != "") { txtFind.Text = s; }
        }

        private void btnFindFolder_Click(object sender, EventArgs e)
        {
            var s = getFolder();
            if (s != "") { txtFind.Text = s; }
        }

        private void btnReplaceFile_Click(object sender, EventArgs e)
        {
            var s = getFileOpen();
            if (s != "") { txtReplaceWith.Text = s; }
        }

        private void btnReplaceFolder_Click(object sender, EventArgs e)
        {
            var s = getFolder();
            if (s != "") { txtReplaceWith.Text = s; }
        }

        private void btnUndoFileBrowse_Click(object sender, EventArgs e)
        {
            var s = getFileSave();
            if (s != "") { txtUndoFile.Text = s; }
        }

        private void btnRedoFileBrowse_Click(object sender, EventArgs e)
        {
            var s = getFileSave();
            if (s != "") { txtRedoFile.Text = s; }
        }

        private void btnDump_Click(object sender, EventArgs e)
        {
            btnDump.Enabled = false;
            btnStop.Enabled = true;
            m_userCancel = false;
            m_lastRefreshTime = TimeSpan.Zero;
            SearchOptions op = optionSnapshot();
            txtResult.Text = "Scanning...\r\n";
            //txtResult.Refresh();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            m_runningTime.Reset();
            m_runningTime.Start();
            m_lastRefreshTime = TimeSpan.Zero;

            // make copies of these fields in case events are processed and text box changes
            string findstr = txtFind.Text;
            string repstr = txtReplaceWith.Text;
            string undoFilename = txtUndoFile.Text;
            string redoFilename = txtRedoFile.Text;


            int repBinLength = System.Text.Encoding.Unicode.GetBytes(repstr).Length;
            int searchBinLength = System.Text.Encoding.Unicode.GetBytes(findstr).Length;
            int padBinLength = System.Text.Encoding.Unicode.GetBytes(new String(padchar, 1)).Length;
            if (((repBinLength < searchBinLength) && op.flags.HasFlag(SearchOptionFlags.BINARY)))
            {
                if (MessageBox.Show("Binary replacement data is longer than search term. This will result in a different length, which may cause pointer errors, crashes, and/or data corruption. Exercise caution before executing the script file. Do you want to continue?", "Binary replace", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    return;
                }
            }
            if (((int)Math.Abs(searchBinLength - repBinLength)) % padBinLength != 0)
            {
                txtResult.Text += "WARNING: UTF-16 alignment error. The difference in length between search text and replacement text is not divisible by the encoded length of the pad character.";
                if (MessageBox.Show("UTF-16 alignment error: The difference in length between search text and replacement text is not divisible by the encoded length of the pad character. This will cause misalignment. Do you want to continue?", "Binary replace", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (chkUseRootPath.Checked)
            {
                RegistryKey keyTest = OpenKey(txtRootPath.Text, op);
                if (keyTest == null)
                {
                    txtResult.Text += "ERROR: Root path not found.\r\n";
                    m_runningTime.Stop();
                    return;
                }
                keyTest.Close();
            }

            StreamWriter undo, redo;
            try
            {
                undo = new System.IO.StreamWriter(undoFilename);
            }
            catch (Exception ex)
            {
                txtResult.Text += ex.Message + "\r\n";
                m_runningTime.Stop();
                return;
            }

            try
            {
                redo = new System.IO.StreamWriter(redoFilename);
            }
            catch (Exception ex)
            {
                undo.Close();
                txtResult.Text += ex.Message + "\r\n";
                m_runningTime.Stop();
                return;
            }

            //undo.WriteLine("REGEDIT4");
            //undo.WriteLine("");
            //redo.WriteLine("REGEDIT4");
            //redo.WriteLine("");



            List<KeyMatch> keyMatches = new List<KeyMatch>();
            List<ValueMatch> valueMatches = new List<ValueMatch>();
            List<DataMatch> dataMatches = new List<DataMatch>();

            //#############################################################################################################
            // MAIN DUMP FUNCTION
            try
            {
                regfind(txtFind.Text, op,
                    //(string keyPath, string valueName, object dataObj, RegistryValueKind kind, MatchTypeEnum matchType) =>
                    (string keyPath, string valueName, RegistryValueKind kind, MatchTypeEnum matchType) =>
                    {
                        if (matchType.HasFlag(MatchTypeEnum.KEY))
                        {
                            KeyMatch km = new KeyMatch();
                            km.path = keyPath;
                            km.name = valueName;
                            keyMatches.Add(km);
                        }
                        if (matchType.HasFlag(MatchTypeEnum.VALUE))
                        {
                            ValueMatch m = new ValueMatch();
                            m.path = keyPath;
                            m.valueName = valueName;
                            valueMatches.Add(m);
                        }
                        if (matchType.HasFlag(MatchTypeEnum.DATA))
                        {
                            DataMatch m = new DataMatch();
                            m.path = keyPath;
                            m.matchType = matchType;
                            if (valueName == DefaultRegistryValueSentinelStringConstant)
                            {
                                m.defaultValue = true;
                                m.valueName = "";
                            }
                            else
                            {
                                m.defaultValue = false;
                                m.valueName = valueName;
                            }
                            //m.data = dataObj;
                            m.kind = kind;
                            if (matchType.HasFlag(MatchTypeEnum.DEFAULT)) { m.defaultValue = true; }
                            //else { m.defaultValue = false; }
                            dataMatches.Add(m);
                        }
                    });
            }
            // END MAIN DUMP FUNCTION
            //#############################################################################################################
            catch (Exception ex)
            {
                txtResult.Text += ex.Message + "\r\n" + ex.StackTrace + "\r\n";
                undo.Close();
                redo.Close();
                sw.Stop();
                txtResult.Text += "Error after " + sw.Elapsed.TotalSeconds.ToString() + " seconds.\r\n";
                return;
            }
            //finally
            //{
            //    undo.Close();
            //    redo.Close();
            //    sw.Stop();
            //    txtResult.Text += "Finished in " + sw.Elapsed.TotalSeconds.ToString() + " seconds.\r\n";
            //}

            // For the 'do' or 'redo' file, we set data, then values, then keys, so all the paths work out.
            // If there's a key that needs renaming, but there's also a data value under it that will be modified,
            // need to change the data and values before the key path.
            // For the 'undo' file, need to work in the opposite order

            //int p;

            // HKLM and HKCU are built in
            undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKU -Root HKEY_USERS");
            redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKU -Root HKEY_USERS");
            undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCR -Root HKEY_CLASSES_ROOT");
            redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCR -Root HKEY_CLASSES_ROOT");
            undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCC -Root HKEY_CURRENT_CONFIG");
            redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCC -Root HKEY_CURRENT_CONFIG");
            undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKPD -Root HKEY_PERFORMANCE_DATA");
            redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKPD -Root HKEY_PERFORMANCE_DATA");
            //if (op.flags.HasFlag(SearchOptionFlags.HKU))
            //{
            //    undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKU -Root HKEY_USERS");
            //    redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKU -Root HKEY_USERS");
            //}
            //if (op.flags.HasFlag(SearchOptionFlags.HKCR))
            //{
            //    undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCR -Root HKEY_CLASSES_ROOT");
            //    redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCR -Root HKEY_CLASSES_ROOT");
            //}
            //if (op.flags.HasFlag(SearchOptionFlags.HKCC))
            //{
            //    undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCC -Root HKEY_CURRENT_CONFIG");
            //    redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKCC -Root HKEY_CURRENT_CONFIG");
            //}
            //if (op.flags.HasFlag(SearchOptionFlags.HKPD))
            //{
            //    undo.WriteLine("New-PSDrive -PSProvider Registry -Name HKPD -Root HKEY_PERFORMANCE_DATA");
            //    redo.WriteLine("New-PSDrive -PSProvider Registry -Name HKPD -Root HKEY_PERFORMANCE_DATA");
            //}

            string line;
            object tempDataObjRef;
            RegistryKey tempKey;
            foreach (var dm in dataMatches)
            {
                tempKey = OpenKey(dm.path, op);
                if (tempKey == null)
                {
                    txtResult.Text += "WARNING: Can not open previously matched key " + dm.path + "\r\n";
                    continue;
                }
                if (dm.defaultValue)
                {
                    tempDataObjRef = tempKey.GetValue(null, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }
                else
                {
                    tempDataObjRef = tempKey.GetValue(dm.valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }
                tempKey.Close();
                //dm.newData = doReplace(dm.kind, dm.data, findstr, repstr, op);
                //object newData = doReplace(dm.kind, tempDataObjRef, findstr, repstr, op, dm.matchType);
                object newData = doReplace(dm.kind, tempDataObjRef, op, dm.matchType);
                if (dm.defaultValue)
                {
                    //line = "Set-ItemProperty -LiteralPath \"" + dm.path + "\" -Name '(Default)' -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, dm.newData);
                    line = "Set-ItemProperty -LiteralPath \"" + dm.path + "\" -Name '(Default)' -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, newData);
                }
                else
                {
                    //line = "Set-ItemProperty -Path \"" + dm.path + "\" -Name \"" + dm.valueName + "\" -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, dm.newData);
                    line = "Set-ItemProperty -Path \"" + dm.path + "\" -Name \"" + dm.valueName + "\" -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, newData);
                }
                redo.WriteLine(line);
            }
            tempDataObjRef = null;
            foreach (var vm in valueMatches)
            {
                //vm.rep = ((string)doReplace(RegistryValueKind.String, vm.valueName, findstr, repstr, op));
                vm.rep = ((string)doReplace(RegistryValueKind.String, vm.valueName, op));
                line = "Rename-ItemProperty -Path \"" + vm.path + "\" -Name \"" + vm.valueName + "\" -NewName \"" + vm.rep + "\"";
                redo.WriteLine(line);
            }
            foreach (var km in keyMatches)
            {
                //km.newName = (string)doReplace(RegistryValueKind.String, km.name, findstr, repstr, op);
                km.newName = (string)doReplace(RegistryValueKind.String, km.name, op);
                int pp = km.path.LastIndexOf("\\");
                if (pp >= 0)
                {
                    km.newPath = km.path.Substring(0, pp) + km.newName;
                    //string checkPath = (string)doReplace(RegistryValueKind.String, km.path, findstr, repstr, op);
                    string checkPath = (string)doReplace(RegistryValueKind.String, km.path, op);
                    if (checkPath == km.newPath)
                    {
                        line = "Rename-Item -Path \"" + km.path + "\" -NewName \"" + km.newName + "\"";
                        redo.WriteLine(line);
                    }
                    else
                    {
                        km.newPath = km.path;
                        km.newName = km.name;
                        txtResult.Text += "WARNING: Rename operation results in an inconsistent path, skipping " + km.path + "\r\n";
                    }
                }
                else
                {
                    km.newPath = km.path;
                    km.newName = km.name;
                    txtResult.Text += "WARNING: Rename operation results in an inconsistent path, skipping " + km.path + "\r\n";
                }
            }
            tempDataObjRef = null;
            tempKey = null;

            // Now the undo/backup
            keyMatches.Reverse();
            foreach (var km in keyMatches)
            {
                line = "Rename-Item -Path \"" + km.newPath + "\" -NewName \"" + km.name + "\"";
                undo.WriteLine(line);
            }
            foreach (var vm in valueMatches)
            {
                line = "Rename-ItemProperty -Path \"" + vm.path + "\" -Name \"" + vm.rep + "\" -NewName \"" + vm.valueName + "\"";
                undo.WriteLine(line);
            }
            foreach (var dm in dataMatches)
            {
                tempKey = OpenKey(dm.path, op);
                if (tempKey == null)
                {
                    txtResult.Text += "WARNING: Can not open previously matched key " + dm.path + "\r\n";
                    continue;
                }
                if (dm.defaultValue)
                {
                    tempDataObjRef = tempKey.GetValue(null, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }
                else
                {
                    tempDataObjRef = tempKey.GetValue(dm.valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                }
                tempKey.Close();
                //dm.newData = doReplace(dm.kind, dm.data, findstr, repstr, op);
                //object newData = doReplace(dm.kind, tempDataObjRef, findstr, repstr, op, dm.matchType);
                //object newData = doReplace(dm.kind, tempDataObjRef, op, dm.matchType);
                if (dm.defaultValue)
                {
                    //line = "Set-ItemProperty -LiteralPath \"" + dm.path + "\" -Name '(Default)' -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, dm.data);
                    line = "Set-ItemProperty -LiteralPath \"" + dm.path + "\" -Name '(Default)' -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, tempDataObjRef);
                }
                else
                {
                    //line = "Set-ItemProperty -Path \"" + dm.path + "\" -Name \"" + dm.valueName + "\" -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, dm.data);
                    line = "Set-ItemProperty -Path \"" + dm.path + "\" -Name \"" + dm.valueName + "\" -Type " + regKindStr(dm.kind) + " -Value " + exportStr(dm.kind, tempDataObjRef);
                }
                undo.WriteLine(line);
            }

            undo.WriteLine("write-host \"Finished. Press any key to close...\"");
            undo.WriteLine("[void][System.Console]::ReadKey($true)");
            redo.WriteLine("write-host \"Finished. Press any key to close...\"");
            redo.WriteLine("[void][System.Console]::ReadKey($true)");

            undo.Close();
            redo.Close();

            sw.Stop();
            m_runningTime.Stop();

            txtResult.Text += "Finished in " + sw.Elapsed.TotalSeconds.ToString() + " seconds.\r\n You may need to enable PowerShell scripts by running the following command:\r\nSet-ExecutionPolicy RemoteSigned\r\n ... This can be set back to the most secure setting later by saying \r\nSet-ExecutionPolicy Restricted";
            txtCurrentLocation.Text = "";

        }

        string binAsciiToString(byte[] binData)
        {
            if (binData == null) { return ""; }
            return System.Text.Encoding.ASCII.GetString(binData);

            //var enc = System.Text.Encoding.ASCII;
            ////var enc = new System.Text.ASCIIEncoding(); 
            //return enc.GetString(binData);
            ////string ret = "";
            ////for (int i = 0; i < binData.Length; i++)
            ////{
            ////    ret += (char)(binData[i]);
            ////}
            ////return ret;
        }
        string binWCharToStringLittleEndian(byte[] binData)
        {
            if (binData == null) { return ""; }
            return System.Text.Encoding.Unicode.GetString(binData);

            //var enc = System.Text.Encoding.Unicode;
            ////var enc = new System.Text.UnicodeEncoding(false, false);
            //return enc.GetString(binData);
            ////string ret = "";
            ////for (int i = 0; i + 1 < binData.Length; i += 2)
            ////{
            ////    ret += (char)(((int)binData[i]) + (((int)(binData[i + 1])) * 0x100));
            ////}
            ////if (binData.Length % 2 != 0)
            ////{
            ////    ret += (char)(binData[binData.Length - 1]);
            ////}
            ////return ret;
        }

        string ToUpperOption(string orig, SearchOptions op)
        {
            switch (op.stringCompareStyle)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return orig.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return orig.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                default:
                    return orig.ToUpper();
            }
        }
        string ToLowerOption(string orig, SearchOptions op)
        {
            switch (op.stringCompareStyle)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                    return orig.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return orig.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                default:
                    return orig.ToLower();
            }
        }

        string replacementStringCased(string dataStr, string repStr, SearchOptions op)
        {
            StringCaseType detection = determineCase(dataStr, op);
            switch (detection)
            {
                case StringCaseType.UPPER:
                    return ToUpperOption(repStr, op);
                case StringCaseType.LOWER:
                    return ToLowerOption(repStr, op);
                case StringCaseType.MIXED:
                default:
                    return repStr;
            }
        }

        void dumpBin(String filename, Byte[] data)
        {
            var fsBefore = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            var before = new BinaryWriter(fsBefore);
            before.Write(data);
            before.Close();
            fsBefore.Close();
        }

        object doReplace(RegistryValueKind kind, object data, SearchOptions op, MatchTypeEnum matchType = MatchTypeEnum.ZERO)
        {
//#if DEBUG
//            const bool debugDumpBinaryReplacement = true;
//#endif
            string findstr = op.searchString;
            string repstr = op.replacementString;
            string retStr;
            string dataStr;
            string[] dataMultiStr;
            string[] retMultiStr;
            byte[] dataBin;
            byte[] retBin;
            byte[] t1;
            //byte[] repBin;
            //string binAStr, binWStr;

            int p;

            //bool foundBinaryMatch = false;

            //string findCase;
            //if (op.flags.HasFlag(SearchOptions.CASE_SENSITIVE)) { findCase = findstr; }
            //else { findCase = findstr.ToLower(); }
            //string searchCase = "";

            switch (kind)
            {
                case RegistryValueKind.Binary:
                    // Should I worry about wide characters... bleh
                    dataBin = (byte[])data;
                    if (matchType.HasFlag(MatchTypeEnum.ASCII))
                    {
                        List<long> ascMatchList;
                        if (op.flags.HasFlag(SearchOptionFlags.CASE_SENSITIVE))
                        {
                            ascMatchList = dataBin.IndexesOf(op.searchStringBinA);
                        }
                        else
                        {
                            ascMatchList = dataBin.IndexesOfNoCase(op.searchString, System.Text.Encoding.ASCII, op.culture);
                        }
                            
                        if (ascMatchList.Count > 0)
                        {
                            if (op.replacementStringBinA.Length > op.searchStringBinA.Length)
                            {
                                int sizeDiffPerElement = op.replacementStringBinA.Length - op.searchStringBinA.Length;
                                t1 = new byte[dataBin.Length + (ascMatchList.Count * sizeDiffPerElement)];
                            }
                            else
                            {
                                t1 = new byte[dataBin.Length];
                            }
                            int ascMatchListIndex = 0;
                            int ascSourceOfs = 0;
                            int t1Next = 0;
                            int copyLen;
                            int matchOfs = 0;
                            foreach (var asciiMatchOfs in ascMatchList)
                            {
                                matchOfs = (int)asciiMatchOfs;
                                copyLen = matchOfs - ascSourceOfs;
                                Buffer.BlockCopy(dataBin, ascSourceOfs, t1, t1Next, copyLen);
                                t1Next += copyLen;
                                if (op.flags.HasFlag(SearchOptionFlags.MATCH_RESULT_CASE))
                                {
                                    byte[] binCaseTest = new byte[op.searchStringBinA.Length];
                                    Buffer.BlockCopy(dataBin, matchOfs, binCaseTest, 0, op.searchStringBinA.Length);
                                    string caseTest = System.Text.Encoding.ASCII.GetString(binCaseTest);
                                    var ct = determineCase(caseTest, op);
                                    if (ct == StringCaseType.LOWER)
                                    {
                                        Buffer.BlockCopy(op.replacementStringLowerBinA, 0, t1, t1Next, op.replacementStringLowerBinA.Length);
                                        t1Next += op.replacementStringLowerBinA.Length;
                                    }
                                    else if (ct == StringCaseType.UPPER)
                                    {
                                        Buffer.BlockCopy(op.replacementStringUpperBinA, 0, t1, t1Next, op.replacementStringUpperBinA.Length);
                                        t1Next += op.replacementStringUpperBinA.Length;
                                    }
                                    else
                                    {
                                        Buffer.BlockCopy(op.replacementStringBinA, 0, t1, t1Next, op.replacementStringBinA.Length);
                                        t1Next += op.replacementStringBinA.Length; // dest buffer ofs, add replacement string length
                                    }
                                }
                                else
                                {
                                    Buffer.BlockCopy(op.replacementStringBinA, 0, t1, t1Next, op.replacementStringBinA.Length);
                                    t1Next += op.replacementStringBinA.Length; // dest buffer ofs, add replacement string length
                                }
                                ascSourceOfs += copyLen + op.searchStringBinA.Length; // source buffer, add search string length
                                ascMatchListIndex++;
                            }
                            Buffer.BlockCopy(dataBin, ascSourceOfs, t1, t1Next, dataBin.Length - ascSourceOfs);
                        }
                        else
                        {
                            t1 = dataBin;
                        }
                    }
                    else { t1 = dataBin; }

                    // Now work with t1 in place of dataBin
                    if (matchType.HasFlag(MatchTypeEnum.UNICODE))
                    {
                        //var uniMatchList = dataBin.IndexesOf(op.searchStringBinW);
                        List<long> uniMatchList;
                        if (op.flags.HasFlag(SearchOptionFlags.CASE_SENSITIVE))
                        {
                            uniMatchList = t1.IndexesOf(op.searchStringBinW);
                        }
                        else
                        {
                            uniMatchList = t1.IndexesOfNoCase(op.searchString, System.Text.Encoding.Unicode, op.culture);
                        }
                        if (uniMatchList.Count > 0)
                        {
                            if (op.replacementStringBinW.Length > op.searchStringBinW.Length)
                            {
                                int sizeDiffPerElement = op.replacementStringBinW.Length - op.searchStringBinW.Length;
                                retBin = new byte[t1.Length + (uniMatchList.Count * sizeDiffPerElement)];
                            }
                            else
                            {
                                retBin = new byte[t1.Length];
                            }
                            int uniMatchListIndex = 0;
                            int uniSourceOfs = 0;
                            int retNext = 0;
                            int copyLen;
                            int matchOfs = 0;
                            foreach (var uniMatchOfs in uniMatchList)
                            {
                                matchOfs = (int)uniMatchOfs;
                                copyLen = matchOfs - uniSourceOfs;
                                Buffer.BlockCopy(dataBin, uniSourceOfs, retBin, retNext, copyLen);
                                retNext += copyLen;
                                if (op.flags.HasFlag(SearchOptionFlags.MATCH_RESULT_CASE))
                                {
                                    byte[] binCaseTest = new byte[op.searchStringBinW.Length];
                                    Buffer.BlockCopy(dataBin, matchOfs, binCaseTest, 0, op.searchStringBinW.Length);
                                    string caseTest = System.Text.Encoding.Unicode.GetString(binCaseTest);
                                    var ct = determineCase(caseTest, op);
                                    if (ct == StringCaseType.LOWER)
                                    {
                                        Buffer.BlockCopy(op.replacementStringLowerBinW, 0, retBin, retNext, op.replacementStringLowerBinW.Length);
                                        retNext += op.replacementStringLowerBinW.Length;
                                    }
                                    else if (ct == StringCaseType.UPPER)
                                    {
                                        Buffer.BlockCopy(op.replacementStringUpperBinW, 0, retBin, retNext, op.replacementStringUpperBinW.Length);
                                        retNext += op.replacementStringUpperBinW.Length;
                                    }
                                    else
                                    {
                                        Buffer.BlockCopy(op.replacementStringBinW, 0, retBin, retNext, op.replacementStringBinW.Length);
                                        retNext += op.replacementStringBinW.Length; // dest buffer ofs, add replacement string length
                                    }
                                }
                                else
                                {
                                    Buffer.BlockCopy(op.replacementStringBinW, 0, retBin, retNext, op.replacementStringBinW.Length);
                                    retNext += op.replacementStringBinW.Length; // dest buffer ofs, add replacement string length
                                }

                                uniSourceOfs += copyLen + op.searchStringBinW.Length; // source buffer, add search string length
                                uniMatchListIndex++;
                            }
                            Buffer.BlockCopy(dataBin, uniSourceOfs, retBin, retNext, t1.Length - uniSourceOfs);
                        }
                        else { retBin = t1; }
                    }
                    else { retBin = t1; }

                    //string repCase;
                    //string originalCut;
                    //binAStr = binAsciiToString(dataBin);
                    //
                    //
                    //p = binAStr.IndexOf(findstr, op.stringCompareStyle);
                    //if (p >= 0)
                    //{
                    //    string paddedReplacementA;
                    //    originalCut = binAStr.Substring(p, findstr.Length);
                    //    repCase = replacementStringCased(originalCut, repstr, op);
                    //    repBin = System.Text.Encoding.ASCII.GetBytes(repCase);
                    //    //if (findstr.Length >= repstr.Length)
                    //    //{
                    //        int padlen = findstr.Length - repstr.Length;
                    //        //paddedReplacementA = repstr + new string(padchar, findstr.Length - repstr.Length);
                    //        padlen++;
                    //        do
                    //        {
                    //            padlen--;
                    //            paddedReplacementA = replacementStringCased(originalCut, repstr, op)
                    //                + new string(padchar, padlen);
                    //            retStr = binAStr.Substring(0, p) + paddedReplacementA + binAStr.Substring(p + findstr.Length);
                    //            retBin = System.Text.Encoding.ASCII.GetBytes(retStr);
                    //        } while ((padlen >= 0) && (retBin.Length > dataBin.Length)); // + findstr.Length - repstr.Length));
                    //        // I don't think there's too much reason to worry about encoding length on ascii, but i gotta do it for utf-16 anyway,
                    //        // might as well figure it out here.
                    //        while (retBin.Length < dataBin.Length)
                    //        {
                    //            padlen++;
                    //            paddedReplacementA = replacementStringCased(originalCut, repstr, op)
                    //                + new string(padchar, padlen);
                    //            retStr = binAStr.Substring(0, p) + paddedReplacementA + binAStr.Substring(p + findstr.Length);
                    //            retBin = System.Text.Encoding.ASCII.GetBytes(retStr);
                    //        }
                    //    //}
                    //    //else
                    //    //{
                    //    //    paddedReplacementA = replacementStringCased(originalCut, repstr, op);
                    //    //    retStr = binAStr.Substring(0, p) + paddedReplacementA + binAStr.Substring(p + findstr.Length);
                    //    //    retBin = System.Text.Encoding.ASCII.GetBytes(retStr);
                    //    //}
                    //
                    //    //foundBinaryMatch = true;
                    //    //retStr = binAStr.Substring(0, p) + repstr + binAStr.Substring(p + findstr.Length);
                    //
                    //    //int retlen = dataBin.Length + (paddedReplacementA.Length - findstr.Length);
                    //    //retStr = binAStr.Substring(0, p) + paddedReplacementA + binAStr.Substring(p + findstr.Length);
                    //
                    //    //retBin = new byte[retStr.Length];
                    //    //// TODO: What if they're using some wierd higher characters that use multiple word encoding? The length will be different.
                    //    //for (int i = 0; i < retStr.Length; i++)
                    //    //{
                    //    //    retBin[i] = (byte)retStr[i];
                    //    //}
                    //
                    //    //retBin = System.Text.Encoding.ASCII.GetBytes(retStr);
                    //    //while (retBin.Length > dataBin.Length + findstr.Length - repstr.Length)

                    //foreach (int p in dataBin.IndexesOf(op.searchStringBinW))


//#if DEBUG
//                    if (debugDumpBinaryReplacement)
//                        {
//                            dumpBin("DebugDump_doReplace_01_before.bin", dataBin);
//                            dumpBin("DebugDump_doReplace_02_after.bin", retBin);
//                        }
//#endif
                    //    return retBin;
                    //}
                    //binAStr = null;
                    //
                    //binWStr = binWCharToStringLittleEndian(dataBin);
                    //p = binWStr.IndexOf(findstr, op.stringCompareStyle);
                    //if (p >= 0)
                    //{
                    //    string paddedReplacementW;
                    //    originalCut = binWStr.Substring(p, findstr.Length);
                    //    //if (findstr.Length >= repstr.Length)
                    //    //{
                    //    int padlen = findstr.Length - repstr.Length;
                    //        padlen++;
                    //        do
                    //        {
                    //            padlen--;
                    //            //paddedReplacementW = repstr + new string(padchar, findstr.Length - repstr.Length);
                    //            paddedReplacementW = replacementStringCased(originalCut, repstr, op)
                    //                + new string(padchar, findstr.Length - repstr.Length);
                    //            retStr = binWStr.Substring(0, p) + paddedReplacementW + binWStr.Substring(p + findstr.Length);
                    //            //retBin = new byte[retStr.Length * 2];
                    //            retBin = System.Text.Encoding.Unicode.GetBytes(retStr);
                    //        } while ((padlen >= 0) && (retBin.Length > dataBin.Length));
                    //        while (retBin.Length < dataBin.Length)
                    //        {
                    //            padlen++;
                    //            paddedReplacementW = replacementStringCased(binWStr, repstr, op)
                    //                + new string(padchar, findstr.Length - repstr.Length);
                    //            retStr = binWStr.Substring(0, p) + paddedReplacementW + binWStr.Substring(p + findstr.Length);
                    //            //retBin = new byte[retStr.Length * 2];
                    //            retBin = System.Text.Encoding.Unicode.GetBytes(retStr);
                    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //    //paddedReplacementW = repstr;
                    //    //    paddedReplacementW = replacementStringCased(binWStr, repstr, op);
                    //    //    retStr = binWStr.Substring(0, p) + paddedReplacementW + binWStr.Substring(p + findstr.Length);
                    //    //    retBin = new byte[retStr.Length * 2];
                    //    //}
                    //
                    //
                    //    //retStr = binWStr.Substring(0, p) + repstr + binWStr.Substring(p + findstr.Length);
                    //    //retStr = binWStr.Substring(0, p) + paddedReplacementW + binWStr.Substring(p + findstr.Length);
                    //    //retBin = new byte[retStr.Length * 2];
                    //    //for (int i = 0; i < retStr.Length; i++)
                    //    //{
                    //    //    // little endian
                    //    //    retBin[i * 2] = (byte)(retStr[i] % 0x100);
                    //    //    retBin[(i * 2) + 1] = (byte)(retStr[i] / 0x100);
                    //    //}
//#if DEBUG
//                        if (debugDumpBinaryReplacement)
//                        {
//                            dumpBin("DebugDump_doReplace_01_before.bin", dataBin);
//                            dumpBin("DebugDump_doReplace_02_after.bin", retBin);
//                        }
//#endif
                    //    byte[] ba = new byte[100];
                    //    //Buffer.BlockCopy
                    //    //ba.SequenceEqual()
                    //    //ba.All
                    //    return retBin;
                    //}
                    return data;
                case RegistryValueKind.DWord:
                    return data;
                case RegistryValueKind.MultiString:
                    dataMultiStr = (string[])data;
                    retMultiStr = new string[dataMultiStr.Length];
                    for (int i = 0; i < dataMultiStr.Length; i++)
                    {
                        p = dataMultiStr[i].IndexOf(findstr, op.stringCompareStyle);
                        if (p >= 0)
                        {
                            //retMultiStr[i] = dataMultiStr[i].Substring(0, p) + repstr + dataMultiStr[i].Substring(p + findstr.Length);
                            retMultiStr[i] = dataMultiStr[i].Substring(0, p) 
                                + replacementStringCased(dataMultiStr[i], repstr, op)
                                + dataMultiStr[i].Substring(p + findstr.Length);
                        }
                        else
                        {
                            retMultiStr[i] = dataMultiStr[i];
                        }
                    }
                    return retMultiStr;
                case RegistryValueKind.QWord:
                    return data;
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.String:
                    dataStr = (string)data;
                    p = dataStr.IndexOf(findstr, op.stringCompareStyle);
                    if (p >= 0)
                    {
                        //retStr = dataStr.Substring(0, p) + repstr + dataStr.Substring(p + findstr.Length);
                        retStr = dataStr.Substring(0, p) 
                            + replacementStringCased(dataStr, repstr, op)
                            + dataStr.Substring(p + findstr.Length);
                        return retStr;
                    }
                    else
                    {
                        return dataStr;
                    }
                case RegistryValueKind.None:
                case RegistryValueKind.Unknown:
                default:
                    return null;
            }
        }

        string regKindStr(RegistryValueKind k)
        {
            switch (k)
            {
                case RegistryValueKind.Binary: return "Binary";
                case RegistryValueKind.DWord: return "DWord";
                case RegistryValueKind.ExpandString: return "ExpandString";
                case RegistryValueKind.MultiString: return "MultiString";
                case RegistryValueKind.None: return "None";
                case RegistryValueKind.QWord: return "QWord";
                case RegistryValueKind.String: return "String";
                case RegistryValueKind.Unknown:
                default:
                    return "Unknown";
            }
        }

        SearchOptions optionSnapshot()
        {
            //SearchOptionFlags r = new SearchOptionFlags();
            SearchOptions r = new SearchOptions();
            if (chkUser.Checked)
            {
                if (chkAllUsers.Checked) { r.flags |= SearchOptionFlags.HKU; }
                else { r.flags |= SearchOptionFlags.HKCU; }
            }
            if (chkData.Checked) { r.flags |= SearchOptionFlags.DATA; }
            if (chkHKCC.Checked) { r.flags |= SearchOptionFlags.HKCC; }
            if (chkHKCR.Checked) { r.flags |= SearchOptionFlags.HKCR; }
            if (chkHKLM.Checked) { r.flags |= SearchOptionFlags.HKLM; }
            if (chkHKPD.Checked) { r.flags |= SearchOptionFlags.HKPD; }
            if (chkKeys.Checked) { r.flags |= SearchOptionFlags.KEYS; }
            if (chkValues.Checked) { r.flags |= SearchOptionFlags.VALUES; }
            if (chkString.Checked) { r.flags |= SearchOptionFlags.STRING; }
            if (chkExpandString.Checked) { r.flags |= SearchOptionFlags.EXPANDSTRING; }
            if (chkMultiStr.Checked) { r.flags |= SearchOptionFlags.MULTISTRING; }
            if (chkBinary.Checked) { r.flags |= SearchOptionFlags.BINARY; }
            if (chkCaseSensitive.Checked) { r.flags |= SearchOptionFlags.CASE_SENSITIVE; }
            if (chkMatchResultCase.Checked) { r.flags |= SearchOptionFlags.MATCH_RESULT_CASE; }

            switch(cmbStringCompareStyle.SelectedIndex)
            {
                case 0:
                    r.culture = System.Globalization.CultureInfo.CurrentCulture;
                    if (chkCaseSensitive.Checked) { r.stringCompareStyle = StringComparison.CurrentCulture; }
                    else { r.stringCompareStyle = StringComparison.CurrentCultureIgnoreCase; }
                    break;
                case 1:
                    r.culture = System.Globalization.CultureInfo.InvariantCulture;
                    if (chkCaseSensitive.Checked) { r.stringCompareStyle = StringComparison.InvariantCulture; }
                    else { r.stringCompareStyle = StringComparison.InvariantCultureIgnoreCase; }
                    break;
                case 2:
                    r.culture = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture;
                    if (chkCaseSensitive.Checked) { r.stringCompareStyle = StringComparison.Ordinal; }
                    else { r.stringCompareStyle = StringComparison.OrdinalIgnoreCase; }
                    break;
            }

            if (chkUseRootPath.Checked)
            {
                if (txtRootPath.Text.Substring(txtRootPath.Text.Length - 1) == "\\")
                {
                    r.rootPath = txtRootPath.Text.Substring(0, txtRootPath.Text.Length - 1);
                }
                else { r.rootPath = txtRootPath.Text; }

            }
            else { r.rootPath = ""; }

            r.searchString = txtFind.Text;
            r.replacementString = txtReplaceWith.Text;
            //if (chkBinary.Checked)
            if (r.flags.HasFlag(SearchOptionFlags.BINARY))
            {
                string padCharStr = new string(padchar, 1);
                byte[] padBinA = System.Text.Encoding.ASCII.GetBytes(padCharStr);
                byte[] padBinW = System.Text.Encoding.Unicode.GetBytes(padCharStr);
                r.searchStringBinA      = System.Text.Encoding.ASCII.GetBytes(r.searchString);
                r.replacementStringBinA = padNoTruncate(System.Text.Encoding.ASCII.GetBytes(r.replacementString), padBinA, r.searchStringBinA.Length);
                r.replacementStringUpperBinA = padNoTruncate(System.Text.Encoding.ASCII.GetBytes(r.replacementString.ToUpper(r.culture)), padBinA, r.searchStringBinA.Length);
                r.replacementStringLowerBinA = padNoTruncate(System.Text.Encoding.ASCII.GetBytes(r.replacementString.ToLower(r.culture)), padBinA, r.searchStringBinA.Length);

                r.searchStringBinW      = System.Text.Encoding.Unicode.GetBytes(r.searchString);
                r.replacementStringBinW = padNoTruncate(System.Text.Encoding.Unicode.GetBytes(r.replacementString), padBinW, r.searchStringBinW.Length);
                r.replacementStringUpperBinW = padNoTruncate(System.Text.Encoding.Unicode.GetBytes(r.replacementString.ToUpper(r.culture)), padBinW, r.searchStringBinW.Length);
                r.replacementStringLowerBinW = padNoTruncate(System.Text.Encoding.Unicode.GetBytes(r.replacementString.ToLower(r.culture)), padBinW, r.searchStringBinW.Length);
            }

            return r;
        }

        byte[] padNoTruncate(byte[] data, byte[] padding, int targetLength)
        {
            if (data.Length >= targetLength) { return data; }
            byte[] ret = new byte[targetLength];
            int copyLen = data.Length;
            if (targetLength < copyLen) { copyLen = targetLength; }
            Buffer.BlockCopy(data, 0, ret, 0, copyLen);
            int m = 0;
            for (int i = copyLen; i < targetLength; i++)
            {
                ret[i] = padding[m];
                m = (m + 1) % padding.Length;
            }
            return ret;
        }

        RegistryKey OpenKey(string path, SearchOptions op)
        {
            int p = path.IndexOf("\\");
            string hiveName;
            RegistryKey hive = null;
            string subKeyPath;
            if (p == -1)
            {
                hiveName = path.ToUpper(op.culture);
                //hiveName = path.ToUpper();
                subKeyPath = "";
            }
            else
            {
                hiveName = path.Substring(0, p).ToUpper(op.culture);
                subKeyPath = path.Substring(p + 1);
            }

            if (
                hiveName == "HKCR"
             || hiveName == "HKCR:"
             || hiveName == "HKEY_CLASSES_ROOT"
             || hiveName == "HKEY_CLASSES_ROOT:"
             || hiveName == "HKEY_CLASSESROOT"
             || hiveName == "HKEY_CLASSESROOT:"
                ) { hive = Registry.ClassesRoot; }
            else if (
                hiveName == "HKCU"
             || hiveName == "HKCU:"
             || hiveName == "HKEY_CURRENT_USER"
             || hiveName == "HKEY_CURRENT_USER:"
             || hiveName == "HKEY_CURRENTUSER"
             || hiveName == "HKEY_CURRENTUSER:"
                ) { hive = Registry.CurrentUser; }
            else if (
                hiveName == "HKU"
             || hiveName == "HKU:"
             || hiveName == "HKEY_USERS"
             || hiveName == "HKEY_USERS:"
                ) { hive = Registry.Users; }
            else if (
                hiveName == "HKLM"
             || hiveName == "HKLM:"
             || hiveName == "HKEY_LOCAL_MACHINE"
             || hiveName == "HKEY_LOCAL_MACHINE:"
             || hiveName == "HKEY_LOCALMACHINE"
             || hiveName == "HKEY_LOCALMACHINE:"
                ) { hive = Registry.LocalMachine; }
            else if (
                hiveName == "HKCC"
             || hiveName == "HKCC:"
             || hiveName == "HKEY_CURRENT_CONFIG"
             || hiveName == "HKEY_CURRENT_CONFIG:"
             || hiveName == "HKEY_CURRENTCONFIG"
             || hiveName == "HKEY_CURRENTCONFIG:"
                ) { hive = Registry.CurrentConfig; }
            else if (
                hiveName == "HKPD"
             || hiveName == "HKPD:"
             || hiveName == "HKEY_PERFORMANCE_DATA"
             || hiveName == "HKEY_PERFORMANCE_DATA:"
             || hiveName == "HKEY_PERFORMANCEDATA"
             || hiveName == "HKEY_PERFORMANCEDATA:"
                ) { hive = Registry.PerformanceData; }
            else if (
                hiveName == "HKCC"
             || hiveName == "HKCC:"
             || hiveName == "HKEY_CURRENT_CONFIG"
             || hiveName == "HKEY_CURRENT_CONFIG:"
             || hiveName == "HKEY_CURRENTCONFIG"
             || hiveName == "HKEY_CURRENTCONFIG:"
                ) { hive = Registry.CurrentConfig; }
            //// Dynamic data is deprecated win9x legacy not supported by clr
            //else if (
            //    hiveName == "HKDD"
            // || hiveName == "HKDD:"
            // || hiveName == "HKEY_DYNAMIC_DATA"
            // || hiveName == "HKEY_DYNAMIC_DATA:"
            // || hiveName == "HKEY_DYNAMICDATA"
            // || hiveName == "HKEY_DYNAMICDATA:"
            //    ) { hive = Registry.DynData; }
            else { return null; }
            return hive.OpenSubKey(subKeyPath);
        }

        void regfind(string searchString, SearchOptions op, MatchHandler handler)
        {
            RegistryKey HKCR = Registry.ClassesRoot;
            RegistryKey HKCU = Registry.CurrentUser;
            RegistryKey HKLM = Registry.LocalMachine;
            RegistryKey HKU = Registry.Users;
            RegistryKey HKCC = Registry.CurrentConfig;
            //RegistryKey HKDD = Registry.DynData;
            RegistryKey HKPD = Registry.PerformanceData;


            //if (op.flags.HasFlag(SearchOptions.HKU)) { helper_regfind(HKU, "HKEY_USERS", searchString, op, handler); }
            //else if (op.flags.HasFlag(SearchOptions.HKCU)) { helper_regfind(HKCU, "HKEY_CURRENT_USER", searchString, op, handler); }
            //if (op.flags.HasFlag(SearchOptions.HKCR)) { helper_regfind(HKCR, "HKEY_CLASSES_ROOT", searchString, op, handler); }
            //if (op.flags.HasFlag(SearchOptions.HKLM)) { helper_regfind(HKLM, "HKEY_LOCAL_MACHINE", searchString, op, handler); }
            //if (op.flags.HasFlag(SearchOptions.HKCC)) { helper_regfind(HKCC, "HKEY_CURRENT_CONFIG", searchString, op, handler); }
            //if (op.flags.HasFlag(SearchOptions.HKPD)) { helper_regfind(HKPD, "HKEY_PERFORMANCE_DATA", searchString, op, handler); }

            if (op.rootPath == "")
            {
                if (op.flags.HasFlag(SearchOptionFlags.HKU)) { helper_regfind(HKU, "HKU:", searchString, op, handler); }
                else if
                    (op.flags.HasFlag(SearchOptionFlags.HKCU)) { helper_regfind(HKCU, "HKCU:", searchString, op, handler); }
                if (op.flags.HasFlag(SearchOptionFlags.HKCR)) { helper_regfind(HKCR, "HKCR:", searchString, op, handler); }
                if (op.flags.HasFlag(SearchOptionFlags.HKLM)) { helper_regfind(HKLM, "HKLM:", searchString, op, handler); }
                if (op.flags.HasFlag(SearchOptionFlags.HKCC)) { helper_regfind(HKCC, "HKCC:", searchString, op, handler); }
                if (op.flags.HasFlag(SearchOptionFlags.HKPD)) { helper_regfind(HKPD, "HKPD:", searchString, op, handler); }
            }
            else
            {
                RegistryKey root = OpenKey(txtRootPath.Text, op);
                if (root == null) { throw new FileNotFoundException("Root path not found."); }
                helper_regfind(root, op.rootPath, searchString, op, handler);
            }
        }

        void helper_regfind(RegistryKey key, string path, string searchString, SearchOptions op, MatchHandler handler)
        {
            if (m_userCancel) { return; }
            TimeSpan timeMonitor;
            //txtCurrentLocation.Text = path;
            //txtCurrentLocation.Refresh();

            RegistryValueKind vt;
            MatchTypeEnum matchFlags = new MatchTypeEnum();
            MatchTypeEnum defaultMatchFlags = new MatchTypeEnum();
            //object valueDataObj;
            if (op.flags.HasFlag(SearchOptionFlags.DATA))
            {
                object defaultDataObj = key.GetValue(null, null, RegistryValueOptions.DoNotExpandEnvironmentNames); // TODO: option to expand? search for others DoNotExpandEnvironmentNames
                if (defaultDataObj != null)
                {
                    RegistryValueKind dvt = key.GetValueKind(null);
                    //    if (dvt.GetTypeCode() == TypeCode.String)
                    //    {
                    //        //if (defaultDataObj.GetType().Name)
                    //        string defaultData = (string)defaultDataObj;
                    //        if (defaultData.IndexOf(searchString) >= 0)
                    //        {
                    //            handler(path, "@", defaultDataObj, dvt, MatchTypeEnum.DATA);
                    //        }
                    //    }
                    bool handleDefault = false;
                    //string garbage = "";
                    //defaultMatchFlags = new MatchTypeEnum();
                    //scanData(key, searchString, dvt, ref defaultMatchFlags, "(default)", ref handleDefault, ref garbage, op);
                    scanData(key, searchString, dvt, ref defaultMatchFlags, "(default)", ref handleDefault, op);
                    if (handleDefault)
                    {
                        defaultMatchFlags |= MatchTypeEnum.DEFAULT;
                        handler(path, DefaultRegistryValueSentinelStringConstant, dvt, defaultMatchFlags);
                    }
                }
            }

            if (op.flags.HasFlag(SearchOptionFlags.VALUES) || op.flags.HasFlag(SearchOptionFlags.DATA))
            {
                string[] valueNames = key.GetValueNames();
                foreach (string valueName in valueNames)
                {
                    // Check event queue for user intervention
                    timeMonitor = m_runningTime.Elapsed;
                    if (timeMonitor > m_lastRefreshTime + m_refreshInterval)
                    {
                        m_lastRefreshTime = timeMonitor;
                        txtCurrentLocation.Text = path;
                        Application.DoEvents();
                        if (m_userCancel)
                        {
                            txtResult.Text += "Canceled by user.\r\n";
                            return;
                        }
                    }
                    if (m_userCancel) { return; }

                    bool handleIt = false;
                    //bool objFetched = false;
                    //string valueDataStr = "";
                    vt = key.GetValueKind(valueName);
                    //valueDataObj = null;
                    //valueDataObj = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    if (op.flags.HasFlag(SearchOptionFlags.VALUES))
                    {
                        if (valueName.IndexOf(searchString, op.stringCompareStyle) >= 0)
                        {
                            handleIt = true;
                            matchFlags |= MatchTypeEnum.VALUE;
                            //valueDataStr = ((string)key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames));                            
                            //if (valueDataStr != null)
                            //{
                            //    handleIt = true;
                            //    matchFlags |= MatchTypeEnum.VALUE;
                            //    //valueDataObj = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                            //    //objFetched = true;
                            //}
                        }
                    }
                    if (op.flags.HasFlag(SearchOptionFlags.DATA))
                    {
                        //scanData(key, searchString, vt, ref matchFlags, valueName, ref handleIt, ref valueDataStr, op);
                        scanData(key, searchString, vt, ref matchFlags, valueName, ref handleIt, op);
                        //if (handleIt)
                        //{
                        //    valueDataObj = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        //    objFetched = true;
                        //}
                    }
                    if (handleIt)
                    {
                        //if (!objFetched) { valueDataObj = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames); }
                        //handler(path, valueName, valueDataObj, vt, matchFlags);
                        handler(path, valueName, vt, matchFlags);
                    }

                }
            }

            string[] subKeyNames;
            try
            {
                subKeyNames = key.GetSubKeyNames();
            }
            catch // (Exception e)
            {
                return;
            }
            foreach (string subKeyName in subKeyNames)
            {
                // Check event queue for user intervention
                timeMonitor = m_runningTime.Elapsed;
                if (timeMonitor > m_lastRefreshTime + m_refreshInterval)
                {
                    m_lastRefreshTime = timeMonitor;
                    txtCurrentLocation.Text = path;
                    Application.DoEvents();
                    if (m_userCancel)
                    {
                        txtResult.Text += "Canceled by user.\r\n";
                        return;
                    }
                }
                if (m_userCancel) { return; }

                RegistryKey subkey;
                if (subKeyName.IndexOf(searchString, op.stringCompareStyle) >= 0)
                {
                    //handler(path + "\\" + subKeyName, subKeyName, null, RegistryValueKind.None, MatchTypeEnum.KEY);
                    handler(path + "\\" + subKeyName, subKeyName, RegistryValueKind.None, MatchTypeEnum.KEY);
                }
                try
                {
                    subkey = key.OpenSubKey(subKeyName);
                }
                catch //(Exception e)
                {
                    continue;
                }
                if (subkey == null) { continue; }
                helper_regfind(subkey, path + "\\" + subKeyName, searchString, op, handler);
                if (m_userCancel) { return; }
                subkey.Close();
            }
        }

        //private void scanData(RegistryKey key, string searchString, RegistryValueKind vt, ref MatchTypeEnum matchFlags, string valueName, ref bool handleIt, ref string valueDataStr, SearchOptions op)
        private void scanData(RegistryKey key, string searchString, RegistryValueKind vt, ref MatchTypeEnum matchFlags, string valueName, ref bool handleIt, SearchOptions op)
        {
            string valueDataStr;
            if (((vt == RegistryValueKind.String) && (op.flags.HasFlag(SearchOptionFlags.STRING))) || ((vt == RegistryValueKind.ExpandString) && (op.flags.HasFlag(SearchOptionFlags.EXPANDSTRING))))
            {
                valueDataStr = ((string)key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames));
                if (valueDataStr != null)
                {
                    if (valueDataStr.IndexOf(searchString, op.stringCompareStyle) >= 0)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                    }
                }
            }
            else if ((vt == RegistryValueKind.MultiString) && (op.flags.HasFlag(SearchOptionFlags.MULTISTRING)))
            {
                string[] dataMultiStr = (string[])key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                foreach (string s in dataMultiStr)
                {
                    if (s.IndexOf(searchString, op.stringCompareStyle) >= 0)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                        break;
                    }
                }
            }
            else if ((vt == RegistryValueKind.Binary) && (op.flags.HasFlag(SearchOptionFlags.BINARY)))
            {
                //string astr, wstr;
                byte[] binData = (byte[])key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (op.flags.HasFlag(SearchOptionFlags.CASE_SENSITIVE))
                {
                    if (binData.Length < op.searchStringBinA.Length) { return; }
                    if (binData.IndexOf(op.searchStringBinA) != -1)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                        matchFlags |= MatchTypeEnum.ASCII;
                        //return;
                    }

                    if (binData.Length < op.searchStringBinW.Length) { return; }
                    if (binData.IndexOf(op.searchStringBinW) != -1)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                        matchFlags |= MatchTypeEnum.UNICODE;
                        //return;
                    }
                }
                else
                {
                    // Not case sensitive
                    //var la = binData.IndexesOfNoCase(op.searchString, System.Text.Encoding.ASCII, op.culture);
                    //var lu = binData.IndexesOfNoCase(op.searchString, System.Text.Encoding.Unicode, op.culture);
                    //if (la.Count >= 1)
                    if (binData.IndexOfNoCase(op.searchString, System.Text.Encoding.ASCII) != -1)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                        matchFlags |= MatchTypeEnum.ASCII;
                        //return;
                    }
                    //if (lu.Count >= 1)
                    if (binData.IndexOfNoCase(op.searchString, System.Text.Encoding.Unicode) != -1)
                    {
                        handleIt = true;
                        matchFlags |= MatchTypeEnum.DATA;
                        matchFlags |= MatchTypeEnum.UNICODE;
                        //return;
                    }
                }
                //if (found) { return; }
                //byte[] searchStringEncodedW = System.Text.Encoding.Unicode.GetBytes(searchString);

                //astr = binAsciiToString(binData);
                //wstr = binWCharToStringLittleEndian(binData);
                //if ((astr.IndexOf(searchString, op.stringCompareStyle) >= 0) || (wstr.IndexOf(searchString, op.stringCompareStyle) >= 0))
                //{
                //    handleIt = true;
                //    matchFlags |= MatchTypeEnum.DATA;
                //}


                //binData.SequenceEqual()

            }
        }

        private void chkHKU_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUser.Checked) { chkAllUsers.Enabled = true; }
            else { chkAllUsers.Enabled = false; }
        }

        string exportStr(RegistryValueKind kind, object data)
        {
            string ret = "";
            //string dataStr;
            string[] dataMultiStr;
            byte[] binData;
            //UInt32Converter convDWORD = new UInt32Converter();
            //UInt64Converter convQWORD = new UInt64Converter();
            switch (kind)
            {
                case RegistryValueKind.Binary:
                    //dataStr = (string)data;
                    //ret = "hex:";
                    //for (p = 0; p < dataStr.Length; p++)
                    //{
                    //
                    //}
                    binData = (byte[])data;
                    ret = "([byte[]](" + hexifyPS(binData) + "))";
                    break;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    //ret = convDWORD.ConvertToString(data);
                    TypeDescriptor.GetConverter(ret).ConvertFrom(data);
                    break;
                    //ret = convQWORD.ConvertToString(data);
                    //break;
                case RegistryValueKind.ExpandString:
                    //ret = "hex(2):";
                    ret = "\"" + escapeString((string)data, "\"", "`") + "\"";
                    break;
                case RegistryValueKind.MultiString:
                    //ret = "hex(7):";
                    dataMultiStr = (string[])data;
                    ret = "([string[]](";
                    for (int n = 0; n < dataMultiStr.Length; n++)
                    {
                        if (n != 0) { ret += ","; }
                        ret += "\"";
                        ret += escapeString(dataMultiStr[n], "\"", "`");
                        ret += "\"";
                    }
                    ret += "))";
                    break;
                case RegistryValueKind.None:
                    ret = "";
                    break;
                case RegistryValueKind.String:
                    ret = "\"" + escapeString((string)data, "\"", "`") + "\"";
                    break;
                case RegistryValueKind.Unknown:
                    break;
            }
            return ret;
        }

        string escapeString(string data, string toEscape, string prepend)
        {
            string ret = "";
            int p = -1;
            int prev = 0;
            do
            {
                prev = p;
                p = data.IndexOf(toEscape, p + 1);
                if (p >= 0)
                {
                    ret += data.Substring(prev + 1, p - prev - 1);
                    ret += prepend + toEscape;
                }
            } while (p >= 0);
            if (prev < data.Length - 1)
            {
                ret += data.Substring(prev + 1, data.Length - prev - 1);
            }
            return ret;
        }

        string hexifyPS(byte[] data)
        {
            byte[] bHexChars =
            {
                (byte)'0',
                (byte)'1',
                (byte)'2',
                (byte)'3',
                (byte)'4',
                (byte)'5',
                (byte)'6',
                (byte)'7',
                (byte)'8',
                (byte)'9',
                (byte)'A',
                (byte)'B',
                (byte)'C',
                (byte)'D',
                (byte)'E',
                (byte)'F'
            };
            string ret;
            int p;

            byte[] bret = new byte[(data.Length * 5)];

            for (p = 0; p < data.Length; p++)
            {
                int p5 = p * 5;
                if (p != 0) { bret[(p5) - 1] = (byte)(','); }
                bret[p5] = (byte)('0');
                bret[p5 + 1] = (byte)('x');
                bret[p5 + 2] = bHexChars[(data[p] >> 4) | 0xF];
                bret[p5 + 3] = bHexChars[data[p] & 0xF];
                bret[p5 + 4] = 32;
            }
            ret = Encoding.ASCII.GetString(bret);
            return ret;

            //const string hexchars = "0123456789ABCDEF";
            //ret = "";
            //int c, u, l;
            //for (p = 0; p < data.Length; p++)
            //{
            //    if (p != 0) { ret += ","; }
            //    c = data[p];
            //    u = (c & 0xF0) >> 4;
            //    l = (c & 0xF);
            //    ret += "0x";
            //    ret += hexchars[u];
            //    ret += hexchars[l];
            //}
            //return ret;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            txtCurrentLocation.Width = this.ClientRectangle.Width - (txtCurrentLocation.Left * 2);
            txtResult.Width = this.ClientRectangle.Width - (txtResult.Left * 2);
            txtResult.Height = this.ClientRectangle.Height - txtResult.Top - txtResult.Left;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbStringCompareStyle.SelectedIndex = 0;
            //txtPadChar.Text = new string(padchar, 1);
        }

        private void chkUseRootPath_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUseRootPath.Checked)
            {
                chkUser.Enabled = false;
                chkAllUsers.Enabled = false;
                chkHKCC.Enabled = false;
                chkHKCR.Enabled = false;
                chkHKLM.Enabled = false;
                chkHKPD.Enabled = false;
            }
            else
            {
                chkUser.Enabled = true;
                chkAllUsers.Enabled = true;
                chkHKCC.Enabled = true;
                chkHKCR.Enabled = true;
                chkHKLM.Enabled = true;
                chkHKPD.Enabled = true;

            }
        }

        private void txtRootPath_TextChanged(object sender, EventArgs e)
        {
            if (txtRootPath.Text == "") { chkUseRootPath.Checked = false; }
            else { chkUseRootPath.Checked = true; }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_userCancel = true;
            btnStop.Enabled = false;
            btnDump.Enabled = true;
        }
    }


    public static class Extensions
    {
        /// <summary>
        /// Searches in the haystack array for the given needle using the default equality operator and returns the index at which the needle starts.
        /// </summary>
        /// <typeparam name="T">Type of the arrays.</typeparam>
        /// <param name="haystack">Sequence to operate on.</param>
        /// <param name="needle">Sequence to search for.</param>
        /// <returns>Index of the needle within the haystack or -1 if the needle isn't contained.</returns>
        public static IEnumerable<int> IndexOfLinq<T>(this T[] haystack, T[] needle)
        {
            if ((needle != null) && (haystack.Length >= needle.Length))
            {
                for (int l = 0; l < haystack.Length - needle.Length + 1; l++)
                {
                    if (!needle.Where((data, index) => !haystack[l + index].Equals(data)).Any())
                    {
                        yield return l;
                    }
                }
            }
        }

        public static unsafe long IndexOf(this byte[] Haystack, byte[] Needle)
        {
            fixed (byte* H = Haystack) fixed (byte* N = Needle)
            {
                long i = 0;
                //for (byte* hNext = H, hEnd = H + Haystack.LongLength; hNext < hEnd; i++, hNext++)
                for (byte* hNext = H, hEnd = H + Haystack.LongLength - Needle.LongLength; hNext < hEnd; i++, hNext++)
                {
                    bool Found = true;
                    for (byte* hInc = hNext, nInc = N, nEnd = N + Needle.LongLength; Found && nInc < nEnd; Found = *nInc == *hInc, nInc++, hInc++) ;
                    if (Found) return i;
                }
                return -1;
            }
        }
        public static unsafe List<long> IndexesOf(this byte[] Haystack, byte[] Needle)
        {
            List<long> Indexes = new List<long>();
            fixed (byte* H = Haystack) fixed (byte* N = Needle)
            {
                long i = 0;
                //for (byte* hNext = H, hEnd = H + Haystack.LongLength; hNext < hEnd; i++, hNext++)
                for (byte* hNext = H, hEnd = H + Haystack.LongLength - Needle.LongLength; hNext < hEnd; i++, hNext++)
                {
                    bool Found = true;
                    for (byte* hInc = hNext, nInc = N, nEnd = N + Needle.LongLength; Found && nInc < nEnd; Found = *nInc == *hInc, nInc++, hInc++) ;
                    if (Found) Indexes.Add(i);
                }
                return Indexes;
            }
        }
        public static unsafe long IndexOfNoCase(this byte[] haystack, string needle, System.Text.Encoding enc, System.Globalization.CultureInfo ci)
        {
            return CaseInsensitiveSearchHelper(haystack, needle, enc, ci, null);
        }
        public static unsafe List<long> IndexesOfNoCase(this byte[] haystack, string needle, System.Text.Encoding enc, System.Globalization.CultureInfo ci)
        {
            var ret = new List<long>();
            CaseInsensitiveSearchHelper(haystack, needle, enc, ci, ret);
            return ret;
        }
        public static unsafe long CaseInsensitiveSearchHelper(byte[] haystack, string needle, System.Text.Encoding enc, System.Globalization.CultureInfo ci, List<long> ret = null)
        {
            if (haystack == null) { return -1; }
            if (needle == null || needle == "") { return 0; }
            if (ret != null) { ret.Clear(); }
            //List<long> ret = new List<long>();

            // Build a pattern array for upper and lower case binary data for each char
            // pat[letter][0=Lower,1=Upper][ByteOfs]
            byte[][][] pat = new byte[needle.Length][][];
            for (int si = 0; si < needle.Length; si++)
            {
                pat[si] = new byte[2][];
                string curLetter = new string(needle[si], 1);
                //pat[si][0] = System.Text.Encoding.Unicode.GetBytes(curLetter.ToLower(ci));
                //pat[si][1] = System.Text.Encoding.Unicode.GetBytes(curLetter.ToUpper(ci));
                pat[si][0] = enc.GetBytes(curLetter.ToLower(ci));
                pat[si][1] = enc.GetBytes(curLetter.ToUpper(ci));
            }
            // End build pattern array

            fixed (byte* H = haystack)  //fixed (byte* N = needle)
            {
                bool wordMatch;
                bool upperLetterMatch, lowerLetterMatch;
                byte* wordStart = H;
                byte* curByte;
                byte* letterStart;
                for (long srcStartOfs = 0; srcStartOfs < haystack.LongLength; srcStartOfs++)
                {
                    letterStart = wordStart;
                    wordMatch = true;
                    for (int wordOfs = 0; wordOfs < needle.Length; wordOfs++)
                    {
                        // This loop advances letterStart
                        if (letterStart < H || letterStart >= H + haystack.Length)
                        {
                            // pointer overflow check
                            wordMatch = false;
                            break;
                        }

                        lowerLetterMatch = true;
                        curByte = letterStart;
                        for (int l = 0; l < pat[wordOfs][0].Length; l++)
                        {
                            // This loop also iterates curLetter++ at the end
                            if (curByte >= H && curByte < H + haystack.Length)
                            {
                                if (*curByte != pat[wordOfs][0][l])
                                {
                                    lowerLetterMatch = false;
                                    break;
                                }
                                curByte = curByte + 1; // Not using ++ for a reason, pointers lol
                            }
                            else
                            {
                                // pointer overflow
                                lowerLetterMatch = false;
                                break;
                            }
                        }
                        if (lowerLetterMatch)
                        {
                            letterStart += pat[wordOfs][0].Length;
                            //hasLower = true;
                            continue;
                        }

                        upperLetterMatch = true;
                        curByte = letterStart;
                        for (int u = 0; u < pat[wordOfs][1].Length; u++)
                        {
                            if (curByte >= H && curByte < H + haystack.Length)
                            {
                                if (*curByte != pat[wordOfs][1][u])
                                {
                                    upperLetterMatch = false;
                                    break;
                                }
                                curByte = curByte + 1; // Not using ++ for a reason, pointers lol
                            }
                            else
                            {
                                // pointer overflow
                                upperLetterMatch = false;
                                break;
                            }
                            
                        }
                        if (upperLetterMatch)
                        {
                            //hasUpper = true;
                            letterStart += pat[wordOfs][1].Length;
                        }
                        else
                        {
                            wordMatch = false;
                            break;
                        }

                        //letterStart++;
                    }
                    if (wordMatch)
                    {
                        if (ret == null) { return srcStartOfs; }
                        ret.Add(srcStartOfs);
                    }
                    wordStart = wordStart + 1; // pointer ++ adds 4 not 1
                }
            }
            if (ret == null)
            {
                return -1;
            }
            else
            {
                return ret.Count;
                //return ret;
            }
        }
        public static unsafe List<long> IndexesOfNoCase(this byte[] haystack, string needle)
        {
            return haystack.IndexesOfNoCase(needle, System.Text.Encoding.ASCII, System.Globalization.CultureInfo.CurrentCulture);
        }
        public static unsafe List<long> IndexesOfNoCase(this byte[] haystack, string needle, System.Text.Encoding enc)
        {
            return haystack.IndexesOfNoCase(needle, enc, System.Globalization.CultureInfo.CurrentCulture);
        }
        public static unsafe long IndexOfNoCase(this byte[] haystack, string needle, System.Text.Encoding enc)
        {
            return haystack.IndexOfNoCase(needle, enc, System.Globalization.CultureInfo.CurrentCulture);
        }
        public static unsafe long IndexOfNoCase(this byte[] haystack, string needle)
        {
            return haystack.IndexOfNoCase(needle, System.Text.Encoding.ASCII, System.Globalization.CultureInfo.CurrentCulture);
        }
    }


}
