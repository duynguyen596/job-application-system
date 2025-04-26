namespace JobApplicationSystem.Application.Exceptions;

public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message) { }
}