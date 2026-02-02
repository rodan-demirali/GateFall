using System.ComponentModel.DataAnnotations;

namespace GateFall.ViewModels
{
    public class CreateDeckViewModel
    {
        [Required(ErrorMessage = "Lütfen bir başlık girin.")]
        [Display(Name = "Deste Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Kapak resmi seçmelisiniz.")]
        [Display(Name = "Kapak Resmi")]
        public IFormFile CoverImage { get; set; }
    }
}
