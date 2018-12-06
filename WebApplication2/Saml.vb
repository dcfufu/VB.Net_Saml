Imports System.IO
Imports System.IO.Compression
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Cryptography.Xml
Imports System.Xml

Namespace Saml
    Public NotInheritable Class RSAPKCS1SHA256SignatureDescription
        Inherits SignatureDescription

        Public Sub New()
            KeyAlgorithm = GetType(RSACryptoServiceProvider).FullName
            DigestAlgorithm = GetType(SHA256Managed).FullName
            FormatterAlgorithm = GetType(RSAPKCS1SignatureFormatter).FullName
            DeformatterAlgorithm = GetType(RSAPKCS1SignatureDeformatter).FullName
        End Sub

        Public Overrides Function CreateDeformatter(ByVal key As AsymmetricAlgorithm) As AsymmetricSignatureDeformatter
            If key Is Nothing Then Throw New System.ArgumentNullException("key")
            Dim deformatter As RSAPKCS1SignatureDeformatter = New RSAPKCS1SignatureDeformatter(key)
            deformatter.SetHashAlgorithm("SHA256")
            Return deformatter
        End Function

        Public Overrides Function CreateFormatter(ByVal key As AsymmetricAlgorithm) As AsymmetricSignatureFormatter
            If key Is Nothing Then Throw New System.ArgumentNullException("key")
            Dim formatter As RSAPKCS1SignatureFormatter = New RSAPKCS1SignatureFormatter(key)
            formatter.SetHashAlgorithm("SHA256")
            Return formatter
        End Function

        Private Shared _initialized As Boolean = False

        Public Shared Sub Init()
            If Not _initialized Then CryptoConfig.AddAlgorithm(GetType(RSAPKCS1SHA256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")
            _initialized = True
        End Sub
    End Class

    Public Class Certificate
        Public cert As X509Certificate2

        Public Sub LoadCertificate(ByVal certificate As String)
            LoadCertificate(StringToByteArray(certificate))
        End Sub

        Public Sub LoadCertificate(ByVal certificate As Byte())
            cert = New X509Certificate2()
            cert.Import(certificate)
        End Sub

        Private Function StringToByteArray(ByVal st As String) As Byte()
            ' Dim bytes As Byte() = New Byte(st.Length - 1) {}

            ' For i As Integer = 0 To st.Length - 1
            'bytes(i) = ASCIIEncoding().GetBytes(st(i))
            'Next

            'Return bytes
            Dim encoding As New System.Text.ASCIIEncoding()
            Return encoding.GetBytes(st)
        End Function
    End Class

    Public Class Response
        Private _xmlDoc As XmlDocument
        Private _certificate As Certificate
        Private _xmlNameSpaceManager As XmlNamespaceManager

        Public ReadOnly Property Xml As String
            Get
                Return _xmlDoc.OuterXml
            End Get
        End Property

        Public Sub New(ByVal certificateStr As String)
            RSAPKCS1SHA256SignatureDescription.Init()
            _certificate = New Certificate()
            _certificate.LoadCertificate(certificateStr)
        End Sub

        Public Sub LoadXml(ByVal xml As String)
            _xmlDoc = New XmlDocument()
            _xmlDoc.PreserveWhitespace = True
            _xmlDoc.XmlResolver = Nothing
            _xmlDoc.LoadXml(xml)
            _xmlNameSpaceManager = GetNamespaceManager()
        End Sub

        Public Sub LoadXmlFromBase64(ByVal response As String)
            Dim enc As System.Text.UTF8Encoding = New System.Text.UTF8Encoding()
            LoadXml(enc.GetString(System.Convert.FromBase64String(response)))
        End Sub

        Public Function IsValid() As Boolean
            Dim nodeList As XmlNodeList = _xmlDoc.SelectNodes("//ds:Signature", _xmlNameSpaceManager)
            Dim signedXml As SignedXml = New SignedXml(_xmlDoc)
            If nodeList.Count = 0 Then Return False
            signedXml.LoadXml(CType(nodeList(0), XmlElement))
            Return ValidateSignatureReference(signedXml) AndAlso signedXml.CheckSignature() AndAlso Not IsExpired()
        End Function

        Private Function ValidateSignatureReference(ByVal signedXml As SignedXml) As Boolean
            If signedXml.SignedInfo.References.Count <> 1 Then Return False
            Dim reference = CType(signedXml.SignedInfo.References(0), Reference)
            Dim id = reference.Uri.Substring(1)
            Dim idElement = signedXml.GetIdElement(_xmlDoc, id)

            If idElement Is _xmlDoc.DocumentElement Then
                Return True
            Else
                Dim assertionNode = TryCast(_xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion", _xmlNameSpaceManager), XmlElement)
                If assertionNode IsNot idElement Then Return False
            End If

            Return True
        End Function

        Private Function IsExpired() As Boolean
            Dim expirationDate As System.DateTime = System.DateTime.MaxValue
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", _xmlNameSpaceManager)

            If node IsNot Nothing AndAlso node.Attributes("NotOnOrAfter") IsNot Nothing Then
                System.DateTime.TryParse(node.Attributes("NotOnOrAfter").Value, expirationDate)
            End If

            Return System.DateTime.UtcNow > expirationDate.ToUniversalTime()
        End Function

        Public Function GetAllAttribute() As String
            Dim nodeList As XmlNodeList = _xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:AttributeStatement", _xmlNameSpaceManager)
            Dim sb As System.Text.StringBuilder = New StringBuilder()

            For Each OneNode As XmlNode In nodeList
                Dim nodeLists As XmlNodeList = OneNode.ChildNodes
                Dim StrNodeName As System.String = OneNode.Name.ToString()

                For Each Attr As XmlNode In nodeLists
                    Dim StrAttr As System.String = Attr.Name.ToString()
                    Dim StrValue As System.String = OneNode.Attributes(Attr.Name.ToString()).Value
                    Dim StrInnerText As System.String = OneNode.InnerText
                    sb.Append(StrAttr).Append(": ").Append(StrValue).Append(vbLf)
                Next
            Next

            Return sb.ToString()
        End Function

        Public Function GetNameID() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", _xmlNameSpaceManager)
            Return node.InnerText
        End Function

        Public Function GetDisplayname() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@FriendlyName='displayname']/saml:AttributeValue", _xmlNameSpaceManager)
            If node Is Nothing Then node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetEmail() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@FriendlyName='email']/saml:AttributeValue", _xmlNameSpaceManager)
            If node Is Nothing Then node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetFirstName() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@FriendlyName='firstName']/saml:AttributeValue", _xmlNameSpaceManager)
            If node Is Nothing Then node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetLastName() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@FriendlyName='lastName']/saml:AttributeValue", _xmlNameSpaceManager)
            If node Is Nothing Then node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetDepartment() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/department']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetPhone() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone']/saml:AttributeValue", _xmlNameSpaceManager)
            If node Is Nothing Then node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/telephonenumber']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Public Function GetCompany() As String
            Dim node As XmlNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/companyname']/saml:AttributeValue", _xmlNameSpaceManager)
            Return If(node Is Nothing, Nothing, node.InnerText)
        End Function

        Private Function GetNamespaceManager() As XmlNamespaceManager
            Dim manager As XmlNamespaceManager = New XmlNamespaceManager(_xmlDoc.NameTable)
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl)
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion")
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol")
            Return manager
        End Function
    End Class

    Public Class AuthRequest
        Public _id As String
        Private _issue_instant As String
        Private _issuer As String
        Private _assertionConsumerServiceUrl As String

        Public Enum AuthRequestFormat
            Base64 = 1
        End Enum

        Public Sub New(ByVal issuer As String, ByVal assertionConsumerServiceUrl As String)
            RSAPKCS1SHA256SignatureDescription.Init()
            _id = "_" & System.Guid.NewGuid().ToString()
            _issue_instant = System.DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            _issuer = issuer
            _assertionConsumerServiceUrl = assertionConsumerServiceUrl
        End Sub

        Public Function GetRequest(ByVal format As AuthRequestFormat) As String
            Using sw As StringWriter = New StringWriter()
                Dim xws As XmlWriterSettings = New XmlWriterSettings()
                xws.OmitXmlDeclaration = True

                Using xw As XmlWriter = XmlWriter.Create(sw, xws)
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol")
                    xw.WriteAttributeString("ID", _id)
                    xw.WriteAttributeString("Version", "2.0")
                    xw.WriteAttributeString("IssueInstant", _issue_instant)
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST")
                    xw.WriteAttributeString("AssertionConsumerServiceURL", _assertionConsumerServiceUrl)
                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion")
                    xw.WriteString(_issuer)
                    xw.WriteEndElement()
                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol")
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified")
                    xw.WriteAttributeString("AllowCreate", "true")
                    xw.WriteEndElement()
                    xw.WriteEndElement()
                End Using

                If format = AuthRequestFormat.Base64 Then
                    Dim memoryStream = New MemoryStream()
                    Dim writer = New StreamWriter(New DeflateStream(memoryStream, CompressionMode.Compress, True), New UTF8Encoding(False))
                    writer.Write(sw.ToString())
                    writer.Close()
                    Dim result As String = System.Convert.ToBase64String(memoryStream.GetBuffer(), 0, CInt(memoryStream.Length), System.Base64FormattingOptions.None)
                    Return result
                End If

                Return Nothing
            End Using
        End Function

        Public Function GetRedirectUrl(ByVal samlEndpoint As String) As String
            Dim queryStringSeparator = If(samlEndpoint.Contains("?"), "&", "?")
            Return samlEndpoint & queryStringSeparator & "SAMLRequest=" & HttpUtility.UrlEncode(Me.GetRequest(AuthRequest.AuthRequestFormat.Base64))
        End Function
    End Class
End Namespace
