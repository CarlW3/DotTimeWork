using DotTimeWork.ConsoleService;
using DotTimeWork.DataProvider;
using DotTimeWork.Project;
using DotTimeWork.Properties;
using Moq;

namespace UnitTests.DotTimeWork
{
    public class ProjectConfigControllerTest
    {
        [Fact]
        public void CreateProjectConfig_NoEndDate()
        {
            Mock<IProjectConfigDataProvider> projectConfigDataProvider = new Mock<IProjectConfigDataProvider>();
            projectConfigDataProvider.Setup(x => x.ProjectConfigFileExists()).Returns(false);
            projectConfigDataProvider.Setup(x => x.PersistProjectConfig(It.IsAny<ProjectConfig>())).Verifiable();
            Mock<IInputAndOutputService> inputOutputService = new Mock<IInputAndOutputService>();
            inputOutputService.Setup(x => x.AskForInput( Resources.Project_Ask_Name, It.IsAny<string>())).Returns("Test Project");
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_Description, It.IsAny<string>())).Returns("This is a test project.");
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_TimePerDay, It.IsAny<int>())).Returns(8);
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_StartDate, It.IsAny<string>())).Returns(DateTime.Now.ToString("yyyy-MM-dd"));
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_EndDate, It.IsAny<string>())).Returns("N/A");
            // Arrange
            var projectConfigController = new ProjectConfigController(projectConfigDataProvider.Object,inputOutputService.Object);

            // Act
            projectConfigController.CreateProjectConfigFile();

            var createdProject = projectConfigController.GetCurrentProjectConfig();
            // Assert
            Assert.NotNull(createdProject);
            Assert.Equal("Test Project", createdProject.ProjectName);
            Assert.Equal("This is a test project.", createdProject.Description);
            Assert.Equal(DateTime.Now.Date, createdProject.ProjectStart.Date);
            // Default end date is 30 days from now
            Assert.Equal(DateTime.Now.AddDays(30).Date, createdProject.ProjectEnd.Value.Date);
            Assert.Equal(8, createdProject.MaxTimePerDay);
            projectConfigDataProvider.Verify(x => x.PersistProjectConfig(It.IsAny<ProjectConfig>()), Times.Once);
        }

        [Fact]
        public void CreateProjectConfig_WithEndDateAndOtherDifferentValues()
        {
            Mock<IProjectConfigDataProvider> projectConfigDataProvider = new Mock<IProjectConfigDataProvider>();
            projectConfigDataProvider.Setup(x => x.ProjectConfigFileExists()).Returns(false);
            projectConfigDataProvider.Setup(x => x.PersistProjectConfig(It.IsAny<ProjectConfig>())).Verifiable();
            Mock<IInputAndOutputService> inputOutputService = new Mock<IInputAndOutputService>();
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_Name, It.IsAny<string>())).Returns("Small Project");
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_Description, It.IsAny<string>())).Returns("This is a great project.");
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_TimePerDay, It.IsAny<int>())).Returns(2);
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_StartDate, It.IsAny<string>())).Returns(DateTime.Now.ToString("yyyy-MM-dd"));
            inputOutputService.Setup(x => x.AskForInput(Resources.Project_Ask_EndDate, It.IsAny<string>())).Returns(DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"));
            // Arrange
            var projectConfigController = new ProjectConfigController(projectConfigDataProvider.Object, inputOutputService.Object);

            // Act
            projectConfigController.CreateProjectConfigFile();

            var createdProject = projectConfigController.GetCurrentProjectConfig();
            // Assert
            Assert.NotNull(createdProject);
            Assert.Equal("Small Project", createdProject.ProjectName);
            Assert.Equal("This is a great project.", createdProject.Description);
            Assert.Equal(DateTime.Now.Date, createdProject.ProjectStart.Date);
            // User entered an end date:
            Assert.Equal(DateTime.Now.AddDays(5).Date, createdProject.ProjectEnd.Value.Date);
            Assert.Equal(2, createdProject.MaxTimePerDay);
            projectConfigDataProvider.Verify(x => x.PersistProjectConfig(It.IsAny<ProjectConfig>()), Times.Once);
        }
    }
}