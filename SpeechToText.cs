using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Windows.Foundation;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;

namespace SpeechToTextAPI
{
    public class SpeechToText
    {
        // Ses sentezleyicisi
        private SpeechSynthesizer Synthesizer;

        // Ses tanımlayıcı
        private SpeechRecognizer Recognizer;

        // Ses tanımlama işlemi
        private IAsyncOperation<SpeechRecognitionResult> RecognitionOperation;

        // Ses tanımlama işlemi sürüyor mu?
        private bool recognitionEnabled = false;

        public bool RecognitionEnabled
        {
            get { return recognitionEnabled; }
        }

        // Ses tanımlama işlemi başladığında tetiklenecek event/handler
        public delegate void RecognitionStarted(object sender, RecognitionStartingArgs e);
        public event RecognitionStarted OnRecognitionStarted;

        // Ses tanımlama işlemi tamamlandığında tetiklenecek event/handler
        public delegate void RecognitionCompleted(object sender, RecognitionCompletingArgs e);
        public event RecognitionCompleted OnRecognitionCompleted;

        /// <summary>
        /// Kurucu Metod - Parametresiz
        /// </summary>
        public SpeechToText()
        {
            if (Synthesizer == null)
            {
                Synthesizer = new SpeechSynthesizer();
            }
            if (Recognizer == null)
            {
                Recognizer = new SpeechRecognizer();
            }
        }

        /// <summary>
        /// Kurucu Metod - Gramer listesi ile
        /// </summary>
        /// <param name="grammerKey"></param>
        /// <param name="grammerList"></param>
        public SpeechToText(string grammerKey, List<string> grammerList)
        {
            if (Synthesizer == null)
            {
                Synthesizer = new SpeechSynthesizer();
            }
            if (Recognizer == null)
            {
                Recognizer = new SpeechRecognizer();

                // Grammer listesinin ayarlanması
                Recognizer.Grammars.AddGrammarFromList(grammerKey, grammerList);
            }
        }

        /// <summary>
        /// Dinlemeye başla
        /// </summary>
        public async Task ListeningStart()
        {
            string result = string.Empty;

            this.OnRecognitionStarted(this, null);

            this.recognitionEnabled = true;

            while (this.RecognitionEnabled)
            {
                try
                {
                    // Ses tanımlama işlemi;
                    RecognitionOperation = Recognizer.RecognizeAsync();
                    var recoResult = await this.RecognitionOperation;

                    // Sonucun başarım oranı kontrolü
                    if (recoResult.TextConfidence < SpeechRecognitionConfidence.Low)
                    {
                        // Eğer yeterince anlaşılır bir sonuç elde edilememişse
                        throw new NotSureWhatYouSaidException("Anlaşılamadı, lütfen tekrar deneyiniz.");
                    }
                    else
                    {
                        // Sonucun çıktıya yazılması
                        result = recoResult.Text;
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // İşlem iptal edilmiştir.
                }
                catch (Exception err)
                {
                    // İşlem esnasında hata oluştuğunda;
                    const int privacyPolicyHResult = unchecked((int)0x80045509);

                    if (err.HResult == privacyPolicyHResult)
                    {
                        throw new PrivacyPolicyException("Bu özelliği kullanmak için gizlilik sözleşmesi onayı gereklidir. Cihaz ayarlarına giderek konuşma özelliğini aktif ediniz.");
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    this.recognitionEnabled = false;
                }
            }

            this.OnRecognitionCompleted(this, new RecognitionCompletingArgs(result));
        }

        public void Cancel()
        {
            if (this.RecognitionEnabled)
            {
                this.recognitionEnabled = false;

                // Cancel the outstanding recognition operation, if one exists
                if (RecognitionOperation != null && RecognitionOperation.Status == AsyncStatus.Started)
                {
                    RecognitionOperation.Cancel();
                }
            }
        }

        public async void TextToSpeech(string text)
        {
            await this.Synthesizer.SpeakTextAsync(text);
        }
    }

    public class RecognitionStartingArgs : EventArgs
    {

    }

    public class RecognitionCompletingArgs : EventArgs
    {
        string result;
        /// <summary>
        /// Tanımlama sonucu
        /// </summary>
        public string Result
        {
            get { return result; }
            set { result = value; }
        }

        public RecognitionCompletingArgs(string arg)
        {
            // Sonucun sonuna nokta (.) eklenmişse silinir.
            if (arg[arg.Length - 1] == '.')
                arg = arg.Substring(0, arg.Length - 1);
            this.Result = arg;
        }
    }

    public class PrivacyPolicyException : Exception
    {
        public PrivacyPolicyException(string message) : base(message)
        { }
    }

    public class NotSureWhatYouSaidException : Exception
    {
        public NotSureWhatYouSaidException(string message) : base(message)
        { }
    }
}
