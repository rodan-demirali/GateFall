using System.ComponentModel.DataAnnotations;

namespace GateFall.Models
{
    public class Deck
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Deste başlığı zorunludur.")]
        [Display(Name = "Deste Adı")]
        public string Title { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        public string? AppUserId { get; set; }

        [Display(Name = "Kapak Resmi")]
        public string CoverImageUrl { get; set; } 

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // İlişki: Bir destenin içinde birden fazla kart (seçenek) olur.
        public List<Card> Cards { get; set; } = new List<Card>();

        public int PlayCount { get; set; } = 0; // Varsayılan 0

    }
}
