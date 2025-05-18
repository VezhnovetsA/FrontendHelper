using System.ComponentModel.DataAnnotations;

namespace FrontendHelper.Models.CustomValidationAttributes
{
    public class OnlyOneFontResourceAttribute : ValidationAttribute
    {
        private readonly string[] _props;

        public OnlyOneFontResourceAttribute(params string[] propertyNames)
        {
            _props = propertyNames;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var type = value.GetType();
            var filled = _props
                .Select(name => type.GetProperty(name)?.GetValue(value))
                .Count(val => val is string s
                               ? !string.IsNullOrWhiteSpace(s)
                               : val is not null);

            if (filled == 1)
                return ValidationResult.Success!;

            return new ValidationResult(
                $"Укажите ровно одно из полей: {string.Join(", ", _props)}.",
                _props);
        }
    }
}
