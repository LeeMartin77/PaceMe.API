using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record TrainingPlanActivitySegmentIntervalRecord
    {
        public Guid TrainingPlanActivitySegmentId { get; init; }
        public Guid TrainingPlanActivitySegmentIntervalId { get; init; }
        public string Note { get; init; }
        public int Order { get; init; }
        public IntervalType Type { get; init; }
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
