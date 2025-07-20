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
            developerCommand.Execute(false);
            
            // Assert
            mockDeveloperConfigController.Verify(x => x.CreateDeveloperConfigFile(), Times.Once);
        }

        [Fact]
        public void Execute_ExistingBefore_CallsCreateDeveloperConfigFile()
        {
            Mock<IDeveloperConfigController> mockDeveloperConfigController = new Mock<IDeveloperConfigController>();
            mockDeveloperConfigController.Setup(x => x.IsDeveloperConfigFileExisting()).Returns(true);

            // Arrange
            var developerCommand = new DeveloperCommand(mockDeveloperConfigController.Object);

            // Act
            developerCommand.Execute(true);

            // Assert
            mockDeveloperConfigController.Verify(x => x.CreateDeveloperConfigFile(), Times.Once);
        }
    }
}
