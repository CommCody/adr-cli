using System.Linq;
using System.Threading;

namespace adr.Adr
{
    /// <summary>
    /// ADR link model
    /// </summary>
    public class AdrLink
    {
        /// <summary>
        /// Gets or sets the linked record number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the link description (e.g. "Amends")
        /// </summary>
        public string LinkDescription { get; set; }

        /// <summary>
        /// Gets or sets the reverse link record description (e.g. "Amended by")
        /// </summary>
        public string ReverseLinkDescription { get; set; }

        /// <summary>
        /// Try to parse the given link
        /// </summary>
        /// <param name="link">the link to parse</param>
        /// <returns><code>true</code> if parsed successfully, <code>false</code> otherwise</returns>
        public static bool TryParse(string link, out AdrLink adrLink)
        {
            bool res = false;
            adrLink = null;

            if (string.IsNullOrEmpty(link))
            {
                throw new System.ArgumentException($"'{nameof(link)}' cannot be null or empty", nameof(link));
            }

            try
            {
                var tokens = link.Split(':', System.StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Count() == 3)
                {
                    int number = 0;

                    if (int.TryParse(tokens[0], out number))
                    {
                        adrLink = new AdrLink()
                        {
                            Number = number,
                            LinkDescription = tokens[1],
                            ReverseLinkDescription = tokens[2]
                        };

                        res = true;
                    }
                }
            }
            catch (System.Exception)
            {
                // nop
                res = false;
            }

            return res;
        }
    }
}
