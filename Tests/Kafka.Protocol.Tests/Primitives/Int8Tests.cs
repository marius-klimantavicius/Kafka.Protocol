﻿using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using Test.It.With.XUnit;
using Xunit;

namespace Kafka.Protocol.Tests.Primitives
{
    public partial class Given_an_int8
    {
        public class When_writing : XUnit2SpecificationAsync
        {
            private readonly byte[] _buffer = new byte[1];
            private readonly Int8 _value = 65;
            private MemoryStream _stream = default!;

            protected override Task GivenAsync()
            {
                _stream = new MemoryStream(_buffer);
                return base.GivenAsync();
            }

            protected override async Task WhenAsync()
            {
                await _value.WriteToAsync(_stream, false)
                    .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_parse_correctly()
            {
                _buffer.Should()
                    .Equal(65);
            }

            [Fact]
            public void It_should_report_correct_size()
            {
                _value.GetSize(false).Should().Be(1);
            }
        }

        public class When_reading : XUnit2SpecificationAsync
        {
            private PipeReader _reader = default!;
            private Int8 _value;

            protected override async Task GivenAsync()
            {
                _reader = await new byte[] { 65 }.ToReaderAsync()
                    .ConfigureAwait(false);
            }

            protected override async Task WhenAsync()
            {
                _value = await Int8.FromReaderAsync(_reader, false)
                    .ConfigureAwait(false);
            }

            [Fact]
            public void It_should_parse_correctly()
            {
                _value.Value.Should().Be(65);
            }
        }
    }
}