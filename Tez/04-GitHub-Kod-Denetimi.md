# Kod envanteri / başlangıç araştırma durumu

**Depo:** https://github.com/yunusylcnkya/TubitakProje  
**İncelenen ana revizyon:** `a12960af3879b965b7346fc55db264f8ad453fe8`  
**Not:** Bu, GitHub kaynak dosyalarının statik incelemesidir. Bu paket hiçbir Unity sahnesini çalıştırdığını veya tüm testlerden geçirdiğini iddia etmez.

| Mevcut dosya | Mevcut işlev | Tez için önerilen iyileştirme |
|---|---|---|
| `Assets/_Project/Scripts/Circuit/CircuitValidator.cs` | Gerekli terminal çiftlerine göre doğru/eksik/yanlış bağlantı sayısı | İşlevsel-topolojik referans doğrulamasıyla karşılaştırma |
| `Assets/_Project/Scripts/Circuit/Definitions/RequiredConnection.cs` | Sıra bağımsız iki terminal eşleştirmesi | Alternatif güvenli devre sınıflarının analizi |
| `Assets/_Project/Scripts/Electrical/ElectricalSimulationSystem.cs` | Grafik komşuluğu ve potansiyel yayılımıyla cihaz durumları | Deterministik iz, referans model, hata kapsamı |
| `Assets/_Project/Scripts/Electrical/ElectricalSimulationSnapshot.cs` | Terminal potansiyel bayrakları ve kısa devre bayrağı | Davranış izleri; gerçek akım/gerilim tahminiyle karıştırmama |
| `Assets/_Project/Scripts/Scenario/ScenarioRunner.cs` | Cihaz yerleştirme, bağlantı ve işlevsel durum adımları | Yeniden oynatılabilir senaryo ve test girişleri |
| `Assets/_Project/Scripts/Electrical/Devices/TimeRelayDevice.cs` | TON zaman sayacı Unity `Time.deltaTime` | Deterministik deney zamanı ve tolerans testi |
| `Assets/_Project/Scripts/Electrical/Devices/ThermalRelayDevice.cs` | Termik TEST/RESET modellemesi | Termal yük modeli **değil**: bilimsel sınırlamayı açık tut |
| `Assets/_Project/Scripts/Assessment/AssessmentSystem.cs` | Sabit ceza ağırlıklı puanlama | Değerlendirici uyumu, geçerlilik, hata sınıflandırması |
| `Assets/_Project/Scripts/Persistence/ModuleProgressData.cs` | Deneme, puan, en iyi süre özeti | Zaman damgalı ve kimliksiz olay günlüğü |
| `Assets/_Project/Scripts/Safety/ElectricalSafetySystem.cs` | Enerjili kablo sökme uyarısı / engelleme | Özel güvenlik değişmezlerinin otomatik kontrolü |

## Kodda bulunan sekiz uygulama
01: 24V lamba; 02: START/kontaktör; 03: START-STOP mühürleme; 04: termik korumalı üç fazlı motor; 05: röle NO/NC lamba; 06: röle mühürleme; 07: TON gecikmeli lamba; 08: TON gecikmeli motor.

## Bilinen açık noktalar
1. Beklenen terminallerle kablo karşılaştırması tek başına devre eşdeğerliği kanıtlamaz.
2. Simülasyon fiziksel gerilim/akım büyüklüklerini bütünüyle çözmüyor.
3. Test edilmiş doğruluk, yanlış alarm, duyarlılık gibi bilimsel ölçümler repo dosyalarından çıkarılamıyor.
4. Öğrenci kayıtları tam olay akışı yerine sonuç özeti içeriyor.
5. README prototip aşamasını tarif ediyor; tez için güncel mimari belgelenmeli.
6. Repo içinde kendi proje testleri için kapsamlı bir otomatik test klasörü görünmüyor; daha ayrıntılı tarama ve Unity test koşumu yapılmalı.
