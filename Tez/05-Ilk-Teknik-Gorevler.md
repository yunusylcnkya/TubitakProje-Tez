# Başlangıç Sprinti: V1.0 — Baseline ve referans devre tablosu

**Amaç:** Yeni algoritmaya geçmeden önce mevcut sistemin değiştirilmemiş davranışını ölçmek.

## GÖREV 1: Ortam doğrulaması
- [ ] Ayrı tez klasöründe Unity 6000.6.5f1 proje açılabiliyor.
- [ ] `main` projesi değişmedi; yeni çalışma dalı var.
- [ ] `Boot`, `MainMenu`, `Training_ModuleSelect` ve 01–08 sahneleri açılıyor.
- [ ] Console hata/uyarı çıktıları kayıt altına alındı.

## GÖREV 2: Temrin başına referans tablo
- [ ] Her temrin için güç bağlantıları, kumanda bağlantıları ve olay dizisi yazıldı.
- [ ] Her olay için kontaktör, röle, motor, lamba, koruma durumu belirlendi.
- [ ] Deney referansları uzman denetimi / güvenilir şemayla bağımsız doğrulandı.

## GÖREV 3: Baseline otomatik testleri
- [ ] Doğru bağlantı eksiksiz kurulduğunda `CircuitValidator.IsValid` sonucu yazıldı.
- [ ] Bir kablo eksiltme, bir kablo değiştirme ve bir fazla kablo test edildi.
- [ ] START/STOP, termik TEST/RESET, TON 3s davranışları zaman iziyle kaydedildi.
- [ ] Testler tekrarlanabilir; başarısız testler de raporda yer alıyor.

## GÖREV 4: Tez ölçütlerinin ön kaydı
- [ ] Referans test veri setinde `ground_truth` kim tarafından/neye göre veriliyor açıklandı.
- [ ] Güvenli olmayan devre yanlış kabulü birincil güvenlik ölçütü olarak tanımlandı.
- [ ] İstatistiksel karşılaştırma planı sonuçlar görülmeden yazıldı.

## Çıkış ölçütü
En az Temrin 03 ve Temrin 07 için referans devre durum tablosu, gözlenen Unity çıktısı ve varsa uyuşmazlıkların kayıtları hazır olmadan yeni doğrulama motoru mevcut sisteme bağlanmaz.
