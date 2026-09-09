namespace Auth.Dtos;

public record ResponseDto<T>
{
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public ICollection<string> Errors { get; init; } = null!;
}
