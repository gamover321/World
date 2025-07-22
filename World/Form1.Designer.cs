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
			buttonCameraLeft = new Button();
			buttonCameraRight = new Button();
			buttonCameraUp = new Button();
			buttonCameraDown = new Button();
			cameraZoomTrackbar = new TrackBar();
			label2 = new Label();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
			((System.ComponentModel.ISupportInitialize)cameraZoomTrackbar).BeginInit();
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
			timer2.Tick += timer2_Tick;
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
			// buttonCameraLeft
			// 
			buttonCameraLeft.Location = new Point(1054, 457);
			buttonCameraLeft.Name = "buttonCameraLeft";
			buttonCameraLeft.Size = new Size(52, 29);
			buttonCameraLeft.TabIndex = 6;
			buttonCameraLeft.Text = "Left";
			buttonCameraLeft.UseVisualStyleBackColor = true;
			buttonCameraLeft.Click += buttonCameraLeft_Click;
			// 
			// buttonCameraRight
			// 
			buttonCameraRight.Location = new Point(1112, 457);
			buttonCameraRight.Name = "buttonCameraRight";
			buttonCameraRight.Size = new Size(52, 29);
			buttonCameraRight.TabIndex = 7;
			buttonCameraRight.Text = "Right";
			buttonCameraRight.UseVisualStyleBackColor = true;
			buttonCameraRight.Click += buttonCameraRight_Click;
			// 
			// buttonCameraUp
			// 
			buttonCameraUp.Location = new Point(1079, 422);
			buttonCameraUp.Name = "buttonCameraUp";
			buttonCameraUp.Size = new Size(70, 29);
			buttonCameraUp.TabIndex = 8;
			buttonCameraUp.Text = "Up";
			buttonCameraUp.UseVisualStyleBackColor = true;
			buttonCameraUp.Click += buttonCameraUp_Click;
			// 
			// buttonCameraDown
			// 
			buttonCameraDown.Location = new Point(1079, 492);
			buttonCameraDown.Name = "buttonCameraDown";
			buttonCameraDown.Size = new Size(70, 29);
			buttonCameraDown.TabIndex = 9;
			buttonCameraDown.Text = "Down";
			buttonCameraDown.UseVisualStyleBackColor = true;
			buttonCameraDown.Click += buttonCameraDown_Click;
			// 
			// cameraZoomTrackbar
			// 
			cameraZoomTrackbar.Location = new Point(1039, 360);
			cameraZoomTrackbar.Maximum = 20;
			cameraZoomTrackbar.Minimum = 1;
			cameraZoomTrackbar.Name = "cameraZoomTrackbar";
			cameraZoomTrackbar.Size = new Size(145, 56);
			cameraZoomTrackbar.TabIndex = 10;
			cameraZoomTrackbar.Value = 5;
			cameraZoomTrackbar.Scroll += cameraZoomTrackbar_Scroll;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(1084, 337);
			label2.Name = "label2";
			label2.Size = new Size(49, 20);
			label2.TabIndex = 11;
			label2.Text = "Zoom";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1196, 690);
			Controls.Add(label2);
			Controls.Add(cameraZoomTrackbar);
			Controls.Add(buttonCameraDown);
			Controls.Add(buttonCameraUp);
			Controls.Add(buttonCameraRight);
			Controls.Add(buttonCameraLeft);
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
			((System.ComponentModel.ISupportInitialize)cameraZoomTrackbar).EndInit();
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
		private Button buttonCameraLeft;
		private Button buttonCameraRight;
		private Button buttonCameraUp;
		private Button buttonCameraDown;
		private TrackBar cameraZoomTrackbar;
		private Label label2;
	}
}