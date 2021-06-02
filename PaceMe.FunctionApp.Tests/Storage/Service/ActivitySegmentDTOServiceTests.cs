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

namespace PaceMe.FunctionApp.Tests.Storage.Service
{
    [TestClass]
    public class ActivitySegmentDTOServiceTests
    {
        private Mock<IActivitySegmentRepository> _mockActivitySegmentRepository;
        private Mock<ISegmentIntervalRepository> _mockSegmentIntervalRepository;
        
        [TestInitialize]
        public void Setup() {
            _mockActivitySegmentRepository = new Mock<IActivitySegmentRepository>();
            _mockSegmentIntervalRepository = new Mock<ISegmentIntervalRepository>();
        }

        [TestMethod]
        public async Task GivenCreate_WhenSegment_ThenReturnsNewSegmentGuid()
        {
            var inputDTO = new ActivitySegmentDTO
            {
                ActivityId = Guid.Parse("0c2351f7-02c2-4d4e-9be5-a103435386f5"),
                Name = "Segment"
            };

            ActivitySegmentRecord callbackRecord = null;

            _mockActivitySegmentRepository.Setup(x => x.Create(It.IsAny<ActivitySegmentRecord>()))
                .Callback((ActivitySegmentRecord record) => callbackRecord = record);

            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            var result = await segmentDTOService.Create(inputDTO);

            //Assert
            Assert.IsNotNull(callbackRecord);
            Assert.AreEqual(inputDTO.ActivityId, callbackRecord.ActivityId);
            Assert.AreEqual(result, callbackRecord.ActivitySegmentId);
        }

        [TestMethod]
        public async Task GivenCreate_WhenSegmentIntervals_ThenSetsNewSegmentGuidOnAllIntervals()
        {
            var inputDTO = new ActivitySegmentDTO
            {
                ActivityId = Guid.Parse("0c2351f7-02c2-4d4e-9be5-a103435386f5"),
                Name = "Segment",
                Intervals = new SegmentIntervalRecord[] 
                {
                    new SegmentIntervalRecord { Note = "Interval One" },
                    new SegmentIntervalRecord { Note = "Interval Two" },
                }
            };

            ActivitySegmentRecord callbackRecord = null;
            List<SegmentIntervalRecord> segmentIntervalRecords = new List<SegmentIntervalRecord>();

            _mockActivitySegmentRepository.Setup(x => x.Create(It.IsAny<ActivitySegmentRecord>()))
                .Callback((ActivitySegmentRecord record) => callbackRecord = record);
            
            _mockSegmentIntervalRepository.Setup(x => x.Create(It.IsAny<SegmentIntervalRecord>()))
                .Callback((SegmentIntervalRecord record) => segmentIntervalRecords.Add(record));

            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            var result = await segmentDTOService.Create(inputDTO);

            //Assert
            Assert.AreEqual(2, segmentIntervalRecords.Count());
            CollectionAssert.AllItemsAreUnique(segmentIntervalRecords.Select(x => x.SegmentIntervalId).ToList());
            foreach(var interval in segmentIntervalRecords){
                Assert.AreEqual(callbackRecord.ActivitySegmentId, interval.SegmentId);
            }
        }
    }
}
