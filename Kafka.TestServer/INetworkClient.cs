﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.TestServer
{
    public interface INetworkClient : ISendingNetworkClient
    {
        ValueTask<int> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default);
    }
}