namespace EmployerApp.Api.Services;

public interface ILoginHashService
{
    string HashLogin(string login);
    bool VerifyLogin(string login, string hashedLogin);
}
