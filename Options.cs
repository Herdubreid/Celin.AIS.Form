using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class BreakOnError
    {
        static readonly Parser<char, bool> YES
            = Try(CIString("yes")).ThenReturn(true);
        static readonly Parser<char, bool> NO
            = Try(CIString("no")).ThenReturn(false);
        static readonly Parser<char, bool> TRUE
            = Try(CIString("true")).ThenReturn(true);
        static readonly Parser<char, bool> FALSE
            = Try(CIString("false")).ThenReturn(false);
        public static Parser<char, bool> Parser
            => Try(Data.Skipper.Next(String(".breakOnError")))
                .Then(OneOf(YES, NO, TRUE, FALSE).Between(Char('('), Char(')')));
    }
}
