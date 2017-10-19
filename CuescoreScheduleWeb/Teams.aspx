<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Teams.aspx.cs" Inherits="CuescoreScheduleWeb._Teams" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Cuescore Scheduler</h1>
        <h2>Kies team</h2>
        <p class="lead">
        <%
        var i = 0;
        foreach (var team in GetTeams()) { %>
            <a href="/Matches/<%: LeagueID + "/" + HttpUtility.UrlEncode(team) %>">
            <%: (i+1) + ". " + team %>
            </a>
            <br />
            <%
            i++;
        }
        %>
        </p>
    </div>
</asp:Content>
