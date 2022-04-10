using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class StackExecute
    {
        static AIS.ActionRequest ActionRequest(IEnumerable<Action.Type> t, int row, IEnumerable<object> cols)
            => new AIS.ActionRequest
            {
                stopOnWarning = AIS.Request.FALSE,
                formActions = t.Select(a => a.action ?? FormAction.Make(a.formAction, row, cols))
            };
        public static Parser<char, IEnumerable<AIS.ActionRequest>> Parser
            => Map((acs, es) => es.HasValue
                ? es.Value.Aggregate(Enumerable.Empty<AIS.ActionRequest>(),
                    (a, e) => a.Concat(acs.Select(ac => ActionRequest(ac, a.Count() / acs.Count(), e))))
                : acs.Select(ac => ActionRequest(ac, 0, Enumerable.Empty<object>())),
                Action.Parser.Many(),
                Each.Parser.Optional());
    }
}
