﻿using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.Ecommerce;
using CMS.CMSHelper;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.GlobalHelper;
using CMS.PortalControls;
using CMS.PortalEngine;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Protection;
using CMS.Membership;
using CMS.MacroEngine;
using CMS.Localization;
using CMS.DocumentEngine;
 

public partial class CMSWebParts_Membership_Registration_registrationformservranx : CMSAbstractWebPart
{
    private readonly EventLogProvider ev = new EventLogProvider();
    #region "Text properties"

    /// <summary>
    /// Gets or sets the Skin ID.
    /// </summary>
    public override string SkinID
    {
        get
        {
            return base.SkinID;
        }
        set
        {
            base.SkinID = value;
            SetSkinID(value);
        }
    }


    /// <summary>
    /// Gets or sets the first name text.
    /// </summary>
    public string FirstNameText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("FirstNameText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.FirstName$}"));
        }
        set
        {
            SetValue("FirstNameText", value);
            lblFirstName.Text = value;
        }
    }


    /// <summary>
    /// Gets or sets the last name text.
    /// </summary>
    public string LastNameText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("LastNameText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.LastName$}"));
        }
        set
        {
            SetValue("LastNameText", value);
            lblLastName.Text = value;
        }
    }


    /// <summary>
    /// Gets or sets the e-mail text.
    /// </summary>
    public string EmailText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("EmailText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Email$}"));
        }
        set
        {
            SetValue("EmailText", value);
            lblEmail.Text = value;
        }
    }


    /// <summary>
    /// Gets or sets the password text.
    /// </summary>
    public string PasswordText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("PasswordText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Password$}"));
        }
        set
        {
            SetValue("PasswordText", value);
            lblPassword.Text = value;
        }
    }


    /// <summary>
    /// Gets or sets the confirmation password text.
    /// </summary>
    public string ConfirmPasswordText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ConfirmPasswordText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.ConfirmPassword$}"));
        }
        set
        {
            SetValue("ConfirmPasswordText", value);
            lblConfirmPassword.Text = value;
        }
    }

    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string ButtonText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ButtonText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Button$}"));
        }
        set
        {
            SetValue("ButtonText", value);
            btnOk.Text = value;
        }
    }

    /// <summary>
    /// Gets or sets the captcha label text.
    /// </summary>
    public string CaptchaText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("CaptchaText"), ResHelper.LocalizeString("{$Webparts_Membership_RegistrationForm.Captcha$}"));
        }
        set
        {
            SetValue("CaptchaText", value);
            lblCaptcha.Text = value;
        }
    }


    /// <summary>
    /// Gets or sets registration approval page URL.
    /// </summary>
    public string ApprovalPage
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ApprovalPage"), "");
        }
        set
        {
            SetValue("ApprovalPage", value);
        }
    }

    #endregion


    #region "Registration properties"

    /// <summary>
    /// Gets or sets the value that indicates whether email to user should be sent.
    /// </summary>
    public bool SendWelcomeEmail
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("SendWelcomeEmail"), true);
        }
        set
        {
            SetValue("SendWelcomeEmail", value);
        }
    }


    /// <summary>
    /// Gets or sets the value that indicates whether user is enabled after registration.
    /// </summary>
    public bool EnableUserAfterRegistration
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("EnableUserAfterRegistration"), true);
        }
        set
        {
            SetValue("EnableUserAfterRegistration", value);
        }
    }


    /// <summary>
    /// Gets or sets the sender email (from).
    /// </summary>
    public string FromAddress
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("FromAddress"), SettingsKeyInfoProvider.GetStringValue(SiteContext.CurrentSiteName + ".CMSNoreplyEmailAddress"));
        }
        set
        {
            SetValue("FromAddress", value);
        }
    }


    /// <summary>
    /// Gets or sets the recipient email (to).
    /// </summary>
    public string ToAddress
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ToAddress"), SettingsKeyInfoProvider.GetStringValue(SiteContext.CurrentSiteName + ".CMSAdminEmailAddress"));
        }
        set
        {
            SetValue("ToAddress", value);
        }
    }


    /// <summary>
    /// Gets or sets the value that indicates whether after successful registration is 
    /// notification email sent to the administrator 
    /// </summary>
    public bool NotifyAdministrator
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("NotifyAdministrator"), false);
        }
        set
        {
            SetValue("NotifyAdministrator", value);
        }
    }


    /// <summary>
    /// Gets or sets the roles where is user assigned after successful registration.
    /// </summary>
    public string AssignRoles
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AssignToRoles"), "");
        }
        set
        {
            SetValue("AssignToRoles", value);
        }
    }


    /// <summary>
    /// Gets or sets the sites where is user assigned after successful registration.
    /// </summary>
    public string AssignToSites
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AssignToSites"), "");
        }
        set
        {
            SetValue("AssignToSites", value);
        }
    }


    /// <summary>
    /// Gets or sets the message which is displayed after successful registration.
    /// </summary>
    public string DisplayMessage
    {
        get
        {
            return ValidationHelper.GetString(GetValue("DisplayMessage"), "");
        }
        set
        {
            SetValue("DisplayMessage", value);
        }
    }


    /// <summary>
    /// Gets or set the url where is user redirected after successful registration.
    /// </summary>
    public string RedirectToURL
    {
        get
        {
            return ValidationHelper.GetString(GetValue("RedirectToURL"), "");
        }
        set
        {
            SetValue("RedirectToURL", value);
        }
    }


    /// <summary>
    /// Gets or sets value that indicates whether the captcha image should be displayed.
    /// </summary>
    public bool DisplayCaptcha
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("DisplayCaptcha"), false);
        }
        set
        {
            SetValue("DisplayCaptcha", value);
            plcCaptcha.Visible = value;
        }
    }


    /// <summary>
    /// Gets or sets the default starting alias path for newly registered user.
    /// </summary>
    public string StartingAliasPath
    {
        get
        {
            return ValidationHelper.GetString(GetValue("StartingAliasPath"), "");
        }
        set
        {
            SetValue("StartingAliasPath", value);
        }
    }


    /// <summary>
    /// Gets or sets the password minimal length.
    /// </summary>
    public int PasswordMinLength
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("PasswordMinLength"), 0);
        }
        set
        {
            SetValue("PasswordMinLength", 0);
        }
    }

    #endregion


    #region "Conversion properties"

    /// <summary>
    /// Gets or sets the conversion track name used after successful registration.
    /// </summary>
    public string TrackConversionName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("TrackConversionName"), "");
        }
        set
        {
            if (value.Length > 400)
            {
                value = value.Substring(0, 400);
            }
            SetValue("TrackConversionName", value);
        }
    }


    /// <summary>
    /// Gets or sets the conversion value used after successful registration.
    /// </summary>
    public double ConversionValue
    {
        get
        {
            return ValidationHelper.GetDoubleSystem(GetValue("ConversionValue"), 0);
        }
        set
        {
            SetValue("ConversionValue", value);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        try
        {
            base.OnContentLoaded();
            SetupControl();
        }
        catch (Exception ex)
        {
            ev.LogEvent("E", DateTime.Now, "CMSWebParts_Membership_Registration_registrationformservranx.OnContentLoaded",
            ex.Message);
        }

    }


    /// <summary>
    /// Reloads data.
    /// </summary>
    public override void ReloadData()
    {
        try
        {
            base.ReloadData();
            SetupControl();
        }
        catch(Exception ex)
        {
            ev.LogEvent("E", DateTime.Now, "CMSWebParts_Membership_Registration_registrationformservranx.ReloadData",
            ex.Message);
        }

    }
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            btnOk.CssClass = GetString("btnokcss");
            if(!IsPostBack)
            {
                ShowBillingCountryList();
                ShowCivility();
                ShowShippingCountryList();
            }
        }
        catch(Exception ex)
        {
            ev.LogEvent("E", DateTime.Now, "CMSWebParts_Membership_Registration_registrationformservranx.Page_Load",
            ex.Message);
        }

    }

    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        try
        {
            if(StopProcessing)
            {
                // Do not process
                //rfvFirstName.Enabled = false;
                //rfvEmail.Enabled = false;
                //rfvConfirmPassword.Enabled = false;
                //rfvLastName.Enabled = false;
            }
            else
            {
                txtFirstName.ToolTip = GetString("prenom");
                wmFirstName.WatermarkText = GetString("prenom");
                txtLastName.ToolTip = GetString("nom");
                wmLastName.WatermarkText = GetString("nom");
                txtnumero.ToolTip = GetString("numerorue");
                wmnumero.WatermarkText = GetString("numerorue");
                txtadresse1.ToolTip = GetString("adresse1");
                wmadresse1.WatermarkText = GetString("adresse1");
                txtadresse2.ToolTip = GetString("adresse2");
                wmadresse2.WatermarkText = GetString("adresse2");
                txtcp.ToolTip = GetString("cp");
                wmcp.WatermarkText = GetString("cp");
                txtville.ToolTip = GetString("ville");
                wmville.WatermarkText = GetString("ville");
                txtConfirmPassword.ToolTip = GetString("confirmdp");
                txtEmail.ToolTip = GetString("adressemail");
                wmmail.WatermarkText = GetString("adressemail");
                lblSociete.Text = GetString("societe");
                rbnon.Text = string.Format("{0}   &nbsp;", GetString("non"));
                rboui.Text = string.Format("{0}   &nbsp;", GetString("oui"));
                txtnomsociete.ToolTip = GetString("nomsociete");
                wmnomsociete.WatermarkText = GetString("nomsociete");
                txtTva.ToolTip = GetString("tva");
                wmtva.WatermarkText = GetString("tva");
                txtvilleshipping.ToolTip = GetString("ville");
                txtcpshipping.ToolTip = GetString("cp");
                LabelAdresse.Text = GetString("differenceadresse");
                txtnumeroshipping.ToolTip = GetString("numerorue");
                wmnumeroshipping.WatermarkText = GetString("numerorue");
                txtadresse1shipping.ToolTip = GetString("adresse1");
                wmadresse1shipping.WatermarkText = GetString("adresse1");
                txtadresse2shipping.ToolTip = GetString("adresse2");
                wmadresse2shipping.WatermarkText = GetString("adresse2");
                LabelAccept.Text = GetString("yesaccept");
                LabelCondition.Text = GetString("conditiongenerale");
                LabelEmailInfo.Text = GetString("yesreceive");
                LabelLastArticle.Text = GetString("lastarticle");


                // Set default visibility
                pnlForm.Visible = true;
                lblText.Visible = false;

                // Set texts
                lblFirstName.Text = FirstNameText;
                lblLastName.Text = LastNameText;
                lblEmail.Text = EmailText;
                lblPassword.Text = PasswordText;
                lblConfirmPassword.Text = ConfirmPasswordText;
                btnOk.Text = ButtonText;
                lblCaptcha.Text = CaptchaText;

                // Set required field validators texts
                //rfvFirstName.ErrorMessage = "*";//GetString("Webparts_Membership_RegistrationForm.rfvFirstName");
                //rfvLastName.ErrorMessage = "*";// GetString("Webparts_Membership_RegistrationForm.rfvLastName");
                //rfvEmail.ErrorMessage = "*";//GetString("Webparts_Membership_RegistrationForm.rfvEmail");
                //rfvConfirmPassword.ErrorMessage = "*";//GetString("Webparts_Membership_RegistrationForm.rfvConfirmPassword");

                // Add unique validation form
                // rfvFirstName.ValidationGroup = ClientID + "_registration";
                //rfvLastName.ValidationGroup = ClientID + "_registration";
                //rfvEmail.ValidationGroup = ClientID + "_registration";
                passStrength.ValidationGroup = ClientID + "_registration";
                //rfvConfirmPassword.ValidationGroup = ClientID + "_registration";
                btnOk.ValidationGroup = ClientID + "_registration";


                // Set SkinID
                if(!StandAlone && (PageCycle < PageCycleEnum.Initialized))
                {
                    SetSkinID(SkinID);
                }

                plcCaptcha.Visible = DisplayCaptcha;

                // WAI validation
                lblPassword.AssociatedControlClientID = passStrength.InputClientID;
            }
        }
        catch (Exception ex)
        {
            ev.LogEvent("E", DateTime.Now, "CMSWebParts_Membership_Registration_registrationformservranx.SetupControl",
                        ex.Message);
        }
        
    }


    /// <summary>
    /// Sets SkinID.
    /// </summary>
    private void SetSkinID(string skinId)
    {
        if (skinId != "")
        {
            lblFirstName.SkinID = skinId;
            lblLastName.SkinID = skinId;
            lblEmail.SkinID = skinId;
            lblPassword.SkinID = skinId;
            lblConfirmPassword.SkinID = skinId;
            txtFirstName.SkinID = skinId;
            txtLastName.SkinID = skinId;
            txtEmail.SkinID = skinId;
            passStrength.SkinID = skinId;
            txtConfirmPassword.SkinID = skinId;
            btnOk.SkinID = skinId;
        }
    }


    /// <summary>
    /// OK click handler (Proceed registration).
    /// </summary>
    protected void btnOK_Click(object sender, EventArgs e)
    {
        System.Globalization.CultureInfo currentUI = System.Globalization.CultureInfo.CurrentUICulture;

        if ((PageManager.ViewMode == ViewModeEnum.Design) || (HideOnCurrentPage) || (!IsVisible))
        {
            // Do not process
        }
        else
        {
            if (chkAccept.Checked)
            {
                int CountryID = ValidationHelper.GetInteger(ddlBillingCountry.SelectedValue, 0);

                int ShippingCountryID = ValidationHelper.GetInteger(ddlShippingCountry.SelectedValue, 0);
                
                String siteName = SiteContext.CurrentSiteName;

                #region "Banned IPs"

                // Ban IP addresses which are blocked for registration
                if (!BannedIPInfoProvider.IsAllowed(siteName, BanControlEnum.Registration))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("banip.ipisbannedregistration");
                    return;
                }

                #endregion

                #region "Email"

                if ((txtEmail.Text == "") || (txtEmail.Text == "E-mail"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("erroremail");
                    return;
                }
               
                // Check whether user with same email does not exist 
                UserInfo ui = UserInfoProvider.GetUserInfo(txtEmail.Text);
                SiteInfo si = SiteContext.CurrentSite;
                UserInfo siteui = UserInfoProvider.GetUserInfo(UserInfoProvider.EnsureSitePrefixUserName(txtEmail.Text, si));

                if ((ui != null) || (siteui != null))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errormail");
                    return;
                }

                if (!ValidationHelper.IsEmail(txtEmail.Text.ToLowerCSafe()))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errorinvalidmail");
                    return;
                }

                 #endregion

                #region "Pr�nom"

                if ((txtFirstName.Text == "") || (txtFirstName.Text == "FirstName") || (txtFirstName.Text == "Pr�nom"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errorprenom");
                    return;
                }

                #endregion       

                #region "Nom"

                if ((txtLastName.Text == "") || (txtLastName.Text == "LastName") || (txtLastName.Text == "Nom"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errornom");              
                    return;
                }

                #endregion
                       
                #region "n�"

                if ((txtnumero.Text == "") || (txtnumero.Text == "Numero") || (txtnumero.Text == "Number"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errornumerorue");
                    return;
                }

                #endregion

                #region "adresse 1"

                if ((txtadresse1.Text == "") || (txtadresse1.Text == "Adresse 1") || (txtadresse1.Text == "Address 1"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("erroradresse1");
                    return;
                }

                #endregion

                #region "adresse 2"

                if ((txtadresse2.Text == "Adresse 2") || (txtadresse2.Text == "Address 2"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("erroradresse2");
                    return;
                }

                #endregion

                #region "CP"

                if ((txtcp.Text == "") || (txtcp.Text == "CP") || (txtcp.Text == "ZIP"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errorcp");                   
                    return;
                }

                #endregion

                #region "Ville"

                if ((txtville.Text == "") || (txtville.Text == "Ville") || (txtville.Text == "City"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errorville");
                    return;
                }

                #endregion

                #region "Pays"

                if ((ddlBillingCountry.Text == "Choose your country") || (ddlBillingCountry.Text == "Choisissez votre pays"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errorchoixpays ");
                    return;
                }

                #endregion

                #region "T�l�phone"

                if ((txtTelephone.Text == "") || (txtTelephone.Text == "TELEPHONE") || (txtadresse1.Text == "TELEPHONE"))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("errortelephone");
                    return;
                }

                #endregion

                #region "Mot de passe"
                // Check whether password is same
                //if (passStrength.Text != txtConfirmPassword.Text)
                //{
                    //lblError.Visible = true;
                    //lblError.Text = GetString("Webparts_Membership_RegistrationForm.PassworDoNotMatch");

                   if ((passStrength.Text == "") || (passStrength.Text == "Mot de passe") || (passStrength.Text == "Password"))
                    {                        
                        lblError.Visible = true;
                        lblError.Text = GetString("errormdprequired");
                        return;                       
                    }
                
                   if (passStrength.Text != txtConfirmPassword.Text)
                   {                       
                        lblError.Visible = true;
                        lblError.Text = GetString("errormdp");
                        return;                        
                   }
                   
                               
               // }

                if ((PasswordMinLength > 0) && (passStrength.Text.Length < PasswordMinLength))
                {
                    lblError.Visible = true;
                    lblError.Text = String.Format(GetString("Webparts_Membership_RegistrationForm.PasswordMinLength"), PasswordMinLength.ToString());
                    return;
                }

                if (!passStrength.IsValid())
                {
                    lblError.Visible = true;
                    lblError.Text = AuthenticationHelper.GetPolicyViolationMessage(SiteContext.CurrentSiteName);
                    return;
                }
             
                #endregion

                #region Soci�t�
                if (rboui.Checked)
                {
                    if ((txtnomsociete.Text == "") || (txtnomsociete.Text == "Nom soci�t�") || (txtnomsociete.Text == "Company Name"))
                    {
                        lblError.Visible = true;
                        lblError.Text = GetString("errornomsociete ");
                        return;
                    }

                    //if ((txtTva.Text == "") || (txtTva.Text == "TVA") || (txtTva.Text == "VAT"))
                    //{
                    //    lblError.Visible = true;
                    //    lblError.Text = GetString("errortva ");
                    //    return;
                    //}
					
                    //if (!EUVatChecker.Check(txtTva.Text))
                    //{
                    //    lblError.Visible = true;
                    //    lblError.Text = GetString("errortva2 ");
                    //    return;
                    //}

                }
                #endregion
                
                #region "Adresse Shipping"
                if (chkShippingAddr.Checked)
                {
                    #region "n� shipping"

                    if ((txtnumeroshipping.Text == "") || (txtnumeroshipping.Text == "Numero") || (txtnumeroshipping.Text == "Number"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("errornumerorue");
                        return;
                    }
                    #endregion

                    #region "adresse 1 shipping"

                    if ((txtadresse1shipping.Text == "") || (txtadresse1shipping.Text == "Adresse 1") || (txtadresse1shipping.Text == "Address 1"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("erroradresse1");
                        return;
                    }

                    #endregion

                    #region "adresse 2 shipping"

                    if ((txtadresse2shipping.Text == "Adresse 2") || (txtadresse2shipping.Text == "Address 2"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("erroradresse2");
                        return;
                    }

                    #endregion

                    #region "CP Shipping"

                    if ((txtcpshipping.Text == "") || (txtcpshipping.Text == "CP") || (txtcpshipping.Text == "ZIP"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("errorcp");
                        return;
                    }

                    #endregion

                    #region "Ville Shipping"

                    if ((txtvilleshipping.Text == "") || (txtvilleshipping.Text == "Ville") || (txtvilleshipping.Text == "City"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("errorville");                       
                        return;
                    }

                    #endregion

                    #region "Pays"

                    if ((ddlShippingCountry.Text == "Choose your country") || (ddlShippingCountry.Text == "Choisissez votre pays"))
                    {
                        lblErrorShipping.Visible = true;
                        lblErrorShipping.Text = GetString("errorchoixpays ");
                        return;
                    }

                    #endregion
                }
                #endregion

                #region "Captcha"

                // Check if captcha is required
                if (DisplayCaptcha)
                {
                    // Verifiy captcha text
                    if (!scCaptcha.IsValid())
                    {
                        // Display error message if catcha text is not valid
                        lblError.Visible = true;
                        lblError.Text = GetString("Webparts_Membership_RegistrationForm.captchaError");
                        return;
                    }
                    else
                    {
                        // Generate new captcha
                        scCaptcha.GenerateNew();
                    }
                }

                #endregion


                #region "User properties"

                ui = new UserInfo();
                ui.PreferredCultureCode = "";
                ui.Email = txtEmail.Text.Trim();
                ui.FirstName = txtFirstName.Text.Trim();
                ui.FullName = UserInfoProvider.GetFullName(txtFirstName.Text.Trim(), String.Empty, txtLastName.Text.Trim());
                ui.LastName = txtLastName.Text.Trim();
                ui.SetValue("UserPhone", txtTelephone.Text);
                //eto
               // ui.SetValue("CustomerPhone", txtTelephone.Text.Trim());
                ui.MiddleName = "";
                if (ddlFrom.Text != "")
                {
                    ui.SetValue("Civilite", ddlFrom.Text);
                }
                ui.SetValue("Telephone", txtTelephone.Text);
                // User name as put by user (no site prefix included)
                String plainUserName = txtEmail.Text.Trim();
                ui.UserName = plainUserName;

                // Ensure site prefixes
                if (UserInfoProvider.UserNameSitePrefixEnabled(siteName))
                {
                    ui.UserName = UserInfoProvider.EnsureSitePrefixUserName(txtEmail.Text.Trim(), si);
                }
                
                ui.Enabled = EnableUserAfterRegistration;
                ui.IsEditor = false;
                ui.IsGlobalAdministrator = false;
                ui.UserURLReferrer = MembershipContext.AuthenticatedUser.URLReferrer;
                ui.UserCampaign = AnalyticsHelper.Campaign;

                ui.UserSettings.UserRegistrationInfo.IPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                ui.UserSettings.UserRegistrationInfo.Agent = HttpContext.Current.Request.UserAgent;

                // Check whether confirmation is required
                bool requiresConfirmation = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation");
                bool requiresAdminApprove = false;

                if (!requiresConfirmation)
                {
                    // If confirmation is not required check whether administration approval is reqiures
                    if ((requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval")))
                    {
                        ui.Enabled = false;
                        ui.UserSettings.UserWaitingForApproval = true;
                    }
                }
                else
                {
                    // EnableUserAfterRegistration is overrided by requiresConfirmation - user needs to be confirmed before enable
                    ui.Enabled = false;
                }

                // Set user's starting alias path
                if (!String.IsNullOrEmpty(StartingAliasPath))
                {
                    ui.UserStartingAliasPath = MacroResolver.ResolveCurrentPath(StartingAliasPath);
                }


                #endregion


                #region "Reserved names"

                // Check for reserved user names like administrator, sysadmin, ...
                if (UserInfoProvider.NameIsReserved(siteName, plainUserName))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("Webparts_Membership_RegistrationForm.UserNameReserved").Replace("%%name%%", HTMLHelper.HTMLEncode(Functions.GetFormattedUserName(ui.UserName, true)));
                    return;
                }

                if (UserInfoProvider.NameIsReserved(siteName, plainUserName))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("Webparts_Membership_RegistrationForm.UserNameReserved").Replace("%%name%%", HTMLHelper.HTMLEncode(ui.UserNickName));
                    return;
                }

                #endregion


                #region "License limitations"

                // Check limitations for Global administrator
                if (ui.IsGlobalAdministrator)
                {
                    if (!UserInfoProvider.LicenseVersionCheck(URLHelper.GetCurrentDomain(), FeatureEnum.GlobalAdmininistrators, ObjectActionEnum.Insert, false))
                    {
                        lblError.Visible = true;
                        lblError.Text = GetString("License.MaxItemsReachedGlobal");
                        return;
                    }
                }

                // Check limitations for editors
                if (ui.IsEditor)
                {
                    if (!UserInfoProvider.LicenseVersionCheck(URLHelper.GetCurrentDomain(), FeatureEnum.Editors, ObjectActionEnum.Insert, false))
                    {
                        lblError.Visible = true;
                        lblError.Text = GetString("License.MaxItemsReachedEditor");
                        return;
                    }
                }

                // Check limitations for site members
                if (!UserInfoProvider.LicenseVersionCheck(URLHelper.GetCurrentDomain(), FeatureEnum.SiteMembers, ObjectActionEnum.Insert, false))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("License.MaxItemsReachedSiteMember");
                    return;
                }

                #endregion


                // Check whether email is unique if it is required
                string checkSites = (String.IsNullOrEmpty(AssignToSites)) ? siteName : AssignToSites;
                if (!UserInfoProvider.IsEmailUnique(txtEmail.Text.Trim(), checkSites, 0))
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("UserInfo.EmailAlreadyExist");
                    return;
                }

                // Set password            
                UserInfoProvider.SetPassword(ui, passStrength.Text);


                // Prepare macro data source for email resolver
                UserInfo userForMail = ui.Clone();
                userForMail.SetValue("UserPassword", string.Empty);

                object[] data = new object[1];
                data[0] = userForMail;

                // Prepare resolver for notification and welcome emails
                ContextResolver resolver = MacroContext.CurrentResolver;
                resolver.SourceData = data;
                resolver.EncodeResolvedValues = true;
                #region "Servranx"
                //add to customer
                // Customer
                CustomerInfo ci = new CustomerInfo();
                ci.CustomerFirstName = txtFirstName.Text.Trim();
                ci.CustomerLastName = txtLastName.Text.Trim();
                ci.CustomerEmail = txtEmail.Text.Trim();
                // eto
                ci.CustomerPhone = txtTelephone.Text.Trim();
                ui.SetValue("Civilite", ddlFrom.SelectedValue);
                ci.CustomerOrganizationID = "";
                if (rboui.Checked)
                {
                    ci.CustomerCompany = txtnomsociete.Text.ToString();
                    ci.CustomerTaxRegistrationID = txtTva.Text.ToString();
                }
                else
                {
                    ci.CustomerCompany = "";
                    ci.CustomerTaxRegistrationID = "";
                }

                ci.CustomerCreated = DateTime.Now;
                ci.CustomerEnabled = true;
                ci.CustomerSiteID = CMSContext.CurrentSiteID;

                // Assign registered user to customer
                ci.CustomerUserID = ui.UserID;
                CustomerInfoProvider.SetCustomerInfo(ci);
                if (chkNewsletter.Checked)
                {
                    SubscribeToNewsLetter(txtFirstName.Text, txtLastName.Text, txtEmail.Text);
                }
                // create address
                            
                if (chkShippingAddr.Checked)
                {                                                                    
                    SetAdresseBilling(Convert.ToInt32(txtnumero.Text),txtadresse1.Text, txtadresse2.Text, txtcp.Text, txtville.Text, ci.CustomerID, true,false, CountryID);
                    SetAdresseShipping(Convert.ToInt32(txtnumeroshipping.Text), txtadresse1shipping.Text, txtadresse2shipping.Text, txtcpshipping.Text, txtvilleshipping.Text, ci.CustomerID, false, true, ShippingCountryID);
                }
                else 
                {                   
                    SetAdresseBilling(Convert.ToInt32(txtnumero.Text),txtadresse1.Text, txtadresse2.Text, txtcp.Text, txtville.Text, ci.CustomerID, true, true, CountryID);
                }
                
                #endregion

                #region "Welcome Emails (confirmation, waiting for approval)"

                bool error = false;
                EventLogProvider ev = new EventLogProvider();
                EmailTemplateInfo template = null;

                string emailSubject = null;
                // Send welcome message with username and password, with confirmation link, user must confirm registration
                if (requiresConfirmation)
                {
                    template = EmailTemplateProvider.GetEmailTemplate("RegistrationConfirmation", siteName);
                    emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationConfirmationEmailSubject"));
                }
                // Send welcome message with username and password, with information that user must be approved by administrator
                else if (SendWelcomeEmail)
                {
                    if (requiresAdminApprove)
                    {
                        template = EmailTemplateProvider.GetEmailTemplate("Membership.RegistrationWaitingForApproval", siteName);
                        emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationWaitingForApprovalSubject"));
                    }
                    // Send welcome message with username and password, user can logon directly
                    else
                    {
                        template = EmailTemplateProvider.GetEmailTemplate("Membership.Registration", siteName);
                        emailSubject = EmailHelper.GetSubject(template, GetString("RegistrationForm.RegistrationSubject"));
                    }
                }

                if (template != null)
                {
                    // Retrieve contact ID for confirmation e-mail
                    int contactId = 0;
                    if (ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(siteName))
                    {
                        // Check if loggin registration activity is enabled
                        if (ActivitySettingsHelper.UserRegistrationEnabled(siteName))
                        {
                            if (ActivitySettingsHelper.ActivitiesEnabledForThisUser(ui))
                            {
                                contactId = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                            }
                        }
                    }

                    // Prepare macro replacements
                    string[,] replacements = new string[6, 2];
                    replacements[0, 0] = "confirmaddress";
                    replacements[0, 1] = (ApprovalPage != String.Empty) ? URLHelper.GetAbsoluteUrl(ApprovalPage) : URLHelper.GetAbsoluteUrl("~/CMSPages/Dialogs/UserRegistration.aspx");
                    replacements[0, 1] += "?userguid=" + ui.UserGUID + (contactId > 0 ? "&contactid=" + contactId.ToString() : String.Empty);
                    replacements[1, 0] = "username";
                    replacements[1, 1] = plainUserName;
                    replacements[2, 0] = "password";
                    replacements[2, 1] = passStrength.Text;
                    replacements[3, 0] = "Email";
                    replacements[3, 1] = txtEmail.Text;
                    replacements[4, 0] = "FirstName";
                    replacements[4, 1] = txtFirstName.Text;
                    replacements[5, 0] = "LastName";
                    replacements[5, 1] = txtLastName.Text;

                    // Set resolver
                    resolver.SourceParameters = replacements;

                    // Email message
                    EmailMessage email = new EmailMessage();
                    email.EmailFormat = EmailFormatEnum.Default;
                    email.Recipients = ui.Email;

                    email.From = EmailHelper.GetSender(template, SettingsKeyInfoProvider.GetStringValue(siteName + ".CMSNoreplyEmailAddress"));
                    email.Body = resolver.ResolveMacros(template.TemplateText);

                    resolver.EncodeResolvedValues = false;
                    email.PlainTextBody = resolver.ResolveMacros(template.TemplatePlainText);
                    email.Subject = resolver.ResolveMacros(emailSubject);

                    email.CcRecipients = template.TemplateCc;
                    email.BccRecipients = template.TemplateBcc;

                    try
                    {
                        MetaFileInfoProvider.ResolveMetaFileImages(email, template.TemplateID, EmailObjectType.EMAILTEMPLATE, MetaFileInfoProvider.OBJECT_CATEGORY_TEMPLATE);
                        // Send the e-mail immediately
                        EmailSender.SendEmail(siteName, email, true);
                    }
                    catch (Exception ex)
                    {
                        ev.LogEvent("E", "RegistrationForm - SendEmail", ex);
                        error = true;
                    }
                }

                // If there was some error, user must be deleted
                if (error)
                {
                    lblError.Visible = true;
                    lblError.Text = GetString("RegistrationForm.UserWasNotCreated");

                    // Email was not send, user can't be approved - delete it
                    UserInfoProvider.DeleteUser(ui);
                    return;
                }

                #endregion


                #region "Administrator notification email"

                // Notify administrator if enabled and e-mail confirmation is not required
                if (!requiresConfirmation && NotifyAdministrator && (FromAddress != String.Empty) && (ToAddress != String.Empty))
                {
                    EmailTemplateInfo mEmailTemplate = null;

                    if (requiresAdminApprove)
                    {
                        mEmailTemplate = EmailTemplateProvider.GetEmailTemplate("Registration.Approve", siteName);
                    }
                    else
                    {
                        mEmailTemplate = EmailTemplateProvider.GetEmailTemplate("Registration.New", siteName);
                    }

                    if (mEmailTemplate == null)
                    {
                        // Log missing e-mail template
                        ev.LogEvent("E", DateTime.Now, "RegistrationForm", "GetEmailTemplate", HTTPHelper.GetAbsoluteUri());
                    }
                    else
                    {
                        string[,] replacements = new string[4, 2];
                        replacements[0, 0] = "firstname";
                        replacements[0, 1] = ui.FirstName;
                        replacements[1, 0] = "lastname";
                        replacements[1, 1] = ui.LastName;
                        replacements[2, 0] = "email";
                        replacements[2, 1] = ui.Email;
                        replacements[3, 0] = "username";
                        replacements[3, 1] = plainUserName;

                        // Set resolver
                        resolver.SourceParameters = replacements;

                        EmailMessage message = new EmailMessage();

                        message.EmailFormat = EmailFormatEnum.Default;
                        message.From = EmailHelper.GetSender(mEmailTemplate, FromAddress);
                        message.Recipients = ToAddress;
                        message.Body = resolver.ResolveMacros(mEmailTemplate.TemplateText);

                        resolver.EncodeResolvedValues = false;
                        message.PlainTextBody = resolver.ResolveMacros(mEmailTemplate.TemplatePlainText);
                        message.Subject = resolver.ResolveMacros(EmailHelper.GetSubject(mEmailTemplate, GetString("RegistrationForm.EmailSubject")));

                        message.CcRecipients = mEmailTemplate.TemplateCc;
                        message.BccRecipients = mEmailTemplate.TemplateBcc;

                        try
                        {
                            // Attach template meta-files to e-mail
                            MetaFileInfoProvider.ResolveMetaFileImages(message, mEmailTemplate.TemplateID, EmailObjectType.EMAILTEMPLATE, MetaFileInfoProvider.OBJECT_CATEGORY_TEMPLATE);
                            EmailSender.SendEmail(siteName, message);
                        }
                        catch
                        {
                            ev.LogEvent("E", DateTime.Now, "Membership", "RegistrationEmail", SiteContext.CurrentSite.SiteID);
                        }
                    }
                }

                #endregion


                #region "Web analytics"

                // Track successful registration conversion
                if (TrackConversionName != String.Empty)
                {
                    if (AnalyticsHelper.AnalyticsEnabled(siteName) && AnalyticsHelper.TrackConversionsEnabled(siteName) && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
                    {
                        // Log conversion
                        HitLogProvider.LogConversions(siteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
                    }
                }

                // Log registered user if confirmation is not required
                if (!requiresConfirmation)
                {
                    AnalyticsHelper.LogRegisteredUser(siteName, ui);
                }

                #endregion


                #region "On-line marketing - activity"

                // Log registered user if confirmation is not required
                if (!requiresConfirmation)
                {

                    Activity activity = new ActivityRegistration(ui, DocumentContext.CurrentDocument, AnalyticsContext.ActivityEnvironmentVariables);
                    if (activity.Data != null)
                    {
                        activity.Data.ContactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                        activity.Log();
                    }
                    // Log login activity
                    if (ui.Enabled)
                    {
                        // Log activity
                        int contactID = ModuleCommands.OnlineMarketingGetUserLoginContactID(ui);
                        Activity activityLogin = new ActivityUserLogin(contactID, ui, DocumentContext.CurrentDocument, AnalyticsContext.ActivityEnvironmentVariables);
                        activityLogin.Log();
                    }
                }

                #endregion


                #region "Roles & authentication"

                string[] roleList = AssignRoles.Split(';');
                string[] siteList;

                // If AssignToSites field set
                if (!String.IsNullOrEmpty(AssignToSites))
                {
                    siteList = AssignToSites.Split(';');
                }
                else // If not set user current site 
                {
                    siteList = new string[] { siteName };
                }

                foreach (string sn in siteList)
                {
                    // Add new user to the current site
                    UserInfoProvider.AddUserToSite(ui.UserName, sn);
                    foreach (string roleName in roleList)
                    {
                        if (!String.IsNullOrEmpty(roleName))
                        {
                            String s = roleName.StartsWithCSafe(".") ? "" : sn;

                            // Add user to desired roles
                            if (RoleInfoProvider.RoleExists(roleName, s))
                            {
                                UserInfoProvider.AddUserToRole(ui.UserName, roleName, s);
                            }
                        }
                    }
                }

                if (DisplayMessage.Trim() != String.Empty)
                {
                    pnlForm.Visible = false;
                    lblText.Visible = true;
                    lblText.Text = DisplayMessage;
                }
                else
                {
                    if (ui.Enabled)
                    {
                        CMSContext.AuthenticateUser(ui.UserName, true);
                    }

                    if (RedirectToURL != String.Empty)
                    {
                        URLHelper.Redirect(RedirectToURL);
                    }

                    else if (QueryHelper.GetString("ReturnURL", "") != String.Empty)
                    {
                        string url = QueryHelper.GetString("ReturnURL", "");

                        // Do url decode 
                        url = Server.UrlDecode(url);

                        // Check that url is relative path or hash is ok
                        if (url.StartsWithCSafe("~") || url.StartsWithCSafe("/") || QueryHelper.ValidateHash("hash"))
                        {
                            URLHelper.Redirect(url);
                        }
                        // Absolute path with wrong hash
                        else
                        {
                            URLHelper.Redirect(ResolveUrl("~/CMSMessages/Error.aspx?title=" + ResHelper.GetString("general.badhashtitle") + "&text=" + ResHelper.GetString("general.badhashtext")));
                        }
                    }
                }

                #endregion

                // suscribe newsletter

                lblError.Visible = false;
            }
            else { 
             lblError.Visible = true;             
             lblError.Text = GetString("conditionobligatoire");
             return;                      
            }
        }
    }

    #endregion
    protected void chkShippingAddr_CheckedChanged(object sender, EventArgs e)
    {
        /*
        if (plhShipping.Visible)
        {
            plhShipping.Visible = false;           
        }
        else
        {
            plhShipping.Visible = true;
            ShowShippingCountryList();           
        }*/
    }

   

   
    private void SubscribeToNewsLetter(string firstName, string lastName, string email)
    {
        EventLogProvider _eventLogProvider = new EventLogProvider();
        var newsletter = CMS.Newsletters.NewsletterInfoProvider.GetNewsletterInfo("NewsLetter1", CMSContext.CurrentSiteID);
        if (newsletter == null)
        {
            _eventLogProvider.LogEvent("I", DateTime.Now, "NewsLetter1", "Insert", CMSContext.CurrentSiteID);
            return;
        }

        if (!CMS.Newsletters.SubscriberInfoProvider.EmailExists(email))
        {
            var subscriberGuid = Guid.NewGuid();

            var sb = new CMS.Newsletters.SubscriberInfo()
            {
                SubscriberGUID = subscriberGuid,
                SubscriberFirstName = firstName,
                SubscriberLastName = lastName,
                SubscriberFullName = firstName + " " + lastName,
                SubscriberEmail = email,
                SubscriberSiteID = CMSContext.CurrentSiteID
            };

            sb.SetValue("SubscriberCreateDate", DateTime.Now);

            try
            {
                CMS.Newsletters.SubscriberInfoProvider.SetSubscriberInfo(sb);
                CMS.Newsletters.SubscriberInfoProvider.Subscribe(subscriberGuid, newsletter.NewsletterID, CMSContext.CurrentSiteID);
                _eventLogProvider.LogEvent("I", DateTime.Now, "register", "Delete", CMSContext.CurrentSiteID);
            }
            catch (Exception e)
            {
                _eventLogProvider.LogEvent("register", "Insert", e, CMSContext.CurrentSiteID);
            }
        }

    }
    private void SetAdresseBilling(int numero, string adresse1, string adresse2, string cp1, string ville1, int cuid, bool adrbil, bool adrship, int pays)
    {

            // Create new address object
            AddressInfo newAddress = new AddressInfo();

            // Set the properties
            newAddress.SetValue("AddressNumber", numero);
            newAddress.AddressName = txtFirstName.Text + " " + txtLastName.Text + " , " + numero + " " +  adresse1 + " - " + cp1 + " " + ville1;
            newAddress.AddressLine1 = adresse1;
            newAddress.AddressLine2 = adresse2;
            newAddress.AddressCity = ville1;
            newAddress.AddressZip =cp1;
            newAddress.AddressIsBilling = adrbil;
            newAddress.AddressIsShipping = adrship;
            newAddress.AddressEnabled = true;
            newAddress.AddressPersonalName = txtFirstName.Text + " " + txtLastName.Text;
            newAddress.AddressCustomerID = cuid;
            newAddress.AddressCountryID = pays;
          
            // Create the address
            AddressInfoProvider.SetAddressInfo(newAddress);
        
    }
    private void SetAdresseShipping(int numero, string adresse1, string adresse2, string cp1, string ville1, int cuid, bool adrbil, bool adrship, int pays)
    {

        // Create new address object
        AddressInfo newAddress = new AddressInfo();

        // Set the properties       
        newAddress.SetValue("AddressNumber", numero);
        newAddress.AddressName = txtFirstName.Text + " " + txtLastName.Text + " , " + numero + " " + adresse1 + " - " + cp1 + " " + ville1;
        newAddress.AddressLine1 = adresse1;
        newAddress.AddressLine2 = adresse2;
        newAddress.AddressCity = ville1;
        newAddress.AddressZip = cp1;
        newAddress.AddressIsBilling = adrbil;
        newAddress.AddressIsShipping = adrship;
        newAddress.AddressEnabled = true;
        newAddress.AddressPersonalName = txtFirstName.Text + " " + txtLastName.Text;
        newAddress.AddressCustomerID = cuid;
        newAddress.AddressCountryID = pays;

        // Create the address
        AddressInfoProvider.SetAddressInfo(newAddress);

    }

    private void ShowCivility()
    {
        ListItem[] items = new ListItem[3];
        items[0] = new ListItem(GetString("civ"), "1");
        items[1] = new ListItem(GetString("mr"), "2");
        items[2] = new ListItem(GetString("mrs"), "3");

        if (ddlFrom.Items.Count == 0)
        {
            ddlFrom.Items.AddRange(items);
            ddlFrom.DataBind();
        }     
    }

    private void ShowShippingCountryList()
    {
       // if (!IsPostBack)
       // {
                                
            GeneralConnection cn = ConnectionHelper.GetConnection();            
			string stringQuery = string.Format("SELECT CountryID, CountryDisplayName FROM CMS_Country where countryid in (select shippingcountryid from customtable_shippingextensioncountry) ORDER BY CountryDisplayName");
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            cn.Close();
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ds = LocalizedCountry.LocalizeCountry(ds);
                ddlShippingCountry.DataSource = ds;
                ddlShippingCountry.DataTextField = "CountryDisplayName";
                ddlShippingCountry.DataValueField = "CountryId";
                ddlShippingCountry.DataBind();
           
                ddlShippingCountry.Items.Insert(0, new ListItem(GetString("choixpays")));               
            }
       // }
    }

    private void ShowBillingCountryList()
    {
        if (!IsPostBack)
        {

            GeneralConnection cn = ConnectionHelper.GetConnection();
            //changer les donn�es du ddlcountry comme celle de la 1�re �tape
            //string stringQuery = string.Format("SELECT CountryID, CountryDisplayName FROM CMS_Country ORDER BY CountryDisplayName");
            string stringQuery = string.Format("SELECT CountryID, CountryDisplayName FROM CMS_Country  WHERE CountryId IN (SELECT ShippingCountryID from customtable_shippingextensioncountry)");
            DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            cn.Close();
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ds = LocalizedCountry.LocalizeCountry(ds);
                ddlBillingCountry.DataSource = ds;
                ddlBillingCountry.DataTextField = "CountryDisplayName";
                ddlBillingCountry.DataValueField = "CountryId";
                ddlBillingCountry.DataBind();

                ddlBillingCountry.Items.Insert(0, new ListItem(GetString("choixpays")));
            }
        }
    }

    protected void rbnom_CheckedChanged(object sender, EventArgs e)
    {
        txtTva.Visible = false;
        txtnomsociete.Visible = false;
        rboui.Checked = false;
    }

    protected void rboui_CheckedChanged(object sender, EventArgs e)
    {
        txtTva.Visible = true;
        txtnomsociete.Visible = true;
        rbnon.Checked = false;
    }
    protected void txttva_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if((txtTva.Text == "") || (txtTva.Text == "TVA") || (txtTva.Text == "VAT"))
            {
                lblerror1.Visible = true;
                lblerror1.Text = GetString("errortva ");
                return;
            }

            if(!EUVatChecker.Check(txtTva.Text))
            {
                lblerror1.Visible = true;
                lblerror1.Text = GetString("errortva2 ");
                return;
            }
        }
        catch (Exception ex)
        {
            ev.LogEvent("E", DateTime.Now,
                        "CMSWebParts_Membership_Registration_registrationformservranx.txttva_TextChanged", ex.Message);
        }

    }
}