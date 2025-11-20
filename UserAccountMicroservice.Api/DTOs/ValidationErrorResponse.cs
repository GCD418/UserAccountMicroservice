namespace UserAccountMicroservice.Api.DTOs;

public class ValidationErrorResponse
{
    public string Message { get; set; } = "Validación fallida";
    public List<string> Errors { get; set; } = new();
}
