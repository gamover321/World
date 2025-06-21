namespace World
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			pictureBox1 = new PictureBox();
			timer1 = new System.Windows.Forms.Timer(components);
			button1 = new Button();
			button2 = new Button();
			button3 = new Button();
			timer2 = new System.Windows.Forms.Timer(components);
			trackBar1 = new TrackBar();
			label1 = new Label();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
			SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Cursor = Cursors.Cross;
			pictureBox1.Location = new Point(0, 0);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(1033, 692);
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			pictureBox1.Click += pictureBox1_Click;
			// 
			// timer1
			// 
			timer1.Enabled = true;
			timer1.Interval = 30;
			timer1.Tick += timer1_Tick;
			// 
			// button1
			// 
			button1.Location = new Point(1066, 34);
			button1.Name = "button1";
			button1.Size = new Size(118, 29);
			button1.TabIndex = 1;
			button1.Text = "Update";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// button2
			// 
			button2.Location = new Point(1066, 637);
			button2.Name = "button2";
			button2.Size = new Size(94, 29);
			button2.TabIndex = 2;
			button2.Text = "Start";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// button3
			// 
			button3.Location = new Point(1066, 92);
			button3.Name = "button3";
			button3.Size = new Size(118, 29);
			button3.TabIndex = 3;
			button3.Text = "Update x100";
			button3.UseVisualStyleBackColor = true;
			button3.Click += button3_Click;
			// 
			// timer2
			// 
			timer2.Interval = 16;
			// 
			// trackBar1
			// 
			trackBar1.Location = new Point(1054, 575);
			trackBar1.Maximum = 40;
			trackBar1.Minimum = 1;
			trackBar1.Name = "trackBar1";
			trackBar1.Size = new Size(130, 56);
			trackBar1.TabIndex = 4;
			trackBar1.Value = 20;
			trackBar1.Scroll += trackBar1_Scroll;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(1084, 542);
			label1.Name = "label1";
			label1.Size = new Size(65, 20);
			label1.TabIndex = 5;
			label1.Text = "Sim SPD";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1196, 690);
			Controls.Add(label1);
			Controls.Add(trackBar1);
			Controls.Add(button3);
			Controls.Add(button2);
			Controls.Add(button1);
			Controls.Add(pictureBox1);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
		private Button button1;
		private Button button2;
		private Button button3;
		private System.Windows.Forms.Timer timer2;
		private TrackBar trackBar1;
		private Label label1;
	}
}