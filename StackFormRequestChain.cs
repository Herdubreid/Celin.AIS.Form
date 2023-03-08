using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class StackFormRequestChain
    {
        public record OpenType(FormRequest request, Maybe<IEnumerable<Output.Type>> outputs);
        public record ExecuteType(ActionRequest request, Maybe<IEnumerable<Output.Type>> outputs);
        public record Type(OpenType open, IEnumerable<ExecuteType> execute);
        public IEnumerable<Type>? Chain { get; set; }
        public FormRequest? Demo { get; set; }
        public bool BreakOE { get; set; }
        static ExecuteType ActionRequest(StackExecute.Type t, int row, IEnumerable<object> cols)
            => new ExecuteType(new ActionRequest
            {
                stopOnWarning = Request.FALSE,
                formActions = t.actions.Select(a => a.action ?? FormAction.Make(a.formAction, row, cols))
            }, t.outputs.HasValue ? Maybe.Just(Output.Parse(t.outputs.Value, row, cols)) : t.outputs);
        public static Parser<char, StackFormRequestChain> Parser
            => Map((o, brk, ex, es) => new StackFormRequestChain
            {
                BreakOE = brk.HasValue ? brk.Value : false,
                Demo = o.form.demo && !ex.Any()
                ? new FormRequest
                {
                    formName = o.form.name,
                    formServiceDemo = Request.TRUE,
                    loadBaseFormOnly = Request.TRUE,
                    showActionControls = true
                }
                : null,
                Chain = es.HasValue
                ? es.Value.Aggregate(Enumerable.Empty<Type>(),
                    (l, e) =>
                    {
                        var t = new Type(
                            new OpenType
                            (
                                new FormRequest
                                {
                                    formName = o.form.name,
                                    version = o.form.version,
                                    stopOnWarning = Request.FALSE,
                                    maxPageSize = "No Max",
                                    formActions = o.actions.HasValue
                                    ? o.actions.Value.Select(a => a.action ?? FormAction.Make(a.formAction, l.Count(), e))
                                    : null
                                },
                                o.outputs.HasValue
                                ? Maybe.Just(Output.Parse(o.outputs.Value, l.Count(), e)) : o.outputs
                            ),
                            ex.Select(x => ActionRequest(x, l.Count(), e)));
                        return l.Append(t);
                    })
                    : new Type[]
                    {
                        new Type
                        (
                            new OpenType
                            (
                                new FormRequest
                                {
                                    formName = o.form.name,
                                    version = o.form.version,
                                    stopOnWarning = Request.FALSE,
                                    maxPageSize = "No Max",
                                    formActions = o.actions.HasValue
                                    ? o.actions.Value.Select(a => a.action ?? FormAction.Make(a.formAction, 0, Enumerable.Empty<object>()))
                                    : null
                                },
                                o.outputs
                            ),
                            ex.Select(x => ActionRequest(x, 0, Enumerable.Empty<object>()))
                        )
                    }
            },
                Data.Skipper.Next(StackOpen.Parser),
                Data.Skipper.Next(BreakOnError.Parser.Optional()),
                Data.Skipper.Next(StackExecute.Parser),
                Data.Skipper.Next(Each.Parser.Optional()));
    }
}
