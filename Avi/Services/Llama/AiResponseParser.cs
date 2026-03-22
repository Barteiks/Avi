using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Avi.Services.Llama
{
    public class AiResponseParser
    {
        public event Action<JsonElement>? OnParsedItem;

        private readonly StringBuilder _buffer = new();

        private bool _insideString = false;
        private bool _escape = false;

        private int _braceDepth = 0;
        private int _objectStart = -1;

        public void HandleToken(string token)
        {
            foreach (char c in token)
            {
                _buffer.Append(c);

                if (_escape)
                {
                    _escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    _escape = true;
                    continue;
                }

                if (c == '"')
                {
                    _insideString = !_insideString;
                    continue;
                }

                if (_insideString)
                    continue;

                if (c == '{')
                {
                    if (_braceDepth == 0)
                        _objectStart = _buffer.Length - 1;

                    _braceDepth++;
                }

                if (c == '}')
                {
                    _braceDepth--;

                    if (_braceDepth == 0 && _objectStart >= 0)
                    {
                        EmitObject();
                    }
                }
            }
        }

        private void EmitObject()
        {
            try
            {
                var text = _buffer.ToString();
                var objJson = text.Substring(_objectStart, _buffer.Length - _objectStart);

                var item = JsonSerializer.Deserialize<JsonElement>(objJson);

                // Protect the parser from exceptions thrown by handlers.
                try
                {
                    OnParsedItem?.Invoke(item);
                }
                catch (Exception exHandler)
                {
                    Debug.WriteLine($"[AiResponseParser] Handler error: {exHandler.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AiResponseParser] JSON parse error: {ex.Message}");
            }
            finally
            {
                // Reset parser state so we don't get stuck if handler threw or parsing failed.
                _buffer.Clear();
                _objectStart = -1;
                _braceDepth = 0;
                _insideString = false;
                _escape = false;
            }
        }
    }
}
