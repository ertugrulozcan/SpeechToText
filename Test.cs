// Dinleyici nesnesinin oluşturulması
SpeechToTextAPI.SpeechToText listener = new SpeechToTextAPI.SpeechToText();

// Dinleyici ayarlamalarının yapılması
this.listener.OnRecognitionStarted += (s, e) =>
{
  // Dinleme başlatıldığında yapılacak işlemler  
};
this.listener.OnRecognitionCompleted += (s, e) =>
{
  // Dinleme tamamlandığında yapılacak işlemler
  
  // Dinleme sonucu;
  string text = e.Result;
};

// Dinleme işlemi ve hata denetimleri
try
{
    if (!listener.RecognitionEnabled)
        await listener.ListeningStart();
    else
        listener.Cancel();
}
catch(SpeechToTextAPI.NotSureWhatYouSaidException notSureExc)
{
    // Anlaşılamadı, tekrar deneyiniz.
}
catch (SpeechToTextAPI.PrivacyPolicyException privacyExc)
{
    // Gizlilik sözleşmesi onayı gerekli, cihaz ayarlarına giderek gerekli izinleri ayarlayınız.
}
catch(Exception exc)
{
    // Beklenmeyen bir hata oluştu
}
