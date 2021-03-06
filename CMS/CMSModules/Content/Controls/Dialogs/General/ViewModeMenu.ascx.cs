using System;
using System.Collections;
using System.Web;
using System.Web.UI;

using CMS.ExtendedControls;
using CMS.Helpers;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.UIControls;

public partial class CMSModules_Content_Controls_Dialogs_General_ViewModeMenu : CMSUserControl
{
    /// <summary>
    /// Returns currently selected tab view mode.
    /// </summary>
    public DialogViewModeEnum SelectedViewMode
    {
        get
        {
            string viewMode = hdnLastSelectedTab.Value.Trim().ToLowerCSafe();

            // Get view mode
            return CMSDialogHelper.GetDialogViewMode(viewMode);
        }
        set
        {
            hdnLastSelectedTab.Value = CMSDialogHelper.GetDialogViewMode(value);
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!StopProcessing)
        {
            btnList.ToolTip = GetString("dialogs.viewmode.list");
            btnThumbs.ToolTip = GetString("dialogs.viewmode.thumbnails");

            if (SelectedViewMode == DialogViewModeEnum.ListView)
            {
                btnList.AddCssClass("active");
                btnThumbs.RemoveCssClass("active");
            }
            else
            {
                btnThumbs.AddCssClass("active");
                btnList.RemoveCssClass("active");
            }

            // Set last view function
            string setLastView =
@"
function SetLastViewAction(viewMode) {
    var lastView = document.getElementById('" + hdnLastSelectedTab.ClientID + @"');
    if ((lastView!=null) && (viewMode!=null)) {
        var btnList = $j('#' + '" + btnList.ClientID + @"');
        var btnThumbs = $j('#' + '" + btnThumbs.ClientID + @"');  
        if (viewMode == 'list') {
            btnList.addClass('active');
            btnThumbs.removeClass('active');
        }     
        else{
            btnThumbs.addClass('active');
            btnList.removeClass('active');
        }   


        lastView.value = viewMode;
    }
}
";

            ltlScript.Text += ScriptHelper.GetScript(setLastView);
        }
    }

}