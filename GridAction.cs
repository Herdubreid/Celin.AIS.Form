using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class GridAction
    {
        record ColumnType(string id, Value.Type value);
        record RowType(Value.Type row, IEnumerable<ColumnType> cols, Maybe<IEnumerable<IEnumerable<object>>> es);
        static readonly Parser<char, ColumnType> COLUMNEVENT
            = Map((id, v) => new ColumnType(id, v),
                Data.Skipper.Next(Digit.AtLeastOnceString()),
                Char(':')
                    .Then(Value.Variable.Or(Value.Literal)))
              .Labelled("Column");
        static readonly Parser<char, RowType> ROWEVENT
            = Map((r, c, es) => new RowType(r, c, es),
                Value.Variable.Or(Digit.AtLeastOnceString().Select(s => new Value.Type(false, s, -1))),
                Char(':')
                  .Then(COLUMNEVENT
                    .Separated(Char(',').Then(SkipWhitespaces))
                    .Between(Char('('), SkipWhitespaces.Then(Char(')')))),
                Each.Parser.Optional())
              .Labelled("Row Event");
        static AIS.RowEvent RowEvent(RowType r, int row, IEnumerable<object> cols)
            => new AIS.RowEvent
            {
                rowNumber = r.row.variable
                ? r.row.index == -1
                    ? row
                    : Convert.ToInt32(cols.ElementAt(r.row.index))
                : Convert.ToInt32(r.row.literal),
                gridColumnEvents = r.cols.Select(c => new AIS.ColumnEvent
                {
                    columnID = c.id,
                    value = c.value.variable
                    ? c.value.index == -1
                        ? row.ToString()
                        : cols.ElementAt(c.value.index).ToString()
                    : c.value.literal,
                    command = AIS.GridAction.SetGridCellValue
                })
            };
        static IEnumerable<AIS.RowEvent> RowEvents(IEnumerable<RowType> rs)
            => rs.Aggregate(Enumerable.Empty<AIS.RowEvent>(), (re, r) =>
            {
                if (r.es.HasValue)
                {
                    int row = 0;
                    return r.es.Value.Aggregate(re, (re1, e) => re1.Append(RowEvent(r, row++, e)));
                }
                else
                {
                    return re.Append(RowEvent(r, 0, Enumerable.Empty<object>()));
                }
            });
        static Parser<char, AIS.Grid> INSERT
            = Map((g, rs, es) => new AIS.GridInsert
            {
                gridID = g,
                gridRowInsertEvents = RowEvents(rs)
            } as AIS.Grid,
                Data.Skipper.Next(Digit.AtLeastOnceString()),
                Data.Skipper.Next(ROWEVENT.Separated(Whitespaces)),
                Each.Parser.Optional())
            .Labelled("Grid Insert");
        static Parser<char, AIS.Grid> UPDATE
            = Map((g, rs, es) => new AIS.GridUpdate
            {
                gridID = g,
                gridRowUpdateEvents = RowEvents(rs)
            } as AIS.Grid,
                Data.Skipper.Next(Digit.AtLeastOnceString()),
                Data.Skipper.Next(ROWEVENT.Separated(Whitespaces)),
                Each.Parser.Optional())
            .Labelled("Grid Update");
        public static Parser<char, AIS.Grid> InsertParser
            => String("insert")
                .Then(INSERT.Between(Char('['), SkipWhitespaces.Then(Char(']'))))
            .Labelled("Insert");
        public static Parser<char, AIS.Grid> UpdateParser
            => String("update")
                .Then(UPDATE.Between(Char('['), SkipWhitespaces.Then(Char(']'))))
            .Labelled("Update");
        public static Parser<char, Action.Type> Parser
            => InsertParser.Or(UpdateParser)
                .Select(g => new Action.Type(new AIS.GridAction { gridAction = g }, null));
    }
}
