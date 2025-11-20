namespace UserAccountMicroservice.Domain.Services.Validations;

public interface IValidator<T>
{
    Result Validate(T entity);
}