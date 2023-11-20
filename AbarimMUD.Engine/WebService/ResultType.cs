using System.Collections.Generic;

namespace AbarimMUD.WebService
{
	public enum ResultType
	{
		OK,
		AccountNotFound,
		WrongPassword,
		NotAdmin,
		NoMaps,
		UnrecognizedError,
		AreaNotFound,
		RequiredParameterIsNotSet
	}

	public static class ResultTypeExtensions
	{
		private static readonly Dictionary<ResultType, string> _resultStrings = new Dictionary<ResultType, string>();

		static ResultTypeExtensions()
		{
			_resultStrings[ResultType.OK] = "Success.";
			_resultStrings[ResultType.AccountNotFound] = "Account '{0}' is not found.";
			_resultStrings[ResultType.WrongPassword] = "Wrong password.";
			_resultStrings[ResultType.NotAdmin] = "The account doesnt have an admin character.";
			_resultStrings[ResultType.NoMaps] = "The account doesnt have accessible maps.";
			_resultStrings[ResultType.UnrecognizedError] = "Unrecognized error.";
			_resultStrings[ResultType.AreaNotFound] = "Could not find an area with id '{0}'.";
			_resultStrings[ResultType.RequiredParameterIsNotSet] = "Required parameter '{0}' is not set.";
		}

		public static string GetResultTypeString(this ResultType type)
		{
			return _resultStrings[type];
		}
	}
}
