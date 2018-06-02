namespace RazzleServer.Common.Wz.Util
{
    public static class XmlUtil
    {

        private static readonly char[] specialCharacters = { '"', '\'', '&', '<', '>' };
        private static readonly string[] replacementStrings = { "&quot;", "&apos;", "&amp;", "&lt;", "&gt;" };

        public static string SanitizeText(string text)
        {
            var fixedText = "";
            foreach (var chr in text)
            {
                var charFixed = false;
                for (var k = 0; k < specialCharacters.Length; k++)
                {

                    if (chr == specialCharacters[k])
                    {
                        fixedText += replacementStrings[k];
                        charFixed = true;
                        break;
                    }
                }
                if (!charFixed)
                {
                    fixedText += chr;
                }
            }
            return fixedText;
        }

        public static string EmptyNamedTag(string tag, string name)
        {
            return OpenNamedTag(tag, name, true, true);
        }

        public static string EmptyNamedValuePair(string tag, string name, string value)
        {
            return OpenNamedTag(tag, name, false) + Attrib("value", value, true, true);
        }

        public static string OpenNamedTag(string tag, string name, bool finish, bool empty = false)
        {
            return "<" + tag + " name=\"" + name + "\"" + (finish ? (empty ? "/>" : ">") : " ");
        }

        public static string Attrib(string name, string value, bool closeTag = false, bool empty = false)
        {
            return name + "=\"" + SanitizeText(value) + "\"" + (closeTag ? (empty ? "/>" : ">") : " ");
        }

        public static string CloseTag(string tag)
        {
            return "</" + tag + ">";
        }

        public static string Indentation(int level)
        {
            var indent = new char[level];
            for (var i = 0; i < indent.Length; i++)
            {
                indent[i] = '\t';
            }
            return new string(indent);
        }
    }
}