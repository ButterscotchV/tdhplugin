using System.Collections.Generic;
using System.Text;

namespace TDHPlugin.Commands
{
	public static class CommandUtils
	{
		public static int IndexOfNonEscaped(string inString, char inChar, char escapeChar = '\\', int startIndex = 0)
		{
			if (!string.IsNullOrEmpty(inString))
			{
				bool escaped = false;

				for (int i = startIndex; i < inString.Length; i++)
				{
					char stringChar = inString[i];

					if (!escaped)
					{
						if (stringChar == escapeChar)
						{
							escaped = true;
							continue;
						}
					}

					// If the character is escaped or the character that's escaped is an escape character then check if it matches
					if ((!escaped || stringChar == escapeChar) && stringChar == inChar)
					{
						return i;
					}

					escaped = false;
				}
			}

			return -1;
		}

		public static string[] StringToArgs(string inString, char separator = ' ', char escapeChar = '\\', char quoteChar = '\"', bool keepQuotes = false)
		{
			if (string.IsNullOrEmpty(inString))
				return new string[0];

			List<string> args = new List<string>();
			StringBuilder strBuilder = new StringBuilder();
			bool inQuotes = false;
			bool escaped = false;

			for (int i = 0; i < inString.Length; i++)
			{
				char stringChar = inString[i];

				if (!escaped)
				{
					if (stringChar == escapeChar)
					{
						escaped = true;
						continue;
					}

					if (stringChar == quoteChar && (inQuotes || IndexOfNonEscaped(inString, quoteChar, escapeChar, i + 1) > 0))
					{
						// Ignore quotes if there's no future non-escaped quotes

						inQuotes = !inQuotes;
						if (!keepQuotes)
							continue;
					}
					else if (!inQuotes && stringChar == separator)
					{
						args.Add(strBuilder.ToString());
						strBuilder.Clear();
						continue;
					}
				}

				strBuilder.Append(stringChar);
				escaped = false;
			}

			args.Add(strBuilder.ToString());

			return args.ToArray();
		}
	}
}