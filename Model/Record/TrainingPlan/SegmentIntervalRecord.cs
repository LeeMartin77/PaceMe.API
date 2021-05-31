using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record SegmentIntervalRecord
    {
        public Guid SegmentId { get; init; }
        public Guid SegmentIntervalId { get; init; }
        public string Note { get; init; }
        public int Order { get; init; }
        public IntervalType IntervalType { get; init; }
        public int DistanceMeters { get; init; }
        public int DurationSeconds { get; init; }
    }

    public enum IntervalType 
    {
        Distance,
        Duration,
        DistanceDuration,
        PaceDuration,
        PaceTime
    }
}
