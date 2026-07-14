namespace DoctorMobileApp.CommonClass.Services.Interface
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int HospitalId { get; }
        string IpAddress { get; }
        string Browser { get; }

        int HospitalGroupId { get; }
    }
}