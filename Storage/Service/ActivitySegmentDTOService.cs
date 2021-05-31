using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using PaceMe.Storage.Utilities;
using System;
using PaceMe.Model.DTO;

namespace PaceMe.Storage.Service 
{

    public interface IActivitySegmentDTOService : IBaseService<ActivitySegmentDTO> { 
        Task<ActivitySegmentDTO[]> GetForParentId(Guid traingPlanActivityId);
    }

    public class ActivitySegmentDTOService : IActivitySegmentDTOService
    {
        public Task<ActivitySegmentDTO[]> GetForParentId(Guid traingPlanActivityId)
        {
            throw new NotImplementedException();
        }
        
        public Task Create(ActivitySegmentDTO record)
        {
            throw new NotImplementedException();
        }

        public Task<ActivitySegmentDTO> Get(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task Delete(ActivitySegmentDTO record)
        {
            throw new NotImplementedException();
        }

        public Task Update(ActivitySegmentDTO record)
        {
            throw new NotImplementedException();
        }

    }
}