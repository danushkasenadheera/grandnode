using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Cms;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.GoogleAnalytics
{
    /// <summary>
    /// Live person provider
    /// </summary>
    public class GoogleAnalyticPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        public GoogleAnalyticPlugin(ISettingService settingService, IWebHelper webHelper, ILocalizationService localizationService, ILanguageService languageService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsGoogleAnalytics/Configure";
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                "body_end_html_tag_before", "clean_body_end_html_tag_before"
            };
        }        

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task Install()
        {
            var settings = new GoogleAnalyticsEcommerceSettings
            {
                GoogleId = "UA-0000000-0",
                TrackingScript = @"<!-- Google code for Analytics tracking -->
                    <script>
                    var _gaq = _gaq || [];
                    _gaq.push(['_setAccount', '{GOOGLEID}']);
                    _gaq.push(['_trackPageview']);
                    {ECOMMERCE}
                    (function() {
                        var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                        ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
                        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
                    })();
                    </script>",
                                    EcommerceScript = @"_gaq.push(['_addTrans', '{ORDERID}', '{SITE}', '{TOTAL}', '{TAX}', '{SHIP}', '{CITY}', '{STATEPROVINCE}', '{COUNTRY}']);
                    {DETAILS} 
                    _gaq.push(['_trackTrans']); ",
                EcommerceDetailScript = @"_gaq.push(['_addItem', '{ORDERID}', '{PRODUCTSKU}', '{PRODUCTNAME}', '{CATEGORYNAME}', '{UNITPRICE}', '{QUANTITY}' ]); ",

            };
            await _settingService.SaveSetting(settings);

            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.GoogleId", "ID");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.GoogleId.Hint", "Enter Google Analytics ID.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.TrackingScript", "Tracking code with {ECOMMERCE} line");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.TrackingScript.Hint", "Paste the tracking code generated by Google Analytics here. {GOOGLEID} and {ECOMMERCE} will be dynamically replaced.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceScript", "Tracking code for {ECOMMERCE} part, with {DETAILS} line");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceScript.Hint", "Paste the tracking code generated by Google analytics here. {ORDERID}, {SITE}, {TOTAL}, {TAX}, {SHIP}, {CITY}, {STATEPROVINCE}, {COUNTRY}, {DETAILS} will be dynamically replaced.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceDetailScript", "Tracking code for {DETAILS} part");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceDetailScript.Hint", "Paste the tracking code generated by Google analytics here. {ORDERID}, {PRODUCTSKU}, {PRODUCTNAME}, {CATEGORYNAME}, {UNITPRICE}, {QUANTITY} will be dynamically replaced.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.IncludingTax", "Include tax");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.IncludingTax.Hint", "Check to include tax when generating tracking code for {ECOMMERCE} part.");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<GoogleAnalyticsEcommerceSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.GoogleId");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.GoogleId.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.TrackingScript");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.TrackingScript.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceScript");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceScript.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceDetailScript");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.EcommerceDetailScript.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.IncludingTax");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.GoogleAnalytics.IncludingTax.Hint");

            await base.Uninstall();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "WidgetsGoogleAnalytics";
        }
    }
}
