using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form
{
    public class Output
    {
        public enum Option
        {
            specs,
            dump,
            data,
            grid
        }
        public enum Variable
        {
            row,
            col,
            form,
            title,
            datacol,
            gridcol,
            records
        };
        public record VariableType(Variable variable, int index, int row, object? mapped);
        public record Type(Maybe<string> fmt, Maybe<IEnumerable<VariableType>> variables, Option? option);
        public static IEnumerable<Type> Parse(IEnumerable<Type> ts, int row, IEnumerable<object> cols)
            => ts.Select(t => new Type(t.fmt,
                t.variables.HasValue
                ? Maybe.Just(t.variables.Value.Select(v =>
                {
                    switch (v.variable)
                    {
                        case Variable.row:
                            return v with { mapped = row };
                        case Variable.col:
                            return v with { mapped = cols.ElementAt(v.index) };
                        default:
                            return v;
                    }
                }))
                : t.variables, t.option));
        static readonly Parser<char, Option> DUMP
            = Try(String(".dump").ThenReturn(Option.dump));
        static readonly Parser<char, Option> DATA
            = Try(String(".data").ThenReturn(Option.data));
        static readonly Parser<char, Option> GRID
            = Try(String(".grid").ThenReturn(Option.grid));
        static readonly Parser<char, VariableType> COL
            = Try(String($"${Variable.col}")
                .Then(DecimalNum.Between(Char('['), Char(']'))
                .Select(c => new VariableType(Variable.col, c, 0, null))));
        static readonly Parser<char, Tuple<int, int>> GRIDINDEX
            = Map((index, row) => new Tuple<int, int>(index, row),
                DecimalNum,
                Char(',')
                    .Then(SkipWhitespaces)
                    .Then(DecimalNum));
        static readonly Parser<char, VariableType> GRIDCOL
            = Try(String($"${Variable.gridcol}")
                .Then(GRIDINDEX.Between(Char('['), Char(']'))
                .Select(d => new VariableType(Variable.gridcol, d.Item1, d.Item2, null))));
        static readonly Parser<char, VariableType> DATACOL
            = Try(String($"${Variable.datacol}")
               .Then(DecimalNum.Between(Char('['), Char(']'))
               .Select(d => new VariableType(Variable.datacol, d, 0, null))));
        static readonly Parser<char, VariableType> ROW
            = Try(String($"${Variable.row}").ThenReturn(new VariableType(Variable.row, -1, 0, null)));
        static readonly Parser<char, VariableType> FORM
            = Try(String($"${Variable.form}").ThenReturn(new VariableType(Variable.form, -1, 0, null)));
        static readonly Parser<char, VariableType> TITLE
            = Try(String($"${Variable.title}").ThenReturn(new VariableType(Variable.title, -1, 0, null)));
        static readonly Parser<char, VariableType> RECORDS
            = Try(String($"${Variable.records}").ThenReturn(new VariableType(Variable.records, -1, 0, null)));
        static readonly Parser<char, IEnumerable<VariableType>> VARIABLES
            = Try(Char(',')
                .Then(SkipWhitespaces
                  .Then(OneOf(ROW, COL, FORM, TITLE, DATACOL, GRIDCOL, RECORDS)
                    .Separated(Char(',').Then(SkipWhitespaces)))))
              .Or(OneOf(ROW, COL, FORM, TITLE, DATACOL, RECORDS).Select(v => new[] { v } as IEnumerable<VariableType>));
        static readonly Parser<char, string> FMT
            = AnyCharExcept('"')
                .ManyString()
                .Between(Char('"'));
        static Parser<char, Type> PARAMETERS
            = Map((f, v) => new Type(f, v, null),
                Try(FMT).Optional(), VARIABLES.Optional());
        public static Parser<char, Type> Parser
            => Try(Data.Skipper.Next(String(".output")))
                 .Then(Try(OneOf(DUMP,DATA,GRID).Select(o => new Type(Maybe.Nothing<string>(), Maybe.Nothing<IEnumerable<VariableType>>(), o)))
                   .Or(PARAMETERS.Between(Char('('), Char(')'))));
    }
}
