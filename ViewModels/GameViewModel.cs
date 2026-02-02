namespace GateFall.ViewModels
{
    public class GameViewModel
    {
        // Ekranda kapışacak olan iki kart
        public int OptionA_Id { get; set; }
        public string OptionA_Name { get; set; }
        public string OptionA_Image { get; set; }

        public int OptionB_Id { get; set; }
        public string OptionB_Name { get; set; }
        public string OptionB_Image { get; set; }

        // --- Durum Yönetimi (State Management) ---

        // Henüz sırası gelmemiş, bekleyen kartların ID'leri (Virgülle ayrılmış string olarak tutacağız: "1,5,8,12")
        public string RemainingContestants { get; set; }

        // Bu turda seçilip bir üst tura çıkanların ID'leri
        public string CurrentWinners { get; set; }

        // Görsellik için (Örn: Son 16, Çeyrek Final vs. yazmak için)
        public int RoundNumber { get; set; }
    }
}