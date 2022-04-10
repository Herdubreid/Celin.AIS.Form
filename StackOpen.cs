using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class StackOpen
    {
        static readonly Parser<char, string> FORM =
            String("open.")
            .Then(OneOf('w', 'W'))
            .Then(LetterOrDigit
                .ManyString(), (p, b) => $"P{b.Remove(b.Length - 1)}_{p}{b}".ToUpper());
        public static Parser<char, AIS.FormRequest> Parser
            => Map((f, v) => new AIS.FormRequest
            {
                formName = f,
                version = v.ToUpper(),
                stopOnWarning = AIS.Request.FALSE,
            },
                FORM,
                Char('.').Then(LetterOrDigit.ManyString()));
    }
}
