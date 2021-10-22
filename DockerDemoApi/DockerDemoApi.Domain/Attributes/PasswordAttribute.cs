using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DockerDemoApi.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class PasswordAttribute : DataTypeAttribute
	{
		private const int MinPasswordRequirements = 3;
		private const string ValidSymbols = "~!@#$%^&*()-_+{}|[]\\:;\"'?,./";
		private readonly int _minLength;
		private readonly int _maxLength;

		public PasswordAttribute(int minLength, int maxLength) : base(DataType.Password)
		{
			_minLength = minLength;
			_maxLength = maxLength;
		}

		public override bool IsValid(object value)
		{
			if (value == null) return false;

			if (value is not string valueAsString)
				return false;
			else if (string.IsNullOrWhiteSpace(valueAsString))
				return false;

			if (valueAsString.Length < _minLength || valueAsString.Length > _maxLength) return false;

			bool hasUpper = false, hasLower = false, hasDigit = false, hasSymbol = false;
			foreach (char c in valueAsString)
			{
				if (hasUpper && hasLower && hasDigit && hasSymbol) break;

				if (char.IsDigit(c)) hasDigit = true;
				else if (char.IsUpper(c)) hasUpper = true;
				else if (char.IsLower(c)) hasLower = true;
				else if (ValidSymbols.Contains(c)) hasSymbol = true;
				else return false;  // Any other character not allowed
			}

			//return new bool[] { hasUpper, hasLower, hasDigit, hasSymbol }.Count(x => x) >= MinPasswordRequirements;
			return CountTrue(hasUpper, hasLower, hasDigit, hasSymbol) >= MinPasswordRequirements;
		}

		private static int CountTrue(params bool[] args)
		{
			return args.Count(x => x);
		}
	}
}