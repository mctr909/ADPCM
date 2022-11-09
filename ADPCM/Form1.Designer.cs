
namespace ADPCM {
    partial class Form1 {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.numChannels = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numPackingSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numSampleRate = new System.Windows.Forms.NumericUpDown();
            this.btnApply = new System.Windows.Forms.Button();
            this.trackbar1 = new ADPCM.Trackbar();
            this.numPlayChannel = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPackingSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSampleRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPlayChannel)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "開く";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 41);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 2;
            this.btnPlay.Text = "再生";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // numChannels
            // 
            this.numChannels.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numChannels.Location = new System.Drawing.Point(366, 24);
            this.numChannels.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numChannels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numChannels.Name = "numChannels";
            this.numChannels.Size = new System.Drawing.Size(61, 22);
            this.numChannels.TabIndex = 3;
            this.numChannels.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numChannels.ValueChanged += new System.EventHandler(this.numChannels_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(364, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "チャンネル数";
            // 
            // numPackingSize
            // 
            this.numPackingSize.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numPackingSize.Location = new System.Drawing.Point(93, 24);
            this.numPackingSize.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.numPackingSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPackingSize.Name = "numPackingSize";
            this.numPackingSize.Size = new System.Drawing.Size(74, 22);
            this.numPackingSize.TabIndex = 5;
            this.numPackingSize.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(91, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "パッキングサイズ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(171, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "サンプリング周波数";
            // 
            // numSampleRate
            // 
            this.numSampleRate.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numSampleRate.Location = new System.Drawing.Point(173, 24);
            this.numSampleRate.Maximum = new decimal(new int[] {
            192000,
            0,
            0,
            0});
            this.numSampleRate.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numSampleRate.Name = "numSampleRate";
            this.numSampleRate.Size = new System.Drawing.Size(92, 22);
            this.numSampleRate.TabIndex = 8;
            this.numSampleRate.Value = new decimal(new int[] {
            44100,
            0,
            0,
            0});
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(271, 24);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(48, 23);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "反映";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // trackbar1
            // 
            this.trackbar1.BackColor = System.Drawing.Color.Transparent;
            this.trackbar1.Location = new System.Drawing.Point(12, 72);
            this.trackbar1.Name = "trackbar1";
            this.trackbar1.Size = new System.Drawing.Size(512, 43);
            this.trackbar1.TabIndex = 0;
            // 
            // numPlayChannel
            // 
            this.numPlayChannel.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numPlayChannel.Location = new System.Drawing.Point(433, 24);
            this.numPlayChannel.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numPlayChannel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPlayChannel.Name = "numPlayChannel";
            this.numPlayChannel.Size = new System.Drawing.Size(61, 22);
            this.numPlayChannel.TabIndex = 10;
            this.numPlayChannel.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numPlayChannel.ValueChanged += new System.EventHandler(this.numPlayChannel_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(431, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "再生チャンネル";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 127);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numPlayChannel);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.numSampleRate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numPackingSize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numChannels);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.trackbar1);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numChannels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPackingSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSampleRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPlayChannel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Trackbar trackbar1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown numChannels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numPackingSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numSampleRate;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.NumericUpDown numPlayChannel;
        private System.Windows.Forms.Label label4;
    }
}

