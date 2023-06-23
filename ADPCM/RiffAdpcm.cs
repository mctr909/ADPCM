﻿using System.IO;

class RiffAdpcm : RiffFile {
    long mPosFmt;
    long mPosData;

    public RiffAdpcm(string inputPath) : base(inputPath) {
        if (IsLoadComplete) {
            mFs.Position = mPosData;
        }
    }

    protected override bool CheckType(string type) {
        return "ADPM" == type;
    }

    protected override bool LoadChunk(string type, int size) {
        switch (type) {
        case "fmt ":
            mPosFmt = mFs.Position;
            break;
        case "data":
            mPosData = mFs.Position;
            break;
        }
        return false;
    }

    protected override bool LoadList(string type, int size) {
        return false;
    }

    public void DecodeFile(string outputPath) {
        if (!IsLoadComplete) {
            return;
        }

        var br = new BinaryReader(mFs);

        mFs.Position = mPosFmt;
        var tag = br.ReadUInt16();
        var channels = br.ReadUInt16();
        var sampleRate = br.ReadInt32();
        var bytesPerSec = br.ReadUInt32();
        var packes = br.ReadUInt16();
        var type = (ADPCM2.TYPE)br.ReadUInt16();

        mFs.Position = mPosData;
        var wav = new RiffWave(
            outputPath,
            2 == channels ? RiffWave.TYPE.INT16_CH2 : RiffWave.TYPE.INT16_CH1,
            sampleRate
        );
        switch (wav.Channels) {
        case 1: {
            var adpcm = new ADPCM2(packes, type);
            var output = new short[adpcm.Samples];
            var input = new byte[adpcm.PackBytes];
            wav.AllocateBuffer(adpcm.Samples);
            while (mFs.Position < mFs.Length) {
                mFs.Read(input, 0, adpcm.PackBytes);
                adpcm.Decode(output, input);
                wav.WriteInt(output);
            }
            break;
        }
        case 2: {
            var adpcmL = new ADPCM2(packes, type);
            var adpcmR = new ADPCM2(packes, type);
            var outputL = new short[adpcmL.Samples];
            var outputR = new short[adpcmR.Samples];
            var inputL = new byte[adpcmL.PackBytes];
            var inputR = new byte[adpcmR.PackBytes];
            wav.AllocateBuffer(adpcmL.Samples);
            while (mFs.Position < mFs.Length) {
                mFs.Read(inputL, 0, adpcmL.PackBytes);
                mFs.Read(inputR, 0, adpcmR.PackBytes);
                adpcmL.Decode(outputL, inputL);
                adpcmR.Decode(outputR, inputR);
                wav.WriteInt(outputL, outputR);
            }
            break;
        }
        }
        wav.Close();
    }

    public static bool EncodeFile(string inputPath, string outputPath, ADPCM2.TYPE type, int packes) {
        var wav = new RiffWave(inputPath);
        if (!wav.IsLoadComplete) {
            wav.Close();
            return false;
        }

        var fs = new FileStream(outputPath, FileMode.Create);
        var bw = new BinaryWriter(fs);
        bw.Write(new byte[44]);

        switch (wav.Channels) {
        case 1: {
            var adpcm = new ADPCM2(packes, type);
            var input = new short[adpcm.Samples];
            var output = new byte[adpcm.PackBytes];
            wav.AllocateBuffer(adpcm.Samples);
            for (int i = 0; i < wav.Samples; i += adpcm.Samples) {
                wav.SetBufferInt(input);
                adpcm.Encode(input, output);
                fs.Write(output, 0, adpcm.PackBytes);
            }
            break;
        }
        case 2: {
            var adpcmL = new ADPCM2(packes, type);
            var adpcmR = new ADPCM2(packes, type);
            var inputL = new short[adpcmL.Samples];
            var inputR = new short[adpcmR.Samples];
            var outputL = new byte[adpcmL.PackBytes];
            var outputR = new byte[adpcmR.PackBytes];
            wav.AllocateBuffer(adpcmL.Samples);
            for (int i = 0; i < wav.Samples; i += adpcmL.Samples) {
                wav.SetBufferInt(inputL, inputR);
                adpcmL.Encode(inputL, outputL);
                adpcmR.Encode(inputR, outputR);
                fs.Write(outputL, 0, adpcmL.PackBytes);
                fs.Write(outputR, 0, adpcmR.PackBytes);
            }
            break;
        }
        }

        fs.Position = 0;
        bw.Write(new char[] { 'R', 'I', 'F', 'F' });
        bw.Write((uint)(fs.Length - 8));
        bw.Write(new char[] { 'A', 'D', 'P', 'M' });

        bw.Write(new char[] { 'f', 'm', 't', ' ' });
        bw.Write((uint)16);
        bw.Write((ushort)0);
        bw.Write((ushort)wav.Channels);
        bw.Write(wav.SampleRate);
        bw.Write((uint)(wav.SampleRate * wav.Channels * (int)type >> 3));
        bw.Write((ushort)packes);
        bw.Write((ushort)type);

        bw.Write(new char[] { 'd', 'a', 't', 'a' });
        bw.Write((uint)(fs.Length - 44));

        fs.Close();
        fs.Dispose();
        wav.Close();
        return true;
    }
}