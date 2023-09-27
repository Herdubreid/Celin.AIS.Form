using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class Value
    {
        public record Type(bool variable, string literal, int index);
        static readonly Parser<char, Type> PLAIN
            = Try(LetterOrDigit.Or(Char('.')).ManyString().Select(s => new Type(false, s, -1)));
        static readonly Parser<char, Type> SINGLEEQUOTED
            = AnyCharExcept('\'')
                .ManyString()
                .Between(Char('\''))
              .Select(s => new Type(false, s, -1));
        static readonly Parser<char, Type> DOUBLEQUOTED
            = AnyCharExcept('"')
                .ManyString()
                .Between(Char('"'))
              .Select(s => new Type(false, s, -1));
        static readonly Parser<char, Type> ROWVAR
            = Try(String("row").Select(_ => new Type(true, string.Empty, -1)));
        static readonly Parser<char, Type> COLVAR
            = Try(String("col")
                .Then(DecimalNum.Between(Char('['), Char(']'))
                .Select(c => new Type(true, string.Empty, c))));
        public static Parser<char, Type> Variable
            => Try(Char('$')
                .Then(ROWVAR.Or(COLVAR)));
        public static Parser<char, Type> Literal
            => OneOf(DOUBLEQUOTED, SINGLEEQUOTED, PLAIN);
    }
}
