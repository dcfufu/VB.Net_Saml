���M�׬�
�@��²��C#.NET MVC �M��
Framework 4.5

�M�׫إ߫�
�Q��NuGet�M��A�U���W��" AspNetSaml" �M��
�|�X�{ Saml.cs �ɮ�

�̷ӿ��~�T���A�N�M�׻ݭn�� Dependency �[�J

�إߤ@�ӷs��controller
�R�W�ݭn�ŦX keycloak ���w�� saml client �W�� master url �]�p���|

�Ѧ� https://github.com/jitbit/AspNetSaml/

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


log out �����A�|�����զ��\
�ثe�����ɦV�n�X SAM https://10.1.4.71/pkmslogout