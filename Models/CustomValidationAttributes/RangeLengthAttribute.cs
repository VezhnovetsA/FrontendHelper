using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models.CustomValidationAttributes
{
    public class RangeLengthAttribute : ValidationAttribute
    {
        private int _minLength;
        private int _maxLength;

        public RangeLengthAttribute(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public RangeLengthAttribute(int minLength)
        {
            _minLength = minLength;
            _maxLength = int.MaxValue;
        }

        public override string FormatErrorMessage(string name)
        {
            return !string.IsNullOrEmpty(ErrorMessage)
                ? ErrorMessage
                : $"Поле {name} должно содержать от {_minLength} до {_maxLength} символов.";
        }

        public override bool IsValid(object? value)
        {
            var str = value as string;

            if (str == null)
            {
                return false;
            }

            if (str.Length > _maxLength)
            {
                return false;
            }

            if (str.Length < _minLength)
            {
                return false;
            }

            //return base.IsValid(value);
            return true;
        }

    }


}
