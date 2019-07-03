﻿using System;

namespace Kafka.Protocol.Records
{
    public class RecordBatch : ISerialize
    {
        public Int64 BaseOffset { get; set; } = Int64.Default;
        public Int32 BatchLength { get; set; } = Int32.Default;
        public Int32 PartitionLeaderEpoch { get; set; } = Int32.Default;
        public Int8 Magic { get; set; } = Int8.Default;
        public Int32 Crc { get; set; } = Int32.Default;
        public Int16 Attributes { get; set; } = Int16.Default;
        public Int32 LastOffsetDelta { get; set; } = Int32.Default;
        public Int64 FirstTimestamp { get; set; } = Int64.Default;
        public Int64 MaxTimestamp { get; set; } = Int64.Default;
        public Int64 ProducerId { get; set; } = Int64.Default;
        public Int16 ProducerEpoch { get; set; } = Int16.Default;
        public Int32 BaseSequence { get; set; } = Int32.Default;
        public Record[] Records { get; set; } = new Record[0];
        public ControlRecord ControlRecord { get; set; }

        public Compressions Compression => new Compressions(this);

        public class Compressions
        {
            private readonly RecordBatch _recordBatch;

            internal Compressions(RecordBatch recordBatch)
            {
                _recordBatch = recordBatch;
            }

            private enum Attribute
            {
                NoCompression = 0,
                Gzip = 1,
                Snappy = 2,
                Lz4 = 3,
                Zstd = 4,
            }

            public bool NoCompression =>
                _recordBatch.Attributes.GetValueOfBitRange(0, 2) ==
                (int) Attribute.NoCompression;

            public bool Gzip =>
                _recordBatch.Attributes.GetValueOfBitRange(0, 2) ==
                (int) Attribute.Gzip;

            public bool Snappy =>
                _recordBatch.Attributes.GetValueOfBitRange(0, 2) ==
                (int) Attribute.Snappy;

            public bool Lz4 =>
                _recordBatch.Attributes.GetValueOfBitRange(0, 2) ==
                (int) Attribute.Lz4;

            public bool Zstd =>
                _recordBatch.Attributes.GetValueOfBitRange(0, 2) ==
                (int) Attribute.Zstd;
        }

        public void ReadFrom(IKafkaReader reader)
        {
            BaseOffset = Int64.From(reader.ReadInt64());
            BatchLength = Int32.From(reader.ReadInt32());
            PartitionLeaderEpoch = Int32.From(reader.ReadInt32());
            Magic = Int8.From(reader.ReadInt8());
            Crc = Int32.From(reader.ReadInt32());
            Attributes = Int16.From(reader.ReadInt16());
            LastOffsetDelta = Int32.From(reader.ReadInt32());
            FirstTimestamp = Int64.From(reader.ReadInt64());
            MaxTimestamp = Int64.From(reader.ReadInt64());
            ProducerId = Int64.From(reader.ReadInt64());
            ProducerEpoch = Int16.From(reader.ReadInt16());
            BaseSequence = Int32.From(reader.ReadInt32());
            Records = reader.Read(() => new Record());
        }

        public void WriteTo(IKafkaWriter writer)
        {
            writer.WriteInt64(BaseOffset.Value);
            writer.WriteInt32(BatchLength.Value);
            writer.WriteInt32(PartitionLeaderEpoch.Value);
            writer.WriteInt8(Magic.Value);
            writer.WriteInt32(Crc.Value);
            writer.WriteInt16(Attributes.Value);
            writer.WriteInt32(LastOffsetDelta.Value);
            writer.WriteInt64(FirstTimestamp.Value);
            writer.WriteInt64(MaxTimestamp.Value);
            writer.WriteInt64(ProducerId.Value);
            writer.WriteInt16(ProducerEpoch.Value);
            writer.WriteInt32(BaseSequence.Value);
            writer.Write(Records);
        }
    }

    internal static class Int16Extensions
    {
        internal static bool IsBitSet(this Int16 value, int bitNumber)
        {
            if (bitNumber < 0 ||
                bitNumber > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(bitNumber), "Must be in range 0-15");
            }

            if (value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Must be equal or greater than 0");
            }

            return ((int)value.Value)
                .IsBitSet(bitNumber);
        }

        internal static int GetValueOfBitRange(this Int16 value, 
            int fromBit, 
            int toBit)
        {
            return ((int) value.Value).GetValueOfBitRange(fromBit, toBit);
        }
    }
}