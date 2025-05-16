using DotTimeWork.Commands;
using DotTimeWork.DataProvider;
using DotTimeWork.Project;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.DotTimeWork.Commands
{
    public class CreateProjectCommandTest
    {
        [Fact]
        public void Execute_ShouldCreateProjectConfigFile_WhenCalled()
        {
            // Arrange
            var projectConfigController = new Mock<IProjectConfigController>();
            var createProjectCommand = new CreateProjectCommand(projectConfigController.Object);

            // Act
            createProjectCommand.Execute(false);

            // Assert
            projectConfigController.Verify(x => x.CreateProjectConfigFile(), Times.Once);
        }
    }
}
