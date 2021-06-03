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
    public class ActivitySegmentDTOService_GivenUpdate
    {
        private Mock<IActivitySegmentRepository> _mockActivitySegmentRepository;
        private Mock<ISegmentIntervalRepository> _mockSegmentIntervalRepository;
        
        [TestInitialize]
        public void Setup() {
            _mockActivitySegmentRepository = new Mock<IActivitySegmentRepository>();
            _mockSegmentIntervalRepository = new Mock<ISegmentIntervalRepository>();
        }

        [TestMethod]
        public async Task WhenIntervalsDoNotExist_ThenSubmitsCreateRequests()
        {
            Guid segmentGuid = Guid.Parse("770f326a-a8eb-48e4-972f-64500e289941");

            var mockSegmentRecord = new ActivitySegmentRecord 
                { 
                    ActivitySegmentId = segmentGuid,
                };

            List<SegmentIntervalRecord> inputSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, Note = "Segment One"},
                new SegmentIntervalRecord { SegmentId = segmentGuid, Note = "Segment Two"}
            };

            var input = new ActivitySegmentDTO 
            {
                ActivitySegmentId = segmentGuid,
                Intervals = inputSegmentIntervals.ToArray()
            };

            var callbackSegments = new List<ActivitySegmentRecord>();
            var callbackIntervals = new List<SegmentIntervalRecord>();

            _mockActivitySegmentRepository.Setup(x => x.Update(It.IsAny<ActivitySegmentRecord>())).Callback((ActivitySegmentRecord record) => callbackSegments.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentGuid)).ReturnsAsync(new List<SegmentIntervalRecord>());

            _mockSegmentIntervalRepository.Setup(x => x.Create(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackIntervals.Add(record));
            
            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            await segmentDTOService.Update(input);

            //Assert
            
            Assert.AreEqual(1, callbackSegments.Count());
            Assert.AreEqual(2, callbackIntervals.Count());
            Assert.AreEqual(segmentGuid, callbackSegments[0].ActivitySegmentId);
            CollectionAssert.AreEquivalent(
                inputSegmentIntervals.Select(x => x.Note).ToList(),
                callbackIntervals.Select(x => x.Note).ToList()
                );
        }

        [TestMethod]
        public async Task WhenIntervalsExist_ThenSubmitsUpdateRequests()
        {
            Guid segmentGuid = Guid.Parse("770f326a-a8eb-48e4-972f-64500e289941");

            Guid segmentIntervalOneGuid = Guid.Parse("edebcfe2-0e06-4868-9654-d485071ac66f");
            Guid segmentIntervalTwoGuid = Guid.Parse("2cee99dd-6598-4c03-86f7-dfb023ebd46e");

            var mockSegmentRecord = new ActivitySegmentRecord 
                { 
                    ActivitySegmentId = segmentGuid,
                };

            List<SegmentIntervalRecord> inputSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalOneGuid, Note = "Segment Update One"},
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalTwoGuid, Note = "Segment Update Two"}
            };

            
            List<SegmentIntervalRecord> mockSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalOneGuid, Note = "Segment One"},
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalTwoGuid, Note = "Segment Two"}
            };

            var input = new ActivitySegmentDTO 
            {
                ActivitySegmentId = segmentGuid,
                Intervals = inputSegmentIntervals.ToArray()
            };

            var callbackSegments = new List<ActivitySegmentRecord>();
            var callbackIntervals = new List<SegmentIntervalRecord>();

            _mockActivitySegmentRepository.Setup(x => x.Update(It.IsAny<ActivitySegmentRecord>())).Callback((ActivitySegmentRecord record) => callbackSegments.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentGuid)).ReturnsAsync(mockSegmentIntervals);

            _mockSegmentIntervalRepository.Setup(x => x.Update(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackIntervals.Add(record));
            
            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            await segmentDTOService.Update(input);

            //Assert
            
            Assert.AreEqual(1, callbackSegments.Count());
            Assert.AreEqual(2, callbackIntervals.Count());
            Assert.AreEqual(segmentGuid, callbackSegments[0].ActivitySegmentId);
            CollectionAssert.AreEquivalent(
                inputSegmentIntervals.Select(x => x.SegmentIntervalId).ToList(),
                callbackIntervals.Select(x => x.SegmentIntervalId).ToList()
                );
                
            CollectionAssert.AreEquivalent(
                inputSegmentIntervals.Select(x => x.Note).ToList(),
                callbackIntervals.Select(x => x.Note).ToList()
                );
        }

        [TestMethod]
        public async Task WhenIntervalsExistAndNotInUpdate_ThenSubmitsDeleteRequests()
        {
            Guid segmentGuid = Guid.Parse("770f326a-a8eb-48e4-972f-64500e289941");

            Guid segmentIntervalOneGuid = Guid.Parse("edebcfe2-0e06-4868-9654-d485071ac66f");
            Guid segmentIntervalTwoGuid = Guid.Parse("2cee99dd-6598-4c03-86f7-dfb023ebd46e");

            var mockSegmentRecord = new ActivitySegmentRecord 
                { 
                    ActivitySegmentId = segmentGuid,
                };

            List<SegmentIntervalRecord> inputSegmentIntervals = new List<SegmentIntervalRecord>
            {
            };
            
            List<SegmentIntervalRecord> mockSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalOneGuid, Note = "Segment One"},
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalTwoGuid, Note = "Segment Two"}
            };

            var input = new ActivitySegmentDTO 
            {
                ActivitySegmentId = segmentGuid,
                Intervals = inputSegmentIntervals.ToArray()
            };

            var callbackSegments = new List<ActivitySegmentRecord>();
            var callbackIntervals = new List<SegmentIntervalRecord>();

            _mockActivitySegmentRepository.Setup(x => x.Update(It.IsAny<ActivitySegmentRecord>())).Callback((ActivitySegmentRecord record) => callbackSegments.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentGuid)).ReturnsAsync(mockSegmentIntervals);

            _mockSegmentIntervalRepository.Setup(x => x.Delete(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackIntervals.Add(record));
            
            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            await segmentDTOService.Update(input);

            //Assert
            
            Assert.AreEqual(1, callbackSegments.Count());
            Assert.AreEqual(2, callbackIntervals.Count());
            Assert.AreEqual(segmentGuid, callbackSegments[0].ActivitySegmentId);
            CollectionAssert.AreEquivalent(
                mockSegmentIntervals.Select(x => x.SegmentIntervalId).ToList(),
                callbackIntervals.Select(x => x.SegmentIntervalId).ToList()
                );
        }

                [TestMethod]
        public async Task WhenIntervalsInAllStates_ThenSubmitsAllRequests()
        {
            Guid segmentGuid = Guid.Parse("770f326a-a8eb-48e4-972f-64500e289941");

            Guid segmentIntervalOneGuid = Guid.Parse("edebcfe2-0e06-4868-9654-d485071ac66f");
            Guid segmentIntervalTwoGuid = Guid.Parse("2cee99dd-6598-4c03-86f7-dfb023ebd46e");
            Guid segmentIntervalThreeGuid = Guid.Parse("6069b4b0-ef3a-490d-82a2-5f45735f5598");

            var mockSegmentRecord = new ActivitySegmentRecord 
                { 
                    ActivitySegmentId = segmentGuid,
                };

            string updateNote = "Segment One Update";
            string createNote = "Segment Three";

            List<SegmentIntervalRecord> inputSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalOneGuid, Note = updateNote },
                new SegmentIntervalRecord { SegmentId = segmentGuid, Note = createNote }
            };
            
            List<SegmentIntervalRecord> mockSegmentIntervals = new List<SegmentIntervalRecord>
            {
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalOneGuid, Note = "Segment One"},
                new SegmentIntervalRecord { SegmentId = segmentGuid, SegmentIntervalId = segmentIntervalTwoGuid, Note = "Segment Two"}
            };

            var input = new ActivitySegmentDTO 
            {
                ActivitySegmentId = segmentGuid,
                Intervals = inputSegmentIntervals.ToArray()
            };

            var callbackSegments = new List<ActivitySegmentRecord>();
            var callbackCreateIntervals = new List<SegmentIntervalRecord>();
            var callbackUpdateIntervals = new List<SegmentIntervalRecord>();
            var callbackDeleteIntervals = new List<SegmentIntervalRecord>();

            _mockActivitySegmentRepository.Setup(x => x.Update(It.IsAny<ActivitySegmentRecord>())).Callback((ActivitySegmentRecord record) => callbackSegments.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.GetForParentId(segmentGuid)).ReturnsAsync(mockSegmentIntervals);

            _mockSegmentIntervalRepository.Setup(x => x.Create(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackCreateIntervals.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.Update(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackUpdateIntervals.Add(record));
            _mockSegmentIntervalRepository.Setup(x => x.Delete(It.IsAny<SegmentIntervalRecord>())).Callback((SegmentIntervalRecord record) => callbackDeleteIntervals.Add(record));
            
            var segmentDTOService = new ActivitySegmentDTOService(_mockActivitySegmentRepository.Object, _mockSegmentIntervalRepository.Object);

            //Act
            await segmentDTOService.Update(input);

            //Assert
            
            Assert.AreEqual(1, callbackSegments.Count());
            Assert.AreEqual(1, callbackCreateIntervals.Count());
            Assert.AreEqual(1, callbackUpdateIntervals.Count());
            Assert.AreEqual(1, callbackDeleteIntervals.Count());
            Assert.AreEqual(segmentGuid, callbackSegments[0].ActivitySegmentId);
            Assert.AreEqual(createNote, callbackCreateIntervals[0].Note);
            Assert.AreEqual(segmentIntervalTwoGuid, callbackUpdateIntervals[0].SegmentIntervalId);
            Assert.AreEqual(updateNote, callbackUpdateIntervals[0].Note);
            Assert.AreEqual(segmentIntervalThreeGuid, callbackDeleteIntervals[0].SegmentIntervalId);
        }
    }
}
