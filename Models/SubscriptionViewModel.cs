namespace FrontendHelper.Models
{
    public class SubscriptionViewModel
    {
        public bool IsPremium { get; set; }

        public BuyPremiumViewModel Purchase { get; set; }
            = new BuyPremiumViewModel();
    }

}
