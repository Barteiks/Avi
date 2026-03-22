using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.Services
{
    public interface ISpeechListener
    {
        /// <summary>
        /// Wywoływane gdy Whisper rozpozna tekst z mikrofonu
        /// </summary>
        bool IsInterested(string text);
        void HandleSpeech(string text);
        Task OnSpeechRecognizedAsync(string text);
    }

}
