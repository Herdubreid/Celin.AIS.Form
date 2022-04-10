using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class Action
    {
        public record Type(AIS.Action? action, FormAction.Type? formAction);
        public static Parser<char, IEnumerable<Type>> Parser
            => Try(String(".action"))
                .Then(GridAction.Parser.Or(FormAction.Parser)
                .Separated(Whitespaces)
                .Between(Char('['), SkipWhitespaces.Then(Char(']'))));
    }
}
