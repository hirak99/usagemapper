namespace UsageMapper
{
    partial class UsageMapView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            fc.CancelProcessing();
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // UsageMapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "UsageMapView";
            this.Size = new System.Drawing.Size(475, 333);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UsageMapView_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UsageMapView_MouseMove);
            this.Resize += new System.EventHandler(this.UsageMapView_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.UsageMapView_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
    }
}