using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record ActivitySegmentRecord
    {
        public Guid ActivityId { get; init; }
        public Guid ActivitySegmentId { get; init; }
        public string Name { get; init; }
        public int Order { get; init; }
        public int Repetitions { get; init; }
    }
}
