namespace DoctorMobileApp.WebService.PatientFeedback.Interface
{
    public interface IShortTokenService
    {
        string Generate(int length = 10);
    }
}