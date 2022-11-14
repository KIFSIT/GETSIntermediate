namespace GETSIntermediate
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
            this.tbDebugLog = new System.Windows.Forms.RichTextBox();
            this.clientConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RMSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.severToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbDebugLog
            // 
            this.tbDebugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDebugLog.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbDebugLog.Location = new System.Drawing.Point(0, 24);
            this.tbDebugLog.Name = "tbDebugLog";
            this.tbDebugLog.Size = new System.Drawing.Size(698, 523);
            this.tbDebugLog.TabIndex = 14;
            this.tbDebugLog.Text = "";
            // 
            // clientConnectToolStripMenuItem
            // 
            this.clientConnectToolStripMenuItem.Name = "clientConnectToolStripMenuItem";
            this.clientConnectToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
            this.clientConnectToolStripMenuItem.Text = "ClientConnect";
            // 
            // GUIToolStripMenuItem
            // 
            this.GUIToolStripMenuItem.Name = "GUIToolStripMenuItem";
            this.GUIToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.GUIToolStripMenuItem.Text = "GUI";
            this.GUIToolStripMenuItem.Click += new System.EventHandler(this.GUIToolStripMenuItem_Click);
            // 
            // RMSToolStripMenuItem
            // 
            this.RMSToolStripMenuItem.Name = "RMSToolStripMenuItem";
            this.RMSToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.RMSToolStripMenuItem.Text = "RMS";
            this.RMSToolStripMenuItem.Click += new System.EventHandler(this.RMSToolStripMenuItem_Click);
            // 
            // severToolStripMenuItem
            // 
            this.severToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RMSToolStripMenuItem,
            this.GUIToolStripMenuItem});
            this.severToolStripMenuItem.Name = "severToolStripMenuItem";
            this.severToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.severToolStripMenuItem.Text = "Server";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.severToolStripMenuItem,
            this.clientConnectToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(698, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 547);
            this.Controls.Add(this.tbDebugLog);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "GETS INTERMEDIATE";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RichTextBox tbDebugLog;
        private System.Windows.Forms.ToolStripMenuItem clientConnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GUIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RMSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem severToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;

    }
}

