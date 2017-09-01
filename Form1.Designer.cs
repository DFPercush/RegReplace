namespace RegReplace
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.txtReplaceWith = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUndoFile = new System.Windows.Forms.TextBox();
            this.txtRedoFile = new System.Windows.Forms.TextBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnReplaceFolder = new System.Windows.Forms.Button();
            this.btnFindFolder = new System.Windows.Forms.Button();
            this.btnRedoFileBrowse = new System.Windows.Forms.Button();
            this.btnUndoFileBrowse = new System.Windows.Forms.Button();
            this.btnReplaceFile = new System.Windows.Forms.Button();
            this.btnFindFile = new System.Windows.Forms.Button();
            this.ttBrowse = new System.Windows.Forms.ToolTip(this.components);
            this.btnDump = new System.Windows.Forms.Button();
            this.chkAllUsers = new System.Windows.Forms.CheckBox();
            this.chkKeys = new System.Windows.Forms.CheckBox();
            this.chkValues = new System.Windows.Forms.CheckBox();
            this.chkData = new System.Windows.Forms.CheckBox();
            this.chkUser = new System.Windows.Forms.CheckBox();
            this.chkHKLM = new System.Windows.Forms.CheckBox();
            this.chkHKCR = new System.Windows.Forms.CheckBox();
            this.chkHKCC = new System.Windows.Forms.CheckBox();
            this.chkHKPD = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkString = new System.Windows.Forms.CheckBox();
            this.chkMultiStr = new System.Windows.Forms.CheckBox();
            this.chkExpandString = new System.Windows.Forms.CheckBox();
            this.chkBinary = new System.Windows.Forms.CheckBox();
            this.txtCurrentLocation = new System.Windows.Forms.TextBox();
            this.chkCaseSensitive = new System.Windows.Forms.CheckBox();
            this.cmbStringCompareStyle = new System.Windows.Forms.ComboBox();
            this.chkMatchResultCase = new System.Windows.Forms.CheckBox();
            this.chkUseRootPath = new System.Windows.Forms.CheckBox();
            this.txtRootPath = new System.Windows.Forms.TextBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(88, 18);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(412, 20);
            this.txtFind.TabIndex = 0;
            // 
            // txtReplaceWith
            // 
            this.txtReplaceWith.Location = new System.Drawing.Point(87, 70);
            this.txtReplaceWith.Name = "txtReplaceWith";
            this.txtReplaceWith.Size = new System.Drawing.Size(413, 20);
            this.txtReplaceWith.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Find";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Replace with";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 174);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Undo file";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Script file";
            // 
            // txtUndoFile
            // 
            this.txtUndoFile.Location = new System.Drawing.Point(87, 171);
            this.txtUndoFile.Name = "txtUndoFile";
            this.txtUndoFile.Size = new System.Drawing.Size(413, 20);
            this.txtUndoFile.TabIndex = 11;
            // 
            // txtRedoFile
            // 
            this.txtRedoFile.Location = new System.Drawing.Point(87, 136);
            this.txtRedoFile.Name = "txtRedoFile";
            this.txtRedoFile.Size = new System.Drawing.Size(413, 20);
            this.txtRedoFile.TabIndex = 9;
            // 
            // txtResult
            // 
            this.txtResult.AcceptsReturn = true;
            this.txtResult.HideSelection = false;
            this.txtResult.Location = new System.Drawing.Point(10, 450);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.Size = new System.Drawing.Size(562, 136);
            this.txtResult.TabIndex = 35;
            // 
            // btnReplaceFolder
            // 
            this.btnReplaceFolder.Image = global::RegReplace.Properties.Resources.folder;
            this.btnReplaceFolder.Location = new System.Drawing.Point(542, 63);
            this.btnReplaceFolder.Name = "btnReplaceFolder";
            this.btnReplaceFolder.Size = new System.Drawing.Size(30, 32);
            this.btnReplaceFolder.TabIndex = 7;
            this.ttBrowse.SetToolTip(this.btnReplaceFolder, "Browse for existing folder");
            this.btnReplaceFolder.UseVisualStyleBackColor = true;
            this.btnReplaceFolder.Click += new System.EventHandler(this.btnReplaceFolder_Click);
            // 
            // btnFindFolder
            // 
            this.btnFindFolder.Image = global::RegReplace.Properties.Resources.folder;
            this.btnFindFolder.Location = new System.Drawing.Point(542, 12);
            this.btnFindFolder.Name = "btnFindFolder";
            this.btnFindFolder.Size = new System.Drawing.Size(30, 32);
            this.btnFindFolder.TabIndex = 2;
            this.ttBrowse.SetToolTip(this.btnFindFolder, "Browse for existing folder");
            this.btnFindFolder.UseVisualStyleBackColor = true;
            this.btnFindFolder.Click += new System.EventHandler(this.btnFindFolder_Click);
            // 
            // btnRedoFileBrowse
            // 
            this.btnRedoFileBrowse.Image = global::RegReplace.Properties.Resources.floppy;
            this.btnRedoFileBrowse.Location = new System.Drawing.Point(506, 129);
            this.btnRedoFileBrowse.Name = "btnRedoFileBrowse";
            this.btnRedoFileBrowse.Size = new System.Drawing.Size(30, 32);
            this.btnRedoFileBrowse.TabIndex = 10;
            this.ttBrowse.SetToolTip(this.btnRedoFileBrowse, "Browse for output file");
            this.btnRedoFileBrowse.UseVisualStyleBackColor = true;
            this.btnRedoFileBrowse.Click += new System.EventHandler(this.btnRedoFileBrowse_Click);
            // 
            // btnUndoFileBrowse
            // 
            this.btnUndoFileBrowse.Image = global::RegReplace.Properties.Resources.floppy;
            this.btnUndoFileBrowse.Location = new System.Drawing.Point(506, 164);
            this.btnUndoFileBrowse.Name = "btnUndoFileBrowse";
            this.btnUndoFileBrowse.Size = new System.Drawing.Size(30, 32);
            this.btnUndoFileBrowse.TabIndex = 12;
            this.ttBrowse.SetToolTip(this.btnUndoFileBrowse, "Browse for output file");
            this.btnUndoFileBrowse.UseVisualStyleBackColor = true;
            this.btnUndoFileBrowse.Click += new System.EventHandler(this.btnUndoFileBrowse_Click);
            // 
            // btnReplaceFile
            // 
            this.btnReplaceFile.Image = global::RegReplace.Properties.Resources.TextFile;
            this.btnReplaceFile.Location = new System.Drawing.Point(506, 63);
            this.btnReplaceFile.Name = "btnReplaceFile";
            this.btnReplaceFile.Size = new System.Drawing.Size(30, 32);
            this.btnReplaceFile.TabIndex = 6;
            this.ttBrowse.SetToolTip(this.btnReplaceFile, "Browse for existing file");
            this.btnReplaceFile.UseVisualStyleBackColor = true;
            this.btnReplaceFile.Click += new System.EventHandler(this.btnReplaceFile_Click);
            // 
            // btnFindFile
            // 
            this.btnFindFile.Image = global::RegReplace.Properties.Resources.TextFile;
            this.btnFindFile.Location = new System.Drawing.Point(506, 11);
            this.btnFindFile.Name = "btnFindFile";
            this.btnFindFile.Size = new System.Drawing.Size(30, 32);
            this.btnFindFile.TabIndex = 1;
            this.ttBrowse.SetToolTip(this.btnFindFile, "Browse for existing file");
            this.btnFindFile.UseVisualStyleBackColor = true;
            this.btnFindFile.Click += new System.EventHandler(this.btnFindFile_Click);
            // 
            // ttBrowse
            // 
            this.ttBrowse.ToolTipTitle = "Browse";
            // 
            // btnDump
            // 
            this.btnDump.Location = new System.Drawing.Point(441, 205);
            this.btnDump.Name = "btnDump";
            this.btnDump.Size = new System.Drawing.Size(95, 40);
            this.btnDump.TabIndex = 30;
            this.btnDump.Text = "Generate";
            this.btnDump.UseVisualStyleBackColor = true;
            this.btnDump.Click += new System.EventHandler(this.btnDump_Click);
            // 
            // chkAllUsers
            // 
            this.chkAllUsers.AutoSize = true;
            this.chkAllUsers.Checked = true;
            this.chkAllUsers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllUsers.Location = new System.Drawing.Point(33, 228);
            this.chkAllUsers.Name = "chkAllUsers";
            this.chkAllUsers.Size = new System.Drawing.Size(65, 17);
            this.chkAllUsers.TabIndex = 14;
            this.chkAllUsers.Text = "All users";
            this.chkAllUsers.UseVisualStyleBackColor = true;
            // 
            // chkKeys
            // 
            this.chkKeys.AutoSize = true;
            this.chkKeys.Checked = true;
            this.chkKeys.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKeys.Location = new System.Drawing.Point(117, 205);
            this.chkKeys.Name = "chkKeys";
            this.chkKeys.Size = new System.Drawing.Size(78, 17);
            this.chkKeys.TabIndex = 21;
            this.chkKeys.Text = "Key names";
            this.chkKeys.UseVisualStyleBackColor = true;
            // 
            // chkValues
            // 
            this.chkValues.AutoSize = true;
            this.chkValues.Checked = true;
            this.chkValues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkValues.Location = new System.Drawing.Point(117, 228);
            this.chkValues.Name = "chkValues";
            this.chkValues.Size = new System.Drawing.Size(87, 17);
            this.chkValues.TabIndex = 22;
            this.chkValues.Text = "Value names";
            this.chkValues.UseVisualStyleBackColor = true;
            // 
            // chkData
            // 
            this.chkData.AutoSize = true;
            this.chkData.Checked = true;
            this.chkData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkData.Location = new System.Drawing.Point(117, 251);
            this.chkData.Name = "chkData";
            this.chkData.Size = new System.Drawing.Size(49, 17);
            this.chkData.TabIndex = 23;
            this.chkData.Text = "Data";
            this.chkData.UseVisualStyleBackColor = true;
            // 
            // chkUser
            // 
            this.chkUser.AutoSize = true;
            this.chkUser.Checked = true;
            this.chkUser.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUser.Location = new System.Drawing.Point(16, 205);
            this.chkUser.Name = "chkUser";
            this.chkUser.Size = new System.Drawing.Size(48, 17);
            this.chkUser.TabIndex = 13;
            this.chkUser.Text = "User";
            this.chkUser.UseVisualStyleBackColor = true;
            this.chkUser.CheckedChanged += new System.EventHandler(this.chkHKU_CheckedChanged);
            // 
            // chkHKLM
            // 
            this.chkHKLM.AutoSize = true;
            this.chkHKLM.Checked = true;
            this.chkHKLM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHKLM.Location = new System.Drawing.Point(15, 251);
            this.chkHKLM.Name = "chkHKLM";
            this.chkHKLM.Size = new System.Drawing.Size(96, 17);
            this.chkHKLM.TabIndex = 15;
            this.chkHKLM.Text = "Local Machine";
            this.chkHKLM.UseVisualStyleBackColor = true;
            // 
            // chkHKCR
            // 
            this.chkHKCR.AutoSize = true;
            this.chkHKCR.Checked = true;
            this.chkHKCR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHKCR.Location = new System.Drawing.Point(15, 274);
            this.chkHKCR.Name = "chkHKCR";
            this.chkHKCR.Size = new System.Drawing.Size(88, 17);
            this.chkHKCR.TabIndex = 16;
            this.chkHKCR.Text = "Classes Root";
            this.chkHKCR.UseVisualStyleBackColor = true;
            // 
            // chkHKCC
            // 
            this.chkHKCC.AutoSize = true;
            this.chkHKCC.Location = new System.Drawing.Point(15, 297);
            this.chkHKCC.Name = "chkHKCC";
            this.chkHKCC.Size = new System.Drawing.Size(93, 17);
            this.chkHKCC.TabIndex = 17;
            this.chkHKCC.Text = "Current Config";
            this.chkHKCC.UseVisualStyleBackColor = true;
            // 
            // chkHKPD
            // 
            this.chkHKPD.AutoSize = true;
            this.chkHKPD.Location = new System.Drawing.Point(15, 320);
            this.chkHKPD.Name = "chkHKPD";
            this.chkHKPD.Size = new System.Drawing.Size(112, 17);
            this.chkHKPD.TabIndex = 18;
            this.chkHKPD.Text = "Performance Data";
            this.chkHKPD.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(85, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(289, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "Outputs a PowerShell script, does not modify registry directly";
            // 
            // chkString
            // 
            this.chkString.AutoSize = true;
            this.chkString.Checked = true;
            this.chkString.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkString.Location = new System.Drawing.Point(240, 205);
            this.chkString.Name = "chkString";
            this.chkString.Size = new System.Drawing.Size(53, 17);
            this.chkString.TabIndex = 24;
            this.chkString.Text = "String";
            this.chkString.UseVisualStyleBackColor = true;
            // 
            // chkMultiStr
            // 
            this.chkMultiStr.AutoSize = true;
            this.chkMultiStr.Checked = true;
            this.chkMultiStr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMultiStr.Location = new System.Drawing.Point(240, 228);
            this.chkMultiStr.Name = "chkMultiStr";
            this.chkMultiStr.Size = new System.Drawing.Size(90, 17);
            this.chkMultiStr.TabIndex = 25;
            this.chkMultiStr.Text = "Multi String [ ]";
            this.chkMultiStr.UseVisualStyleBackColor = true;
            // 
            // chkExpandString
            // 
            this.chkExpandString.AutoSize = true;
            this.chkExpandString.Checked = true;
            this.chkExpandString.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExpandString.Location = new System.Drawing.Point(240, 251);
            this.chkExpandString.Name = "chkExpandString";
            this.chkExpandString.Size = new System.Drawing.Size(112, 17);
            this.chkExpandString.TabIndex = 26;
            this.chkExpandString.Text = "Expandable String";
            this.chkExpandString.UseVisualStyleBackColor = true;
            // 
            // chkBinary
            // 
            this.chkBinary.AutoSize = true;
            this.chkBinary.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chkBinary.Location = new System.Drawing.Point(240, 274);
            this.chkBinary.Name = "chkBinary";
            this.chkBinary.Size = new System.Drawing.Size(55, 17);
            this.chkBinary.TabIndex = 27;
            this.chkBinary.Text = "Binary";
            this.chkBinary.UseVisualStyleBackColor = true;
            // 
            // txtCurrentLocation
            // 
            this.txtCurrentLocation.Location = new System.Drawing.Point(10, 421);
            this.txtCurrentLocation.Name = "txtCurrentLocation";
            this.txtCurrentLocation.ReadOnly = true;
            this.txtCurrentLocation.Size = new System.Drawing.Size(562, 20);
            this.txtCurrentLocation.TabIndex = 34;
            // 
            // chkCaseSensitive
            // 
            this.chkCaseSensitive.AutoSize = true;
            this.chkCaseSensitive.Location = new System.Drawing.Point(267, 45);
            this.chkCaseSensitive.Name = "chkCaseSensitive";
            this.chkCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.chkCaseSensitive.TabIndex = 4;
            this.chkCaseSensitive.Text = "Case sensitive";
            this.chkCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // cmbStringCompareStyle
            // 
            this.cmbStringCompareStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStringCompareStyle.FormattingEnabled = true;
            this.cmbStringCompareStyle.Items.AddRange(new object[] {
            "Current culture",
            "Culture invariant",
            "Ordinal (U+xxxx)"});
            this.cmbStringCompareStyle.Location = new System.Drawing.Point(87, 43);
            this.cmbStringCompareStyle.MaxDropDownItems = 3;
            this.cmbStringCompareStyle.Name = "cmbStringCompareStyle";
            this.cmbStringCompareStyle.Size = new System.Drawing.Size(174, 21);
            this.cmbStringCompareStyle.TabIndex = 3;
            // 
            // chkMatchResultCase
            // 
            this.chkMatchResultCase.AutoSize = true;
            this.chkMatchResultCase.Location = new System.Drawing.Point(88, 96);
            this.chkMatchResultCase.Name = "chkMatchResultCase";
            this.chkMatchResultCase.Size = new System.Drawing.Size(203, 17);
            this.chkMatchResultCase.TabIndex = 8;
            this.chkMatchResultCase.Text = "Use auto detected search result case";
            this.chkMatchResultCase.UseVisualStyleBackColor = true;
            // 
            // chkUseRootPath
            // 
            this.chkUseRootPath.AutoSize = true;
            this.chkUseRootPath.Location = new System.Drawing.Point(16, 343);
            this.chkUseRootPath.Name = "chkUseRootPath";
            this.chkUseRootPath.Size = new System.Drawing.Size(73, 17);
            this.chkUseRootPath.TabIndex = 19;
            this.chkUseRootPath.Text = "Root path";
            this.chkUseRootPath.UseVisualStyleBackColor = true;
            this.chkUseRootPath.CheckedChanged += new System.EventHandler(this.chkUseRootPath_CheckedChanged);
            // 
            // txtRootPath
            // 
            this.txtRootPath.Location = new System.Drawing.Point(88, 341);
            this.txtRootPath.Name = "txtRootPath";
            this.txtRootPath.Size = new System.Drawing.Size(451, 20);
            this.txtRootPath.TabIndex = 20;
            this.txtRootPath.TextChanged += new System.EventHandler(this.txtRootPath_TextChanged);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(441, 255);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(94, 36);
            this.btnStop.TabIndex = 31;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 372);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(279, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "To enable PowerShell scripts, run the following command:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(334, 369);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(205, 20);
            this.textBox1.TabIndex = 32;
            this.textBox1.Text = "Set-ExecutionPolicy RemoteSigned";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 394);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(304, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "This can be set back to the most secure setting later by saying ";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(334, 391);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(202, 20);
            this.textBox2.TabIndex = 33;
            this.textBox2.Text = "Set-ExecutionPolicy Restricted";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 591);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.txtRootPath);
            this.Controls.Add(this.chkUseRootPath);
            this.Controls.Add(this.chkMatchResultCase);
            this.Controls.Add(this.cmbStringCompareStyle);
            this.Controls.Add(this.chkCaseSensitive);
            this.Controls.Add(this.txtCurrentLocation);
            this.Controls.Add(this.chkBinary);
            this.Controls.Add(this.chkExpandString);
            this.Controls.Add(this.chkMultiStr);
            this.Controls.Add(this.chkString);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkHKPD);
            this.Controls.Add(this.chkHKCC);
            this.Controls.Add(this.chkHKCR);
            this.Controls.Add(this.chkHKLM);
            this.Controls.Add(this.chkUser);
            this.Controls.Add(this.chkData);
            this.Controls.Add(this.chkValues);
            this.Controls.Add(this.chkKeys);
            this.Controls.Add(this.chkAllUsers);
            this.Controls.Add(this.btnDump);
            this.Controls.Add(this.btnReplaceFolder);
            this.Controls.Add(this.btnFindFolder);
            this.Controls.Add(this.btnRedoFileBrowse);
            this.Controls.Add(this.btnUndoFileBrowse);
            this.Controls.Add(this.btnReplaceFile);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.txtRedoFile);
            this.Controls.Add(this.txtUndoFile);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFindFile);
            this.Controls.Add(this.txtReplaceWith);
            this.Controls.Add(this.txtFind);
            this.Name = "Form1";
            this.Text = "Registry find and replace";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.TextBox txtReplaceWith;
        private System.Windows.Forms.Button btnFindFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUndoFile;
        private System.Windows.Forms.TextBox txtRedoFile;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnReplaceFile;
        private System.Windows.Forms.Button btnUndoFileBrowse;
        private System.Windows.Forms.Button btnRedoFileBrowse;
        private System.Windows.Forms.Button btnFindFolder;
        private System.Windows.Forms.Button btnReplaceFolder;
        private System.Windows.Forms.ToolTip ttBrowse;
        private System.Windows.Forms.Button btnDump;
        private System.Windows.Forms.CheckBox chkAllUsers;
        private System.Windows.Forms.CheckBox chkKeys;
        private System.Windows.Forms.CheckBox chkValues;
        private System.Windows.Forms.CheckBox chkData;
        private System.Windows.Forms.CheckBox chkUser;
        private System.Windows.Forms.CheckBox chkHKLM;
        private System.Windows.Forms.CheckBox chkHKCR;
        private System.Windows.Forms.CheckBox chkHKCC;
        private System.Windows.Forms.CheckBox chkHKPD;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkString;
        private System.Windows.Forms.CheckBox chkMultiStr;
        private System.Windows.Forms.CheckBox chkExpandString;
        private System.Windows.Forms.CheckBox chkBinary;
        private System.Windows.Forms.TextBox txtCurrentLocation;
        private System.Windows.Forms.CheckBox chkCaseSensitive;
        private System.Windows.Forms.ComboBox cmbStringCompareStyle;
        private System.Windows.Forms.CheckBox chkMatchResultCase;
        private System.Windows.Forms.CheckBox chkUseRootPath;
        private System.Windows.Forms.TextBox txtRootPath;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox2;
    }
}

