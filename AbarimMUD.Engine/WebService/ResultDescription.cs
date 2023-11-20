
namespace AbarimMUD.WebService
{
	public class ResultDescription
	{
		public ResultType ResultType { get; set; }
		public string ResultString { get; set; }
		
		public ResultDescription()
		{
		}

		public ResultDescription(ResultType type, string desc)
		{
			ResultType = type;
			ResultString = desc;
		}

		public static ResultDescription CreateFromResultType(ResultType type)
		{
			return new ResultDescription(type, type.GetResultTypeString());
		}

		public static ResultDescription CreateInvalidAccount(string accountName)
		{
			return new ResultDescription(ResultType.AccountNotFound,
				string.Format(ResultType.AccountNotFound.GetResultTypeString(), accountName));
		}

		public static ResultDescription CreateAreaNotFound(int areaId)
		{
			return new ResultDescription(ResultType.AreaNotFound,
				string.Format(ResultType.AreaNotFound.GetResultTypeString(), areaId));
		}

		public static ResultDescription CreateRequiredParameterIsNotSet(string parameter)
		{
			return new ResultDescription(ResultType.RequiredParameterIsNotSet,
				string.Format(ResultType.RequiredParameterIsNotSet.GetResultTypeString(), parameter));
		}
	}
}
