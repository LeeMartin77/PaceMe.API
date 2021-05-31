using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using PaceMe.Storage.Utilities;
using System;
using PaceMe.Model.DTO;
using PaceMe.Storage.Repository;

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
        
        public async Task<Guid> Create(ActivitySegmentDTO record)
        {
            throw new NotImplementedException();
        }

        public async Task<ActivitySegmentDTO> Get(Guid recordId)
        {
            var segment = await _activitySegmentRepository.GetById(recordId);

            var segmentIntervalRecords =  await _segmentIntervalRepository.GetForParentId(recordId);
            
            return ActivitySegmentDTOBuilder.FromRecordWithSegments(segment, segmentIntervalRecords);
        }

        public async Task Delete(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public async Task Update(ActivitySegmentDTO record)
        {
            throw new NotImplementedException();
        }

    }
}