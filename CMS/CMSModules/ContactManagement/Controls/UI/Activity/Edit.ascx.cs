using System;

using CMS.Helpers;
using CMS.OnlineMarketing;
using CMS.SiteProvider;
using CMS.UIControls;

public partial class CMSModules_ContactManagement_Controls_UI_Activity_Edit : CMSAdminEditControl
{
    #region "Variables"

    private ContactInfo mContact;

    #endregion


    #region "Properties"
    
    /// <summary>
    /// Indicates if the control should perform the operations.
    /// </summary>
    public override bool StopProcessing
    {
        get
        {
            return base.StopProcessing;
        }
        set
        {
            base.StopProcessing = value;
            EditForm.StopProcessing = value;
        }
    }


    /// <summary>
    /// Indicates if the control is used on the live site.
    /// </summary>
    public override bool IsLiveSite
    {
        get
        {
            return base.IsLiveSite;
        }
        set
        {
            base.IsLiveSite = value;
            EditForm.IsLiveSite = value;
        }
    }

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        int ContactID = QueryHelper.GetInteger("ContactID", 0);

        if (ContactID > 0)
        {
            // Change redirect URL when new custom activity is created under edited contact
            EditForm.RedirectUrlAfterCreate = "~/CMSModules/ContactManagement/Pages/Tools/Contact/Tab_Activities.aspx?saved=1&contactid=" + ContactID;
        }

        if (QueryHelper.GetBoolean("issitemanager", false))
        {
            EditForm.RedirectUrlAfterCreate += "&issitemanager=1";
        }

        if (!RequestHelper.IsPostBack())
        {
            EditForm.FieldControls["ActivityCreated"].Value = DateTime.Now;
            
            if (ContactID > 0)
            {
                EditForm.FieldControls["ActivityActiveContactID"].Value = ContactID;
            }
        }

        EditForm.FieldControls["ActivityActiveContactID"].SetValue("ObjectSiteName", SiteInfoProvider.GetSiteName(QueryHelper.GetInteger("SiteID", 0)));            
    }


    protected void EditForm_OnBeforeSave(object sender, EventArgs e)
    {
        var activity = (ActivityInfo)EditForm.EditedObject;

        activity.ActivityOriginalContactID = activity.ActivityActiveContactID;
        activity.ActivitySiteID = mContact.ContactSiteID;
        activity.ActivityItemID = 0;

        // Test if there is US_NONE_RECORD (then replace with NULL value)
        if (activity.ActivityCampaign == UniSelector.US_NONE_RECORD.ToString())
        {
            activity.ActivityCampaign = null;
        }
    }


    protected void EditForm_OnAfterValidate(object sender, EventArgs e)
    {
        // Test if selected date is not empty
        if (ValidationHelper.GetString(EditForm.GetFieldValue("ActivityCreated"), String.Empty) == String.Empty)
        {
            ShowError(GetString("om.sctivity.selectdatetime"));
            StopProcessing = true;
        }

        // Test if selected contact exists
        int contactID = ValidationHelper.GetInteger(EditForm.GetFieldValue("ActivityActiveContactID"), 0);
        mContact = ContactInfoProvider.GetContactInfo(contactID);
        if (mContact == null)
        {
            ShowError(GetString("om.activity.contactdoesnotexist"));
            StopProcessing = true;
        }
    }

    #endregion
    }