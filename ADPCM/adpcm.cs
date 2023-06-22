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
				output |= (byte)((code - MIN_VALUE) << (BIT * j));
				update(code);
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
				int code = ((input >> (BIT * j)) & MASK) + MIN_VALUE;
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
}
