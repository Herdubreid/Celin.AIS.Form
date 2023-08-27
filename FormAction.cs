using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class FormAction
    {
        public record Type(string id, string cmd, Maybe<Value.Type> value);
        public static AIS.Action Make(Type? t, int row, IEnumerable<object> cols)
            => new AIS.FormAction
            {
                command = t.cmd,
                controlID = t.id,
                value = t.value.HasValue
                    ? t.value.Value.variable
                        ? t.value.Value.index == -1
                            ? row.ToString()
                            : cols.ElementAt(t.value.Value.index).ToString()
                        : t.value.Value.literal
                    : null
            };
        static readonly Parser<char, (string id, Maybe<Value.Type> value)> SETPARAMETERS
            = Map((id, v) => (id, Maybe.Just(v)),
                Digit.ManyString(),
                Char(',')
                    .Then(SkipWhitespaces)
                    .Then(Value.Variable.Or(Value.Literal)));
        static readonly Parser<char, Type> SET
            = Map((a, p) => new Type(p.id, a, p.value),
                Try(String("set")).ThenReturn(AIS.FormAction.SetControlValue),
                SETPARAMETERS.Between(Char('('), Char(')')));
        static readonly Parser<char, Type> RADIO
            = Map((a, p) => new Type(p.id, a, p.value),
                Try(String("radio")).ThenReturn(AIS.FormAction.SetRadioButton),
                SETPARAMETERS.Between(Char('('), Char(')')));
        static readonly Parser<char, Type> DO
            = Map((a, id) => new Type(id, a, Maybe.Nothing<Value.Type>()),
                Try(String("do")).ThenReturn(AIS.FormAction.DoAction),
                Digit.ManyString().Between(Char('('), Char(')')));
        static readonly Parser<char, string> SELECTPARAMETER
            = Map((g, r) => $"{g}.{r}",
                Digit.ManyString(),
                Char('.')
                    .Then(Digit.ManyString()));
        static readonly Parser<char, Type> SELECT
            = Map((a, id) => new Type(id, a, Maybe.Nothing<Value.Type>()),
                Try(String("select")).ThenReturn(AIS.FormAction.SelectRow),
                SELECTPARAMETER.Between(Char('('), Char(')')));
        static readonly Parser<char, Type> UNSELECTALL
            = Map((a, id) => new Type(id, a, Maybe.Nothing<Value.Type>()),
                Try(String("unselectAll")).ThenReturn(AIS.FormAction.UnSelectAllRows),
                Digit.ManyString().Between(Char('('), Char(')')));
        static readonly Parser<char, (string id, Value.Type value)> QBEPARAMETERS
            = Map((g, id, v) => ($"{g}[{id}]", v),
                Digit.ManyString(),
                Digit.ManyString().Between(Char('['), Char(']')),
                Char(',')
                    .Then(SkipWhitespaces)
                    .Then(Value.Variable.Or(Value.Literal)));
        static readonly Parser<char, Type> QBE
            = Map((a, p) => new Type(p.id, a, Maybe.Just(p.value)),
                Try(String("qbe")).ThenReturn(AIS.FormAction.SetQBEValue),
                QBEPARAMETERS.Between(Char('('), Char(')')));
        public static Parser<char, Action.Type> Parser
            => OneOf(DO, SET, RADIO, SELECT, UNSELECTALL, QBE).Select(t => new Action.Type(null, t));
    }
}
