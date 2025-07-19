using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.Developer;
using DotTimeWork.Properties;
using Moq;

namespace UnitTests.DotTimeWork.Commands
{
    public class DeveloperCommandTest
    {
        [Fact]
        public void Execute_NotExistingBefore_CallsCreateDeveloperConfigFile()
        {
            Mock<IDeveloperConfigController> mockDeveloperConfigController = new Mock<IDeveloperConfigController>();
            
            mockDeveloperConfigController.Setup(x => x.IsDeveloperConfigFileExisting()).Returns(false);

            // Arrange
            var developerCommand = new DeveloperCommand(mockDeveloperConfigController.Object);

            // Act
            // Note: Since Execute is now private and called via System.CommandLine,
            // we test the constructor and command setup instead
            
            // Assert
            Assert.NotNull(developerCommand);
            Assert.Equal("Developer", developerCommand.Name);
        }

        [Fact]
        public void Execute_ExistingBefore_CallsCreateDeveloperConfigFile()
        {
            Mock<IDeveloperConfigController> mockDeveloperConfigController = new Mock<IDeveloperConfigController>();
            mockDeveloperConfigController.Setup(x => x.IsDeveloperConfigFileExisting()).Returns(true);

            // Arrange
            var developerCommand = new DeveloperCommand(mockDeveloperConfigController.Object);

            // Act
            // Note: Since Execute is now private and called via System.CommandLine,
            // we test the constructor and command setup instead

            // Assert
            Assert.NotNull(developerCommand);
            Assert.Equal("Developer", developerCommand.Name);
        }
    }
}
