namespace IseAddons {
    partial class frmReferences {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.cLst = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bGoto = new System.Windows.Forms.Button();
            this.bClose = new System.Windows.Forms.Button();
            this.cContext = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // cLst
            // 
            this.cLst.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cLst.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.cLst.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.cLst.FullRowSelect = true;
            this.cLst.HideSelection = false;
            this.cLst.Location = new System.Drawing.Point(0, 0);
            this.cLst.MultiSelect = false;
            this.cLst.Name = "cLst";
            this.cLst.Size = new System.Drawing.Size(981, 238);
            this.cLst.TabIndex = 0;
            this.cLst.UseCompatibleStateImageBehavior = false;
            this.cLst.View = System.Windows.Forms.View.Details;
            this.cLst.SelectedIndexChanged += new System.EventHandler(this.cLst_SelectedIndexChanged);
            this.cLst.DoubleClick += new System.EventHandler(this.cLst_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Number";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Function";
            this.columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Line";
            // 
            // bGoto
            // 
            this.bGoto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bGoto.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bGoto.Location = new System.Drawing.Point(989, 12);
            this.bGoto.Name = "bGoto";
            this.bGoto.Size = new System.Drawing.Size(92, 23);
            this.bGoto.TabIndex = 1;
            this.bGoto.Text = "Go To";
            this.bGoto.UseVisualStyleBackColor = true;
            this.bGoto.Click += new System.EventHandler(this.bGoto_Click);
            // 
            // bClose
            // 
            this.bClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bClose.Location = new System.Drawing.Point(989, 41);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(92, 23);
            this.bClose.TabIndex = 1;
            this.bClose.Text = "Close";
            this.bClose.UseVisualStyleBackColor = true;
            this.bClose.Click += new System.EventHandler(this.BClose_Click);
            // 
            // cContext
            // 
            this.cContext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cContext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cContext.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cContext.Location = new System.Drawing.Point(0, 244);
            this.cContext.Name = "cContext";
            this.cContext.Size = new System.Drawing.Size(981, 163);
            this.cContext.TabIndex = 2;
            this.cContext.Text = "";
            // 
            // frmReferences
            // 
            this.AcceptButton = this.bGoto;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bClose;
            this.ClientSize = new System.Drawing.Size(1085, 410);
            this.ControlBox = false;
            this.Controls.Add(this.cContext);
            this.Controls.Add(this.bClose);
            this.Controls.Add(this.bGoto);
            this.Controls.Add(this.cLst);
            this.Name = "frmReferences";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmReferencias";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmReferences_FormClosing);
            this.ResumeLayout(false);

            }

        #endregion
        private System.Windows.Forms.ListView cLst;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button bGoto;
        private System.Windows.Forms.Button bClose;
        private System.Windows.Forms.RichTextBox cContext;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        }
    }