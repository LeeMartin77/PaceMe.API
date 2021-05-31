using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using PaceMe.Model.Record;

namespace PaceMe.Storage.Repository
{
    public interface ISegmentIntervalRepository : IBaseRepository<SegmentIntervalRecord>
    {
        Task<IEnumerable<SegmentIntervalRecord>> GetForParentId(Guid segmentId);
    }

    public class SegmentIntervalRepository : BaseRepository<SegmentIntervalRecord>, ISegmentIntervalRepository
    {
        public SegmentIntervalRepository(IConfiguration configuration): base("TrainingPlanActivitySegmentInterval", configuration){
        }

        public override DynamicTableEntity GenerateEntityFromRecord(SegmentIntervalRecord record)
        {
            return new DynamicTableEntity(record.SegmentId.ToString(), record.SegmentIntervalId.ToString()){
                Properties = TableEntity.Flatten(record, new OperationContext())
            };
        }

    }
}