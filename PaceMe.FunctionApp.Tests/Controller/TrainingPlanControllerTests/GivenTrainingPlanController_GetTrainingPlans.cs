using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaceMe.FunctionApp.Authentication;
using PaceMe.Storage.Repository;
using PaceMe.FunctionApp.Controller;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using PaceMe.Model.Record;

namespace PaceMe.FunctionApp.Tests.Controller.TrainingPlanControllerTests
{
    [TestClass]
    public class GivenTrainingPlanController_GetTrainingPlans
    {
        private Mock<IRequestAuthenticator> _mockRequestAuthenticator;
        private Mock<ITrainingPlanRepository> _mockTrainingPlanRepository;
        
        [TestInitialize]
        public void Setup() {
            _mockRequestAuthenticator = new Mock<IRequestAuthenticator>();
            _mockTrainingPlanRepository = new Mock<ITrainingPlanRepository>();
        }

        private TrainingPlanController BuildTrainingPlanControllerFromMocks()
        {
            return new TrainingPlanController(_mockRequestAuthenticator.Object, _mockTrainingPlanRepository.Object);
        }

        [TestMethod]
        public async Task WhenUserIdNotMatched_ReturnsUnauthorised()
        {
            //Arrange
            var req = new Mock<HttpRequest>().Object;
            var userId = Guid.NewGuid();
            var controller = BuildTrainingPlanControllerFromMocks();
            _mockRequestAuthenticator.Setup(x => x.AuthenticateRequest(userId, req)).ReturnsAsync(false);
            //Act
            var result = await controller.GetTrainingPlans(req, userId);
            //Assert
            Assert.AreEqual(typeof(UnauthorizedResult), result.GetType());
        }

        [TestMethod]
        public async Task WhenUserIdMatched_ReturnsResultFromRepository()
        {
            //Arrange
            var req = new Mock<HttpRequest>().Object;
            var userId = Guid.NewGuid();
            var controller = BuildTrainingPlanControllerFromMocks();
            var repositoryPlans = new List<TrainingPlanRecord>();
            _mockRequestAuthenticator.Setup(x => x.AuthenticateRequest(userId, req)).ReturnsAsync(true);
            _mockTrainingPlanRepository.Setup(x => x.GetForParentId(userId)).ReturnsAsync(repositoryPlans);
            //Act
            var result = await controller.GetTrainingPlans(req, userId);
            //Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            _mockTrainingPlanRepository.Verify(x => x.GetForParentId(userId), Times.Once);
        }
    }
}