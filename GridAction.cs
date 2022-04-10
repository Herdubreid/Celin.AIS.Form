using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class GridAction
    {
        record ColumnType(string id, Value.Type value);
        record RowType(Value.Type row, IEnumerable<ColumnType> Columns);
        static readonly Parser<char, ColumnType> COLUMNEVENT
            = Map((id, v) => new ColumnType(id, v),
                Digit.ManyString(),
                Char(':')
                    .Then(Value.Variable.Or(Value.Literal)));
        static readonly Parser<char, RowType> ROWEVENT
            = Map((r, c) => new RowType(r, c),
                Value.Variable.Or(Digit.ManyString().Select(s => new Value.Type(false, s, -1))),
                Char(',')
                    .Then(SkipWhitespaces.Then(COLUMNEVENT.Separated(Whitespace))));
        static AIS.RowEvent RowEvent(RowType r, int row, IEnumerable<object> cols)
            => new AIS.RowEvent
            {
                rowNumber = r.row.variable ? row : Convert.ToInt32(r.row.literal),
                gridColumnEvents = r.Columns.Select(c => new AIS.ColumnEvent
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
        static Parser<char, AIS.Grid> INSERT
            = Map((g, rs, es) => new AIS.GridInsert
            {
                gridID = g,
                gridRowInsertEvents = es.HasValue
                ? es.Value.Aggregate(Enumerable.Empty<AIS.RowEvent>(),
                    (a, e) => a.Concat(rs.Select(r => RowEvent(r, a.Count() / rs.Count(), e))))
                : rs.Select(r => RowEvent(r, 0, Enumerable.Empty<object>()))
            } as AIS.Grid,
                Digit.ManyString(),
                Char(',')
                    .Then(SkipWhitespaces
                        .Then(ROWEVENT.Between(Char('('), SkipWhitespaces.Then(Char(')'))).Separated(Whitespaces))),
                Each.Parser.Optional());
        static Parser<char, AIS.Grid> UPDATE
            = Map((g, rs, es) => new AIS.GridUpdate
            {
                gridID = g,
                gridRowUpdateEvents = es.HasValue
                ? es.Value.Aggregate(Enumerable.Empty<AIS.RowEvent>(),
                    (a, e) => a.Concat(rs.Select(r => RowEvent(r, a.Count() / rs.Count(), e))))
                : rs.Select(r => RowEvent(r, 0, Enumerable.Empty<object>()))
            } as AIS.Grid,
                Digit.ManyString(),
                Char(',')
                    .Then(SkipWhitespaces
                        .Then(ROWEVENT.Between(Char('('), SkipWhitespaces.Then(Char(')'))).Separated(Whitespaces))),
                Each.Parser.Optional());
        public static Parser<char, AIS.Grid> InsertParser
            => String("insert")
                .Then(INSERT.Between(Char('['), SkipWhitespaces.Then(Char(']'))));
        public static Parser<char, AIS.Grid> UpdateParser
            => String("update")
                .Then(UPDATE.Between(Char('['), SkipWhitespaces.Then(Char(']'))));
        public static Parser<char, Action.Type> Parser
            => InsertParser.Or(UpdateParser)
                .Select(g => new Action.Type(new AIS.GridAction { gridAction = g }, null));
    }
}

/*
formActions: [
    {
        gridAction: {
            gridID: "1",
            gridRowInsertEvents: [
                {
                    rowNumber: 1,
                    gridColumnEvents: [
                        {
                            command: "SetGridCellValue",
                            value: "Some Value",
                            columnID: "18"
                        }
                    ]
                }
            ]
        }
    }
]
insert[1 (1 12:"some text" 14:20)]
open.w4312f.zjde001.query[all([10]=4000)].action[insert.each[[1,"A"],[2,"B"],[3,"C"]].[1, ($row,12:$col[0] 13:$col[1]) do(6)].action[do(8)].close
 */