using System.IO;

namespace ADPCM {
    internal class RiffWave : RiffFile {
        public ushort Tag { get; set; }
        public int Channels { get; set; }
        public int SampleRate { get; set; }
        public int Bits { get; set; }

        public bool IsLoadComplete { get; private set; } = false;
        public long Length {
            get { return mFs.Length; }
        }
        public long Position {
            get { return mFs.Position; }
            set { mFs.Position = value; }
        }

        long mPosData = 0;
        long mPosFmt = 0;

        public RiffWave(string path) : base(path) {}

        protected override bool CheckType(string type) {
            return "WAVE" == type;
        }

        protected override bool LoadChunk(string type, int size) {
            switch (type) {
            case "data":
                mPosData = mFs.Position;
                break;
            case "fmt ":
                mPosFmt = mFs.Position;
                break;
            }
            return false;
        }

        protected override bool LoadList(string type, int size) {
            return false;
        }

        protected override void LoadComplete() {
            var br = new BinaryReader(mFs);
            mFs.Position = mPosFmt;
            Tag = br.ReadUInt16();
            Channels = br.ReadUInt16();
            SampleRate = br.ReadInt32();
            br.ReadInt32();
            br.ReadUInt16();
            Bits = br.ReadUInt16();
            mFs.Position = mPosData;
            IsLoadComplete = true;
        }

        public void SetData(byte[] data) {
            mFs.Read(data, 0, data.Length);
        }
    }
}
