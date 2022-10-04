using System.Collections.Generic;

namespace NeoSaveGames
{
    public static class SaveGameUtilities
    {
        public static string FilterPathString(string input)
        {
            var filtered = new List<char>(input.Length);

            bool lastCharWasSeparator = false;
            for (int i = 0; i < input.Length; ++i)
            {
                // Filter path separators (correct type and not start/end)
                var c = input[i];
                if (c == '\\' || c == '/')
                {
                    if (!lastCharWasSeparator)
                    {
                        if (i != 0 && i != input.Length - 1)
                        {
                            filtered.Add('/');
                        }
                        lastCharWasSeparator = true;
                    }
                    continue;
                }
                lastCharWasSeparator = false;

                // Allow alpha-numeric
                if (char.IsLetterOrDigit(c))
                {
                    filtered.Add(c);
                    continue;
                }

                // Allow whitespace and dashes, etc
                if (c == ' ' || c == '_' || c == '-')
                    filtered.Add(c);
            }

            return new string(filtered.ToArray());
        }
    }
}
