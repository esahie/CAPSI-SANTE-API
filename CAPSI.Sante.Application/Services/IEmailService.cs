namespace CAPSI.Sante.Application.Services
{
    public interface IEmailService
    {
        Task<bool> SendReactivationEmailAsync(string toEmail, string patientName, string reactivationToken);
        Task<bool> SendReactivationConfirmationAsync(string toEmail, string patientName);
        Task<bool> SendAdminNotificationAsync(string patientName, string requestReason);
    }
}
