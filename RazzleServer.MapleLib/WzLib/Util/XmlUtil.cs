using System;
using System.Collections;
using System.IO;
using System.Text;

namespace MapleLib.WzLib.Util
{
	public class XmlUtil
	{

		private static readonly char[] specialCharacters = {'"', '\'', '&', '<', '>'};
		private static readonly string[] replacementStrings = {"&quot;", "&apos;", "&amp;", "&lt;", "&gt;"};

		public static string SanitizeText(string pText)
		{
			string fixedText = "";
			bool charFixed;
			for (int i = 0; i < pText.Length; i++)
			{
				charFixed = false;
				for (int k = 0; k < specialCharacters.Length; k++)
				{

					if (pText[i] == specialCharacters[k])
					{
						fixedText += replacementStrings[k];
						charFixed = true;
						break;
					}
				}
				if (!charFixed)
				{
					fixedText += pText[i];
				}
			}
			return fixedText;
		}

		public static string OpenNamedTag(string pTag, string pName, bool pFinish)
		{
			return OpenNamedTag(pTag, pName, pFinish, false);
		}

		public static string EmptyNamedTag(string pTag, string pName)
		{
			return OpenNamedTag(pTag, pName, true, true);
		}

		public static string EmptyNamedValuePair(string pTag, string pName, string pValue)
		{
			return OpenNamedTag(pTag, pName, false, false) + Attrib("value", pValue, true, true);
		}

		public static string OpenNamedTag(string pTag, string pName, bool pFinish, bool pEmpty)
		{
			return "<" + pTag + " name=\"" + pName + "\"" + (pFinish ? (pEmpty ? "/>" : ">") : " ");
		}

		public static string Attrib(string pName, string pValue)
		{
			return Attrib(pName, pValue, false, false);
		}

		public static string Attrib(string pName, string pValue, bool pCloseTag, bool pEmpty)
		{
			return pName + "=\"" + SanitizeText(pValue) + "\"" + (pCloseTag ? (pEmpty ? "/>" : ">") : " ");
		}

		public static string CloseTag(string pTag)
		{
			return "</" + pTag + ">";
		}

		public static string Indentation(int pLevel)
		{
			char[] indent = new char[pLevel];
			for (int i = 0; i < indent.Length; i++)
			{
				indent[i] = '\t';
			}
			return new String(indent);
		}
	}
}