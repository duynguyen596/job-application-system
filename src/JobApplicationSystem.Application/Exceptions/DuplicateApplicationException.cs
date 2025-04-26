namespace JobApplicationSystem.Application.Exceptions;

public class DuplicateApplicationException : Exception
{
    public DuplicateApplicationException(string message) : base(message) { }
}