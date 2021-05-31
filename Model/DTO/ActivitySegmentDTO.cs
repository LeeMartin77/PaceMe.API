using System;
using System.Collections.Generic;
using System.Linq;
using PaceMe.Model.Record;

namespace PaceMe.Model.DTO
{
    public record ActivitySegmentDTO
    {
        public Guid ActivityId { get; init; }
        public Guid ActivitySegmentId { get; init; }
        public string Name { get; init; }
        public int Order { get; init; }
        public int Repetitions { get; init; }
        public SegmentIntervalRecord[] Intervals { get; init; } = new SegmentIntervalRecord[0];
    }

    public static class ActivitySegmentDTOBuilder 
    {
        public static ActivitySegmentDTO FromRecordWithSegments(ActivitySegmentRecord record, IEnumerable<SegmentIntervalRecord> intervals)
        {
            return new ActivitySegmentDTO {
                ActivityId = record.ActivityId,
                ActivitySegmentId = record.ActivitySegmentId,
                Name = record.Name,
                Order = record.Order,
                Repetitions = record.Repetitions,
                Intervals = intervals.ToArray()
            };
        }
        public static ActivitySegmentDTO FromRecord(ActivitySegmentRecord record)
        {
            return new ActivitySegmentDTO {
                ActivityId = record.ActivityId,
                ActivitySegmentId = record.ActivitySegmentId,
                Name = record.Name,
                Order = record.Order,
                Repetitions = record.Repetitions
            };
        }
        public static (ActivitySegmentRecord, SegmentIntervalRecord[]) ToRecordSegmentTuple(ActivitySegmentDTO dto)
        {
            return (
                new ActivitySegmentRecord 
                {
                    ActivityId = dto.ActivityId,
                    ActivitySegmentId = dto.ActivitySegmentId,
                    Name = dto.Name,
                    Order = dto.Order,
                    Repetitions = dto.Repetitions
                },
                dto.Intervals
            );
        }
    }
}
