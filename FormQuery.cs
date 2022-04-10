using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class FormQuery
    {
        public string? maxPageSize { get; set; }
        public AIS.Query? Query { get; set; }
        static Data.AndOrCombinator last { get; set; } = Data.AndOrCombinator.AND;

        static Parser<char, FormQuery> QUERY
            = Map((q, o) => new FormQuery
            {
                maxPageSize = o.HasValue && o.Value.Item3.HasValue
                ? o.Value.Item3.Value.Equals("no") ? "No Max" : o.Value.Item3.Value
                : null,
                Query = q.Count() > 0
                ? new Query()
                {
                    matchType = q.Count() == 1 ? q.First().Item2.ToString("G") : null,
                    condition = q.Count() == 1
                    ? q.First().Item3
                    : null,
                    complexQuery = q.Count() > 1
                        ? q.Select(r =>
                        {
                            var qry = new ComplexQuery
                            {
                                andOr = last.ToString("G"),
                                query = new Query()
                                {
                                    matchType = r.Item2.ToString("G"),
                                    condition = r.Item3
                                }
                            };
                            last = r.Item1.HasValue ? r.Item1.Value : Data.AndOrCombinator.AND;
                            return qry;
                        })
                        : null
                }
                : null,

            },
                Data.QryOp.FormQueries,
                Data.QryOptions.Parser.Optional());
        public static Parser<char, FormQuery> Parser
            => String(".query")
                .Then(QUERY
                .Between(Char('['), SkipWhitespaces.Then(Char(']'))));

    }
}
