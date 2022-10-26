using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class Each
    {
        static Parser<char, object> QUOTED
            = AnyCharExcept('\"')
                .ManyString()
                .Between(Char('\"'))
              .Select(s => s as object);
        static Parser<char, object> DECIMAL
            = Try(Real).Select(d => Convert.ToDecimal(d) as object);
        static Parser<char, IEnumerable<object>> ARRAY
            = QUOTED.Or(DECIMAL)
                .Separated(Char(','))
                .Between(Char('['), Char(']'));
        public static Parser<char, IEnumerable<IEnumerable<object>>> Parser
            => Try(Data.Skipper.Next(String(".each")))
                .Then(ARRAY
                    .Separated(Char(','))
                    .Between(Char('['), Char(']')));
    }
}
