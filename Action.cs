using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class Action
    {
        public record Type(AIS.Action? action, FormAction.Type? formAction);
        public static Parser<char, StackExecute.Type> Parser
            => Map((a, o) => new StackExecute.Type(a, o),
                Try(Data.Skipper.Next(String(".action")))
                  .Then(Data.Skipper.Next(GridAction.Parser.Or(FormAction.Parser))
                    .Separated(Whitespaces)
                    .Between(Char('['), SkipWhitespaces.Then(Char(']')))),
                Output.Parser.Many().Optional());
    }
}
