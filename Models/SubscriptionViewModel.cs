namespace FrontendHelper.Models
{
    public class SubscriptionViewModel
    {
        public bool IsPremium { get; set; }

        // <-- вот тут «вживаем» покупку
        public BuyPremiumViewModel Purchase { get; set; }
            = new BuyPremiumViewModel();
    }

}
