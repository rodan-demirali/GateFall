using GateFall.Data;
using GateFall.Models;
using GateFall.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GateFall.Controllers
{
    public class DeckController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dosya kaydetmek için gerekli

        public DeckController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            // Desteleri getirirken içindeki kart sayısını da bilelim
            //var decks = await _context.Decks
            //    .Include(d => d.Cards)
            //    .OrderByDescending(d => d.CreatedDate) // En yeniler en başta
            //    .ToListAsync();

            var decks = await _context.Decks
                    .Include(d => d.Cards)
                    .OrderByDescending(d => d.PlayCount) // En çok oynanan en başta!
                    .ThenByDescending(d => d.CreatedDate) // Eşitlik varsa en yeni olan üste
                    .ToListAsync();

            return View(decks);
        }

        // GET: Deck/Create (Formu gösterir)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Deck/Create (Formu işler)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateDeckViewModel model)
        {
            if (ModelState.IsValid)
            {
                string stringFileName = null;

                // 1. Resim Yükleme İşlemi
                if (model.CoverImage != null)
                {
                    // Resimler için 'wwwroot/images/decks' klasörünü kullanacağız.
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "decks");

                    // Klasör yoksa oluştur
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Dosya adı çakışmasın diye başına GUID ekliyoruz (örn: askdj2-resim.jpg)
                    stringFileName = Guid.NewGuid().ToString() + "_" + model.CoverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, stringFileName);

                    // Dosyayı sunucuya kopyala
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }
                }

                // 2. Veritabanı Nesnesini Oluşturma
                Deck newDeck = new Deck
                {
                    Title = model.Title,
                    Description = model.Description,
                    CoverImageUrl = stringFileName, // Sadece dosya adını kaydediyoruz
                    CreatedDate = DateTime.Now,
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                _context.Add(newDeck);
                await _context.SaveChangesAsync();

                // Başarılı olursa anasayfaya veya kart ekleme sayfasına yönlendir
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Deck/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Desteyi getirirken içindeki Kartları (Cards) da dahil et (Include)
            var deck = await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (deck == null) return NotFound();

            return View(deck);
        }

        // POST: Deck/AddCards
        [HttpPost]
        public async Task<IActionResult> AddCards(int deckId, List<IFormFile> cardImages)
        {
            // Eğer hiç resim seçilmediyse geri dön
            if (cardImages == null || cardImages.Count == 0)
                return RedirectToAction("Details", new { id = deckId });

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cards");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var file in cardImages)
            {
                if (file.Length > 0)
                {
                    // 1. Dosyayı Kaydet
                    string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 2. Kart Nesnesini Oluştur
                    // İpucu: Kart adını şimdilik dosya adından alalım (örn: "witcher.jpg" -> "witcher")
                    // Kullanıcı isterse sonra düzenler.
                    var cardName = Path.GetFileNameWithoutExtension(file.FileName);

                    var newCard = new Card
                    {
                        Name = cardName,
                        ImageUrl = fileName,
                        DeckId = deckId
                    };

                    _context.Cards.Add(newCard);
                }
            }

            await _context.SaveChangesAsync();

            // İşlem bitince tekrar detay sayfasına dön
            return RedirectToAction("Details", new { id = deckId });
        }

        // GET: Deck/MyDecks
        [Authorize]
        public async Task<IActionResult> MyDecks()
        {
            // Şu anki kullanıcının ID'sini al
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece bu ID'ye sahip desteleri getir
            var myDecks = await _context.Decks
                .Where(d => d.AppUserId == currentUserId)
                .Include(d => d.Cards)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            return View(myDecks);
        }

        // POST: Deck/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Desteyi bul (Kartlarıyla beraber, çünkü resimlerini sileceğiz)
            var deck = await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deck == null) return NotFound();

            // 2. GÜVENLİK KONTROLÜ: Bu deste gerçekten silmek isteyen kişinin mi?
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (deck.AppUserId != currentUserId)
            {
                return Unauthorized(); // "Senin değil, silemezsin!"
            }

            // 3. TEMİZLİK: Kapak Resmini Sil
            if (!string.IsNullOrEmpty(deck.CoverImageUrl))
            {
                string coverPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "decks", deck.CoverImageUrl);
                if (System.IO.File.Exists(coverPath)) System.IO.File.Delete(coverPath);
            }

            // 4. TEMİZLİK: İçindeki Kartların Resimlerini Sil
            foreach (var card in deck.Cards)
            {
                if (!string.IsNullOrEmpty(card.ImageUrl))
                {
                    string cardPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cards", card.ImageUrl);
                    if (System.IO.File.Exists(cardPath)) System.IO.File.Delete(cardPath);
                }
            }

            // 5. Veritabanından Sil
            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            // İşlem bitince listeye dön
            return RedirectToAction(nameof(MyDecks));
        }


        // POST: Deck/DeleteCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteCard(int cardId)
        {
            // Kartı bulurken Destesini de getiriyoruz (Sahibini kontrol etmek için)
            var card = await _context.Cards
                .Include(c => c.Deck)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null) return NotFound();

            // GÜVENLİK: Kartın bağlı olduğu deste, şu anki kullanıcıya mı ait?
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (card.Deck.AppUserId != currentUserId)
            {
                return Unauthorized();
            }

            // DOSYA SİLME: Kartın resmini sunucudan sil
            if (!string.IsNullOrEmpty(card.ImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cards", card.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // VERİTABANI SİLME
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            // Aynı sayfaya (Detaylara) geri dön
            return RedirectToAction("Details", new { id = card.DeckId });
        }


        // GET: Deck/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var deck = await _context.Decks.FindAsync(id);
            if (deck == null) return NotFound();

            // Güvenlik Kontrolü
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (deck.AppUserId != currentUserId) return Unauthorized();

            // Mevcut veriyi View'a gönderiyoruz
            return View(deck);
        }

        // POST: Deck/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Deck deck)
        {
            if (id != deck.Id) return NotFound();

            // Güvenlik Kontrolü
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Veritabanındaki orijinal veriyi çekiyoruz
            var deckToUpdate = await _context.Decks.FindAsync(id);

            if (deckToUpdate == null) return NotFound();
            if (deckToUpdate.AppUserId != currentUserId) return Unauthorized();

            // --- DÜZELTME BURADA BAŞLIYOR ---
            // Formdan gelmeyen ama Modelde zorunlu olabilecek alanları doğrulama dışı bırakıyoruz
            ModelState.Remove("CoverImageUrl");
            ModelState.Remove("AppUserId");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("Cards");
            // --------------------------------

            if (ModelState.IsValid)
            {
                // Sadece değişen alanları güncelliyoruz
                deckToUpdate.Title = deck.Title;
                deckToUpdate.Description = deck.Description;

                try
                {
                    _context.Update(deckToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Decks.Any(e => e.Id == deck.Id)) return NotFound();
                    else throw;
                }
                // Başarılı olursa listeye dön
                return RedirectToAction(nameof(MyDecks));
            }

            // Hata varsa sayfayı tekrar göster
            return View(deck);
        }


    }
}
