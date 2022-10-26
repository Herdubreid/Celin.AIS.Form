using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class StackOpen
    {
        public record Form(string name, string? version, bool demo);
        public record Type(Form form, Maybe<IEnumerable<Action.Type>> actions, Maybe<IEnumerable<Output.Type>> outputs);
        static readonly Parser<char, (string form, Maybe<string> version)> FORM
            = Map((f, v) => (f, v),
                OneOf('w', 'W')
                  .Then(LetterOrDigit
                  .AtLeastOnceString(), (p, b) => $"P{b.Remove(b.Length - 1)}_{p}{b}".ToUpper()),
                Char(',')
                  .Then(SkipWhitespaces
                    .Then(LetterOrDigit.ManyString())).Optional());
        public static Parser<char, Type> Parser
            => Map((f, r, o) => new Type(
                new Form(f.form, f.version.HasValue ? f.version.Value.ToUpper() : null, !r.HasValue),
                r, o),
                String("open")
                  .Then(FORM.Between(Char('('), Char(')'))),
                Data.Skipper.Next(GridAction.Parser.Or(FormAction.Parser))
                  .Separated(Whitespaces)
                  .Between(Data.Skipper.Next(Char('[')), SkipWhitespaces.Then(Char(']')))
                    .Optional(),
                Output.Parser.Many().Optional());
    }
}
