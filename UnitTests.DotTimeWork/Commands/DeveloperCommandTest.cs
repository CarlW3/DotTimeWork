using DotTimeWork.Commands;
using DotTimeWork.Developer;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.DotTimeWork.Commands
{
    public class DeveloperCommandTest
    {
        [Fact]
        public void Execute_CallsCreateDeveloperConfigFile()
        {
            Mock<IDeveloperConfigController> mockDeveloperConfigController = new Mock<IDeveloperConfigController>();
            // Arrange
            var developerCommand = new DeveloperCommand(mockDeveloperConfigController.Object);

            // Act
            developerCommand.Execute(false);

            // Assert
            mockDeveloperConfigController.Verify(x => x.CreateDeveloperConfigFile(), Times.Once);
        }
    }
}
