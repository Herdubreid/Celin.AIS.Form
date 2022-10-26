using Pidgin;

namespace Celin.AIS.Form
{
    public class StackExecute
    {
        public record Type(IEnumerable<Action.Type> actions, Maybe<IEnumerable<Output.Type>> outputs);
        public static Parser<char, IEnumerable<Type>> Parser
            => Data.Skipper.Next(Action.Parser.Many());
    }
}
