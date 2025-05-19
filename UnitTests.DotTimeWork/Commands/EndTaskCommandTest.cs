using DotTimeWork.Commands;
using DotTimeWork.ConsoleService;
using DotTimeWork.TimeTracker;
using Moq;

namespace UnitTests.DotTimeWork.Commands
{
    public class EndTaskCommandTest
    {

        [Fact]
        public void Execute_TaskAsParameter()
        {
            // Arrange
            var taskController = new Mock<ITaskTimeTracker>();
            var inputAndOutputService = new Mock<IInputAndOutputService>();
            
            inputAndOutputService.Setup(x => x.AskForInput<string>(It.IsAny<string>())).Returns("ABCDEF");
            var endTaskCommand = new EndTaskCommand(taskController.Object, inputAndOutputService.Object);

            // Act
            endTaskCommand.Execute("Task1",false);

            // Assert
            taskController.Verify(x => x.EndTask("Task1"), Times.Once);
            inputAndOutputService.Verify(x => x.AskForInput<string>(It.IsAny<string>()), Times.Never);
        }

    }
}
