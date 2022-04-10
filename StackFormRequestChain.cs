using Pidgin;
using Pidgin.Comment;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class StackFormRequestChain
    {
        public AIS.FormRequest? Open { get; set; }
        public IEnumerable<AIS.ActionRequest>? Execute { get; set; }
        public static Parser<char, StackFormRequestChain> Parser
            => Map((o, e) => new StackFormRequestChain
            {
                Open = o,
                Execute = e,
            },
                StackOpen.Parser,
                StackExecute.Parser.Before(String(".close")));
    }
}
