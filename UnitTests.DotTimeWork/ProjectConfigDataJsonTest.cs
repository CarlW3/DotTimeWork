using DotTimeWork.DataProvider;
using DotTimeWork.Project;
using System.IO;
using Xunit;

namespace UnitTests.DotTimeWork
{
    public class ProjectConfigDataJsonTest : IDisposable
    {
        private readonly string _testFilePath = Path.Combine(Path.GetTempPath(), "TestProjectConfig.json");

        [Fact]
        public void ProjectConfigFileExists_FileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            var result = projectConfigDataJson.ProjectConfigFileExists();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ProjectConfigFileExists_FileExists_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "{}");
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            var result = projectConfigDataJson.ProjectConfigFileExists();

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(_testFilePath);
        }

        [Fact]
        public void DeleteProjectConfigFile_FileExists_FileIsDeleted()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "{}");
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            projectConfigDataJson.DeleteProjectConfigFile();

            // Assert
            Assert.False(File.Exists(_testFilePath));
        }

        [Fact]
        public void LoadProjectConfig_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            var result = projectConfigDataJson.LoadProjectConfig();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LoadProjectConfig_FileExists_ReturnsProjectConfig()
        {
            // Arrange
            var projectConfig = new ProjectConfig
            {
                ProjectName = "Test Project",
                Description = "Test Description",
                ProjectStart = DateTime.Now,
                ProjectEnd = DateTime.Now.AddDays(30),
                MaxTimePerDay = 8
            };
            var json = System.Text.Json.JsonSerializer.Serialize(projectConfig);
            File.WriteAllText(_testFilePath, json);
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            var result = projectConfigDataJson.LoadProjectConfig();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.ProjectName);
            Assert.Equal("Test Description", result.Description);

            // Cleanup
            File.Delete(_testFilePath);
        }

        [Fact]
        public void PersistProjectConfig_ValidProjectConfig_FileIsCreated()
        {
            // Arrange
            var projectConfig = new ProjectConfig
            {
                ProjectName = "Test Project",
                Description = "Test Description",
                ProjectStart = DateTime.Now,
                ProjectEnd = DateTime.Now.AddDays(30),
                MaxTimePerDay = 8,
                TimeTrackingFolder = "TestFolder"
            };
            var projectConfigDataJson = new ProjectConfigDataJson(_testFilePath);

            // Act
            projectConfigDataJson.PersistProjectConfig(projectConfig);

            // Assert
            Assert.True(File.Exists(_testFilePath));
            var json = File.ReadAllText(_testFilePath);
            var loadedConfig = System.Text.Json.JsonSerializer.Deserialize<ProjectConfig>(json);
            Assert.NotNull(loadedConfig);
            Assert.Equal("Test Project", loadedConfig.ProjectName);

            // Cleanup
            File.Delete(_testFilePath);
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "TestFolder"), true);
        }


        public ProjectConfigDataJsonTest()
        {
            // Ensure the test file is deleted before each test
            TearDown();
        }

        public void Dispose()
        {
            // Cleanup after all tests
            TearDown();
        }
        private void TearDown()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}
