���M�׬�
�@��²��VB.NET MVC �M��
Framework 4.7




���M�װѦ� https://github.com/jitbit/AspNetSaml/ �HVB.NET��@

�إߤ@�ӷs��controller
�R�W�ݭn�ŦX keycloak ���w�� saml client �W�� master url �]�p���|

�bcontroller�̡A�[�J�W��s���̪��{��
��s samlEndpoint�Bclient name�Bredirect url

�q�L keycloak �ɦ^����i��t�@�Ӧۭq��k
samlCertificate �B�ק令 keycloak �W������ certificate

�ثe�]���������Y�ASaml.cs ���e�b IsValid() �B�y�@�ק�
signedXml.CheckSignature(_certificate.cert, true) �קאּ signedXml.CheckSignature()

�q�L���ҫ�
�{���i�� XML ��Ƨ��
�N�쥻 Saml.cs ���H�U��k�A���ק�
GetNameID();
GetEmail();
GetFirstName();
GetLastName();

�ݭn������ keycloak �W���]�w
���� Attribute ��� FriendlyName

�ۭq�s�W GetDisplayname() ��� displayname

�̫�
�N�Ҧ�����X�o�ȡA��ܩ󭶭��W
