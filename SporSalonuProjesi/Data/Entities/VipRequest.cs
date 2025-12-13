using System.ComponentModel.DataAnnotations;

namespace SporSalonuProjesi.Data.Entities
{
    public class VipRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen adınızı girin.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Lütfen e-posta adresinizi girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lütfen telefon numaranızı girin.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [StringLength(20)]
        public string Phone { get; set; }

        // Formun ne zaman gönderildiğini kaydet
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
