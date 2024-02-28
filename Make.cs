using System.Text.RegularExpressions;

namespace Celin.AIS.Form
{
    // AIS Element Factories
    public class Make
	{
		// Generic list converter
		public static IEnumerable<T> List<T>(params T[] items)
			=> items;
		// Literal value
		public static AIS.Value Literal(string content)
			=> new AIS.Value
			{
				content = content,
				specialValueId = AIS.Value.LITERAL
			};
		// Equal condition
		public static Condition Equal(string controlId, string value)
			=> new Condition
            {
				controlId = controlId,
				@operator = Condition.EQUAL,
				value = List(Literal(value))
			};
		// QBE FormAction
		public static AIS.FormAction QBE(string controlId, string value)
			=> new AIS.FormAction
			{
				controlID = $"1[{controlId}]",
				command = AIS.FormAction.SetQBEValue,
				value = value
			};
		// Set FormAction
		public static AIS.FormAction Set(string controlId, string value)
			=> new AIS.FormAction
			{
				controlID = controlId,
				command = AIS.FormAction.SetControlValue,
				value = value
			};
		// Select FormAction
		public static AIS.FormAction Select(int row, int grid = 1)
			=> new AIS.FormAction
			{
				controlID = $"{grid}.{row}",
				command = AIS.FormAction.SelectRow
			};
		// SelectAll FormAction
		public static AIS.FormAction SelectAll(int grid = 1)
			=> new AIS.FormAction
			{
				controlID = $"{grid}",
				command = AIS.FormAction.SelectAllRows
			};
		// UnSelect FormAction
		public static AIS.FormAction UnSelect(int row, int grid = 1)
			=> new AIS.FormAction
			{
				controlID = $"{grid}.{row}",
				command = AIS.FormAction.UnSelectRow
			};
		// UnSelectAll FormAction
		public static AIS.FormAction UnSelectAll(int row, int grid = 1)
			=> new AIS.FormAction
			{
				controlID = $"{grid}",
				command = AIS.FormAction.UnSelectAllRows
			};
		// Do FormAction
		public static AIS.FormAction Do(string controlId)
			=> new AIS.FormAction
			{
				controlID = controlId,
				command = AIS.FormAction.DoAction
			};
		// Query
		public static Query Query(IEnumerable<Condition> condition)
			=> new AIS.Query
			{
				matchType = AIS.Query.MATCH_ALL,
				autoFind = true,
				condition = condition
			};
		// Open Stack Form
		public static StackFormRequest Open(FormRequest fm)
			=> new StackFormRequest
            {
				action = StackFormRequest.open,
				formRequest = fm
			};
		static readonly Regex FormPat = new Regex("[^_]+_([^_]+).*");
		// Execute Stack Form
		public static StackFormRequest Execute(FormResponse rs, ActionRequest rq)
		{
			rq.formOID = FormPat.Match(rs.currentApp).Groups[1].Value;
			return new StackFormRequest
            {
				action = StackFormRequest.execute,
				stackId = rs.stackId,
				stateId = rs.stateId,
				rid = rs.rid,
				actionRequest = rq
			};
		}
		// Close Stack Form
		public static StackFormRequest Close(FormResponse rs)
			=> new StackFormRequest
            {
				action = StackFormRequest.close,
				stackId = rs.stackId,
				stateId = rs.stateId,
				rid = rs.rid
			};
	}
}
