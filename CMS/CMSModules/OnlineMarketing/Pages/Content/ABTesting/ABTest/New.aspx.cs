using System;

using CMS.Helpers;
using CMS.PortalEngine;
using CMS.UIControls;

[Title("abtest.new")]
[Security(Resource = "CMS.ABTest", UIElements = "New")]
[UIElement("CMS.ABTest", "New")]
public partial class CMSModules_OnlineMarketing_Pages_Content_ABTesting_ABTest_New : CMSABTestPage
{
    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Set disabled module info
        ucDisabledModule.SettingsKeys = "CMSAnalyticsEnabled;CMSABTestingEnabled";
        ucDisabledModule.InfoTexts.Add(GetString("WebAnalytics.Disabled") + "</br>");
        ucDisabledModule.InfoTexts.Add(GetString("abtesting.disabled"));
        ucDisabledModule.ParentPanel = pnlDisabled;

        InitBreadcrumbs();

        InitTitle();
    }


    /// <summary>
    /// Creates breadcrumbs.
    /// </summary>
    private void InitBreadcrumbs()
    {
        string url = UIContextHelper.GetElementUrl("CMS.ABTest", "ABTestListing", false);
        if (NodeID > 0)
        {
            url = URLHelper.AddParameterToUrl(url, "NodeID", NodeID.ToString());
        }

        PageBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
        {
            Text = GetString("abtesting.abtest.list"),
            RedirectUrl = ResolveUrl(url)
        });

        PageBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
        {
            Text = GetString("abtesting.abtest.new")
        });
    }


    /// <summary>
    /// Sets title if not in content.
    /// </summary>
    private void InitTitle()
    {
        if (NodeID <= 0)
        {
            SetTitle(GetString("analytics_codename.abtests"));
        }
    }

    #endregion
}