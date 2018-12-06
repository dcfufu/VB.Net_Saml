@Code
    ViewData("Title") = "Success"
End Code

<h2>username: @ViewData("username").</h2>
<h2>email: @ViewData("email").</h2>
<h2>displayname: @ViewData("displayname").</h2>

<p><a href="/keycloakSamlServlet/Logout" class="btn btn-primary btn-lg">Log Out SAM &raquo;</a></p>