using System.IO;

class ADPCM2 {
	public const int PACKING_SAMPLES = 8;
	public const int PACKING_BYTES = 3;

	public int SAMPLES { get; private set; }

	const int MAX_VALUE = 3;
	const int MIN_VALUE = -4;
	const int BIT = 3;
	const int MASK = (1 << BIT) - 1;
	readonly double[] DELTA_STEP = {
		0.75, 1.0, 1.25, 1.75, 1.75
	};

	double m_delta = 4.0;
	double m_predict = 0.0;
	double m_filter = 0.0;

	public ADPCM2(int samples) {
		SAMPLES = samples;
	}

	void update(int code) {
		if (code < 0) {
			m_delta *= DELTA_STEP[-code];
		} else {
			m_delta *= DELTA_STEP[code];
		}
		if (m_delta < 3) {
			m_delta = 3;
		}
		if (12288 < m_delta) {
			m_delta = 12288;
		}
		m_predict += code * m_delta;
	}

	public void Encode(short[] p_input, byte[] p_output) {
		for (int si = 0, bi = 0; si < SAMPLES; si += PACKING_SAMPLES, bi += PACKING_BYTES) {
			int output = 0;
			for (int j = 0, sj = si; j < PACKING_SAMPLES && sj < SAMPLES; j++, sj++) {
				/*** フィルタ ***/
				m_filter = (m_filter + p_input[sj]) * 0.5;
				/*** エンコード ***/
				var code = (int)((m_filter - m_predict) / m_delta);
				if (code < MIN_VALUE) {
					code = MIN_VALUE;
				}
				if (MAX_VALUE < code) {
					code = MAX_VALUE;
				}
				update(code);
				if (code < 0) {
					output |= (code + MASK + 1) << (BIT * j);
				} else {
					output |= code << (BIT * j);
				}
			}
			for (int j = 0, bj = bi; j < PACKING_BYTES; j++, bj++) {
				p_output[bj] = (byte)((output >> (8 * j)) & 0xFF);
			}
		}
	}
	public void Decode(short[] p_output, byte[] p_input) {
		for (int si = 0, bi = 0; si < SAMPLES; si += PACKING_SAMPLES, bi += PACKING_BYTES) {
			int input = 0;
			for (int j = 0, bj = bi; j < PACKING_BYTES; j++, bj++) {
				input |= p_input[bj] << (8 * j);
			}
			for (int j = 0, sj = si; j < PACKING_SAMPLES && sj < SAMPLES; j++, sj++) {
				/*** デコード ***/
				int code = (input >> (BIT * j)) & MASK;
				if (MAX_VALUE < code) {
					code -= MASK + 1;
				}
				update(code);
				/*** 出力 ***/
				var output = m_predict + (m_predict - m_filter);
				m_filter = m_predict;
				if (output < -32768) {
					output = -32768;
				}
				if (32767 < output) {
					output = 32767;
				}
				p_output[sj] = (short)output;
			}
		}
	}

	public static bool EncodeFile(string inputPath, string outputPath) {
		var wav = new RiffWave(inputPath);
		if (!wav.IsLoadComplete) {
			wav.Close();
			return false;
		}

		var fs = new FileStream(outputPath, FileMode.Create);
		var bw = new BinaryWriter(fs);
		bw.Write(wav.SampleRate);
		bw.Write(wav.Channels);

		wav.AllocateBuffer(PACKING_SAMPLES);

		switch (wav.Channels) {
		case 1: {
			var adpcm = new ADPCM2(PACKING_SAMPLES);
			var input = new short[PACKING_SAMPLES];
			var output = new byte[PACKING_BYTES];
			for (int i = 0; i < wav.Samples; i += PACKING_SAMPLES) {
				wav.SetBuffer(input);
				adpcm.Encode(input, output);
				fs.Write(output, 0, PACKING_BYTES);
			}
			break;
		}
		case 2: {
			var adpcmL = new ADPCM2(PACKING_SAMPLES);
			var adpcmR = new ADPCM2(PACKING_SAMPLES);
			var inputL = new short[PACKING_SAMPLES];
			var inputR = new short[PACKING_SAMPLES];
			var outputL = new byte[PACKING_BYTES];
			var outputR = new byte[PACKING_BYTES];
			for (int i = 0; i < wav.Samples; i += PACKING_SAMPLES) {
				wav.SetBuffer(inputL, inputR);
				adpcmL.Encode(inputL, outputL);
				adpcmR.Encode(inputR, outputR);
				fs.Write(outputL, 0, PACKING_BYTES);
				fs.Write(outputR, 0, PACKING_BYTES);
			}
			break;
		}
		}
		fs.Close();
		fs.Dispose();
		wav.Close();
		return true;
	}

	public static void DecodeFile(string inputPath, string outputPath) {
		var fs = new FileStream(inputPath, FileMode.Open);
		var br = new BinaryReader(fs);
		var sampleRate = br.ReadInt32();
		var channels = br.ReadInt32();
		var wav = new RiffWave(
			outputPath,
			2==channels ? RiffWave.TYPE.INT16_CH2 : RiffWave.TYPE.INT16_CH1,
			sampleRate
		);
		wav.AllocateBuffer(PACKING_SAMPLES);

		switch (wav.Channels) {
		case 1: {
			var adpcm = new ADPCM2(PACKING_SAMPLES);
			var output = new short[PACKING_SAMPLES];
			var input = new byte[PACKING_BYTES];
			while (fs.Position < fs.Length) {
				fs.Read(input, 0, PACKING_BYTES);
				adpcm.Decode(output, input);
				wav.Write(output);
			}
			break;
		}
		case 2: {
			var adpcmL = new ADPCM2(PACKING_SAMPLES);
			var adpcmR = new ADPCM2(PACKING_SAMPLES);
			var outputL = new short[PACKING_SAMPLES];
			var outputR = new short[PACKING_SAMPLES];
			var inputL = new byte[PACKING_BYTES];
			var inputR = new byte[PACKING_BYTES];
			while (fs.Position < fs.Length) {
				fs.Read(inputL, 0, PACKING_BYTES);
				fs.Read(inputR, 0, PACKING_BYTES);
				adpcmL.Decode(outputL, inputL);
				adpcmR.Decode(outputR, inputR);
				wav.Write(outputL, outputR);
			}
			break;
		}
		}
		fs.Close();
		fs.Dispose();
		wav.Close();
	}
}