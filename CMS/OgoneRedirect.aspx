﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OgoneRedirect.aspx.cs" Inherits="OgoneRedirect" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Porte d'Orient - Commande en cours de traitement</title>
</head>
<body runat="server" id="body"><%--onload="window.document.forms[0].submit();"--%>
    <form method="post" action="https://secure.ogone.com/ncol/prod/orderstandard.asp" id="paymentForm" name="paymentForm" runat="server">
    <div>
        <%--        
        <asp:Label ID="lblOgoneUrl" Text="text" runat="server" />
        <asp:Label ID="lblOgoneID" Text="text" runat="server" />
        <asp:Label ID="lblOgoneSHA" Text="text" runat="server" />
        --%>
        <asp:Label runat="server" ID="lblInfo" Text="Votre commande est en cours de traitement..."></asp:Label>
    </div>
    </form>
</body>
</html>
