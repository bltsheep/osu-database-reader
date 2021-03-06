using System;
using System.IO;
using System.Text;
using osu.Shared;
using osu.Shared.Serialization;

namespace osu_database_reader.Components.Player
{
    public class Replay : ISerializable //used for both scores.db and .osr files
    {
        public GameMode GameMode;
        public int OsuVersion;
        public string BeatmapHash, PlayerName, ReplayHash;  //may the chickenmcnuggets be with you
        public ushort Count300, Count100, Count50, CountGeki, CountKatu, CountMiss;
        public int Score;
        public ushort Combo;
        public bool FullCombo;
        public Mods Mods;
        public string LifeGraphData;    //null in scores.db, TODO: parse this when implementing .osr
        public DateTime TimePlayed;

        public byte[] ReplayData
        {
            get => _replayData;
            set {
                if (_replayData != value) {
                    _frames = null;
                    _replayData = value;
                }
            }
        }

        public ReplayFrame[] ReplayFrames
        {
            get {
                if (_replayData == null) return null;
                if (_frames != null)
                    return _frames;
                else {
                    byte[] decomp = LZMACoder.Decompress(_replayData);
                    string str = Encoding.ASCII.GetString(decomp); //ascii should be faster than UTF8, though not that it matters
                    return _frames = ReplayFrame.FromStrings(ref str);
                }
            }
        }

        public long? ScoreId;

        private byte[] _replayData; //null in scores.db
        private ReplayFrame[] _frames;
        private bool _readScoreId;

        public static Replay Read(string path) {
            Replay replay;
            using (var r = new SerializationReader(File.OpenRead(path))) {
                replay = ReadFromReader(r); //scoreid should not be needed
            }
            return replay;
        }

        public static Replay ReadFromReader(SerializationReader r, bool readScoreId = false) {
            var replay = new Replay();
            replay._readScoreId = readScoreId;
            replay.ReadFromStream(r);
            return replay;
        }

        public void ReadFromStream(SerializationReader r)
        {
            GameMode = (GameMode) r.ReadByte();
            OsuVersion = r.ReadInt32();
            BeatmapHash = r.ReadString();
            PlayerName = r.ReadString();
            ReplayHash = r.ReadString();

            Count300 = r.ReadUInt16();
            Count100 = r.ReadUInt16();
            Count50 = r.ReadUInt16();
            CountGeki = r.ReadUInt16();
            CountKatu = r.ReadUInt16();
            CountMiss = r.ReadUInt16();

            Score = r.ReadInt32();
            Combo = r.ReadUInt16();
            FullCombo = r.ReadBoolean();
            Mods = (Mods) r.ReadInt32();
            LifeGraphData = r.ReadString();
            TimePlayed = r.ReadDateTime();
            _replayData = r.ReadBytes();
            ScoreId = _readScoreId ? r.ReadInt64() : (long?) null;
        }

        public void WriteToStream(SerializationWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
