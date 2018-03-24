using System;
using System.Web.UI;

using CMS.ExtendedControls;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.UIControls;

public partial class CMSModules_AdminControls_Controls_Class_FormBuilder_Settings : CMSUserControl
{
    // Indicates if selected field is primary key
    private bool FieldIsPrimary
    {
        get;
        set;
    }


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        var btnGeneral = new CMSButtonGroupAction() { Text = GetString("general.properties"), OnClientClick = "manageSelectedTab(1); return false;" };
        var btnValidation = new CMSButtonGroupAction() { Text = GetString("formbuilder.validation"), OnClientClick = "manageSelectedTab(2); return false;", Enabled = !FieldIsPrimary };

        btnGroup.Actions.Add(btnGeneral);
        btnGroup.Actions.Add(btnValidation);

        string manageSelectedTab =
@"function pageLoad(sender, args) { 
     var selectedTab = jQuery('#" + hdnSelectedTab.ClientID + @"').val();
     manageSelectedTab(selectedTab);
}

function manageSelectedTab(tabIndex)
{
    if (tabIndex == 2)
    {
        jQuery('#form-builder-properties').hide();
        jQuery('#form-builder-validation').show();
        jQuery('#" + btnGroup.ClientID + @"').children().eq(0).removeClass('active');
        jQuery('#" + btnGroup.ClientID + @"').children().eq(1).addClass('active');
    }
    else
    {
        jQuery('#form-builder-properties').show();
        jQuery('#form-builder-validation').hide();
        jQuery('#" + btnGroup.ClientID + @"').children().eq(0).addClass('active'); 
        jQuery('#" + btnGroup.ClientID + @"').children().eq(1).removeClass('active');
    }
    
    jQuery('#" + hdnSelectedTab.ClientID + @"').val(tabIndex);
}";
        ScriptHelper.RegisterStartupScript(this, typeof(string), "FormBuilder_" + ClientID, ScriptHelper.GetScript(manageSelectedTab));
    }


    /// <summary>
    /// Loads settings from form field info.
    /// </summary>
    /// <param name="ffi">Form field info</param>
    public void LoadSettings(FormFieldInfo ffi)
    {
        FieldIsPrimary = ffi.PrimaryKey;

        pnlProperties.LoadProperties(ffi);
        if (!FieldIsPrimary)
        {
            pnlValidation.LoadRules(ffi);
        }
        else
        {
            // Reset selected tab value
            hdnSelectedTab.Value = null;
        }
        
        pnlValidation.Visible = !FieldIsPrimary;
    }


    /// <summary>
    /// Saves field settings to given form field info.
    /// </summary>
    /// <param name="ffi">Form field info</param>
    public void SaveSettings(FormFieldInfo ffi)
    {
        pnlProperties.SaveProperties(ffi);
        if (!FieldIsPrimary)
        {
            pnlValidation.SaveValidation(ffi);
        }
    }


    /// <summary>
    /// Updates settings update panel.
    /// </summary>
    public void Update()
    {
        pnlUpdateSettings.Update();
    }


    /// <summary>
    /// Sets visibility of settings panel.
    /// </summary>
    /// <param name="visible">If true settings panel is visible, else info message is shown</param>
    public void SetSettingsVisibility(bool visible)
    {
        pnlEdit.Visible = visible;
        pnlNoSelectedField.Visible = !visible;

        if (pnlNoSelectedField.Visible)
        {
            // Display info message that no field is selected
            mphNoSelected.ShowInformation(GetString("FormBuilder.NoFieldSelected"));
        }
    }

    #endregion
}