class VAG {
public:
	static const int SAMPLES = 64;
	static const int PACKSIZE = 16;
	static const int FILTER_INDEXES = 4;

private:
	const int DELTA_STEP[3] = {
		3, 12, 12
	};
	const double KE[FILTER_INDEXES] = {
		1.0 / 8.0,
		3.0 / 4.0,
		29.0 / 32.0,
		31.0 / 32.0
	};
	const double KD[FILTER_INDEXES] = {
		KE[0] / (1.0 - KE[0]),
		0.0,
		0.0,
		0.0
	};

private:
	double m_delta = 4.0;
	double m_predict = 0.0;
	double m_predict_d = 0.0;
	double m_filter_f = 0.0;
	double m_filter_df = 0.0;
	double m_filter_dr = 0.0;
	double m_err = 0.0;
	double mp_code[FILTER_INDEXES][SAMPLES] = { 0 };

private:
	inline void update(int code, double *p_delta, double *p_predict) {
		if (code < 0) {
			*p_delta = *p_delta * DELTA_STEP[-code] / 8.0;
		} else {
			*p_delta = *p_delta * DELTA_STEP[code] / 8.0;
		}
		if (*p_delta < 3) {
			*p_delta = 3;
		}
		if (16384 < *p_delta) {
			*p_delta = 16384;
		}
		*p_predict += code * *p_delta;
	}

public:
	void encode(short *p_input, unsigned char *p_output) {
		double delta[FILTER_INDEXES];
		double predict[FILTER_INDEXES];
		double predict_d[FILTER_INDEXES];
		double filter_f[FILTER_INDEXES];
		double filter_df[FILTER_INDEXES];
		double filter_dr[FILTER_INDEXES];
		double err[FILTER_INDEXES];
		double err_min = 100000.0;
		int f_index = 0;
		for (int f=0; f<FILTER_INDEXES; f++) {
			delta[f] = m_delta;
			predict[f] = m_predict;
			predict_d[f] = m_predict_d;
			filter_f[f] = m_filter_f;
			filter_df[f] = m_filter_df;
			filter_dr[f] = m_filter_dr;
			err[f] = m_err;
			double err_max = 0.0;
			for (int i=0; i<SAMPLES; i++) {
				/*** フィルタ ***/
				filter_f[f] = filter_f[f] * KE[f] + p_input[i] * (1.0 - KE[f]);
				/*** エンコード ***/
				double code = (filter_f[f] - predict[f]) / delta[f];
				if (code < -2) {
					code = -2;
				}
				if (1 < code) {
					code = 1;
				}
				mp_code[f][i] = code;
				update(code, &delta[f], &predict[f]);
				/*** 逆フィルタ ***/
				double filter_r = predict[f] + (predict[f] - predict_d[f]) * KD[f];
				predict_d[f] = predict[f];
				/*** 誤差確認 ***/
				double diff_f = fabs(filter_f[f] - filter_df[f]);
				double diff_r = fabs(filter_r - filter_dr[f]);
				filter_df[f] = filter_f[f];
				filter_dr[f] = filter_r;
				err[f] = err[f] * 0.75 + fabs(diff_r - diff_f) * 0.25;
				if (err_max < err[f]) {
					err_max = err[f];
				}
			}
			/*** 誤差が最少となるフィルタ係数を選択 ***/
			if (err_max < err_min) {
				err_min = err_max;
				f_index = f;
			}
		}
		m_delta = delta[f_index];
		m_predict = predict[f_index];
		m_predict_d = predict_d[f_index];
		m_filter_f = filter_f[f_index];
		m_filter_df = filter_df[f_index];
		m_filter_dr = filter_dr[f_index];
		m_err = err[f_index];
		for (int i=0, b=0; i<SAMPLES; i+=4, b++) {
			p_output[b] = 0;
			for (int j=0, k=i; j<4 && k < SAMPLES; j++, k++) {
				p_output[b] |= static_cast<unsigned char>(mp_code[f_index][k] + 2) << (2 * j);
			}
		}
	}

	void decode(short *p_output, unsigned char *p_input) {
		for (int i=0, b=0; i<SAMPLES; i+=4, b++) {
			for (int j=0, k=i; j<4 && k < SAMPLES; j++, k++) {
				/*** デコード ***/
				int code = ((p_input[b] >> (2 * j)) & 3) - 2;
				update(code, &m_delta, &m_predict);
				/*** 出力 ***/
				double output = m_predict;
				if (output < -32768) {
					output = -32768;
				}
				if (32767 < output) {
					output = 32767;
				}
				p_output[k] = static_cast<short>(output);
			}
		}
	}
}
