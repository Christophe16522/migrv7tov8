using System;
using System.Collections;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.FormControls;
using CMS.FormEngine;
using CMS.SiteProvider;
using CMS.UIControls;

public partial class CMSModules_AdminControls_Controls_Class_FieldEditor_ControlSettings : CMSUserControl
{
    #region "Variables"

    private FormInfo fi = null;
    private Hashtable mSettings = null;

    /// <summary>
    /// Collection of the macros.
    /// </summary>
    private Hashtable mMacros = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// FormInfo for specific control.
    /// </summary>
    public FormInfo FormInfo
    {
        get
        {
            return fi;
        }
        set
        {
            form.FormInformation = fi = value;
        }
    }


    /// <summary>
    /// Shows in what control is this form used.
    /// </summary>
    public FormTypeEnum FormType
    {
        get
        {
            return form.FormType;
        }
        set
        {
            form.FormType = value;
        }
    }


    /// <summary>
    /// Field settings hashtable.
    /// </summary>
    public Hashtable Settings
    {
        get
        {
            return mSettings;
        }
        set
        {
            mSettings = new Hashtable(value, StringComparer.InvariantCultureIgnoreCase);
        }
    }


    /// <summary>
    /// Basic form data.
    /// </summary>
    public DataRow FormData
    {
        get
        {
            return form.DataRow;
        }
    }


    /// <summary>
    /// Sets basicform to simple or advanced mode.
    /// </summary>
    public bool SimpleMode
    {
        get;
        set;
    }


    /// <summary>
    /// Determines whether to allow mode switching (simple <-> advanced).
    /// </summary>
    public bool AllowModeSwitch
    {
        get
        {
            return form.AllowModeSwitch;
        }
        set
        {
            form.AllowModeSwitch = value;
        }
    }


    /// <summary>
    /// Macro table
    /// </summary>
    public virtual Hashtable MacroTable
    {
        get
        {
            // Ensure table
            if (mMacros == null)
            {
                mMacros = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            }

            return mMacros;
        }
        set
        {
            mMacros = value;
        }
    }


    /// <summary>
    /// Defines minimum of items needed to be visible to display mode switch
    /// </summary>
    public int MinItemsToAllowSwitch
    {
        get;
        set;
    }


    /// <summary>
    /// Gets or sets macroresolver used in appearance field.
    /// </summary>
    public string ResolverName
    {
        get
        {
            return form.ResolverName;
        }
        set
        {
            form.ResolverName = value;
        }
    }


    /// <summary>
    /// Gets the underlaying BasicForm control.
    /// </summary>
    public BasicForm BasicForm
    {
        get
        {
            return form;
        }
    }

    #endregion


    #region "Page events"

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        form.SubmitButton.Visible = false;
        form.SubmitButton.RegisterHeaderAction = false;
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        // Simplify the form
        ProcessSimpleModeVisibility();
        
        if (String.IsNullOrEmpty(lnkAdvanced.Text))
        {
            // Add text during the first load
            lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("fieldeditor.tabs.advanced");
        }
    }


    /// <summary>
    /// Switches simple/advanced mode.
    /// </summary>
    protected void link_Click(object sender, EventArgs e)
    {
        // Switch the mode
        SimpleMode = !SimpleMode;
        
        if (SimpleMode)
        {
            lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("fieldeditor.tabs.advanced");
            icAdvanced.CssClass = "icon-caret-down cms-icon-30";
        }
        else
        {
            lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("fieldeditor.tabs.simplified");
            icAdvanced.CssClass = "icon-caret-up cms-icon-30";
        }
    }

    #endregion


    #region "Public methods"

    /// <summary>
    /// Checks if form is loaded with any controls and returns appropriate value.
    /// </summary>
    public bool CheckVisibility()
    {
        Visible = (form.FormInformation != null) && form.FormInformation.ItemsList.Any() && form.IsAnyFieldVisible();

        return Visible;
    }


    /// <summary>
    /// Reloads control.
    /// </summary>
    /// <param name="forceReloadCategories">Forces categories to get to default collapsion state</param>
    public void Reload(bool forceReloadCategories)
    {
        if (fi != null)
        {
            form.AllowMacroEditing = true;
            form.SiteName = SiteContext.CurrentSiteName;
            form.FormInformation = FormInfo;
            form.MacroTable = MacroTable;
            form.Data = GetData();
            form.ShowPrivateFields = true;
            form.ForceReloadCategories = forceReloadCategories;
            form.StopProcessing = false;
        }
        else
        {
            form.DataRow = null;
            form.FormInformation = null;
            form.StopProcessing = true;
        }

        form.ReloadData();
    }


    /// <summary>
    /// Saves basic form data.
    /// </summary>
    public bool SaveData()
    {
        if (form.Visible && form.FieldControls != null && form.FieldControls.Count != 0)
        {
            return form.SaveData(null, false);
        }

        return true;
    }

    #endregion


    #region "Private methods"

    /// <summary>
    /// Loads DataRow for BasicForm with data from FormFieldInfo settings.
    /// </summary>
    private DataRowContainer GetData()
    {
        DataRowContainer result = new DataRowContainer(FormInfo.GetDataRow());
        FormInfo.LoadDefaultValues(result);

        if (Settings != null)
        {
            foreach (string columnName in result.ColumnNames)
            {
                if (Settings.ContainsKey(columnName) && !String.IsNullOrEmpty(Convert.ToString(Settings[columnName])))
                {
                    result[columnName] = Settings[columnName];
                }
            }
        }
        return result;
    }


    /// <summary>
    /// Sets simple/advanced mode.
    /// </summary>
    private void ProcessSimpleModeVisibility()
    {
        // Check switch visibility - no need to show it if there are no advanced items
        bool mVisible = AllowModeSwitch && ContainsAdvancedItems(FormInfo);

        // If there is not enough items, hide switch and set advanced mode
        bool tooFewItems = HasTooFewItems(FormInfo);
        mVisible &= !tooFewItems;

        // Visibility needs to be set via a variable, since if control is in panel that is not visible at the time when this method is called
        // 'Visible' property isn't rewritten (stays false). In that case control tries to set Visible attribute again at PreRender event.
        pnlSwitch.Visible = mVisible;

        // And process fields visibility depending on mode
        if ((form.IsSimpleMode != (!tooFewItems && SimpleMode)) && (form.FormInformation != null))
        {
            form.IsSimpleMode = !tooFewItems && SimpleMode;
            form.FieldsToHide.AddRange(form.FormInformation.ItemsList.OfType<FormFieldInfo>().Where(x => !x.DisplayInSimpleMode).Select(x => x.Name).ToList());
        }

        lnkAdvanced.Visible = AllowModeSwitch;
    }


    /// <summary>
    /// Checks if FormInfo contains items that are shown only in advanced mode.
    /// </summary>
    private bool ContainsAdvancedItems(FormInfo formInfo)
    {
        if (formInfo == null)
        {
            return false;
        }
        return formInfo.ItemsList.OfType<FormFieldInfo>().Any(y => y.Visible && !y.DisplayInSimpleMode);
    }


    /// <summary>
    /// Indicates whether given FormInfo has too few items to be bothered by mode switching.
    /// </summary>
    private bool HasTooFewItems(FormInfo formInfo)
    {
        if (formInfo == null)
        {
            return false;
        }
        return formInfo.ItemsList.OfType<FormFieldInfo>().Where(y => y.Visible).Count() <= MinItemsToAllowSwitch;
    }

    #endregion


    #region "View State Methods"

    protected override void LoadViewState(object savedState)
    {
        object[] updatedState = (object[])savedState;

        // Load orig ViewState
        if (updatedState[0] != null)
        {
            base.LoadViewState(updatedState[0]);
        }

        // Load Mode settings
        if (updatedState[1] != null)
        {
            SimpleMode = (bool)updatedState[1];
        }
    }


    protected override object SaveViewState()
    {
        object[] updatedState = new object[2];
        updatedState[0] = base.SaveViewState();
        updatedState[1] = SimpleMode;
        return updatedState;
    }

    #endregion
}