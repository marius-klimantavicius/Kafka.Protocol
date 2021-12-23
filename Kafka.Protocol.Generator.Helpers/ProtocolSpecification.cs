﻿using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Kafka.Protocol.Generator.Helpers.BackusNaurForm.Parsers;
using Kafka.Protocol.Generator.Helpers.Definitions;
using Kafka.Protocol.Generator.Helpers.Definitions.Messages;
using Kafka.Protocol.Generator.Helpers.Definitions.Parsers;
using Kafka.Protocol.Generator.Helpers.Extensions;
using Field = Kafka.Protocol.Generator.Helpers.Definitions.Field;

namespace Kafka.Protocol.Generator.Helpers
{
    public class ProtocolSpecification
    {
        private readonly HtmlDocument _definition;

        public static ProtocolSpecification Load(HtmlDocument definition)
        {
            return new ProtocolSpecification(definition);
        }

        private ProtocolSpecification(HtmlDocument definition)
        {
            _definition = definition;

            ErrorCodes = ParseErrorCodes();
            PrimitiveTypes = ParsePrimitiveTypes();
            // UINT16 is missing from the documentation but found in the message specifications
            PrimitiveTypes.Add("UINT16", new PrimitiveType
            {
                Type = "UINT16", 
                Description = "Represents an integer between 0 and 2^16-1 inclusive. The values are encoded using four bytes in network byte order (big-endian)."
            });
            // UVARINT is missing from the documentation but found in the message specifications
            PrimitiveTypes.Add("UVARINT", new PrimitiveType
            {
                Type = "UVARINT",
                Description = "The UNSIGNED_VARINT type describes an unsigned variable length integer."
            });

            // Incorrect protocol definition: Array is never referenced as a primitive type within messages. [] is used.
            PrimitiveTypes.Remove("ARRAY");
            // Compact types are still the same base types but with more efficient length compaction. They are used interchangeable when the message is a variable version
            PrimitiveTypes.Remove("COMPACT_ARRAY");
            PrimitiveTypes.Remove("COMPACT_BYTES");
            PrimitiveTypes.Remove("COMPACT_STRING");
            // Records is a complex type and is hand-rolled
            PrimitiveTypes.Remove("RECORDS");

            RequestHeader = ParseRequestHeader();
            ResponseHeader = ParseResponseHeader();

            //MessageEnvelope = ParseRequestAndResponseStructure();
        }

        public IDictionary<string, PrimitiveType> PrimitiveTypes { get; set; }

        public IDictionary<int, ErrorCode> ErrorCodes { get; }

        public IDictionary<int, Message> Messages { get; } = new Dictionary<int, Message>();

        public Header RequestHeader { get; }

        public Header ResponseHeader { get; }

        private const string ProtocolRequestHeaderXPath = "//*/pre[starts-with(text(),'Request Header')]";

        private Header ParseRequestHeader()
        {
            var headerNode = _definition
                .DocumentNode
                .SelectFirst(ProtocolRequestHeaderXPath);

            return ParseHeader(headerNode);
        }

        private const string ProtocolResponseHeaderXPath = "//*/pre[starts-with(text(),'Response Header')]";

        private Header ParseResponseHeader()
        {
            var headerNode = _definition
                .DocumentNode
                .SelectFirst(ProtocolResponseHeaderXPath);

            return ParseHeader(headerNode);
        }

        private Header ParseHeader(HtmlNode headerNode)
        {
            var headerDefinition = System.Net.WebUtility.HtmlDecode(
                headerNode
                    .InnerText);

            var descriptionTable = headerNode
                .GetFirstSiblingNamed("table")
                .ParseTableNodeTo<FieldDescription>()
                .ToList();

            var specification = BackusNaurParser.Parse(
                new Buffer<char>(
                    headerDefinition
                        .ToCharArray()));

            var header = HeaderParser.Parse(specification, PrimitiveTypes.Values);
        
            SetDescriptions(header.Fields, descriptionTable);

            return header;
        }
        
        private static void SetDescriptions(List<Field> fields, List<FieldDescription> fieldDescriptions)
        {
            foreach (var field in fields)
            {
                field.Description = fieldDescriptions
                    .FirstOrDefault(
                        description =>
                            description.Field == field.Name)?.Description;
            }
        }

        private const string ProtocolErrorCodesXPath = "//*[contains(@id,'protocol_error_codes')]/..";

        private IDictionary<int, ErrorCode> ParseErrorCodes()
        {
            return _definition
                .DocumentNode
                .SelectFirst(ProtocolErrorCodesXPath)
                .GetFirstSiblingNamed("table")
                .ParseTableNodeTo<ErrorCode>()
                .ToDictionary(
                    errorCode => errorCode.Code);
        }

        private const string PrimitiveTypesXPath = "//*[contains(@id,'protocol_types')]/..";

        private IDictionary<string, PrimitiveType> ParsePrimitiveTypes()
        {
            return _definition
                .DocumentNode
                .SelectFirst(PrimitiveTypesXPath)
                .GetFirstSiblingNamed("table")
                .ParseTableNodeTo<PrimitiveType>()
                .ToDictionary(
                    type => type.Type);
        }
    }
}