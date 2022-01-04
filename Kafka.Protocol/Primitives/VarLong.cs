﻿using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Kafka.Protocol
{
    public partial struct VarLong
    {
        private ulong EncodeAsZigZag() =>
            (ulong)((Value << 1) ^ (Value >> 63));

        public int GetSize(bool _ = false)
        {
            var value = EncodeAsZigZag();
            var length = 0;
            do
            {
                // Remove 7 bits
                value >>= 7;
                length++;
            } while (value > 0);

            return length;
        }

        public ValueTask WriteToAsync(Stream writer, bool _ = false,
            CancellationToken cancellationToken = default) =>
            writer.WriteAsLittleEndianAsync(
                Value
                    .EncodeAsZigZag()
                    .EncodeAsVarInt(), cancellationToken);

        public static async ValueTask<VarLong> FromReaderAsync(
            PipeReader reader,
            bool _ = false,
            CancellationToken cancellationToken = default)
        {
            var more = true;
            ulong value = 0;
            var shift = 0;
            while (more)
            {
                var lowerBits = (await reader
                    .ReadAsLittleEndianAsync(1, cancellationToken)
                    .ConfigureAwait(false)).First();

                more = (lowerBits & 128) != 0;
                value |= (uint)((lowerBits & 0x7f) << shift);
                shift += 7;
            }

            return value.DecodeFromZigZag();
        }
    }
}