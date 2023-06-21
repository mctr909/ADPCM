class ADPCM2 {
	public int SAMPLES { get; private set; }

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

	public void encode(short[] p_input, byte[] p_output) {
		for (int i = 0, b = 0; i < SAMPLES; i += 2, b++) {
			int output = 0;
			for (int j = 0, s = i; j < 2 && s < SAMPLES; j++, s++) {
				/*** フィルタ ***/
				m_filter = (m_filter + p_input[s]) * 0.5;
				/*** エンコード ***/
				var code = (int)((m_filter - m_predict) / m_delta);
				if (code < -4) {
					code = -4;
				}
				if (3 < code) {
					code = 3;
				}
				output |= (byte)((code + 4) << (3 * j));
				update(code);
			}
			p_output[b] = (byte)output;
		}
	}

	public void decode(short[] p_output, byte[] p_input) {
		for (int i = 0, b = 0; i < SAMPLES; i += 2, b++) {
			for (int j = 0, s = i; j < 2 && s < SAMPLES; j++, s++) {
				/*** デコード ***/
				int code = ((p_input[b] >> (3 * j)) & 7) - 4;
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
				p_output[s] = (short)output;
			}
		}
	}
}
