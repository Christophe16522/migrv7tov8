﻿using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.Ecommerce;
using CMS.EcommerceProvider;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;

public partial class CMSModules_Ecommerce_Controls_ShoppingCart_ShoppingCartPaymentShipping : ShoppingCartStep
{
	private readonly EventLogProvider p = new EventLogProvider();
    #region "ViewState Constants"

    private const string SHIPPING_OPTION_ID = "OrderShippingOptionID";
    private const string PAYMENT_OPTION_ID = "OrderPaymenOptionID";

    #endregion


    #region "Variables"

    private bool? mIsShippingNeeded = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Returns true if shopping cart items need shipping.
    /// </summary>
    protected bool IsShippingNeeded
    {
        get
        {
            //return true;
            if (mIsShippingNeeded.HasValue)
            {
                return mIsShippingNeeded.Value;
            }
            else
            {
                if (ShoppingCart != null)
                {
                    // Figure out from shopping cart
                    mIsShippingNeeded = ShippingOptionInfoProvider.IsShippingNeeded(ShoppingCart);
                    return mIsShippingNeeded.Value;
                }
                else
                {
                    return true;
                }
            }
        }
    }

    #endregion


    #region "Page methods"

    protected void Page_Load(object sender, EventArgs e)
    {
		p.LogEvent("I", DateTime.Now, "ShoppingCartPaymentShipping "  , "");
        // Init labels
        lblTitle.Text = GetString("shoppingcart.shippingpaymentoptions");
        lblPayment.Text = GetString("shoppingcartpaymentshipping.payment");
        lblShipping.Text = GetString("shoppingcartpaymentshipping.shipping");

        selectShipping.IsLiveSite = IsShippingNeeded && IsLiveSite;
        selectPayment.IsLiveSite = IsLiveSite;

        if ((ShoppingCart != null) && (SiteContext.CurrentSite != null))
        {
            if (ShoppingCart.CountryID == 0)
            {
                string countryName = ECommerceSettings.DefaultCountryName(SiteContext.CurrentSite.SiteName);
                CountryInfo ci = CountryInfoProvider.GetCountryInfo(countryName);
                ShoppingCart.CountryID = (ci != null) ? ci.CountryID : 0;
            }

            if(IsShippingNeeded) selectShipping.ShoppingCart = ShoppingCart;
        }

        if (!ShoppingCartControl.IsCurrentStepPostBack)
        {
            if (IsShippingNeeded)
            {
                SelectShippingOption();
            }
            else
            {
                // Don't use shipping selection
                selectShipping.StopProcessing = true;

                // Hide title
                lblTitle.Visible = false;

                // Change current checkout process step caption
                ShoppingCartControl.CheckoutProcessSteps[ShoppingCartControl.CurrentStepIndex].Caption = GetString("order_new.paymentshipping.titlenoshipping");
            }
        }
        AddressInfo aiBill = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartBillingAddressID);
        AddressInfo aiShip = AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID);
        if (aiBill != null && aiShip != null)
        {
            addressData.Text = string.Format("<h1> ADDRESS DATA </h1> <br/>{0} - {1} <br/> {2} - {3}", aiBill.AddressID.ToString(), aiBill.AddressCity, aiShip.AddressID.ToString(), aiShip.AddressCity);
        }
        else
        {
            if (aiBill == null)
            {
				p.LogEvent("I", DateTime.Now, "aiBill null "  , "");
                addressData.Text = "AIBILL NULL";
            }
            if (IsShippingNeeded && aiShip == null)
            {
				p.LogEvent("I", DateTime.Now, "aiShip null "  , "");
                addressData.Text = string.Format("{0} AISHIP NULL", addressData.Text);
            }

        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (selectShipping.HasData)
        {
            // Show shipping selection
            plcShipping.Visible = true;

            // Initialize payment options for selected shipping option
            selectPayment.ShippingOptionID = selectShipping.ShippingID;
            selectPayment.PaymentID = -1;
            selectPayment.DisplayOnlyAllowedIfNoShipping = false;
        }
        else
        {
            selectPayment.DisplayOnlyAllowedIfNoShipping = true;
        }

        selectPayment.ReloadData();

        SelectPaymentOption();

        plcPayment.Visible = selectPayment.HasData;
    }

    #endregion


    /// <summary>
    /// Back button actions.
    /// </summary>
    public override void ButtonBackClickAction()
    {
        // Save the values to ShoppingCart ViewState
        if(IsShippingNeeded) ShoppingCartControl.SetTempValue(SHIPPING_OPTION_ID, selectShipping.ShippingID);
        ShoppingCartControl.SetTempValue(PAYMENT_OPTION_ID, selectPayment.PaymentID);

        base.ButtonBackClickAction();
    }

    private int GetShippingID()
    {
        int result=0;
        /*GeneralConnection cn= ConnectionHelper.GetConnection();
        string priceID = ValidationHelper.GetString(SessionHelper.GetValue("PriceID") ,"0");
        string stringQuery = string.Format("SELECT ShippingoptionID FROM customtable_shippingextensioncountry WHERE itemid IN (SELECT shippingextensioncountryID FROM customtable_shippingextensionpricing WHERE itemID={0})", priceID);
        DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            result = Convert.ToInt32((ds.Tables[0].Rows[0]["ShippingoptionID"])); 
        }*/
        int PriceID = ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPriceID");
        int ShippingUnit = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
        //string priceID = ValidationHelper.GetString(SessionHelper.GetValue("PriceID"), "0");
        string priceID = PriceID.ToString();
        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", ShippingUnit);
        parameters.Add("@CountryID", (AddressInfoProvider.GetAddressInfo(ShoppingCart.ShoppingCartShippingAddressID)).AddressCountryID);
        parameters.Add("@VATRate", 1 + ShippingExtendedInfoProvider.GetCartShippingVatRate(ShoppingCart) / 100);
        string where = string.Format("ItemID={0}", priceID);
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters, where);


        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow drow in ds.Tables[0].Rows)
            {
                result = ValidationHelper.GetInteger(drow["ShippingoptionID"], 0);
                if (priceID == drow["ItemID"].ToString())
                {
                    return result;
                }
            }
        }
        return result;
    }

    public override bool ProcessStep()
    {
        try
        {
			p.LogEvent("I", DateTime.Now, "ShoppingCartPaymentShipping processstep"  , "");
            //int paymentID = ValidationHelper.GetInteger(SessionHelper.GetValue("PaymentID"), 0);
            int paymentID = ShippingExtendedInfoProvider.GetCustomFieldValue(ShoppingCart, "ShoppingCartPaymentID");
            
            // Cleanup the ShoppingCart ViewState
            if(IsShippingNeeded) ShoppingCartControl.SetTempValue(SHIPPING_OPTION_ID, null);
            
            ShoppingCartControl.SetTempValue(PAYMENT_OPTION_ID, null);
            
            //ShoppingCart.ShoppingCartShippingOptionID = selectShipping.ShippingID;
            //ShoppingCart.ShoppingCartPaymentOptionID = selectPayment.PaymentID;
            if(IsShippingNeeded) ShoppingCart.ShoppingCartShippingOptionID = GetShippingID();
            ShoppingCart.ShoppingCartPaymentOptionID = paymentID;
            
            // Update changes in database only when on the live site
            if (!ShoppingCartControl.IsInternalOrder)
            {
                ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);
            }
            return true;
        }
        catch (Exception ex)
        {
            lblError.Visible = true;
            lblError.Text = ex.Message + " /step " + IsShippingNeeded.ToString();
            return false;
        }
    }


    /// <summary>
    /// Preselects payment option.
    /// </summary>
    protected void SelectPaymentOption()
    {
        try
        {
            // Try to select payment from ViewState first
            object viewStateValue = ShoppingCartControl.GetTempValue(PAYMENT_OPTION_ID);
            if (viewStateValue != null)
            {
                selectPayment.PaymentID = Convert.ToInt32(viewStateValue);
            }
            // Try to select payment option according to saved option in shopping cart object
            else if (ShoppingCart.ShoppingCartPaymentOptionID > 0)
            {
                selectPayment.PaymentID = ShoppingCart.ShoppingCartPaymentOptionID;
            }
            // Try to select payment option according to user preffered option
            else
            {
                CustomerInfo customer = ShoppingCart.Customer;
                int paymentOptionId = (customer.CustomerUser != null) ? customer.CustomerUser.GetUserPreferredPaymentOptionID(SiteContext.CurrentSiteName) : 0;
                if (paymentOptionId > 0)
                {
                    selectPayment.PaymentID = paymentOptionId;
                }
            }
        }
        catch
        {
        }
    }


    /// <summary>
    /// Preselects shipping option.
    /// </summary>
    protected void SelectShippingOption()
    {
        try
        {
            // Try to select shipping from ViewState first
            object viewStateValue = ShoppingCartControl.GetTempValue(SHIPPING_OPTION_ID);
            if (viewStateValue != null)
            {
                selectShipping.ShippingID = Convert.ToInt32(viewStateValue);
            }
            // Try to select shipping option according to saved option in shopping cart object
            else if (ShoppingCart.ShoppingCartShippingOptionID > 0)
            {
                selectShipping.ShippingID = ShoppingCart.ShoppingCartShippingOptionID;
            }
            // Try to select shipping option according to user preffered option
            else
            {
                CustomerInfo customer = ShoppingCart.Customer;
                int shippingOptionId = (customer.CustomerUser != null) ? customer.CustomerUser.GetUserPreferredShippingOptionID(SiteContext.CurrentSiteName) : 0;
                if (shippingOptionId > 0)
                {
                    selectShipping.ShippingID = shippingOptionId;
                }
            }
        }
        catch
        {
        }
    }


    public override bool IsValid()
    {
        string errorMessage = "";

        // If shipping is required
        if (plcShipping.Visible)
        {
            if (selectShipping.ShippingID <= 0)
            {
                //errorMessage = GetString("Order_New.NoShippingOption");
            }
        }

        // If payment is required
        if (plcPayment.Visible)
        {
            if ((errorMessage == "") && (selectPayment.PaymentID <= 0))
            {
                //errorMessage = GetString("Order_New.NoPaymentMethod");
            }
        }

        if (errorMessage == "")
        {
            // Form is valid
            return true;
        }

        // Form is not valid
        lblError.Visible = true;
        lblError.Text = errorMessage;
        return false;
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        ButtonNextClickAction();
    }
}