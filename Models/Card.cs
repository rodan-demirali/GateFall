using System.ComponentModel.DataAnnotations;

namespace GateFall.Models
{
    public class Card
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Seçenek Adı")]
        public string Name { get; set; } // Örn: "The Witcher 3"

        public string ImageUrl { get; set; } // Örn: "/images/decks/witcher.jpg"

        // İlişki: Bu kart hangi desteye ait?
        public int DeckId { get; set; }
        public Deck Deck { get; set; }
    }
}
