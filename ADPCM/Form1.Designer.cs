
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
            this.numPlayChannel = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.lblPos = new System.Windows.Forms.Label();
            this.btnEncode = new System.Windows.Forms.Button();
            this.trackbar1 = new ADPCM.Trackbar();
            this.numBit = new System.Windows.Forms.NumericUpDown();
            this.rbVAG = new System.Windows.Forms.RadioButton();
            this.rbADPCM = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPackingSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSampleRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPlayChannel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBit)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 9);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 38);
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
            this.numChannels.Location = new System.Drawing.Point(381, 24);
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
            this.label1.Location = new System.Drawing.Point(379, 9);
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
            this.btnApply.Location = new System.Drawing.Point(271, 9);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(48, 38);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "反映";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // numPlayChannel
            // 
            this.numPlayChannel.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numPlayChannel.Location = new System.Drawing.Point(448, 24);
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
            this.label4.Location = new System.Drawing.Point(446, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "再生チャンネル";
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(12, 120);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(508, 139);
            this.listBox1.TabIndex = 12;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.listBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox1_DragEnter);
            this.listBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            // 
            // lblPos
            // 
            this.lblPos.AutoSize = true;
            this.lblPos.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPos.Location = new System.Drawing.Point(12, 87);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(101, 15);
            this.lblPos.TabIndex = 13;
            this.lblPos.Text = "9999/9999packs";
            // 
            // btnEncode
            // 
            this.btnEncode.Location = new System.Drawing.Point(445, 91);
            this.btnEncode.Name = "btnEncode";
            this.btnEncode.Size = new System.Drawing.Size(75, 23);
            this.btnEncode.TabIndex = 14;
            this.btnEncode.Text = "変換";
            this.btnEncode.UseVisualStyleBackColor = true;
            this.btnEncode.Click += new System.EventHandler(this.btnEncode_Click);
            // 
            // trackbar1
            // 
            this.trackbar1.BackColor = System.Drawing.Color.Transparent;
            this.trackbar1.Location = new System.Drawing.Point(12, 53);
            this.trackbar1.Name = "trackbar1";
            this.trackbar1.Size = new System.Drawing.Size(512, 31);
            this.trackbar1.TabIndex = 0;
            // 
            // numBit
            // 
            this.numBit.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numBit.Location = new System.Drawing.Point(381, 92);
            this.numBit.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numBit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBit.Name = "numBit";
            this.numBit.Size = new System.Drawing.Size(61, 22);
            this.numBit.TabIndex = 15;
            this.numBit.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numBit.ValueChanged += new System.EventHandler(this.numBit_ValueChanged);
            // 
            // rbVAG
            // 
            this.rbVAG.AutoSize = true;
            this.rbVAG.Location = new System.Drawing.Point(259, 94);
            this.rbVAG.Name = "rbVAG";
            this.rbVAG.Size = new System.Drawing.Size(47, 16);
            this.rbVAG.TabIndex = 16;
            this.rbVAG.TabStop = true;
            this.rbVAG.Text = "VAG";
            this.rbVAG.UseVisualStyleBackColor = true;
            this.rbVAG.CheckedChanged += new System.EventHandler(this.rbVAG_CheckedChanged);
            // 
            // rbADPCM
            // 
            this.rbADPCM.AutoSize = true;
            this.rbADPCM.Location = new System.Drawing.Point(312, 94);
            this.rbADPCM.Name = "rbADPCM";
            this.rbADPCM.Size = new System.Drawing.Size(63, 16);
            this.rbADPCM.TabIndex = 17;
            this.rbADPCM.TabStop = true;
            this.rbADPCM.Text = "ADPCM";
            this.rbADPCM.UseVisualStyleBackColor = true;
            this.rbADPCM.CheckedChanged += new System.EventHandler(this.rbADPCM_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 268);
            this.Controls.Add(this.rbADPCM);
            this.Controls.Add(this.rbVAG);
            this.Controls.Add(this.numBit);
            this.Controls.Add(this.btnEncode);
            this.Controls.Add(this.lblPos);
            this.Controls.Add(this.listBox1);
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
            ((System.ComponentModel.ISupportInitialize)(this.numBit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Trackbar trackbar1;
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
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.Button btnEncode;
        private System.Windows.Forms.NumericUpDown numBit;
        private System.Windows.Forms.RadioButton rbVAG;
        private System.Windows.Forms.RadioButton rbADPCM;
    }
}

