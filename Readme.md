# ASP.Net Core Templates

asp.net core�� ���õ� ������Ʈ���� �ٷ� ���߿� ������ �� �ְ� ���ø� ����� ����

## Documents

### ApiServer

#### ����

���ø����� �����Ǵ� ��Ʈ�ѷ��� Ȱ���� ����Ͻ� ������ ���� �����ϴ� ���� ��ǥ�� �Ѵ�.


#### ����� ����
- Redis
- Memcached
- Dapper ( for RDBS )
- NLog
- Swagger

Redis�� Memcached, DB ���� ����� ���ؼ��� appsetting.json�� ���õ� ������ �������ν� �ٷ� ����� �� �ְ� ASP.Net Core ���¿� �°� �� ���񽺴� DI���� ������ �ȴ�.


#### Ư¡
- Swagger�� �������� ���� Ȯ��� �������·� ������ �Ǹ� �ּ����� ���� api���� ���������� ���� �� �ִ�.
- Log ����� Middleware�� Ȱ���� �� api ��Ʈ�ѷ��� ���Ӽ��� ǥ���� �� �ִ�.
- ��Ʈ�ѷ��� Exception�� ������ ����� ���忡�� ����ó���� �ٷ�� ���� �Ǿ��ִ�.

#### ����

Swagger ���� Ȱ�� ����
![swagger](./Docs/swagger.PNG)

Logging ó�� ����
![log](./Docs/log.PNG)

### SignalRServer (Real Time)

�ǽð� ä���� ���� SignalR Ȱ�� 