Public Class HomeController
    Inherits System.Web.Mvc.Controller

    Function Index() As ActionResult

        '捷運列車到站資料
        Dim Url As String = "https://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=55ec6d6e-dc5c-4268-a725-d04cc262172b"

        Dim JsonStr As String = ""
        Dim objWebClient As New System.Net.WebClient
        JsonStr = Encoding.UTF8.GetString(objWebClient.DownloadData(New Uri(Url.Trim())))

        Dim Obj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.JsonConvert.DeserializeObject(JsonStr)

        Dim result As Object = Obj.Item("result")("results").ToString
        ViewData("result") = result
        Return View()
    End Function

    Function About() As ActionResult
        ViewData("Message") = "Your application description page."

        Return View()
    End Function

    Function Contact() As ActionResult
        ViewData("Message") = "Your contact page."

        Return View()
    End Function
End Class
