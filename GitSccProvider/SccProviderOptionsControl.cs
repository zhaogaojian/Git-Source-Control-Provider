
/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Windows.Controls;
using GitSccProvider;
using Microsoft.VisualStudio.Shell.Interop;
using Button = System.Windows.Forms.Button;
using CheckBox = System.Windows.Forms.CheckBox;
using ComboBox = System.Windows.Forms.ComboBox;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace GitScc
{
    /// <summary>
    /// Summary description for SccProviderOptionsControl.
    /// </summary>
    public class SccProviderOptionsControl : System.Windows.Forms.UserControl
    {
        private IContainer components;
        private OpenFileDialog openFileDialog1;
        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private TextBox textBox2;
        private TextBox textBox3;
        private Button button1;
        private Button button2;
        private Button button3;
        private Label label4;
        private TextBox textBox4;
        private Button button4;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox5;
        private CheckBox checkBox6;
        private ComboBox cbDiffTool;
        private Label label3;
        private Label label5;
        private CheckBox _cbrepoTrack;
        private CheckBox _cbAutoAddFiles;
        private CheckBox _cbAutoAddProject;
        private System.Windows.Forms.ToolTip _ttAddProjects;
        private CheckBox _cbSaveOnCommit;
        private Label HideCmd1;
        private TextBox textBoxHideCmdForGitExtension;
        private Label label6;
        private TextBox textBoxHideCmdForTortoiseGit;

        // The parent page, use to persist data
        private SccProviderOptions _customPage;

        public SccProviderOptionsControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // TODO: Add any initialization after the InitializeComponent call

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                GC.SuppressFinalize(this);
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.cbDiffTool = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._cbrepoTrack = new System.Windows.Forms.CheckBox();
            this._cbAutoAddFiles = new System.Windows.Forms.CheckBox();
            this._cbAutoAddProject = new System.Windows.Forms.CheckBox();
            this._ttAddProjects = new System.Windows.Forms.ToolTip(this.components);
            this._cbSaveOnCommit = new System.Windows.Forms.CheckBox();
            this.HideCmd1 = new System.Windows.Forms.Label();
            this.textBoxHideCmdForGitExtension = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxHideCmdForTortoiseGit = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "Path to Git for Windows (git.exe):";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 21);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(364, 21);
            this.textBox1.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "Path to Git Extensions:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(6, 60);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(364, 21);
            this.textBox2.TabIndex = 14;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(7, 312);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(364, 21);
            this.textBox3.TabIndex = 16;
            this.textBox3.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(379, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "Browse ...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(379, 59);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Browse ...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(380, 310);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 19;
            this.button3.Text = "Browse ...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "Path to TortoiseGit:";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(7, 163);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(364, 21);
            this.textBox4.TabIndex = 21;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(380, 163);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 22;
            this.button4.Text = "Browse ...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(66, 86);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(246, 16);
            this.checkBox1.TabIndex = 23;
            this.checkBox1.Text = "Do not expand Git Extensions commands";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(67, 189);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(228, 16);
            this.checkBox2.TabIndex = 24;
            this.checkBox2.Text = "Do not expand TortoiseGit commands";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox3.Location = new System.Drawing.Point(7, 464);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(204, 16);
            this.checkBox3.TabIndex = 25;
            this.checkBox3.Text = "Use TortoiseGit style icon set";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox5.Location = new System.Drawing.Point(7, 414);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(402, 16);
            this.checkBox5.TabIndex = 27;
            this.checkBox5.Text = "Disable auto switch to this plug-in for Git controlled projects";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox6.Location = new System.Drawing.Point(7, 438);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(252, 16);
            this.checkBox6.TabIndex = 28;
            this.checkBox6.Text = "Disable UTF-8 file names (Git 1.7.10+)";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // cbDiffTool
            // 
            this.cbDiffTool.DisplayMember = "Content";
            this.cbDiffTool.FormattingEnabled = true;
            this.cbDiffTool.Location = new System.Drawing.Point(114, 211);
            this.cbDiffTool.Name = "cbDiffTool";
            this.cbDiffTool.Size = new System.Drawing.Size(341, 20);
            this.cbDiffTool.TabIndex = 30;
            this.cbDiffTool.Tag = "Tag";
            this.cbDiffTool.SelectedIndexChanged += new System.EventHandler(this.cbDiffTool_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 290);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(437, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "Path to external diff tool (optional, by default diffmerge.exe is used):";
            this.label3.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 214);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 31;
            this.label5.Text = "Select Diff Tool";
            // 
            // _cbrepoTrack
            // 
            this._cbrepoTrack.AutoSize = true;
            this._cbrepoTrack.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._cbrepoTrack.Location = new System.Drawing.Point(7, 391);
            this._cbrepoTrack.Name = "_cbrepoTrack";
            this._cbrepoTrack.Size = new System.Drawing.Size(288, 16);
            this._cbrepoTrack.TabIndex = 32;
            this._cbrepoTrack.Text = "Track Active Repository In Solution Explorer";
            this._cbrepoTrack.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._ttAddProjects.SetToolTip(this._cbrepoTrack, "If your project has multiple repositories you can enable this to synchronize the " +
        "current selected file in the Solution Explorer with the Pending Changes Window");
            this._cbrepoTrack.UseVisualStyleBackColor = true;
            this._cbrepoTrack.CheckedChanged += new System.EventHandler(this._cbrepoTrack_CheckedChanged);
            // 
            // _cbAutoAddFiles
            // 
            this._cbAutoAddFiles.AutoSize = true;
            this._cbAutoAddFiles.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._cbAutoAddFiles.Location = new System.Drawing.Point(201, 346);
            this._cbAutoAddFiles.Name = "_cbAutoAddFiles";
            this._cbAutoAddFiles.Size = new System.Drawing.Size(228, 16);
            this._cbAutoAddFiles.TabIndex = 33;
            this._cbAutoAddFiles.Text = "Automatically Add New Files To GIT";
            this._cbAutoAddFiles.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._cbAutoAddFiles.UseVisualStyleBackColor = true;
            // 
            // _cbAutoAddProject
            // 
            this._cbAutoAddProject.AutoSize = true;
            this._cbAutoAddProject.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._cbAutoAddProject.Location = new System.Drawing.Point(7, 346);
            this._cbAutoAddProject.Name = "_cbAutoAddProject";
            this._cbAutoAddProject.Size = new System.Drawing.Size(222, 16);
            this._cbAutoAddProject.TabIndex = 34;
            this._cbAutoAddProject.Text = "Automatically Add Projects To GIT";
            this._cbAutoAddProject.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._ttAddProjects.SetToolTip(this._cbAutoAddProject, "Enabled to Automatically Add Project to GIT if not already added and are in an ex" +
        "isting repository.");
            this._cbAutoAddProject.UseVisualStyleBackColor = true;
            // 
            // _cbSaveOnCommit
            // 
            this._cbSaveOnCommit.AutoSize = true;
            this._cbSaveOnCommit.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._cbSaveOnCommit.Location = new System.Drawing.Point(7, 368);
            this._cbSaveOnCommit.Name = "_cbSaveOnCommit";
            this._cbSaveOnCommit.Size = new System.Drawing.Size(138, 16);
            this._cbSaveOnCommit.TabIndex = 35;
            this._cbSaveOnCommit.Text = "Save on Commit/Sync";
            this._cbSaveOnCommit.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this._cbSaveOnCommit.UseVisualStyleBackColor = true;
            // 
            // HideCmd1
            // 
            this.HideCmd1.AutoSize = true;
            this.HideCmd1.Location = new System.Drawing.Point(5, 101);
            this.HideCmd1.Name = "HideCmd1";
            this.HideCmd1.Size = new System.Drawing.Size(437, 12);
            this.HideCmd1.TabIndex = 36;
            this.HideCmd1.Text = "HideCmd For Git Extensions(eg:[Push,Commit] will Hide the two commands):";
            // 
            // textBoxHideCmdForGitExtension
            // 
            this.textBoxHideCmdForGitExtension.Location = new System.Drawing.Point(7, 116);
            this.textBoxHideCmdForGitExtension.Name = "textBoxHideCmdForGitExtension";
            this.textBoxHideCmdForGitExtension.Size = new System.Drawing.Size(364, 21);
            this.textBoxHideCmdForGitExtension.TabIndex = 37;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(419, 12);
            this.label6.TabIndex = 36;
            this.label6.Text = "HideCmd For TortoiseGit(eg:[Push,Commit] will Hide the two commands):";
            // 
            // textBoxHideCmdForTortoiseGit
            // 
            this.textBoxHideCmdForTortoiseGit.Location = new System.Drawing.Point(5, 255);
            this.textBoxHideCmdForTortoiseGit.Name = "textBoxHideCmdForTortoiseGit";
            this.textBoxHideCmdForTortoiseGit.Size = new System.Drawing.Size(364, 21);
            this.textBoxHideCmdForTortoiseGit.TabIndex = 37;
            // 
            // SccProviderOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.textBoxHideCmdForTortoiseGit);
            this.Controls.Add(this.textBoxHideCmdForGitExtension);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.HideCmd1);
            this.Controls.Add(this._cbSaveOnCommit);
            this.Controls.Add(this._cbAutoAddProject);
            this.Controls.Add(this._cbAutoAddFiles);
            this.Controls.Add(this._cbrepoTrack);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbDiffTool);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Name = "SccProviderOptionsControl";
            this.Size = new System.Drawing.Size(514, 519);
            this.Load += new System.EventHandler(this.SccProviderOptionsControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public SccProviderOptions OptionsPage
        {
            set
            {
                _customPage = value;
            }
        }

        private void SccProviderOptionsControl_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = GitSccOptions.Current.GitBashPath;
            this.textBox2.Text = GitSccOptions.Current.GitExtensionPath;
            this.textBox3.Text = GitSccOptions.Current.DifftoolPath;
            this.textBox4.Text = GitSccOptions.Current.TortoiseGitPath;
            this.checkBox1.Checked = GitSccOptions.Current.NotExpandGitExtensions;
            this.checkBox2.Checked = GitSccOptions.Current.NotExpandTortoiseGit;
            this.checkBox3.Checked = GitSccOptions.Current.UseTGitIconSet;
            //this.checkBox4.Checked = GitSccOptions.Current.DisableAutoRefresh;
            this.checkBox5.Checked = GitSccOptions.Current.DisableAutoLoad;
            this.checkBox6.Checked = GitSccOptions.Current.NotUseUTF8FileNames;
            //this.useVsDiffChk.Checked = GitSccOptions.Current.UseVsDiff;
            _cbrepoTrack.Checked = GitSccOptions.Current.TrackActiveGitRepo;
            _cbAutoAddFiles.Checked = GitSccOptions.Current.AutoAddFiles;
            _cbAutoAddProject.Checked = GitSccOptions.Current.AutoAddProjects;
            _cbSaveOnCommit.Checked = GitSccOptions.Current.SaveOnCommit;
            this.textBoxHideCmdForGitExtension.Text= GitSccOptions.Current.HideCmdForGitExtension;
            this.textBoxHideCmdForTortoiseGit.Text = GitSccOptions.Current.HideCmdForTortoiseGit;
            cbDiffTool.Items.Clear();
            PopulateDiffTools();

            

            if (GitSccOptions.IsVisualStudio2012)
                checkBox3.Text += " (requires restart)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFile("git.exe", textBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFile("*.exe", textBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFile("*.exe", textBox3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFile("*.exe", textBox4);
        }

        private void OpenFile(string shexe, TextBox textBox)
        {
            this.openFileDialog1.FileName = shexe;
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = this.openFileDialog1.FileName;
            }
        }

        internal void Save()
        {
            GitBash.GitExePath = 
            GitSccOptions.Current.GitBashPath      = this.textBox1.Text; 
            GitSccOptions.Current.GitExtensionPath = this.textBox2.Text;
            GitSccOptions.Current.DifftoolPath     = this.textBox3.Text;
            GitSccOptions.Current.TortoiseGitPath  = this.textBox4.Text;
            GitSccOptions.Current.NotExpandGitExtensions = this.checkBox1.Checked;
            GitSccOptions.Current.NotExpandTortoiseGit = this.checkBox2.Checked;
            GitSccOptions.Current.UseTGitIconSet = this.checkBox3.Checked;
            GitSccOptions.Current.DisableAutoRefresh = true;
            GitSccOptions.Current.DisableAutoLoad = this.checkBox5.Checked;
            GitSccOptions.Current.TrackActiveGitRepo = _cbrepoTrack.Checked;
            GitSccOptions.Current.AutoAddFiles = _cbAutoAddFiles.Checked;
            GitSccOptions.Current.AutoAddProjects = _cbAutoAddProject.Checked;
            GitSccOptions.Current.SaveOnCommit = _cbSaveOnCommit.Checked;
            GitSccOptions.Current.NotUseUTF8FileNames = this.checkBox6.Checked;
            GitSccOptions.Current.DisableDiffMargin = true;
            GitSccOptions.Current.UseVsDiff = false;
            GitSccOptions.Current.HideCmdForGitExtension = this.textBoxHideCmdForGitExtension.Text;
            GitSccOptions.Current.HideCmdForTortoiseGit = this.textBoxHideCmdForTortoiseGit.Text;
            GitSccOptions.Current.DiffTool = GetDiffToolId(); // this.cbDiffTool.SelectedItem.

            GitBash.UseUTF8FileNames = !GitSccOptions.Current.NotUseUTF8FileNames;
            GitSccOptions.Current.SaveConfig();

            SccProviderService sccProviderService = (SccProviderService)GetService(typeof(SccProviderService)) as SccProviderService;
            //sccProviderService.MarkDirty(false);
        }

        //private void useVsDiffChk_CheckedChanged(object sender, EventArgs e)
        //{
        //    label3.Enabled = textBox3.Enabled = button3.Enabled 
        //        = !useVsDiffChk.Checked;
        //}

        private void  PopulateDiffTools()
        {
            foreach (var value in Enum.GetValues(typeof(DiffTools)))
            {
                var item = new ComboBoxItem();
                item.Content = GetDescription((DiffTools)value);
                item.Tag = value;
                cbDiffTool.Items.Add(item);
                if ((DiffTools)value == GitSccOptions.Current.DiffTool)
                {
                    cbDiffTool.SelectedItem = item;
                }
                
            }
            
        }

        private DiffTools GetDiffToolId()
        {
            var item = (ComboBoxItem) cbDiffTool.SelectedItem;
            return (DiffTools) item.Tag;
        }

        private static string GetDescription(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        private void cbDiffTool_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void _cbrepoTrack_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

}
