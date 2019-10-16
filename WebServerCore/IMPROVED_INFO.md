

# Reason for improvement

- 현재 제공되는 서버 템플릿의 대한 상세 문서 부재
  > 디렉토리 구조의 설명이나 패키지화 되지 않는 부분이 존재하는데 명확한 설명이 없음

- NPLibrary에 참조된 오픈소스 복사문제
  > 현재 적용된 오픈소스의 버전을 알 수 없으며 라이센스 문제도 있을 수 있음
  
- NPLibrary 로그
  > 라이브러리 안에 설정이 들어가서 단일파일의 로그만 사용할 수 있게 제약적으며 부족한 로그 내용도 추가 할 수 없음

- 데이터베이스 접근
  > DB로직 개발 시 불편함이 존재하며 계속적인 빈객체 할당 코드로 접근하고 있음

- BusinessLogic 클래스
  > 광범위하게 소스가 Coupling 되고 있어서 객체 자체의 역할이 불분명하고 리팩토링이 필요해 보임

### Key Point 

현재 큰 문제가 있어서 개선이 진행되는 부분이 아니라 제약된 라이브러리의 확장성 제공과 리팩토링을 통해 코드 개선의 필요성을 느낌

따라서, 언제나 수정이나 리팩토링이 가능한 구조로 최대한 Coupling을 제거하고 클래스별/패키지별 명확한 역할 분리를 목표로 함.

# As-is

### Base Template Structure

```
WebServer
├──App_Start            # Web Api 설정 정보
├──Controllers          # Web Api Controllers
├──DBProcess            # 데이터 베이스 처리
│  ├──Login             # 논리적 로그인 접근에 필요한 DB 로직
│  └──Session           # 논리적 세션 접근에 필요한 DB 로직
├──Properties           # 프로퍼티 파일
├──BusinessLogic.cs     # Api 로직 처리의 부모 클래스
├──DataService.cs       # Memcached, Redis 접근하는 클래스
├──WebSession.cs        # 웹 세션 클래스
└── ...                 # Config 파일이나 각종 define 파일 등

Dependencies
└──NPLibrary            # C# 라이브러리
```

소스 구조는 앤파크에서 제공되는 웹 서버 템플릿을 기반으로 개발이 진행됨.

> http://183.110.18.219/hwi0324/ServerTemplate/tree/dev Npark 기술 공유 사이트에 제공됨

### Next Template Structure

```
WebServer
├──App_Data             # Web Api 관련 데이터 정보
├──App_Start            # Web Api 설정 정보
├──Common               # 공통 설정 정보
├──Configuration        # 시스템 ConfigurationElement 설정 정보
├──Contents             # Contents 관련 처리
├──Controllers          # Web Api Controllers
├──Data                 # DB에 접근 데이터베이스 처리
│  ├──Access            # DB에 실제로 호출하는 부분 
│  └── ...              # DB에 요청하는 비즈니스 로직 부분
├──DBProcess            # 데이터 베이스 처리
│  ├──Postbox           # Post DB 관련 처리
│  └──SPResult.cs       # 프로시저 호출 결과 클래스
├──LogDB                # Log DB 관련 처리
│  ├──Access            # Log DB에 실제로 호출하는 부분 
│  └── ...              # Log DB에 요청하는 비즈니스 로직 부분
├──Models               # 로직에 사용되는 데이터 모델 모음
│  ├──PB                # 기획 데이터 모델
│  └── ...              # 일반 로직 데이터 모델
├──PBTable              # 기획 데이터 처리 부분
├──Process              # 비즈니스 로직 부분 처리( BusinessLogic 상속 )
│  ├──Account           # 계정 관련 비즈니스 로직
│  └── ...              # ...
├──BusinessLogic.cs     # Api 로직 처리의 부모 클래스
├──DataService.cs       # Memcached, Redis 접근하는 클래스
├──WebSession.cs        # 웹 세션 클래스
└── ...                 # Config 파일이나 각종 define 파일 등

Dependencies
└──NPLibrary            # C# 라이브러리
```

> Next Template Structure 는 Base Template의 확장 형태로 **현재 Npark 프로젝트에서 기본으로 사용**되는 구조임


# To-be

### Structure

```
WebServer
├──Cache                # 게임서버 캐쉬 데이터 관련 처리
│  ├──PBTable           # 기획 데이터 캐쉬 저장 정보
│  └──CacheManager.cs   # 각 캐쉬 관리하는 객체
├──Common               # 공통 설정 정보
│  ├──Define            # Server Define 정보
│  └── ...              # ...
├──Controllers          # Web Api Controllers
├──Core                 # Web Server의 프로세스 관련 처리
│  ├──MessageHandler    # Api Controller의 Web MessageHandler 모음
│  ├──WebService.cs     # 요청/응답에 따른 api 프로세스 객체
│  ├──WebSession.cs     # 세션 정보를 처리하는 객체
│  └── ...              # ...
├──Database             # 데이터 베이스 처리
│  ├──Executor          # DB Connection 실행 처리 부분
│  ├──External          # 외부 DB 처리
│  │  ├──Memcached      # 외부 DB Memcached 처리 부분
│  │  └──Redis          # 외부 DB Redis 처리 부분
│  ├──Utils             # DB 내에서 쓰이는 유틸 정보
│  ├──AccountDB.cs      # 물리적 접근 AccountDB명
│  ├──PostDB.cs         # 물리적 접근 PostDB명
│  ├──LogDB.cs          # 물리적 접근 LogDB명
│  └── ...              # 물리적 접근 데이터베이스 명명 존재 ( LogDB, PostDB 등.. )
├──Models               # 데이터 오브젝트 정보
└── ...                 # Config 설정 파일만 존재
```

Improved

1. Web MVC 서버의 App_Start, App_Start, Models 등의 개념을 가지는 폴더를 지움으로써 Web Api Controller로써의 서버 개념만 가지게 함.

2. BusinessLogic.cs 의 복잡한 구조 및 WebSession.cs 정보를 Core 패키지의 WebService.cs 와 WebSession.cs 으로 표현하며 웹 내에서만 쓰이는 개념으로 정리하고 Api Controller와 함께 세션 기능도 포함한 처리가 가능하게 제공함.
    
3. DataService.cs 의 InMemory 접근 DB처리를 DB 외부 처리의 부분으로 옮기며 모호한 DataService 개념이 아닌 각각의 외부 DB 명칭으로 제공하게 함.

4. PBTable과 Models에 포함되어있던 기획 데이터 자체가 서버 내에서 Cache 하는 데이터이기 때문에 Cache 관리자를 추가해 확장 가능한 구조로 변경. 

5. Data, DBProcess, LogDB 등의 각각의 DB 접근 구조를 하나의 Database 패키지에 넣어 물리적 DB 명을 통해 각각을 분리함.

6. DBProcess 코드에서 처리되던 DB관련 로직 부분을 Process를 거치지 않고 Controller에서 Access(실제로 DB 호출)을 다이렉트로 호출하는 방식으로 처리.

7. API Controller 흐름에서 각 API별 비지니스 로직이 Process폴더 내에 각각의 파일로 구현되어있었는데, Controller코드 내에서 처리되도록 변경.

8. 기존에 한 Controller 파일안에서 Action별 API를 구현하였는데, 각각의 Controller파일로 분리.

9. 참조로 사용되는 NPLibrary는 사용 안함. 

### Improved Feature

##### 로그 설정

- logger 플랫폼에 log4net을 적용하였으며 info(정상+비정상), abnormal(비정상)로그로 구분해서 각각의 파일로 저장
- 클래스별로 logger를 구분해 logger명으로 구분된 로그와 Thead 넘버도 로그에 추가
- 일별로 구분된 로그로 저장해 이슈 발생 시 해당 날짜 바로 파악 가능
- .NET Core로 개발되 Linux 장비에서도 배포할 가능성이 있어 한글 로그는 제거

```
<기존 로그>
[INFO] 2018-08-06 18:08:13.539 : [Data.PBData] ReadGameConstantData - [1] inventory_item_default : 50
[INFO] 2018-08-06 18:08:13.557 : [Data.PBData] ReadGameConstantData - [2] inventory_item_max : 200
[ERROR] 2018-08-06 18:08:17.810 : Redis 셋업 에러

<변경 로그>
10:16:46,120 [1] INFO  WebServer.WebApiApplication - Ready for start web server...
10:16:46,550 [1] INFO  WebServer.ConstConfig - Complete to initialize server const config info
10:16:46,554 [1] INFO  WebServer.DynamicConfig - Complete to initialize dynamic config file
10:16:47,765 [1] INFO  WebServer.WebApiApplication - Complete to start web server...
```

##### Config 및 Manager 클래스

- Config는 Singleton이 아닌 static 객체로 변경하며 const(수정 불가)와 dynamic(수정 가능)의 두가지 Config만 제공
- Manager는 Singleton으로 구현하며 기존 NPLibrary에 있는 Singleton은 성능 이슈가 있어 싱글톤 패턴 방식 변경

< 기존 싱글톤 상속으로 구현 >

```
public class NPSingleton<T> where T : class, new()
{
    private static T __instance;
    private static object __lockObject = new object();
    
    public static T Instance
    {
        get
        {
            lock ( __lockObject ) 
            {
                if ( __instance == null )
                {
                    T local = default( T );
                    __instance = ( local == null ) ? Activator.CreateInstance<T>() : local;
                }
            }

            return __instance;
        }
    }
}
```
Issues

- 기본 생성자를 통해 싱글톤 객체도 new() 로 생성 가능
- 접근 할 때마다 계속적인 락처리 ( 성능 저하 )

< 변경된 싱글톤 패턴( 상속으론 구현 못함 ) >

```
public sealed class ConstConfig
{
    static ConstConfig()
    {
        Instance = new ConstConfig();
    }
    private ConstConfig() { }

    public static ConstConfig Instance { get; }
}
```

##### Api Controller

- BusinessLogic 클래스 사용한 부분을 제거하고 해당 클래스의 로직을 분리해 WebServer, WebSession 으로 객체화 해서 처리하는 방식으로 변경
- Controller 클래스에서 요청/응답에 대한 validation 체크 Func로 Controller 내에서 패킷 구조를 알 수 있음
- Api Handler 적용으로 Api와 WebServer 사이의 패킷 핸들링 가능

<기존 Controller>

```
// Controller 처리
{
    ProcessLogin proc = new ProcessLogin( request, this.Url );

    if ( proc.Initialize() == false )
    {
        return proc.GetFailResponse();
    }

    if ( proc.CheckRequestData() == false )
    {
        return proc.GetFailResponse();
    }

    if ( proc.ProcessRequest() == false )
    {
        return proc.GetFailResponse();
    }
    return proc.GetSuccessResponse();
}

internal sealed class ProcessLogin : BusinessLogic<WebResLogin>
{
    // 로직 구현
    ...
}
```

Issues

- BusinessLogic을 상속받은 객체를 생성후 각 객체의 함수 실행으로 로직 분기 구현
- 패킷 요청/응답을 알 수 없고 패킷의 흐름을 보기 어려움

<변경된 Controller>

```
// 요청/응답 validator func
private static readonly Func<ReqLogin, bool> ReqeustDataValidator = reqData =>
{
    // 요청 데이터 값들 검증
    return true;
};

private static readonly Func<ResLogin, bool> ResponseDataValidator = resLogin =>
{
    // 응답 데이터 값들 검증
    return true;
};

// Controller 처리
{
    // 패킷 고유 번호 (로그 흐름체크에 유용함)(최상단 Api Handler에서 지정)
    int requestNo = ( int )Request.Properties[ HeaderProperties.REQUEST_NO.ToString() ];

    var webService = WebService<ReqLogin, ResLogin>.Create( ConstConfig.LoginKey, ConstConfig.LoginIV, Request, requestBody );
    if ( webService.errorCode != ErrCode.SUCCESS)
    {
        log.ErrorFormat( "[RequestValidator] Request web service error - No:{0}, Error:{1}", requestNo, webService.errorCode );
        return webService.End( webService.errorCode );
    }
    
    // 로직 구현
    ...
    return webService.End();
}
```

Improved

- Controller 상단의 요청/응답에 대해서 바로 파악이 되며 패킷 요청 검증 부분 로직 분리
- WebService 객체를 통한 로직 구현으로 간결하며 구조화된 흐름으로 Controller 개발 가능

> 현 문서에선 개선되는 방향만 다루며 WebService의 상세 설명은 WebServer 문서에 포함될 예정

##### Database

- 로직으로 구분된 DB 접근이 아닌 각각의 물리적 DB별 접근으로 변경
- Executor를 통해서 DB 접근의 특성에 맞는 커스터마이징한 Excutor 제공 가능
- 복잡하고 비효율적인 처리 부분 단순화

<기존 Database 로직 처리>

```
// 호출 부분
if (new Data.Process_Account().GetAccount(ReqData.PubID, ReqData.StoreType, out accountInfo) == false)
{
    //NPLog.PrintError(string.Format("[ProcessLogin, PubID:{0}] ProcessRequest - GetAccountInfo DB Error", ReqData.PubID));
    ResData.ResKey = (int)ErrCode.ERROR_DB;
    return false;
}

// 구현 부분
public bool GetAccount(string pubID, int storeType, out Models.Account resultData)
{
    resultData = null;

    DataSet dataSet = null;
    
    // 내부 DB 처리 접근 
    if (new Access.Account().GetAccount(pubID, storeType, out dataSet) == false)
    {
        NPLog.PrintError(string.Format("[Data] [Account] GetAccount - Not exist dataSet. pubID:{0}", pubID));
        return false;
    }
    ...
}
// 내부 DB 처리 구현 부분
public bool GetAccount(string pubID, int storeType, out DataSet resultDataSet)
{
    resultDataSet = null;
    List<SqlParameter> parameter = new List<SqlParameter>()
    { 
                                    new SqlParameter( "@pub_id", SqlDbType.NVarChar, 40 ),
                                    new SqlParameter( "@store_type", SqlDbType.Int ),
    };
    parameter[ 0 ].Value = pubID;
    parameter[ 1 ].Value = storeType;

    if (!RunStoredProcedure("dbo.USP_AC_ACCOUNT_R", parameter, ConstConfig.AccountDBConnString, out resultDataSet))
    {
        NPLog.PrintError(string.Format("[Data] [Access] GetAccount - RunStoredProcedure() failed. pubID:{0}", pubID));
        return false;
    }
    ...
}
```

Issues

- new 객체 할당 코드를 Access 부분과 일반 호출 부분의 로직 접근에 계속적으로 사용하고 있음
- 내부 DB 처리 안에서도 실제 DB 처리가 이뤄지는 함수를 호출해 DB로직 추가나 변경에 많은 부분를 수정해야 함

<변경된 database 로직 처리>

```
// 호출 부분
var accoutDB = new AccountDB();
if ( !accoutDB.GetAccount( reqData.PubID, reqData.StoreType, out Entity.Account accountInfo ) )
{
    log.ErrorFormat( "[Business] Failed to get account - No:{0}, PubId:{1}, StoreType:{2}", requestNo, reqData.PubID, reqData.StoreType );
    return webService.End( ErrCode.ERROR_DB );
}

// 구현 부분
public bool GetAccount( string pubId, int storeType, out Entity.Account resultData )
{
    resultData = null;
    DataSet dataSet = null;

    using ( SqlConnection conn = new SqlConnection( ConnString ) )
    {
        MaguSPExecutor executor = new MaguSPExecutor( conn );
        executor.AddInputParam( "@pub_id", SqlDbType.NVarChar, 40, pubId );
        executor.AddInputParam( "@store_type", SqlDbType.Int, storeType );
                
        if( !executor.RunStoredProcedure( "dbo.USP_AC_ACCOUNT_R", out dataSet ) || dataSet == null )
        {
            log.ErrorFormat( "[GetAccount] Faied to execute procedure - pubId:{0}, storeType:{1}", pubId, storeType );
            return false;
        }
    }
    ...
}
```

Improved

- 구현 Depth를 줄여 DB 로직 추가나 변경 시 개발 편의 제공 
- DB 접근에 따른 new 객체 할당코드를 제거하고 물리적 접근 DB를 통해 Controller 내에서 접근하는 DB를 알 수 있음

### Summary

- 명확하게 흐름이 보이는 소스 제공 
    > 가독성이 좋은 소스

- 클래스별 Coupling 제거 
    > 가장 큰 Coupling인 BusinessLogic 클래스 제거

- 쉬운 개발 구조 제공 
    > 객체화 및 클래스 개념 분리

