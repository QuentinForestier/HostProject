using PdfSharpCore.Fonts;
using System;
using System.IO;

namespace Host
{
    public class FontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo("Assets/Fonts/OpenSans-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo("Assets/Fonts/OpenSans-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo("Assets/Fonts/OpenSans-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo("Assets/Fonts/OpenSans-Regular.ttf");
                }
            }
            return null;
        }
    }
}