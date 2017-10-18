<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Matches.aspx.cs" Inherits="CuescoreScheduleWeb._Matches" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Cuescore Scheduler</h1>
        <h2>Bekijk schema</h2>
        <table class="lead" style="width:100%;">
        <%
        var i = 0;
        foreach (var appointment in Appointments) { %>
            <tr>
                <td><%: (i+1) %></td>
                <td><%: appointment.Name %></td>
                <td><%: appointment.Location %></td>
                <td><%: appointment.DateTime %></td>
            </tr>
            <%
            i++;
        }
        %>
        </table>
        <p><asp:LinkButton class="btn btn-primary btn-lg" ID="Download_ICAL" runat="server" OnClick="DownloadICAL">Download ICAL &raquo;</asp:LinkButton></p>
    </div>
</asp:Content>
