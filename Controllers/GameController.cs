using Microsoft.AspNetCore.Mvc;
using GateFall.Data;
using GateFall.ViewModels;
using GateFall.Models;
using GateFall.Extensions; // Az önce yazdığımız Extension için

namespace GateFall.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // OYUNU BAŞLAT
        public IActionResult Start(int deckId)
        {
            var deck = _context.Decks.Find(deckId);
            if (deck == null) return NotFound();

            deck.PlayCount++;
            _context.Update(deck);
            _context.SaveChanges();

            var cardIds = _context.Cards
                .Where(c => c.DeckId == deckId)
                .Select(c => c.Id)
                .ToList();

            if (cardIds.Count < 2) return Content("Yetersiz kart sayısı.");

            // Karıştır
            var rng = new Random();
            var shuffledIds = cardIds.OrderBy(a => rng.Next()).ToList();

            // DURUMU SESSION'A KAYDET (URL yerine buraya yazıyoruz)
            HttpContext.Session.SetObject("Remaining", shuffledIds);
            HttpContext.Session.SetObject("Winners", new List<int>());
            HttpContext.Session.SetInt32("Round", 1);

            // Artık parametre göndermeden direkt Play'e gidiyoruz
            return RedirectToAction("Play");
        }

        // OYUN EKRANI (GET)
        public IActionResult Play()
        {
            // Session'dan verileri çek
            var remaining = HttpContext.Session.GetObject<List<int>>("Remaining");
            var winners = HttpContext.Session.GetObject<List<int>>("Winners");
            var round = HttpContext.Session.GetInt32("Round");

            // Eğer Session düşmüşse (zaman aşımı vs.) ana sayfaya at
            if (remaining == null || winners == null) return RedirectToAction("Index", "Deck");

            // --- MANTIK KISMI AYNI ---

            if (remaining.Count < 2)
            {
                if (remaining.Count == 1) winners.Add(remaining[0]);

                if (winners.Count == 1) return RedirectToAction("Winner", new { id = winners[0] });

                // Yeni Tura Geçiş: Kazananları, Bekleyenlere aktar
                HttpContext.Session.SetObject("Remaining", winners); // Kazananlar -> Yeni Yarışmacılar
                HttpContext.Session.SetObject("Winners", new List<int>()); // Kazanan kutusunu boşalt
                HttpContext.Session.SetInt32("Round", (round ?? 1) + 1);

                return RedirectToAction("Play");
            }

            int idA = remaining[0];
            int idB = remaining[1];

            // Çekilenleri listeden sil ve Session'ı güncelle
            remaining.RemoveRange(0, 2);
            HttpContext.Session.SetObject("Remaining", remaining);

            var cardA = _context.Cards.Find(idA);
            var cardB = _context.Cards.Find(idB);

            var model = new GameViewModel
            {
                OptionA_Id = cardA.Id,
                OptionA_Name = cardA.Name,
                OptionA_Image = cardA.ImageUrl,
                OptionB_Id = cardB.Id,
                OptionB_Name = cardB.Name,
                OptionB_Image = cardB.ImageUrl,
                RoundNumber = round ?? 1
            };

            return View(model);
        }

        // SEÇİM YAP (POST)
        [HttpPost]
        public IActionResult MakeChoice(int selectedId)
        {
            // Sadece kazananı alıp Session'daki Winners listesine ekliyoruz
            var winners = HttpContext.Session.GetObject<List<int>>("Winners");

            if (winners != null)
            {
                winners.Add(selectedId);
                HttpContext.Session.SetObject("Winners", winners);
            }

            return RedirectToAction("Play");
        }

        public IActionResult Winner(int id)
        {
            // Oyun bitti, Session'ı temizleyebiliriz
            HttpContext.Session.Clear();

            var card = _context.Cards.Find(id);
            return View(card);
        }
    }
}