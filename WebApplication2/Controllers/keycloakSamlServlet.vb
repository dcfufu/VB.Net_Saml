
Namespace WebApplication7.Controllers
    Public Class keycloakSamlServletController
        Inherits Controller

        Public Function Index() As ActionResult
            Return View()
        End Function

        Public Sub About()
            Dim samlEndpoint = "https://10.1.4.71/auth/realms/TKU_Realm_Main/protocol/saml/clients/tkusaml"
            Dim request = New Saml.AuthRequest("TKUSAML", "http://localhost:8080/keycloakSamlServlet/saml")
            Dim url As String = request.GetRedirectUrl(samlEndpoint)
            Response.Redirect(url)
        End Sub

        Public Function Saml() As ActionResult
            Dim samlCertificate As String =
            "-----BEGIN CERTIFICATE-----
            MIICnTCCAYUCBgFmhO43OzANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQDDAdUS1VTQU1MMB4XDTE4MTAxODAyMDYzNloXDTI4MTAxODAyMDgxNlowEjEQMA4GA1UEAwwHVEtVU0FNTDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAIY+m3o7dzP5U9w1WckLRrtZ6XNGFHY++eRJKTE+ZVEUrkxv4kDOJA3zr6dMhuUuOz134WBix4FogmQlFtDJ/5oD9vYTNebwgnOjWxgNNu4XgEY7kF4H2NemM97E/WrLjMNFVghS8fcYrfYUssyUb6a76aF5mJNLY7tejkJmahHgERSSjzFjX74fFIXtp2RjVD1kguf2uRoXX+vnxq4UHQEJ+4uMAuXU1ku5Pn9nr1gCeqTiwvyotvEMb07K67r+aTCyB2+xicZr/bGy1YZviAHBsHQnhPSSnslhRQkdnwujqLwb/gCgbJnGX13H8huXHpQEsvzmPDDhnx1CK45ApYECAwEAATANBgkqhkiG9w0BAQsFAAOCAQEASyTqspKKFUKTPecYlaH0FZieNvFViR35U0bDfmjcCIC6MSyEP6iPZ1hUdXtPSkk4te7BZunNQe9yDFJz0dfLlnkVKhaZFbuiTG1hskQox9IuhyKhpv8fjUWni7FlYIW2GIMOar5LZVH1MKe91/614SVg+3sKQZ9i7VV31Uj591dzrpVje4Wxr6785cimLPes2UtBpYKzcgNbzRKv8Z8+p3NDwxDRA7qgqlNDmX3rl/8/zyrNSCad0O4kevFvJcFvwQZOsU9vD9ZI0DYXdCD3znJfcgrihtqRHJ7G1QseYvTaM4ZGtWNzpfD8XSRRlIEX6zD6ENU00l2ZHvm/byvD7Q==
            -----END CERTIFICATE-----"

            Dim samlResponse As Saml.Response = New Saml.Response(samlCertificate)
            ViewBag.response = Request.Form("SAMLResponse")

            Try
                samlResponse.LoadXmlFromBase64(Request.Form("SAMLResponse"))
            Catch ex As Exception
                Response.Redirect("~/")
                Return View()
            End Try

            If samlResponse.IsValid() Then
                Dim nameID, email, firstname, lastname, displayname As String

                Try
                    nameID = samlResponse.GetNameID()
                    email = samlResponse.GetEmail()
                    firstname = samlResponse.GetFirstName()
                    lastname = samlResponse.GetLastName()
                    displayname = samlResponse.GetDisplayname()
                Catch ex As Exception
                    Return View()
                End Try

                ViewBag.nameID = nameID
                ViewBag.username = firstname & "  " & lastname
                ViewBag.email = email
                ViewBag.displayname = displayname
            End If

            Return View()
        End Function

        Public Sub Logout()
            Dim samlLogoutEndpoint = "https://10.1.4.71/pkmslogout"
            Response.Redirect(samlLogoutEndpoint)
        End Sub
    End Class
End Namespace