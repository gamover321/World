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
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
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
			button1.Size = new Size(94, 29);
			button1.TabIndex = 1;
			button1.Text = "Update";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// button2
			// 
			button2.Location = new Point(1066, 470);
			button2.Name = "button2";
			button2.Size = new Size(94, 29);
			button2.TabIndex = 2;
			button2.Text = "Start";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1196, 690);
			Controls.Add(button2);
			Controls.Add(button1);
			Controls.Add(pictureBox1);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
		private Button button1;
		private Button button2;
	}
}