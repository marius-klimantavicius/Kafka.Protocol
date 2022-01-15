﻿using System.Collections.Generic;

namespace Kafka.Protocol.Generator.Helpers.Definitions.Messages
{
    public class Field
    {
        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string Versions { get; set; } = default!;

        public string? NullableVersions { get; set; }
        
        public string? FlexibleVersions { get; set; }

        public int? Tag { get; set; }

        public string? TaggedVersions { get; set; }

        public bool Ignorable { get; set; }

        public string Default { get; set; } = default!;

        public string About { get; set; } = default!;

        public bool MapKey { get; set; }

        public List<Field>? Fields { get; set; }

        // Not part of the field definition
        public Message Message { get; set; } = default!;
        public Field? Parent { get; set; }
    }
}