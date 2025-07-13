namespace Domain.Exceptions;

public class ConcurrencyException() : Exception("Version mismatch detected");