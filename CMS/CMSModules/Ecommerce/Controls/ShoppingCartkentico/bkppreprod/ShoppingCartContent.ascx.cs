﻿using System;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;

using CMS.CMSHelper;
using CMS.Ecommerce;
using CMS.EcommerceProvider;
using CMS.ExtendedControls;
using CMS.GlobalHelper;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.URLRewritingEngine;
using CMS.WebAnalytics;
using CMS.EventLog;
using CMS.DataEngine;
using CMS.SettingsProvider;
using CMS.Helpers;
using CMS.Globalization;
using CMS.Membership;
public partial class CMSModules_Ecommerce_Controls_ShoppingCart_ShoppingCartContent : ShoppingCartStep
{
    #region "Variables"

    protected Button btnReload = null;
    protected Button btnAddProduct = null;
    protected HiddenField hidProductID = null;
    protected HiddenField hidQuantity = null;
    protected HiddenField hidOptions = null;

    protected Nullable<bool> mEnableEditMode = null;
    protected bool checkInventory = false;

    #endregion


    #region Properties

    /// <summary>
    /// Indicates whether there are another checkout process steps after the current step, except payment.
    /// </summary>
    private bool ExistAnotherStepsExceptPayment
    {
        get
        {
            return (ShoppingCartControl.CurrentStepIndex + 2 <= ShoppingCartControl.CheckoutProcessSteps.Count - 1);
        }
    }

    #endregion


    /// <summary>
    /// Child control creation.
    /// </summary>
    protected override void CreateChildControls()
    {
        // Add product button
        btnAddProduct = new CMSButton();
        btnAddProduct.Attributes["style"] = "display: none";
        Controls.Add(btnAddProduct);
        btnAddProduct.Click += new EventHandler(btnAddProduct_Click);
        selectCurrency.UniSelector.OnSelectionChanged += delegate(object sender, EventArgs args)
        {
            btnUpdate_Click1(null, null);
        };
        selectCurrency.DropDownSingleSelect.AutoPostBack = true;

        // Add the hidden fields for productId, quantity and product options
        hidProductID = new HiddenField();
        hidProductID.ID = "hidProductID";
        Controls.Add(hidProductID);

        hidQuantity = new HiddenField();
        hidQuantity.ID = "hidQuantity";
        Controls.Add(hidQuantity);

        hidOptions = new HiddenField();
        hidOptions.ID = "hidOptions";
        Controls.Add(hidOptions);
    }


    protected override void OnPreRender(EventArgs e)
    {
        // Register add product script
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "AddProductScript",
                                               ScriptHelper.GetScript(
                                                   "function setProduct(val) { document.getElementById('" + hidProductID.ClientID + "').value = val; } \n" +
                                                   "function setQuantity(val) { document.getElementById('" + hidQuantity.ClientID + "').value = val; } \n" +
                                                   "function setOptions(val) { document.getElementById('" + hidOptions.ClientID + "').value = val; } \n" +
                                                   "function setPrice(val) { document.getElementById('" + hdnPrice.ClientID + "').value = val; } \n" +
                                                   "function setIsPrivate(val) { document.getElementById('" + hdnIsPrivate.ClientID + "').value = val; } \n" +
                                                   "function AddProduct(productIDs, quantities, options, price, isPrivate) { \n" +
                                                   "setProduct(productIDs); \n" +
                                                   "setQuantity(quantities); \n" +
                                                   "setOptions(options); \n" +
                                                   "setPrice(price); \n" +
                                                   "setIsPrivate(isPrivate); \n" +
                                                   Page.ClientScript.GetPostBackEventReference(btnAddProduct, null) +
                                                   ";} \n" +
                                                   "function RefreshCart() {" +
                                                   Page.ClientScript.GetPostBackEventReference(btnAddProduct, null) +
                                                   ";} \n"
                                                   ));

        // Register dialog script
        ScriptHelper.RegisterDialogScript(Page);


        // Disable specific controls
        if (!Enabled)
        {
            lnkNewItem.Enabled = false;
            lnkNewItem.OnClientClick = "";
            selectCurrency.Enabled = false;
            btnEmpty.Enabled = false;
            btnUpdate.Enabled = false;
            chkSendEmail.Enabled = false;
        }

        // Show/Hide dropdownlist with currencies
        pnlCurrency.Visible &= (selectCurrency.UniSelector.HasData && selectCurrency.DropDownSingleSelect.Items.Count > 1);

        // Check session parameters for inventory check
        if (ValidationHelper.GetBoolean(SessionHelper.GetValue("checkinventory"), false))
        {
            checkInventory = true;
            SessionHelper.Remove("checkinventory");
        }

        // Check inventory
        if (checkInventory)
        {
            ShoppingCartCheckResult checkResult = ShoppingCartInfoProvider.CheckShoppingCart(ShoppingCart);

            if (checkResult.CheckFailed)
            {
                lblError.Text = checkResult.GetHTMLFormattedMessage();
            }
        }

        // Display messages if required
        lblError.Visible = !string.IsNullOrEmpty(lblError.Text.Trim());
        lblInfo.Visible = !string.IsNullOrEmpty(lblInfo.Text.Trim());
        base.OnPreRender(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        
        ButtonsEmpty.Visible = (Request.Url.Host.Contains("localhost"));
    }

    protected override void OnLoad(EventArgs e)
    {
        if ((ShoppingCart != null) && (ShoppingCart.CountryID == 0) && (SiteContext.CurrentSite != null))
        {
            string countryName = ECommerceSettings.DefaultCountryName(SiteContext.CurrentSite.SiteName);
            CountryInfo ci = CountryInfoProvider.GetCountryInfo(countryName);
            ShoppingCart.CountryID = (ci != null) ? ci.CountryID : 0;

            // Set currency selectors site ID
            selectCurrency.SiteID = ShoppingCart.ShoppingCartSiteID;
        }

        imgNewItem.ImageUrl = GetImageUrl("Objects/Ecommerce_OrderItem/add.png");
        lblCurrency.Text = GetString("ecommerce.shoppingcartcontent.currency");
        lblCoupon.Text = GetString("ecommerce.shoppingcartcontent.coupon");
        lnkNewItem.Text = GetString("ecommerce.shoppingcartcontent.additem");
        imgNewItem.AlternateText = lnkNewItem.Text;
        btnUpdate.Text = GetString("ecommerce.shoppingcartcontent.btnupdate");
        btnEmpty.Text = GetString("ecommerce.shoppingcartcontent.btnempty");
        btnEmpty.OnClientClick = string.Format("return confirm({0});", ScriptHelper.GetString(GetString("ecommerce.shoppingcartcontent.emptyconfirm")));
        lnkNewItem.OnClientClick = string.Format("OpenAddProductDialog('{0}');return false;", GetCMSDeskShoppingCartSessionName());


        // Register product price detail dialog script
        StringBuilder script = new StringBuilder();
        script.Append("function ShowProductPriceDetail(cartItemGuid, sessionName) {");
        script.Append("if (sessionName != \"\"){sessionName =  \"&cart=\" + sessionName;}");
        string detailUrl = (IsLiveSite) ? "~/CMSModules/Ecommerce/CMSPages/ShoppingCartSKUPriceDetail.aspx" : "~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartSKUPriceDetail.aspx";
        script.Append(string.Format("modalDialog('{0}?itemguid=' + cartItemGuid + sessionName, 'ProductPriceDetail', 750, 500); }}", ResolveUrl(detailUrl)));
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ProductPriceDetail", ScriptHelper.GetScript(script.ToString()));

        // Register add order item dialog script
        script = new StringBuilder();
        script.Append("function OpenOrderItemDialog(cartItemGuid, sessionName) {");
        script.Append("if (sessionName != \"\"){sessionName =  \"&cart=\" + sessionName;}");
        script.Append(string.Format("modalDialog('{0}?itemguid=' + cartItemGuid + sessionName, 'OrderItemEdit', 500, 350); }}", ResolveUrl("~/CMSModules/Ecommerce/Controls/ShoppingCart/OrderItemEdit.aspx")));
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "OrderItemEdit", ScriptHelper.GetScript(script.ToString()));

        script = new StringBuilder();
        string addProductUrl = AuthenticationHelper.ResolveDialogUrl("~/CMSModules/Ecommerce/Pages/Tools/Orders/Order_Edit_AddItems.aspx");
        script.AppendFormat("var addProductDialogURL = '{0}'", URLHelper.GetAbsoluteUrl(addProductUrl));
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "AddProduct", ScriptHelper.GetScript(script.ToString()));


        // Hide "add product" action for live site order
        if (!ShoppingCartControl.IsInternalOrder)
        {
            pnlNewItem.Visible = false;

            ShoppingCartControl.ButtonBack.Text = GetString("ecommerce.cartcontent.buttonbacktext");
            ShoppingCartControl.ButtonBack.CssClass = "LongButton";
            ShoppingCartControl.ButtonNext.Text = GetString("ecommerce.cartcontent.buttonnexttext");

            if (!ShoppingCartControl.IsCurrentStepPostBack)
            {
                // Get shopping cart item parameters from URL
                ShoppingCartItemParameters itemParams = ShoppingCartItemParameters.GetShoppingCartItemParameters();

                // Set item in the shopping cart
                AddProducts(itemParams);
            }
        }

        // Set sending order notification when editing existing order according to the site settings
        if (ShoppingCartControl.CheckoutProcessType == CheckoutProcessEnum.CMSDeskOrderItems)
        {
            if (!ShoppingCartControl.IsCurrentStepPostBack)
            {
                if (!string.IsNullOrEmpty(ShoppingCart.SiteName))
                {
                    chkSendEmail.Checked = ECommerceSettings.SendOrderNotification(ShoppingCart.SiteName);
                }
            }
            // Show send email checkbox
            chkSendEmail.Visible = true;
            chkSendEmail.Text = GetString("shoppingcartcontent.sendemail");

            // Setup buttons
            ShoppingCartControl.ButtonBack.Visible = false;
            ShoppingCart.CheckAvailableItems = false;
            ShoppingCartControl.ButtonNext.Text = GetString("general.next");

            if ((!ExistAnotherStepsExceptPayment) && (ShoppingCartControl.PaymentGatewayProvider == null))
            {
                ShoppingCartControl.ButtonNext.Text = GetString("general.ok");
            }
        }

        // Fill dropdownlist
        if (!ShoppingCartControl.IsCurrentStepPostBack)
        {
            if ((ShoppingCart.CartItems.Count > 0) || ShoppingCartControl.IsInternalOrder)
            {
                if (ShoppingCart.ShoppingCartCurrencyID == 0)
                {
                    // Select customer preferred currency                    
                    if (ShoppingCart.Customer != null)
                    {
                        CustomerInfo customer = ShoppingCart.Customer;
                        ShoppingCart.ShoppingCartCurrencyID = (customer.CustomerUser != null) ? customer.CustomerUser.GetUserPreferredCurrencyID(SiteContext.CurrentSiteName) : 0;
                    }
                }

                if (ShoppingCart.ShoppingCartCurrencyID == 0)
                {
                    if (SiteContext.CurrentSite != null)
                    {
                        ShoppingCart.ShoppingCartCurrencyID = SiteContext.CurrentSite.SiteDefaultCurrencyID;
                    }
                }

                selectCurrency.CurrencyID = ShoppingCart.ShoppingCartCurrencyID;

                if (ShoppingCart.ShoppingCartDiscountCouponID > 0)
                {
                    // Fill textbox with discount coupon code
                    DiscountCouponInfo dci = DiscountCouponInfoProvider.GetDiscountCouponInfo(ShoppingCart.ShoppingCartDiscountCouponID);
                    if (dci != null)
                    {
                        if (ShoppingCart.IsCreatedFromOrder || dci.IsValid)
                        {
                            txtCoupon.Text = dci.DiscountCouponCode;
                        }
                        else
                        {
                            ShoppingCart.ShoppingCartDiscountCouponID = 0;
                        }
                    }
                }
            }
            ShowPaymentList(); 
            ShowCountryList();
            ReloadData();
        }

        // Check if customer is enabled
        if ((ShoppingCart.Customer != null) && (!ShoppingCart.Customer.CustomerEnabled))
        {
            HideCartContent(GetString("ecommerce.cartcontent.customerdisabled"));
        }

        // Turn on available items checking after content is loaded
        ShoppingCart.CheckAvailableItems = true;

        base.OnLoad(e);
    }


    private void gridData_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Set enabled for order item units editing
            e.Row.Cells[7].Enabled = Enabled;
        }
    }


    protected void btnUpdate_Click1(object sender, EventArgs e)
    {
        if (ShoppingCart != null)
        {
            ShoppingCart.ShoppingCartCurrencyID = ValidationHelper.GetInteger(selectCurrency.CurrencyID, 0);
            // CheckDiscountCoupon();
            ReloadData();
            if ((ShoppingCart.ShoppingCartDiscountCouponID > 0) && (!ShoppingCart.IsDiscountCouponApplied))
            {
                // Discount coupon code is valid but not applied to any product of the shopping cart
                lblError.Text = GetString("shoppingcart.discountcouponnotapplied");
            }

            // Inventory shloud be checked
            checkInventory = true;
        }
    }

    protected void ddlShippingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {


        SessionHelper.SetValue("PriceID", -1);
        SessionHelper.SetValue("CountryID", -1);
        lblPriceID.Text = "Nothing";
        int ShippingCartUnits = ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart);
        int CountryID = ValidationHelper.GetInteger(ddlShippingCountry.SelectedValue, 0);
        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", ShippingCartUnits);
        parameters.Add("@CountryID", CountryID);
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters);

        
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            DataTable dt = ds.Tables[0];
            foreach (DataRow drow in dt.Rows)
            {
                double price = Convert.ToDouble(drow["ShippingFinalCost"]); 
                string prices = CurrencyInfoProvider.GetFormattedPrice(price, ShoppingCart.Currency);
                drow["DisplayString"] = string.Format("{0} - ({1})", drow["ShippingOptionDisplayName"].ToString(), prices);
            }
            ddlShippingOption.DataSource = ds;
            ddlShippingOption.DataTextField = "DisplayString";
            ddlShippingOption.DataValueField = "ItemID";
            ddlShippingOption.DataBind();
            int PriceID = ValidationHelper.GetInteger(ddlShippingOption.SelectedValue, -1);
            SessionHelper.SetValue("PriceID", PriceID);
            SessionHelper.SetValue("CountryID", CountryID);
            lblPriceID.Text = PriceID.ToString();
            ddlShippingOption_SelectedIndexChanged(sender, e);
            //btnUpdate_Click1(null, null);
        }
        else
        {
            // NO SHIPPING AVAILABLE
            ddlShippingOption.Items.Clear();
            ddlShippingOption.DataSource = null;
            ListItem listItem = new ListItem("Votre choix", "-1");
            ddlShippingOption.Items.Add(listItem);
        }
        pnlBtnNext.Visible = !DataHelper.DataSourceIsEmpty(ds);
    }

    protected void ddlPaymentOption_SelectedIndexChanged(object sender, EventArgs e)
    {
        int PaymentID = ValidationHelper.GetInteger(ddlPaymentOption.SelectedValue, -1);
        SessionHelper.SetValue("PaymentID", PaymentID);
        lblPayment.Text = PaymentID.ToString();
    }

    protected void txtProductCount_TextChanged(object sender, EventArgs e)
    {
        try
        {
        }
        catch 
        {
        }
    }

    protected void ddlShippingOption_SelectedIndexChanged(object sender, EventArgs e)
    {
        int PriceID = ValidationHelper.GetInteger(ddlShippingOption.SelectedValue, -1);
        SessionHelper.SetValue("PriceID", PriceID);
        lblPriceID.Text = PriceID.ToString();
        btnUpdate_Click1(null, null);
    }

    protected void btnEmpty_Click1(object sender, EventArgs e)
    {
        if (ShoppingCart != null)
        {
            // Log activity "product removed" for all items in shopping cart
            string siteName = SiteContext.CurrentSiteName;
            if (!ShoppingCartControl.IsInternalOrder)
            {
                ShoppingCartControl.TrackActivityAllProductsRemovedFromShoppingCart(ShoppingCart, siteName, ContactID);
            }

            ShoppingCartInfoProvider.EmptyShoppingCart(ShoppingCart);

            ReloadData();
        }
    }


    /// <summary>
    /// Validates this step.
    /// </summary>
    public override bool IsValid()
    {
        // Check modify permissions
        if (ShoppingCartControl.CheckoutProcessType == CheckoutProcessEnum.CMSDeskOrderItems)
        {
            // Check 'ModifyOrders' permission
            if (!ECommerceContext.IsUserAuthorizedForPermission("ModifyOrders"))
            {
                CMSEcommercePage.RedirectToCMSDeskAccessDenied("CMS.Ecommerce", "EcommerceModify OR ModifyOrders");
            }
        }

        // Allow to go to the next step only if shopping cart contains some products
        bool IsValid = (ShoppingCart.CartItems.Count > 0) && (ShoppingCart.TotalShipping >0);

        if (!IsValid)
        {
            HideCartContentWhenEmpty();
        }

        if (ShoppingCart.IsCreatedFromOrder)
        {
            IsValid = true;
        }

        if (!IsValid)
        {
            lblError.Text = GetString("ecommerce.error.insertsomeproducts");
        }

        return IsValid;
    }


    /// <summary>
    /// Process this step.
    /// </summary>
    public override bool ProcessStep()
    {
        // Shopping cart units are already saved in database (on "Update" or on "btnAddProduct_Click" actions) 
        bool isOK = false;

        if (ShoppingCart != null)
        {
            // Reload data
            ReloadData();

            // Check available items before "Check out"
            ShoppingCartCheckResult checkResult = ShoppingCartInfoProvider.CheckShoppingCart(ShoppingCart);

            if (checkResult.CheckFailed)
            {
                lblError.Text = checkResult.GetHTMLFormattedMessage();
            }
            else if (ShoppingCartControl.CheckoutProcessType == CheckoutProcessEnum.CMSDeskOrderItems)
            {
                // Indicates wheter order saving process is successful
                isOK = true;

                try
                {
                    ShoppingCartInfoProvider.SetOrder(ShoppingCart);
                }
                catch (Exception ex)
                {
                    // Log exception
                    EventLogProvider.LogException("Shopping cart", "SAVEORDER", ex, ShoppingCart.ShoppingCartSiteID, null);
                    isOK = false;
                }

                if (isOK)
                {
                    lblInfo.Text = GetString("general.changessaved");

                    // Send order notification when editing existing order
                    if (ShoppingCartControl.CheckoutProcessType == CheckoutProcessEnum.CMSDeskOrderItems)
                    {
                        if (chkSendEmail.Checked)
                        {
                            OrderInfoProvider.SendOrderNotificationToAdministrator(ShoppingCart);
                            OrderInfoProvider.SendOrderNotificationToCustomer(ShoppingCart);
                        }
                    }
                    // Send order notification emails when on the live site
                    else if (ECommerceSettings.SendOrderNotification(SiteContext.CurrentSite.SiteName))
                    {
                        OrderInfoProvider.SendOrderNotificationToAdministrator(ShoppingCart);
                        OrderInfoProvider.SendOrderNotificationToCustomer(ShoppingCart);
                    }
                }
                else
                {
                    lblError.Text = GetString("ecommerce.orderpreview.errorordersave");
                }
            }
            // Go to the next step
            else
            {
                // Save other options
                if (!ShoppingCartControl.IsInternalOrder)
                {
                    ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);
                }

                isOK = true;
            }
        }

        return isOK;
    }


    private void btnAddProduct_Click(object sender, EventArgs e)
    {
        // Get strings with productIDs and quantities separated by ';'
        string productIDs = ValidationHelper.GetString(hidProductID.Value, "");
        string quantities = ValidationHelper.GetString(hidQuantity.Value, "");
        string options = ValidationHelper.GetString(hidOptions.Value, "");
        double price = ValidationHelper.GetDouble(hdnPrice.Value, -1);
        bool isPrivate = ValidationHelper.GetBoolean(hdnIsPrivate.Value, false);

        // Add new products to shopping cart
        if ((productIDs != "") && (quantities != ""))
        {
            string[] arrID = productIDs.TrimEnd(';').Split(';');
            string[] arrQuant = quantities.TrimEnd(';').Split(';');
            int[] intOptions = ValidationHelper.GetIntegers(options.Split(','), 0);

            lblError.Text = "";

            for (int i = 0; i < arrID.Length; i++)
            {
                int skuId = ValidationHelper.GetInteger(arrID[i], 0);

                SKUInfo skuInfo = SKUInfoProvider.GetSKUInfo(skuId);
                if (skuInfo != null)
                {
                    int quant = ValidationHelper.GetInteger(arrQuant[i], 0);

                    ShoppingCartItemParameters cartItemParams = new ShoppingCartItemParameters(skuId, quant, intOptions);

                    // If product is donation
                    if (skuInfo.SKUProductType == SKUProductTypeEnum.Donation)
                    {
                        // Get donation properties
                        if (price < 0)
                        {
                            cartItemParams.Price = SKUInfoProvider.GetSKUPrice(skuInfo, ShoppingCart, false, false);
                        }
                        else
                        {
                            cartItemParams.Price = price;
                        }

                        cartItemParams.IsPrivate = isPrivate;
                    }

                    // Add product to the shopping cart
                    ShoppingCart.SetShoppingCartItem(cartItemParams);

                    // Log activity
                    string siteName = SiteContext.CurrentSiteName;
                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        ShoppingCartControl.TrackActivityProductAddedToShoppingCart(skuId, ResHelper.LocalizeString(skuInfo.SKUName), ContactID, siteName, RequestContext.CurrentRelativePath, quant);
                    }

                    // Show empty button
                    btnEmpty.Visible = true;
                }
            }
        }

        // Invalidate values
        hidProductID.Value = "";
        hidOptions.Value = "";
        hidQuantity.Value = "";
        hdnPrice.Value = "";

        // Update values in table
        btnUpdate_Click1(btnAddProduct, e);

        // Hide cart content when empty
        if (DataHelper.DataSourceIsEmpty(ShoppingCart.ContentTable))
        {
            HideCartContentWhenEmpty();
        }
        else
        {
            // Inventory shloud be checked
            checkInventory = true;
        }
    }


    /// <summary>
    /// Checks whether entered dsicount coupon code is usable for this cart. Returns true if so.
    /// </summary>
    private bool CheckDiscountCoupon()
    {
        bool success = true;

        if (txtCoupon.Text.Trim() != "")
        {
            // Get discount info
            DiscountCouponInfo dci = DiscountCouponInfoProvider.GetDiscountCouponInfo(txtCoupon.Text.Trim(), ShoppingCart.SiteName);

            // Do not validate coupon when editing existing order and coupon code was not changed, otherwise process validation
            if ((dci != null) && (((ShoppingCart.IsCreatedFromOrder) && (ShoppingCart.ShoppingCartDiscountCouponID == dci.DiscountCouponID)) || dci.IsValid))
            {
                ShoppingCart.ShoppingCartDiscountCouponID = dci.DiscountCouponID;
            }
            else
            {
                ShoppingCart.ShoppingCartDiscountCouponID = 0;

                // Discount coupon is not valid                
                lblError.Text = GetString("ecommerce.error.couponcodeisnotvalid");

                success = false;
            }
        }
        else
        {
            ShoppingCart.ShoppingCartDiscountCouponID = 0;
        }

        return success;
    }


    // Displays total price
    protected void DisplayTotalPrice()
    {
        pnlPrice.Visible = true;
        // lblTotalPriceValue.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedValue(ShoppingCart.RoundedTotalPrice, ShoppingCart.Currency).ToString());
        // lblTotalPrice.Text = GetString("ecommerce.cartcontent.totalpricelabel");

        lblShippingPriceValue.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedPrice(ShoppingCart.TotalShipping, ShoppingCart.Currency).ToString());

        //double bulkPrice = ShoppingCart.TotalPrice - ShoppingCart.TotalShipping;
        //lblMontantAchat.Text = CurrencyInfoProvider.GetFormattedPrice(bulkPrice, ShoppingCart.Currency);
        lblMontantAchat.Text = string.Format("{0} <em>�</em>", CurrencyInfoProvider.GetFormattedPrice(ShoppingCart.TotalItemsPriceInMainCurrency   , ShoppingCart.Currency).ToString());
        // lblMontantAchat.Text = CurrencyInfoProvider.GetFormattedPrice(ShoppingCart.RoundedTotalPrice, ShoppingCart.Currency);
        // lblMontantShipping.Text = CurrencyInfoProvider.GetFormattedPrice(ShoppingCart.TotalShipping, ShoppingCart.Currency);
    }


    /// <summary>
    /// Sets product in the shopping cart.
    /// </summary>
    /// <param name="itemParams">Shoppping cart item parameters</param>
    protected void AddProducts(ShoppingCartItemParameters itemParams)
    {
        // Get main product info
        int productId = itemParams.SKUID;
        int quantity = itemParams.Quantity;

        if ((productId > 0) && (quantity > 0))
        {
            // Check product/options combination
            if (ShoppingCartInfoProvider.CheckNewShoppingCartItems(ShoppingCart, itemParams))
            {
                // Get requested SKU info object from database
                SKUInfo skuObj = SKUInfoProvider.GetSKUInfo(productId);
                if (skuObj != null)
                {
                    // On the live site
                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        bool updateCart = false;

                        // Assign current shopping cart to current user
                        CurrentUserInfo ui = MembershipContext.AuthenticatedUser;
                        if (!ui.IsPublic())
                        {
                            ShoppingCart.User = ui;
                            updateCart = true;
                        }

                        // Shopping cart is not saved yet
                        if (ShoppingCart.ShoppingCartID == 0)
                        {
                            updateCart = true;
                        }

                        // Update shopping cart when required
                        if (updateCart)
                        {
                            ShoppingCartInfoProvider.SetShoppingCartInfo(ShoppingCart);
                        }

                        // Set item in the shopping cart
                        ShoppingCartItemInfo product = ShoppingCart.SetShoppingCartItem(itemParams);

                        // Update shopping cart item in database
                        ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(product);

                        // Update product options in database
                        foreach (ShoppingCartItemInfo option in product.ProductOptions)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(option);
                        }

                        // Update bundle items in database
                        foreach (ShoppingCartItemInfo bundleItem in product.BundleItems)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(bundleItem);
                        }

                        // Track add to shopping cart conversion
                        ECommerceHelper.TrackAddToShoppingCartConversion(product);
                    }
                    // In CMSDesk
                    else
                    {
                        // Set item in the shopping cart
                        ShoppingCart.SetShoppingCartItem(itemParams);
                    }
                }
            }

            // Avoid adding the same product after page refresh
            if (lblError.Text == "")
            {
                string url = URLRewriter.CurrentURL;

                if (!string.IsNullOrEmpty(URLHelper.GetUrlParameter(url, "productid")) ||
                    !string.IsNullOrEmpty(URLHelper.GetUrlParameter(url, "quantity")) ||
                    !string.IsNullOrEmpty(URLHelper.GetUrlParameter(url, "options")))
                {
                    // Remove parameters from URL
                    url = URLHelper.RemoveParameterFromUrl(url, "productid");
                    url = URLHelper.RemoveParameterFromUrl(url, "quantity");
                    url = URLHelper.RemoveParameterFromUrl(url, "options");
                    URLHelper.Redirect(url);
                }
            }
        }
    }


    /// <summary>
    /// Hides cart content controls when no items are in shopping cart.
    /// </summary>
    protected void HideCartContentWhenEmpty()
    {
        HideCartContent(null);
    }


    /// <summary>
    /// Hides cart content controls and displays given message.
    /// </summary>
    protected void HideCartContent(string message)
    {
        pnlNewItem.Visible = ShoppingCartControl.IsInternalOrder;
        pnlPrice.Visible = false;
        btnEmpty.Visible = false;
        plcCoupon.Visible = false;
        pnlCartLeftInnerContent.Visible = false;
        pnlCartRightInnerContent.Visible = false;
        SessionHelper.SetValue("HideNext", 1);
        if (!ShoppingCartControl.IsInternalOrder)
        {
            pnlCurrency.Visible = false;
            ShoppingCartControl.ButtonNext.Visible = false;

            message = message ?? GetString("ecommerce.shoppingcartempty");
            lblInfo.Text = message + "<br />";
        }
    }


    /// <summary>
    /// Reloads the form data.
    /// </summary>
    protected void ReloadData()
    {
        rptCart.DataSource = ShoppingCart.ContentTable;

        // Hide coupon placeholder when no coupons are defined
        plcCoupon.Visible = AreDiscountCouponsAvailableOnSite();

        // Bind data
        rptCart.DataBind();

        if (!DataHelper.DataSourceIsEmpty(ShoppingCart.ContentTable))
        {
            // Display total price
            DisplayTotalPrice();

            // Display/hide discount column
            //gridData.Columns[9].Visible = ShoppingCart.IsDiscountApplied;
        }
        else
        {
            // Hide some items
            HideCartContentWhenEmpty();
            
        }

        pnlBtnNext.Visible = !DataHelper.DataSourceIsEmpty(ShoppingCart.ContentTable);
        if (!ShippingOptionInfoProvider.IsShippingNeeded(ShoppingCart))
        {
            plcShippingPrice.Visible = false;
        }
        
    }


    /// <summary>
    /// Determines if the discount coupons are available for the current site.
    /// </summary>
    private bool AreDiscountCouponsAvailableOnSite()
    {
        string siteName = ShoppingCart.SiteName;

        // Check site and global discount coupons
        string where = "DiscountCouponSiteID = " + SiteInfoProvider.GetSiteID(siteName);
        if (ECommerceSettings.AllowGlobalDiscountCoupons(siteName))
        {
            where += " OR DiscountCouponSiteID IS NULL";
        }

        // Coupons are available if found any
        DataSet ds = DiscountCouponInfoProvider.GetDiscountCoupons(where, null, -1, "count(DiscountCouponID)");
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            return (ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0) > 0);
        }

        return false;
    }


    /// <summary>
    /// Returns price detail link.
    /// </summary>
    protected string GetPriceDetailLink(object value)
    {
        if ((ShoppingCartControl.EnableProductPriceDetail) && (ShoppingCart.ContentTable != null))
        {
            Guid cartItemGuid = ValidationHelper.GetGuid(value, Guid.Empty);
            if (cartItemGuid != Guid.Empty)
            {
                return string.Format("<img src=\"{0}\" onclick=\"javascript: ShowProductPriceDetail('{1}', '{2}')\" alt=\"{3}\" class=\"ProductPriceDetailImage\" style=\"cursor:pointer;\" />",
                    GetImageUrl("Design/Controls/UniGrid/Actions/detail.png"),
                    cartItemGuid, GetCMSDeskShoppingCartSessionName(),
                    GetString("shoppingcart.productpricedetail"));
            }
        }

        return "";
    }


    /// <summary>
    /// Returns shopping cart session name.
    /// </summary>
    private string GetCMSDeskShoppingCartSessionName()
    {
        switch (ShoppingCartControl.CheckoutProcessType)
        {
            case CheckoutProcessEnum.CMSDeskOrder:
                return "CMSDeskNewOrderShoppingCart";

            case CheckoutProcessEnum.CMSDeskCustomer:
                return "CMSDeskNewCustomerOrderShoppingCart";

            case CheckoutProcessEnum.CMSDeskOrderItems:
                return "CMSDeskOrderItemsShoppingCart";

            case CheckoutProcessEnum.LiveSite:
            case CheckoutProcessEnum.Custom:
            default:
                return "";
        }
    }


    public override void ButtonBackClickAction()
    {
        if ((!ShoppingCartControl.IsInternalOrder) && (ShoppingCartControl.CurrentStepIndex == 0))
        {
            // Continue shopping
            URLHelper.Redirect(ShoppingCartControl.PreviousPageUrl);
        }
        else
        {
            // Standard action
            base.ButtonBackClickAction();
        }
    }


    #region "Binding methods"

    private string GetQueryString()
    {
        string result = string.Format(@"SELECT CountryID, CountryDisplayName FROM CMS_COUNTRY WHERE COUNTRYID IN (
                                        SELECT ShippingCountryID FROM dbo.customtable_ShippingExtensionCountry WHERE 
                                        dbo.customtable_ShippingExtensionCountry.Enabled=1 AND dbo.customtable_ShippingExtensionCountry.ShippingOptionID IN(select ShippingOPtionID from dbo.customtable_shippingextension WHERE Enabled=1) AND ItemID IN (
                                        SELECT ShippingExtensionCountryID FROM dbo.customtable_shippingextensionpricing where shippingUnitFrom <={0} and shippingUnitTo>={0})
                                        GROUP BY ShippingCountryID) ORDER BY CountryDisplayName", ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart).ToString());
        return result;
    }

    /// <summary>
    /// Display Country list in the combo
    /// </summary>
    private void ShowCountryList()
    {
        if (!IsPostBack && pnlCartRightInnerContent.Visible)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ShippingUnit", ShippingExtendedInfoProvider.GetCartShippingUnit(ShoppingCart)); 
            DataSet ds = cn.ExecuteQuery("customtable.shippingextension.CountryList", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddlShippingCountry.DataSource = ds;
                ddlShippingCountry.DataTextField = "CountryDisplayName";
                ddlShippingCountry.DataValueField = "ShippingCountryId";
                ddlShippingCountry.DataBind();
                /*
                if (CurrentUser.IsAuthenticated())
                {
                    if (ECommerceContext.CurrentCustomer != null)
                    {
                        string where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                        string orderby = "AddressID";
                        ds = AddressInfoProvider.GetAddresses(where, orderby);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            AddressInfo ai = new AddressInfo(ds.Tables[0].Rows[0]);
                            //ddlShippingCountry.SelectedValue = ai.AddressCountryID.ToString();
                        }
                    }
                }*/
                ddlShippingCountry_SelectedIndexChanged(null, null);
                //ShowPaymentList();
            }
            
        }
    }

    private void ShowPaymentList()
    {
        if (!IsPostBack && pnlCartRightInnerContent.Visible)
        {
            GeneralConnection cn = ConnectionHelper.GetConnection();
            string stringQuery = string.Format("SELECT CountryID, CountryDisplayName FROM CMS_Country ORDER BY CountryDisplayName");
            //DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
            DataSet ds = PaymentOptionInfoProvider.GetPaymentOptions(CurrentSite.SiteID, true);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddlPaymentOption.DataSource = ds;
                ddlPaymentOption.DataTextField = "PaymentOptionDisplayName";
                ddlPaymentOption.DataValueField = "PaymentOptionId";
                ddlPaymentOption.DataBind();
                string value = ValidationHelper.GetString(SessionHelper.GetValue("PaymentID"), string.Empty);
                if (!string.IsNullOrEmpty(value))
                {
                    ddlPaymentOption.SelectedValue = value;
                }
                ddlPaymentOption_SelectedIndexChanged(null, null);
            }
        }
    }

    /// <summary>
    /// Returns formatted currency value.
    /// </summary>
    /// <param name="value">Value to format</param>
    protected string GetFormattedValue(object value)
    {
        double price = ValidationHelper.GetDouble(value, 0);
        return CurrencyInfoProvider.GetFormattedValue(price, ShoppingCart.Currency);
    }


    /// <summary>
    /// Returns formatted and localized SKU name.
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="skuSiteId">SKU site ID</param>
    /// <param name="value">SKU name</param>
    /// <param name="isProductOption">Indicates if cart item is product option</param>
    /// <param name="isBundleItem">Indicates if cart item is bundle item</param>
    protected string GetSKUName(object skuId, object skuSiteId, object value, object isProductOption, object isBundleItem, object cartItemIsPrivate, object itemText, object productType)
    {
        string name = ResHelper.LocalizeString((string)value);
        bool isPrivate = ValidationHelper.GetBoolean(cartItemIsPrivate, false);
        string text = itemText as string;
        StringBuilder skuName = new StringBuilder();
        SKUProductTypeEnum type = SKUInfoProvider.GetSKUProductTypeEnum(productType as string);

        // If it is a product option or bundle item
        if (ValidationHelper.GetBoolean(isProductOption, false) || ValidationHelper.GetBoolean(isBundleItem, false))
        {
            skuName.Append("<span style=\"font-size: 90%\"> - ");
            skuName.Append(HTMLHelper.HTMLEncode(name));

            if (!string.IsNullOrEmpty(text))
            {
                skuName.Append(" '" + HTMLHelper.HTMLEncode(text) + "'");
            }

            skuName.Append("</span>");
        }
        // If it is a parent product
        else
        {
            // Add private donation suffix
            if ((type == SKUProductTypeEnum.Donation) && (isPrivate))
            {
                name += string.Format(" ({0})", GetString("com.shoppingcartcontent.privatedonation"));
            }

            // In CMS Desk
            if (ShoppingCartControl.IsInternalOrder)
            {
                // Display SKU name
                skuName.Append(HTMLHelper.HTMLEncode(name));
            }
            // On live site
            else
            {
                if (type == SKUProductTypeEnum.Donation)
                {
                    // Display donation name without link
                    skuName.Append(HTMLHelper.HTMLEncode(name));
                }
                else
                {
                    // Display link to product page
                    skuName.Append(string.Format("<a href=\"{0}?productid={1}\" class=\"CartProductDetailLink\">{2}</a>", ResolveUrl("~/CMSPages/GetProduct.aspx"), skuId.ToString(), HTMLHelper.HTMLEncode(name)));
                }
            }
        }

        return skuName.ToString();
    }


    protected static bool IsProductOption(object isProductOption)
    {
        return ValidationHelper.GetBoolean(isProductOption, false);
    }


    protected static bool IsBundleItem(object isBundleItem)
    {
        return ValidationHelper.GetBoolean(isBundleItem, false);
    }


    /// <summary>
    /// Returns order item edit action HTML.
    /// </summary>
    protected string GetOrderItemEditAction(object value)
    {
        Guid itemGuid = ValidationHelper.GetGuid(value, Guid.Empty);

        if (itemGuid != Guid.Empty)
        {
            return string.Format("<img src=\"{0}\" onclick=\"javascript: OpenOrderItemDialog('{1}', '{2}')\" alt=\"\" title=\"{3}\" class=\"OrderItemEditLink\" style=\"cursor: pointer;\" />",
                GetImageUrl("Objects/Ecommerce_OrderItem/edit.png"),
                itemGuid,
                GetCMSDeskShoppingCartSessionName(),
                GetString("shoppingcart.editorderitem"));
        }

        return "";
    }


    /// <summary>
    /// Returns SKU edit action HTML.
    /// </summary>
    protected string GetSKUEditAction(object skuId, object skuSiteId, object isProductOption, object isBundleItem)
    {
        if (!ValidationHelper.GetBoolean(isProductOption, false) && !ValidationHelper.GetBoolean(isBundleItem, false) && ShoppingCartControl.IsInternalOrder)
        {
            string url = ResolveUrl("~/CMSModules/Ecommerce/Pages/Tools/Products/Product_Edit_Dialog.aspx");
            url = URLHelper.AddParameterToUrl(url, "productid", skuId.ToString());
            url = URLHelper.AddParameterToUrl(url, "siteid", skuSiteId.ToString());
            url = URLHelper.AddParameterToUrl(url, "dialogmode", "1");

            return string.Format("<img src=\"{0}\" onclick=\"modalDialog('{1}', 'SKUEdit', '95%', '95%'); return false;\" alt=\"\" title=\"{2}\" class=\"OrderItemEditLink\" style=\"cursor: pointer;\" />",
                GetImageUrl("Objects/Ecommerce_OrderItem/editsku.png"),
                url,
                GetString("shoppingcart.editproduct"));
        }

        return "";
    }


    /// <summary>
    /// Returns formatted child cart item units. Returns empty string if it is not product option or bundle item.
    /// </summary>
    /// <param name="skuUnits">SKU units</param>
    /// <param name="isProductOption">Indicates if cart item is product option</param>
    /// <param name="isBundleItem">Indicates if cart item is bundle item</param>
    protected static string GetChildCartItemUnits(object skuUnits, object isProductOption, object isBundleItem)
    {
        if (ValidationHelper.GetBoolean(isProductOption, false) || ValidationHelper.GetBoolean(isBundleItem, false))
        {
            return string.Format("<span>{0}</span>", skuUnits);
        }

        return "";
    }

    #endregion

    #region Ajout repeater

    protected void RptCartItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        var drv = (System.Data.DataRowView)e.Item.DataItem;
        if (drv != null)
        {
            int currentSKUID = ValidationHelper.GetInteger(drv["SKUID"], 0);
            if (currentSKUID > 0)
            {
                SKUInfo sku = SKUInfoProvider.GetSKUInfo(currentSKUID);
                if (sku != null)
                {
                    int subTotal = 0;
                    double remise = 0;
                    //Display product image
                    var ltlProductImage = e.Item.FindControl("ltlProductImage") as Literal;
                    if (ltlProductImage != null)
                        //<%--# EcommerceFunctions.GetProductImage(Eval("SKUImagePath"), Eval("SKUName"))--%>
                        ltlProductImage.Text = EcommerceFunctions.GetProductImage(sku.SKUImagePath, sku.SKUName);

                    var ltlProductName = e.Item.FindControl("ltlProductName") as Literal;
                    if (ltlProductName != null)
                        ltlProductName.Text = sku.SKUName;

                    var txtProductCount = e.Item.FindControl("txtProductCount") as TextBox;
                    if (txtProductCount != null)
                    {
                        foreach (ShoppingCartItemInfo shoppingCartItem in ShoppingCart.CartItems)
                        {
                            if (shoppingCartItem.SKUID == sku.SKUID)
                            {
                                remise = shoppingCartItem.UnitTotalDiscount;
                                txtProductCount.Text = shoppingCartItem.CartItemUnits.ToString();
                                subTotal = shoppingCartItem.CartItemUnits;
                                break;
                            }
                        }
                    }

                    var ltlProductPrice = e.Item.FindControl("ltlProductPrice") as Literal;
                    if (ltlProductPrice != null)
                    {
                        ltlProductPrice.Text = EcommerceFunctions.GetFormatedPrice((sku.SKUPrice - remise) * subTotal, sku.SKUDepartmentID, ShoppingCart, sku.SKUID,false );
                        //ltlProductPrice.Text = CurrencyInfoProvider.GetFormattedValue((sku.SKUPrice - remise) * subTotal, ShoppingCart.Currency);// EcommerceFunctions.GetFormatedPrice((sku.SKUPrice - remise) * subTotal, sku.SKUDepartmentID, sku.SKUID);
                        //ltlProductPrice.Text = string.Format("{0}<em>{1}</em>", ltlProductPrice.Text.Substring(0, ltlProductPrice.Text.Length - 1).Trim(), ltlProductPrice.Text.Substring(ltlProductPrice.Text.Length - 1,1).Trim());
                    }   
                }
            }
        }
    }

    protected void RptCartItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName.Equals("Remove"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            // Remove product and its product option from list
            this.ShoppingCart.RemoveShoppingCartItem(cartItemGuid);

            if (!this.ShoppingCartControl.IsInternalOrder)
            {
                // Delete product from database
                ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(cartItemGuid);
            }
            ddlShippingCountry_SelectedIndexChanged(null, null);
            btnUpdate_Click1(null, null);
            //ReloadData();
        }
        if (e.CommandName.Equals("Decrease"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            ShoppingCartItemInfo cartItem = ShoppingCart.GetShoppingCartItem(cartItemGuid);
            if (cartItem != null)
            {
                if (cartItem.CartItemUnits - 1 > 0)
                {
                    cartItem.CartItemUnits--;
                    // Update units of child bundle items
                    foreach (ShoppingCartItemInfo bundleItem in cartItem.BundleItems)
                    {
                        bundleItem.CartItemUnits--;
                    }

                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);

                        // Update product options in database
                        foreach (ShoppingCartItemInfo option in cartItem.ProductOptions)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(option);
                        }
                    }
                    ddlShippingCountry_SelectedIndexChanged(null, null);
                    btnUpdate_Click1(null, null);


                }
            }
        }
        if (e.CommandName.Equals("Increase"))
        {
            var cartItemGuid = new Guid(e.CommandArgument.ToString());
            ShoppingCartItemInfo cartItem = ShoppingCart.GetShoppingCartItem(cartItemGuid);
            if (cartItem != null)
            {
                if (cartItem.CartItemUnits + 1 > 0)
                {
                    cartItem.CartItemUnits++;
                    // Update units of child bundle items
                    foreach (ShoppingCartItemInfo bundleItem in cartItem.BundleItems)
                    {
                        bundleItem.CartItemUnits++;
                    }

                    if (!ShoppingCartControl.IsInternalOrder)
                    {
                        ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(cartItem);

                        // Update product options in database
                        foreach (ShoppingCartItemInfo option in cartItem.ProductOptions)
                        {
                            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(option);
                        }
                    }
                    ddlShippingCountry_SelectedIndexChanged(null, null);
                    btnUpdate_Click1(null, null);
                }
            }
        }

    }

    #endregion
    protected void Button1_Click(object sender, EventArgs e)
    {
        ButtonNextClickAction();
        ButtonNextClickAction();
    }

    /// <summary>
    /// Back button clicked.
    /// </summary>
    protected void btnBack_Click(object sender, EventArgs e)
    {
        ButtonBackClickAction();
    }


    /// <summary>
    /// Next button clicked.
    /// </summary>
    protected void btnNext_Click(object sender, EventArgs e)
    {
        ButtonNextClickAction();
        ButtonNextClickAction();
    }
    protected void Button2_Click(object sender, EventArgs e)
    {
        
    }
	
	protected string GetProductNodeAliasPath(object skuid)
	{        
		SKUInfo sku = SKUInfoProvider.GetSKUInfo((int)skuid);

		if (sku != null)
		{
			GeneralConnection cn = ConnectionHelper.GetConnection();
			string stringQuery = string.Format("select NodeAliasPath from View_CONTENT_Product_Joined where NodeSKUID = " + sku.SKUID);
			DataSet ds = cn.ExecuteQuery(stringQuery, null, CMS.SettingsProvider.QueryTypeEnum.SQLQuery, false);
			string NodeAliasPath = Convert.ToString(ds.Tables[0].Rows[0]["NodeAliasPath"]);
			return "~" + NodeAliasPath + ".aspx";
		}
		return String.Empty;
	}
}
