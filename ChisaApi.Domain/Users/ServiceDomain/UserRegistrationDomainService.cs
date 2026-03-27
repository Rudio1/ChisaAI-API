namespace ChisaApi.Domain.Users.ServiceDomain;

public sealed class UserRegistrationDomainService
{
    public const int MaxNameLength = 200;

    public string NormalizeDisplayName(string name) => name.Trim();

    public void ValidateDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome é obrigatório.");
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"O nome não pode exceder {MaxNameLength} caracteres.");
    }

    public void ValidateNewPassword(string password, int minimumLength)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < minimumLength)
            throw new ArgumentException($"A senha deve ter pelo menos {minimumLength} caracteres.");
    }

    public void ValidatePasswordConfirmation(string password, string confirmPassword)
    {
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            throw new ArgumentException("As senhas não coincidem.");
    }
}
