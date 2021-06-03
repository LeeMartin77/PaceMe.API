using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaceMe.Model.DTO;
using PaceMe.Model.Record;
using PaceMe.Storage.Repository;
using PaceMe.Storage.Service;

namespace PaceMe.FunctionApp.Tests.Storage.Service.ActivitySegmentDTOServiceTests
{
    [TestClass]
    public class ActivitySegmentDTOService_GivenGetForParentId
    {
        private Mock<IActivitySegmentRepository> _mockActivitySegmentRepository;
        private Mock<ISegmentIntervalRepository> _mockSegmentIntervalRepository;
        
        [TestInitialize]
        public void Setup() {
            _mockActivitySegmentRepository = new Mock<IActivitySegmentRepository>();
            _mockSegmentIntervalRepository = new Mock<ISegmentIntervalRepository>();
        }

        [TestMethod]
        public async Task WhenMultipleSegments_ThenAttachesCorrectSegmentIntervalRecords()
        {
            Guid inputTrainingPlanActivityId = Guid.Parse("cb9d1784-7e41-4c68-aef9-4ed2c132c88e");

            Guid segmentOneGuid = Guid.Parse("770f326a-a8eb-48e4-972f-64500e289941");
            Guid segmentTwoGuid = Guid.Parse("edebcfe2-0e06-4868-9654-d485071ac66f");

            Guid segmentOneIntervalOneGuid = Guid.Parse("edebcfe2-0e06-4868-9654-d485071ac66f");
            Guid segmentTwoIntervalOneGuid = Guid.Parse("6d9a5137-850e-4429-a576-34c2f738819c");
            Guid segmentOneIntervalTwoGuid = Guid.Parse("2cee99dd-6598-4c03-86f7-dfb023ebd46e");
            Guid segmentTwoIntervalTwoGuid = Guid.Parse("808e35f8-c903-4969-86e6-917d868bd4dd");

            List<ActivitySegmentRecord> mockSegments = new List<ActivitySegmentRecord> 
            {
                new ActivitySegmentRecord { ActivityId = inputTrainingPlanActivityId, ActivitySegmentId = segmentOneGuid },
                new ActivitySegmentRecord { ActivityId = inputTrainingPlanActivityId, ActivitySegmentId = segmentTwoGuid }
            };

            List<SegmentIntervalRecord> mockSegmentOneIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentOneGuid, SegmentIntervalId = segmentOneIntervalOneGuid },
                new SegmentIntervalRecord { SegmentId = segmentOneGuid, SegmentIntervalId = segmentOneIntervalTwoGuid }
            };

            List<SegmentIntervalRecord> mockSegmentTwoIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentTwoGuid, SegmentIntervalId = segmentTwoIntervalOneGuid },
                new SegmentIntervalRecord { SegmentId = segmentTwoGuid, SegmentIntervalId = segmentTwoIntervalTwoGuid }
            };


            _mockActivitySegmentRepository.Setup(x => x.GetForParentId(inputTrainingPlanActivityId)).ReturnsAsync(mockSegments);
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentOneGuid)).ReturnsAsync(mockSegmentOneIntervals);
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentTwoGuid)).ReturnsAsync(mockSegmentTwoIntervals);

            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            var result = await segmentDTOService.GetForParentId(inputTrainingPlanActivityId);

            //Assert
            Assert.AreEqual(2, result.Count());

            var segmentOneDto = result.FirstOrDefault(x => x.ActivitySegmentId == segmentOneGuid);
            
            var segmentTwoDto = result.FirstOrDefault(x => x.ActivitySegmentId == segmentTwoGuid);
            
            Assert.IsNotNull(segmentOneDto);
            Assert.IsNotNull(segmentTwoDto);
            CollectionAssert.AreEquivalent(
                mockSegmentOneIntervals.Select(x => x.SegmentIntervalId).ToList(),
                segmentOneDto.Intervals.Select(x => x.SegmentIntervalId).ToList()
                );
            CollectionAssert.AreEquivalent(
                mockSegmentTwoIntervals.Select(x => x.SegmentIntervalId).ToList(),
                segmentTwoDto.Intervals.Select(x => x.SegmentIntervalId).ToList()
                );

        }
    }
}
