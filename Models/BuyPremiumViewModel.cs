using System.ComponentModel.DataAnnotations;
public class BuyPremiumViewModel
{
    [Required, CreditCard, Display(Name = "Номер карты")]
    public string CardNumber { get; set; } = "";

    [Required, Display(Name = "ММ/ГГ")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d\d$")]
    public string Expiry { get; set; } = "";

    [Required, Display(Name = "CVV")]
    [RegularExpression(@"^\d{3,4}$")]
    public string Cvv { get; set; } = "";
}