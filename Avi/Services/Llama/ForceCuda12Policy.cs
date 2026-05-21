using LLama.Abstractions;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.Services.Llama
{
    public class ForceCuda12Policy : INativeLibrarySelectingPolicy
    {
        public IEnumerable<INativeLibrary> Apply(
            NativeLibraryConfig.Description description,
            SystemInfo systemInfo,
            NativeLogConfig.LLamaLogCallback? logCallback)
        {
            // Zawsze próbuj CUDA 12 z bundlowanych DLL-ek  
            yield return new NativeLibraryWithCuda(12, description.Library, description.AvxLevel, skipCheck: true);

            // Fallback do CPU jeśli CUDA 12 się nie załaduje  
            if (description.AllowFallback)
            {
                if (description.AvxLevel >= AvxLevel.Avx2)
                    yield return new NativeLibraryWithAvx(description.Library, AvxLevel.Avx2, description.SkipCheck);
                yield return new NativeLibraryWithAvx(description.Library, AvxLevel.None, description.SkipCheck);
            }
        }
    }
}
