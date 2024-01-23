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
				@operator = AIS.Condition.EQUAL,
				value = List(Literal(value))
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
		public static AIS.FormAction Select(int row)
			=> new AIS.FormAction
			{
				controlID = string.Format("1.{0}", row),
				command = AIS.FormAction.SelectRow
			};
		// Do FormAction
		public static AIS.FormAction Do(string controlId)
			=> new AIS.FormAction
			{
				controlID = controlId,
				command = AIS.FormAction.DoAction
			};
		// Query
		public static AIS.Query Query(IEnumerable<AIS.Condition> condition)
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
				action = AIS.StackFormRequest.open,
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
				action = AIS.StackFormRequest.close,
				stackId = rs.stackId,
				stateId = rs.stateId,
				rid = rs.rid
			};
	}
}
