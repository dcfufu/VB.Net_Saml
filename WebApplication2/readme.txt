此專案為
一個簡易VB.NET MVC 專案
Framework 4.7




本專案參考 https://github.com/jitbit/AspNetSaml/ 以VB.NET實作

建立一個新的controller
命名需要符合 keycloak 指定的 saml client 上的 master url 設計路徑

在controller裡，加入上方連結裡的程式
更新 samlEndpoint、client name、redirect url

通過 keycloak 導回之後進到另一個自訂方法
samlCertificate 處修改成 keycloak 上給予的 certificate

目前因為版本關係，Saml.cs 內容在 IsValid() 處稍作修改
signedXml.CheckSignature(_certificate.cert, true) 修改為 signedXml.CheckSignature()

通過驗證後
程式進行 XML 資料抓取
將原本 Saml.cs 內以下方法，做修改
GetNameID();
GetEmail();
GetFirstName();
GetLastName();

需要對應到 keycloak 上面設定
全部 Attribute 改抓 FriendlyName

自訂新增 GetDisplayname() 抓取 displayname

最後
將所有抓取出得值，顯示於頁面上
