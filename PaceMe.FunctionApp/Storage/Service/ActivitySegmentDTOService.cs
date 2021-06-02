using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using PaceMe.Storage.Utilities;
using System;
using PaceMe.Model.DTO;
using PaceMe.Storage.Repository;
using PaceMe.Model.Record;

namespace PaceMe.Storage.Service 
{

    public interface IActivitySegmentDTOService : IBaseService<ActivitySegmentDTO, Guid> { 
        Task<ActivitySegmentDTO[]> GetForParentId(Guid traingPlanActivityId);
    }

    public class ActivitySegmentDTOService : IActivitySegmentDTOService
    {
        private readonly IActivitySegmentRepository _activitySegmentRepository;
        private readonly ISegmentIntervalRepository _segmentIntervalRepository;

        public ActivitySegmentDTOService(IActivitySegmentRepository activitySegmentRepository, ISegmentIntervalRepository segmentIntervalRepository)
        {
            _activitySegmentRepository = activitySegmentRepository;
            _segmentIntervalRepository = segmentIntervalRepository;
        }

        public async Task<ActivitySegmentDTO[]> GetForParentId(Guid traingPlanActivityId)
        {
            var segmentRecords = await _activitySegmentRepository.GetForParentId(traingPlanActivityId);

            var segmentIntervalRecords = await Task.WhenAll(
                segmentRecords.Select(async x => (x.ActivitySegmentId, Intervals: await _segmentIntervalRepository.GetForParentId(x.ActivitySegmentId)))
                );
            
            return segmentRecords.Select(s => 
                ActivitySegmentDTOBuilder.FromRecordWithSegments(s, segmentIntervalRecords.First(i => i.ActivitySegmentId == s.ActivitySegmentId).Intervals))
                .ToArray();
        }
        
        public async Task<Guid> Create(ActivitySegmentDTO activitySegment)
        {
            (var activitySegmentRecord, var segmentIntervalRecords) = ActivitySegmentDTOBuilder.ToRecordSegmentTuple(activitySegment);
            var createSegment = new ActivitySegmentRecord {
                ActivityId = activitySegmentRecord.ActivityId,
                ActivitySegmentId = Guid.NewGuid(),
                Name = activitySegmentRecord.Name,
                Order = activitySegmentRecord.Order,
                Repetitions = activitySegmentRecord.Repetitions
            };
            var createIntervals = segmentIntervalRecords.Select(x => new SegmentIntervalRecord {
                SegmentId = createSegment.ActivitySegmentId,
                SegmentIntervalId = Guid.NewGuid(),
                Note = x.Note,
                Order = x.Order,
                IntervalType = x.IntervalType,
                DistanceMeters = x.DistanceMeters,
                DurationSeconds = x.DurationSeconds
            });
            await _activitySegmentRepository.Create(createSegment);
            await Task.WhenAll(createIntervals.Select(i => _segmentIntervalRepository.Create(i)));
            return createSegment.ActivitySegmentId;
        }

        public async Task<ActivitySegmentDTO> Get(Guid recordId)
        {
            var segment = await _activitySegmentRepository.GetById(recordId);

            var segmentIntervalRecords =  await _segmentIntervalRepository.GetForParentId(recordId);
            
            return ActivitySegmentDTOBuilder.FromRecordWithSegments(segment, segmentIntervalRecords);
        }

        public async Task Delete(Guid recordId)
        {
            var segment = await _activitySegmentRepository.GetById(recordId);

            var segmentIntervalRecords =  await _segmentIntervalRepository.GetForParentId(recordId);

            await Task.WhenAll(segmentIntervalRecords.Select(i => _segmentIntervalRepository.Delete(i)).Append(_activitySegmentRepository.Delete(segment)));
        }

        public async Task Update(ActivitySegmentDTO activitySegment)
        {
            (var activitySegmentRecord, var segmentIntervalRecords) = ActivitySegmentDTOBuilder.ToRecordSegmentTuple(activitySegment);
            var originalIntervals = await _segmentIntervalRepository.GetForParentId(activitySegment.ActivitySegmentId);
            await Task.WhenAll(
                segmentIntervalRecords.Select(interval => CreateUpdateOrDeleteInterval(originalIntervals, interval))
                .Append(_activitySegmentRepository.Update(activitySegmentRecord))
                );
        }

        private Task CreateUpdateOrDeleteInterval(IEnumerable<SegmentIntervalRecord> originalIntervals, SegmentIntervalRecord interval)
        {
            if (interval.SegmentIntervalId == null || interval.SegmentIntervalId == Guid.Empty)
            {
                var createInterval = new SegmentIntervalRecord 
                {
                    SegmentId = interval.SegmentId,
                    SegmentIntervalId = Guid.NewGuid(),
                    Note = interval.Note,
                    Order = interval.Order,
                    IntervalType = interval.IntervalType,
                    DistanceMeters = interval.DistanceMeters,
                    DurationSeconds = interval.DurationSeconds
                };
                return _segmentIntervalRepository.Create(createInterval);
            }
            if(originalIntervals.Select(x => x.SegmentIntervalId).Contains(interval.SegmentIntervalId))
            {
                return _segmentIntervalRepository.Update(interval);
            }
            return _segmentIntervalRepository.Delete(interval);
        }
    }
}