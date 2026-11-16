# Tez araştırma soruları ve hipotezler – V1

## Önerilen tez başlığı
**Mesleki Elektrik Eğitimine Yönelik Üç Boyutlu Sanal Atölyede Topoloji ve Davranış Tabanlı Devre Doğrulama ile Açıklanabilir Hata Geri Bildirim Sisteminin Geliştirilmesi**

## Problem
Mevcut yazılımda `CircuitValidator` beklenen terminal çiftlerinin varlığını ve fazladan bağlantıları kontrol eder. Bu yöntem, farklı kablolama düzenleriyle aynı güvenli işlevi üreten devreleri değerlendirmekte sınırlı kalabilir. Öğrencinin hatalı devresinin *neden* yanlış olduğunun açıklanması ve bu bilginin fiziksel pano becerisine katkısı ayrıca doğrulanmalıdır.

## Araştırma soruları
- **AS1:** Topoloji ve davranış tabanlı yöntem, terminal çifti eşleştirmesine kıyasla devre sınıflandırmasında daha yüksek başarı sağlar mı?
- **AS2:** Birebir bağlantı listesi farklı olsa da aynı tanımlanmış davranışları ve güvenlik koşullarını sağlayan devreler kabul edilebilir mi?
- **AS3:** Yanlış bağlantının işlevsel etkisi ve muhtemel kök nedeni açıklanabilir şekilde sınıflandırılabilir mi?
- **AS4:** Enerji kesme/geri verme, START, STOP, termik TEST/RESET, faz kaybı ve TON gecikmesi gibi olaylarda sanal model referans devrenin beklenen durum geçişleriyle uyuşur mu?
- **AS5:** Açıklanabilir geri bildirim, öğrencinin sanal ortamda hata giderme süresini veya ipucu olmadan tamamlama oranını değiştirir mi?
- **AS6:** Açıklanabilir geri bildirimle çalışan öğrencilerin gerçek eğitim panosunda devre kurma başarısı, referans geri bildirim kullanan öğrencilere göre farklılaşır mı?

## Ön kayıt için hipotez taslakları
- **H1:** Yeni doğrulama yönteminin dengeli F1 skoru, mevcut terminal çifti doğrulayıcısından yüksektir.
- **H2:** Alternatif güvenli ve işlevsel eşdeğer devrelerde yeni yöntemin yanlış ret oranı daha düşüktür.
- **H3:** Hata teşhis sisteminin hata kategorisi makro-F1 skoru, önceden belirlenecek kabul eşiğine ulaşır (eşik, pilot öncesinde belirlenir).
- **H4:** Yeni geri bildirim kullanan grubun gerçek pano görevindeki hata sayısı / tamamlama oranı kontrol grubundan farklıdır (yönlü iddia deney ön kayıt aşamasında belirlenir).

## Ölçümler ve analiz
- Tekil devre etiketleri uzman ve/veya bağımsız referans analizle doğrulanır.
- Teknik doğrulama: F1, macro-F1, balanced accuracy, güvenli olmayan devrenin yanlış kabul oranı, eşdeğer güvenli devrenin yanlış ret oranı; gerekli yerlerde McNemar testi.
- Zaman davranışı: geçiş uyuşma oranı, TON gecikme sapması ve test tekrarlarında kararlılık.
- Fiziksel aktarım: gerçek panoda doğru devre, hata sayısı, süre ve güvenlik kontrol listesi; başlangıç yeterliği için ön test; etki büyüklüğü ve güven aralıkları.
- Yazılım test ve öğrenci deneyi sonuçları birbirinden ayrı raporlanır.

## Kritik kısıt
Sonlu sayıda senaryo testinin geçilmesi **genel devre eşdeğerliğinin matematiksel kanıtı değildir**. Eşdeğerlik iddiasının sınırı açıkça belirlenmeli; güvenlik koşulları ayrıca aranmalıdır.
