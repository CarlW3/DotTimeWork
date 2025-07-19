using DotTimeWork.Common;

namespace DotTimeWork.Validation
{
    /// <summary>
    /// Validation helpers for common business rules
    /// </summary>
    public static class ValidationHelpers
    {
        public static Result ValidateTaskName(string? taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                return Result.Failure("Task name cannot be empty");
            }

            if (taskName.Length > 100)
            {
                return Result.Failure("Task name cannot exceed 100 characters");
            }

            if (taskName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            {
                return Result.Failure("Task name contains invalid characters");
            }

            return Result.Success();
        }

        public static Result ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Success(); // Email is optional
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email ? Result.Success() : Result.Failure("Invalid email format");
            }
            catch
            {
                return Result.Failure("Invalid email format");
            }
        }

        public static Result ValidateHoursPerDay(int hours)
        {
            return hours switch
            {
                < 0 => Result.Failure("Hours per day cannot be negative"),
                > 24 => Result.Failure("Hours per day cannot exceed 24"),
                _ => Result.Success()
            };
        }

        public static Result ValidateProjectName(string? projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return Result.Failure("Project name cannot be empty");
            }

            if (projectName.Length > 200)
            {
                return Result.Failure("Project name cannot exceed 200 characters");
            }

            return Result.Success();
        }

        public static Result ValidateFilePath(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Failure("File path cannot be empty");
            }

            try
            {
                var fullPath = Path.GetFullPath(filePath);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Invalid file path: {ex.Message}");
            }
        }

        public static Result ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return Result.Failure("Start date cannot be after end date");
            }

            return Result.Success();
        }

        public static Result ValidateTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
            {
                return Result.Failure("Time span cannot be negative");
            }

            if (timeSpan > TimeSpan.FromHours(24))
            {
                return Result.Failure("Time span cannot exceed 24 hours");
            }

            return Result.Success();
        }

        public static Result ValidateDeveloperName(string? developerName)
        {
            if (string.IsNullOrWhiteSpace(developerName))
            {
                return Result.Failure("Developer name cannot be empty");
            }

            if (developerName.Length > 50)
            {
                return Result.Failure("Developer name cannot exceed 50 characters");
            }

            if (developerName.Any(c => char.IsControl(c)))
            {
                return Result.Failure("Developer name contains invalid characters");
            }

            return Result.Success();
        }

        public static Result ValidateWorkTimeMinutes(int minutes)
        {
            return minutes switch
            {
                < 1 => Result.Failure("Work time must be at least 1 minute"),
                > 720 => Result.Failure("Work time cannot exceed 12 hours (720 minutes)"),
                _ => Result.Success()
            };
        }

        public static Result ValidateBreakTimeMinutes(int minutes)
        {
            return minutes switch
            {
                < 0 => Result.Failure("Break time cannot be negative"),
                > 60 => Result.Failure("Break time cannot exceed 60 minutes"),
                _ => Result.Success()
            };
        }

        public static Result ValidateDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return Result.Success(); // Description is optional
            }

            if (description.Length > 500)
            {
                return Result.Failure("Description cannot exceed 500 characters");
            }

            return Result.Success();
        }
    }
}
