﻿namespace Kafka.Protocol.Generator.BackusNaurForm
{
    internal class SymbolReference
    {
        internal SymbolReference(
            string name)
        {
            Name = name;
        }

        public string Name { get; }
        
        public override string ToString()
        {
            return Name;
        }
    }
}