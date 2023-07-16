using Celin.AIS.Data;
using Pidgin;
using static Pidgin.Parser;

namespace Celin.AIS.Form;

public class Ube
{
    public record CriteriaType((string, string) Subject, ComparisonType Comparison, IEnumerable<string> Value);
    public enum LiteralType
    {
        SINGLE,
        LIST,
        RANGE
    }
    public static LiteralType Expected(ComparisonType comparisonType) => comparisonType switch
    {
        > ComparisonType.NOT_VALUE_IN_LIST => LiteralType.LIST,
        > ComparisonType.EQUAL_OR_EMPTY => LiteralType.RANGE,
        _ => LiteralType.SINGLE,
    };
    public enum ComparisonType
    {
        EQUAL,
        NOT_EQUAL,
        LESS_THAN,
        GREATER_THAN,
        LESS_EQUAL,
        GREATER_EQUAL,
        EQUAL_OR_EMPTY,
        VALUE_IN_LIST,
        NOT_VALUE_IN_LIST,
        WITHIN_A_RANGE,
        NOT_WITHIN_A_RANGE,
    }
    static readonly Parser<char, ComparisonType> EQUAL =
        Base.Tok('=').ThenReturn(ComparisonType.EQUAL);
    static readonly Parser<char, ComparisonType> NOT_EQUAL =
        Base.Tok("<>").Or(Base.Tok("!=")).ThenReturn(ComparisonType.NOT_EQUAL);
    static readonly Parser<char, ComparisonType> LESS_THAN =
        Base.Tok('<').ThenReturn(ComparisonType.LESS_THAN);
    static readonly Parser<char, ComparisonType> GREATER_THAN =
        Base.Tok('>').ThenReturn(ComparisonType.GREATER_THAN);
    static readonly Parser<char, ComparisonType> LESS_EQUAL =
        Base.Tok("<=").ThenReturn(ComparisonType.LESS_EQUAL);
    static readonly Parser<char, ComparisonType> GREATER_EQUAL =
        Base.Tok(">=").ThenReturn(ComparisonType.GREATER_EQUAL);
    static readonly Parser<char, ComparisonType> VALUE_IN_LIST =
        Base.Tok("in").ThenReturn(ComparisonType.VALUE_IN_LIST);
    static readonly Parser<char, ComparisonType> NOT_VALUE_IN_LIST =
        Base.Tok("!in").ThenReturn(ComparisonType.NOT_VALUE_IN_LIST);
    static readonly Parser<char, ComparisonType> WITHIN_A_RANGE =
        Base.Tok("bw").ThenReturn(ComparisonType.WITHIN_A_RANGE);
    static readonly Parser<char, ComparisonType> NOT_WITHIN_A_RANGE =
        Base.Tok("!bw").ThenReturn(ComparisonType.NOT_WITHIN_A_RANGE);
    static readonly Parser<char, ComparisonType> EQUAL_OR_EMPTY =
        Base.Tok("?=").ThenReturn(ComparisonType.EQUAL_OR_EMPTY);
    public static Parser<char, ComparisonType> COMPARISON_TYPE
        => SkipWhitespaces
            .Then(OneOf(
                EQUAL,
                NOT_EQUAL,
                LESS_THAN,
                GREATER_THAN,
                LESS_EQUAL,
                GREATER_EQUAL,
                VALUE_IN_LIST,
                NOT_VALUE_IN_LIST,
                WITHIN_A_RANGE,
                NOT_WITHIN_A_RANGE,
                EQUAL_OR_EMPTY));
    public static Parser<char, (string, string)> Subject
        => Map((p, s) => (
                s.HasValue ? p.ToUpper() : string.Empty,
                s.HasValue ? s.Value.ToUpper() : p.ToUpper()
            ),
            SkipWhitespaces
            .Then(Letter)
            .Then(LetterOrDigit.ManyString(), (h, t) => h + t),
            Char('.')
            .Then(Letter
            .Then(LetterOrDigit.ManyString(), (h, t) => h + t))
            .Optional());
    public static Parser<char, IEnumerable<CriteriaType>> Criteria
        => Map((s, c, v) => new CriteriaType(s, c, v),
            SkipWhitespaces
            .Then(Subject),
            COMPARISON_TYPE,
            Literal.Array)
          .Separated(Whitespaces);
}
