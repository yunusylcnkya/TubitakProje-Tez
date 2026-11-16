# Deney ve iş planı – V1

## Evre 0 — Mevcut sistemi sabitle
- `main` kodundan tez için çalışma dalı aç.
- Unity sürümü: `ProjectSettings/ProjectVersion.txt` içinde son incelemede `6000.6.5f1` görülmüştür; yerel kopyada yeniden doğrula.
- 01–08 temrinleri EditMode/PlayMode ve manuel gözlemle çalıştır.
- Her temrinin bağlantı tablosunu, giriş olaylarını ve beklenen çıkışlarını yazılı olarak kaydet.
- Başlangıç sürümüne ait ekran görüntüleri ve derleme sonuçları saklanır; başarısızlıklar gizlenmez.

## Evre 1 — Referans veri kümesi
- Temrin 03, 04, 07, 08 için nominal, eksik, yanlış, alternatif işlevsel ve güvenlik açısından yanlış bağlantı adayları hazırlanır.
- Uzmanlar fiziksel bağlantı/çalışma senaryosunu inceleyerek doğru etiketleri bağımsız onaylar.
- Test adaylarının kesin sayısı pilot çalışmayla belirlenir; hedef olarak 200–300 senaryo düşünülebilir.
- Veri kümesi eğitim, geliştirme ve kapalı test kümelerine **devre ailesi ve varyant bazında** ayrılır; çok benzer örneklerin sızıntısına izin verilmez.

## Evre 2 — Mühendislik algoritması
- `CircuitGraphModel`: düğümler, kablo kenarları, bileşenlerin duruma bağlı iç yolları.
- `CircuitBehaviorVerifier`: giriş dizisi, çıkış durumu, güvenlik değişmezleri, davranış izleri.
- `FaultDiagnosisEngine`: olası hata kümesi, karşı-olgusal fark/iz analizi, açıklama ve güven düzeyi.
- Bileşen modellerinin kapsamadığı akım/gerilim büyüklükleri açıkça ayrılır; gerektiğinde fiziksel model eklenir.
- Mevcut `CircuitValidator` referans yöntem olarak korunur.

## Evre 3 — Teknik doğrulama
- Aynı sabit veri kümesini iki yönteme uygula.
- Yanlış kabul ve yanlış ret örneklerini ayrıntılı raporla; özellikle güvenlik ihlalini yanlış kabul etme hatasını izle.
- Dört temrinde durum geçişleri ve zaman rölesi gecikmeleri tekrarlarla kontrol edilir.
- Uygun, izole edilmiş düşük gerilimli fiziksel düzenekte karşılaştırmalı ölçümler yapılır; riskli enerjili arıza oluşturulmaz.

## Evre 4 — Eğitim araştırması (izinlere bağlı)
- Üniversite etik kurul kararı ve gerekli MEB izinleri olmadan katılımcı verisi toplanmaz.
- Gruplar: mevcut doğrulama/geri bildirim ile yeni açıklanabilir geri bildirim.
- Ön test, eşit eğitim süresi, gerçek pano son testi; kör veya bağımsız puanlama tercih edilir.
- Öğrenci kimlikleri araştırma dışa aktarımında kodlanır; ham kişisel kayıtlar açık GitHub'a yüklenmez.
- Örneklem büyüklüğü pilot veriler ve güç analiziyle belirlenecek; sayılar önceden garanti edilmeyecek.

## Başarı durakları
1. Mevcut sistem için açık, tekrarlanabilir baseline ölçümü.
2. Etiketli ve kontrol edilmiş hata/eşdeğerlik veri kümesi.
3. Eski-yeni algoritma karşılaştırması.
4. Bağımsız fiziksel davranış doğrulaması.
5. İzinli öğrencili deney / sonuçlar, yöntem değerlendirmesi, tez metni.
