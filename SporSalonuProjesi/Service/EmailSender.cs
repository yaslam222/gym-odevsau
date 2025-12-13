using Microsoft.AspNetCore.Identity.UI.Services;

namespace SporSalonuProjesi.Service
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Bu bir demo proje olduğu için e-posta gönderme işlemi yapmıyoruz.
            // Sadece "gönderdim" diyerek görevi tamamlıyoruz.
            // Gelecekte buraya SendGrid veya SMTP kodu ekleyebilirsiniz.
            return Task.CompletedTask;
        }
    }
}
